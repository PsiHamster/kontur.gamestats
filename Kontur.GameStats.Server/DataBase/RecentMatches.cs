using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using System.Collections.Concurrent;
using NLog;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.DataBase {
    class RecentMatches {
        private SynchronizedCollection<Match> recentMatches;
        public ConcurrentQueue<Match> newMatches= new ConcurrentQueue<Match> ();

        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();
        private BinaryFormatter formatter = new BinaryFormatter ();

        public RecentMatches() {
            LoadRecentMatches ();
            StartListen ();
        }

        #region Thread

        private Thread listenerThread;

        private void StartListen() {
            listenerThread = new Thread (ListenMatches) {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            listenerThread.Start ();
        }

        private void ListenMatches() {
            while(true) {
                try {
                    while(!newMatches.IsEmpty) {
                        Match p;
                        if(newMatches.TryDequeue (out p))
                            Add (p);
                    }
                    SaveRecentMatches ();
                    Thread.Sleep (5 * 1000); // Sleep 5 seconds
                } catch(Exception e) {
                    logger.Error (e);
                }
            }
        }

        #endregion

        #region File

        private void LoadRecentMatches() {
            try {
                using(var file = new FileStream ("recentMatches.dat", System.IO.FileMode.Open, FileAccess.Read)) {
                    recentMatches = (SynchronizedCollection<Match>)formatter.Deserialize (file);
                }
            } catch(FileNotFoundException e) {
                recentMatches = new SynchronizedCollection<Match> (50);
            }
        }

        private void SaveRecentMatches() {
            try {
                using(var file = new FileStream ("recentMatches.dat", System.IO.FileMode.Create, FileAccess.Write)) {
                    formatter.Serialize (file, recentMatches);
                }
            } catch(Exception e) {
                logger.Error (e);
            }
        }

        #endregion

        #region Add

        private void Add(Match match) {
            var newList = new SynchronizedCollection<Match> ();
            for (int i = 0; i < newList.Count; i++) {
                if(match.TimeStamp > newList[i].TimeStamp) {
                    newList.Insert (i, match);
                    return;
                }
            }
            if(newList.Count < 50) {
                newList.Add(match);
            }
        }

        #endregion

        #region Take

        public string Take(int count) {
            string s;
            count = Math.Min (Math.Max (count, 0), Math.Min(50, recentMatches.Count));
            var results = recentMatches.Take (count);
            s = JsonConvert.SerializeObject (results);
            return s;
        }

        #endregion
    }
}
