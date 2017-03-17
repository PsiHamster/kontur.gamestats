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
    /// <summary>
    /// Класс реализующий хранение последних 50 матчей
    /// При вызове конструктора создает поток, проверяющий матчи
    /// Все новые матчи складывать в newMatches
    /// </summary>
    class RecentMatches {
        private SynchronizedCollection<RecentMatchInfo> recentMatches;
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
                        Match match;
                        if(newMatches.TryDequeue (out match)) {
                            Add (new RecentMatchInfo () {
                                server = match.EndPoint,
                                timestamp = match.TimeStamp,
                                matchResult = new MatchResults {
                                    map = match.Map,
                                    gameMode = match.GameMode,
                                    fragLimit = match.FragLimit,
                                    timeLimit = match.TimeLimit,
                                    timeElapsed = match.TimeElapsed,
                                    scoreboard = match.ScoreBoard
                                }
                            });
                        }
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
            recentMatches = new SynchronizedCollection<RecentMatchInfo> (50);
            try {
                using(var file = new FileStream ("recentMatches.dat", System.IO.FileMode.Open, FileAccess.Read)) {
                    var array = (RecentMatchInfo[])formatter.Deserialize (file);
                    foreach (var e in array) {
                        recentMatches.Add (e);
                    }
                }
            } catch(FileNotFoundException e) {

            } catch(Exception e) {
                logger.Error (e);
            }
        }

        private void SaveRecentMatches() {
            try {
                if(recentMatches.Count == 0)
                    return;
                using(var file = new FileStream ("recentMatches.dat", System.IO.FileMode.Create, FileAccess.Write)) {
                    formatter.Serialize (file, recentMatches.ToArray ());
                }
            } catch(Exception e) {
                logger.Error (e);
            }
        }

        #endregion

        #region Add

        private void Add(RecentMatchInfo match) {
            if(recentMatches.Count > 50) {
                recentMatches.RemoveAt (50);
            }
            for (int i = 0; i < recentMatches.Count; i++) {
                if(match.timestamp > recentMatches[i].timestamp) {
                    recentMatches.Insert (i, match);
                    return;
                }
            }
            if(recentMatches.Count < 50) {
                recentMatches.Add(match);
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
