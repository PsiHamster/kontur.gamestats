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
    /// <summary>
    /// Класс реализует хранение 50 лучших игроков
    /// Работает с классом BestPlayer
    /// </summary>
    public class BestPlayers {

        #region Fields

        private string workDirectory;

        private List<BestPlayer> bestPlayers;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();

        private double minKD = -1.0;

        #endregion

        #region Constructor

        public BestPlayers(string directory, bool deletePrev = false) {
            bestPlayers = new List<BestPlayer> ();
            workDirectory = directory + "\\best-players";
            if(deletePrev && Directory.Exists (workDirectory)) {
                Directory.Delete (workDirectory, true);
            } else if(!deletePrev && Directory.Exists (workDirectory)) {
                LoadBestPlayers ();
            }
            Directory.CreateDirectory (workDirectory);
        }

        #endregion

        #region Load

        private string LoadPlayerFromFile(string adress) {
            string playerInfo;
            try {
                using(var file = new StreamReader (adress)) {
                    playerInfo = file.ReadToEnd ();
                }
            } catch(FileNotFoundException) {
                throw (new RequestException ("Match wasn't found"));
            }
            return playerInfo;
        }

        private void LoadBestPlayers() {
            lock(bestPlayers) {
                foreach(var file in Directory.GetFiles (workDirectory)) {
                    bestPlayers.Add (
                        JsonConvert.DeserializeObject<BestPlayer> (LoadPlayerFromFile (file))
                    );
                }
                bestPlayers = bestPlayers.OrderByDescending (x => x.KillToDeathRatio).ToList ();
            }
        }

        #endregion

        #region Add

        private void WriteInFile(BestPlayer player) {
            var adress = string.Format (workDirectory + "\\{0}.json",
                        player.Name);
            using(var file = new StreamWriter (adress, false)) {
                file.Write (
                    JsonConvert.SerializeObject (player)
                );
            }
        }

        private void DeleteFromRecent(BestPlayer player) {
            var oldAdress = string.Format (workDirectory + "\\{0}.json",
                        player.Name);
            File.Delete (oldAdress);
        }

        public void Add(Player player) {
            if(player.KD < minKD || player.TotalMatches < 10 || player.TotalDeaths == 0)
                return;
            var bestPlayer = player.FormatAsBestPlayer ();
            lock(bestPlayers) {
                bestPlayers.RemoveAll (x => x.Name == bestPlayer.Name);
                bestPlayers.Add (bestPlayer);
                bestPlayers = bestPlayers
                    .OrderByDescending (x => x.KillToDeathRatio)
                    .ToList ();
                for(int i = 50; i < bestPlayers.Count; i++)
                    DeleteFromRecent (bestPlayers[i]);
                bestPlayers = bestPlayers.Take (50).ToList ();
            }
            WriteInFile (bestPlayer);
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
            lock(bestPlayers) {
                s = JsonConvert.SerializeObject (bestPlayers.Take (count));
            }
            return s;
        }

        #endregion
    }
}
