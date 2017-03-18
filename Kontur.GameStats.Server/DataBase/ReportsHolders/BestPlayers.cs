using LiteDB;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Kontur.GameStats.Server.DataBase {
    public class BestPlayers {

        #region Fields

        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();
        private string dbConn;
        private Thread cleanerThread;
        private bool isCleaning;

        private double minKD = -1.0;

        #endregion

        public BestPlayers(string dbConnectionString) {
            dbConn = dbConnectionString;
        }

        #region Thread

        /// <summary>
        /// Запускает поток очищающий лишние матчи
        /// </summary>
        public void StartCleanThread() {
            isCleaning = true;
            cleanerThread = new Thread (CleanPlayers) {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            cleanerThread.Start ();
        }

        public void StopCleanThread() {
            isCleaning = false;
        }

        private void CleanPlayers() {
            while(isCleaning) {
                try {
                    using(var db = new LiteDatabase (dbConn)) {
                        var bestPlayersCol = db.GetCollection<BestPlayer> ();
                        if(bestPlayersCol.Count () > 0) {
                            var kd = bestPlayersCol.Find (
                                        Query.All ("KillToDeathRatio",
                                        Query.Descending), limit:50
                                ).Last ().KillToDeathRatio;
                            minKD = kd;
                            bestPlayersCol.Delete (
                                Query.LT ("KillToDeathRatio", kd)
                            );
                        }
                    }
                } catch(Exception e) {
                    logger.Error (e);
                } finally {
                    Thread.Sleep (60 * 1000); // Sleep 60 seconds
                }
            }
        }

        #endregion

        #region Add

        public void Add(Player player) {
            if(player.KD < minKD || player.TotalMatches < 10 || player.TotalDeaths == 0)
                return;
            using(var db = new LiteRepository (dbConn)) {
                db.Upsert (player.FormatAsBestPlayer());
            }
        }

        #endregion

        #region Take

        /// <summary>
        /// Возвращает в json массив из count последних
        /// матчей
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public string Take(int count) {
            string s;
            count = Math.Min (Math.Max (count, 0), 50);
            BestPlayer[] results;
            using(var db = new LiteDatabase (dbConn)) {
                var playersCol = db.GetCollection<BestPlayer> ();
                results = playersCol.Find(
                        Query.All ("KillToDeathRatio",
                        Query.Descending), limit:count)
                    .ToArray ();
            }
            s = JsonConvert.SerializeObject (results);
            return s;
        }

        #endregion
    }
}
