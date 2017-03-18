﻿using LiteDB;
using Newtonsoft.Json;
using NLog;
using System;
using System.Linq;
using System.Threading;

namespace Kontur.GameStats.Server.DataBase {

    /// <summary>
    /// Класс реализующий хранение последних 50 матчей
    /// При добавлении матча, добавляет его в базу данных,
    /// При запуске потока чистки будет очищать бд каждые 30 секунд
    /// </summary>
    class RecentMatches {

        #region Fields

        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();
        private string dbConn;
        private Thread cleanerThread;
        private bool isCleaning;

        #endregion

        public RecentMatches(string dbConnectionString) {
            dbConn = dbConnectionString;
        }

        #region Thread
        
        /// <summary>
        /// Запускает поток очищающий лишние матчи
        /// </summary>
        public void StartCleanThread() {
            isCleaning = true;
            cleanerThread = new Thread (ListenUpdatedPlayers) {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            cleanerThread.Start ();
        }

        public void StopCleanThread() {
            isCleaning = false;
        }

        private void ListenUpdatedPlayers() {
            while(isCleaning) {
                try {
                    using(var db = new LiteRepository (dbConn)) {
                        var firstMatch = db.Query<MatchInfo> ()
                            .Where (
                                    Query.All ("Timestamp",
                                    Query.Descending)
                            ).Limit(50)
                            .ToEnumerable()
                            .Last();
                        db.Delete<MatchInfo> (
                            Query.LT("Timestamp", firstMatch.Timestamp)
                        );
                    }
                    Thread.Sleep (30 * 1000); // Sleep 30 seconds
                } catch(Exception e) {
                    logger.Error (e);
                }
            }
        }

        #endregion

        #region Add

        public void Add(MatchInfo match) {
            using (var db = new LiteRepository(dbConn)) {
                db.Insert (match);
            }
        }

        public DateTime GetLastMatchTime() {
            DateTime last;
            using(var db = new LiteRepository (dbConn)) {
                last = db.Query<MatchInfo> ()
                    .Where (
                            Query.All ("Timestamp",
                            Query.Descending)
                    ).First ().Timestamp;
            }
            if(last != null) {
                return last;
            } else {
                return new DateTime (0).Date;
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
            MatchInfo[] results;
            using(var db = new LiteRepository (dbConn)) {
                results = db.Query<MatchInfo>()
                    .Where(
                        Query.All("Timestamp",
                        Query.Descending)
                    ).Limit(count)
                    .ToEnumerable()
                    .Select(match => { // Необходимое преобразование, т.к. БД переводит в локальное время
                        match.Timestamp = match.Timestamp.ToUniversalTime (); 
                        return match;
                    }).ToArray();
            }
            s = JsonConvert.SerializeObject (results);
            return s;
        }

        #endregion
    }
}