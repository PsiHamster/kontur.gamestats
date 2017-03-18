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
        private SynchronizedCollection<BestPlayer> bestPlayers;
        public ConcurrentQueue<BestPlayer> toUpdatePlayers = new ConcurrentQueue<BestPlayer> ();
        public double minKD = -1;

        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();
        private BinaryFormatter formatter = new BinaryFormatter();

        public BestPlayers() {
            LoadBestPlayers ();
        }

        #region Thread

        private Thread listenerThread;

        public void StartListen() {
            listenerThread = new Thread (ListenUpdatedPlayers) {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            listenerThread.Start ();
        }

        private void ListenUpdatedPlayers() {
            while(true) {
                try {
                    while (!toUpdatePlayers.IsEmpty) {
                        BestPlayer p;
                        if (toUpdatePlayers.TryDequeue(out p))
                            UpdateBestPlayers (p);
                    }
                    SaveBestPlayers ();
                    Thread.Sleep (20 * 1000); // Sleep 20 seconds
                } catch(Exception e) {
                    logger.Error (e);
                }
            }
        }

        #endregion

        #region FileLogic

        private void LoadBestPlayers() {
            bestPlayers = new SynchronizedCollection<BestPlayer> (50);
            try {
                if(bestPlayers.Count == 0)
                    return;
                using(var file = new FileStream ("bestPlayers.dat", System.IO.FileMode.Open, FileAccess.Read)) {
                    var array = (BestPlayer[])formatter.Deserialize (file);
                    foreach(var e in array) {
                        bestPlayers.Add (e);
                    }
                }
            } catch (FileNotFoundException e) {
            } catch (Exception e) {
                logger.Error (e);
            }
        }

        private void SaveBestPlayers() {
            try {
                if(bestPlayers.Count == 0)
                    return;
                using(var file = new FileStream ("bestPlayers.dat", System.IO.FileMode.Create, FileAccess.Write)) {
                    formatter.Serialize (file, bestPlayers.ToArray());
                }
            } catch(Exception e) {
                logger.Error (e);
            }
        }

        #endregion

        #region Updater

        private void UpdateBestPlayers(BestPlayer player) {
            var newList = new SynchronizedCollection<BestPlayer> ();
            var count = 0;
            var inserted = false;
            foreach(var elem in bestPlayers) {
                if(count >= 50)
                    return;
                if(elem.Name == player.Name)
                    continue;
                if(player.killToDeathRatio > elem.killToDeathRatio && !inserted) {
                    newList.Add (player);
                    inserted = true;
                } else {
                    newList.Add (elem);
                }
                count+=1;
            }
            if(count < 50) {
                newList.Add (player);
                inserted = true;
            }
            if(count == 50) {
                minKD = newList.Last().killToDeathRatio;
            }
            if(inserted) {
                bestPlayers = newList;
            }
        }

        #endregion

        #region Take

        public string Take(int count) {
            string s;
            count = Math.Min (Math.Min (Math.Max (count, 0), bestPlayers.Count), 50);
            var results = bestPlayers.Take (count);
            s = JsonConvert.SerializeObject (results);
            return s;
        }

        #endregion
    }
}
