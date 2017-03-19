using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Kontur.GameStats.Server.DataBase {

    /// <summary>
    /// Класс реализующий хранение последних 50 матчей
    /// Матчи связаны hard link'ом с оригиналами,
    /// без добавления матча в бд, матч к последним не добавить
    /// </summary>
    public class RecentMatches {

        #region Fields

        private string workDirectory;

        private List<MatchInfo> recentMatches;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();
        private readonly object Locker = new object ();

        #endregion

        #region Constructor

        public RecentMatches(string directory, bool deletePrev = false) {
            recentMatches = new List<MatchInfo> ();
            workDirectory = directory + "\\recent-matches";
            if(deletePrev && Directory.Exists (workDirectory)) {
                Directory.Delete (workDirectory, true);
            } else if(!deletePrev && Directory.Exists (workDirectory)) {
                LoadRecentMatches ();
            }
            Directory.CreateDirectory (workDirectory);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Удаляет матч из каталога с последними матчами
        /// </summary>
        /// <param name="matchInfo"></param>
        private void DeleteMatchFromRecent(MatchInfo matchInfo) {
            var oldAdress = string.Format (workDirectory + "\\{0}{1}.json",
                        matchInfo.Timestamp.ToString ("yyyy'-'MM'-'dd'T'HH'D'mm'D'ss'.'fffZ"), matchInfo.Server);
            File.Delete (oldAdress);
        }

        #endregion

        #region Add

        /// <summary>
        /// Добавляет матч в последние
        /// </summary>
        /// <param name="matchAdress">Адрес к матчу в папке</param>
        /// <param name="matchInfo">Информация о матче</param>
        public void Add(MatchInfo matchInfo, string matchAdress) {
            lock(Locker) {
                recentMatches.Add (matchInfo);
                recentMatches = recentMatches
                    .OrderByDescending (x => x.Timestamp)
                    .ToList ();
                if(recentMatches.Count > 200) {
                    for(int i = 50; i < recentMatches.Count; i++)
                        DeleteMatchFromRecent (recentMatches[i]);
                    recentMatches = recentMatches.Take (50).ToList();
                }
                var newAdress = string.Format (workDirectory + "\\{0}{1}.json",
                        matchInfo.Timestamp.ToString ("yyyy'-'MM'-'dd'T'HH'D'mm'D'ss'.'fffZ"), matchInfo.Server);
                HardLinks.CreateHardLink (newAdress, matchAdress, IntPtr.Zero);
            }
        }

        /// <summary>
        /// Возвращает время последнего сыгранного матча
        /// </summary>
        public DateTime GetLastMatchTime() {
            DateTime last;
            lock(Locker) {
                if (recentMatches.Count() > 0) {
                    last = recentMatches[0].Timestamp;
                } else {
                    last = new DateTime (0).Date;
                }
            }
            return last;
        }

        #endregion

        #region Load

        private string LoadMatchFromFile(string adress) {
            string matchInfo;
            try {
                using(var file = new StreamReader (adress)) {
                    matchInfo = file.ReadToEnd ();
                }
            } catch(FileNotFoundException) {
                throw (new RequestException ("Match wasn't found"));
            }
            return matchInfo;
        }

        /// <summary>
        /// Загрузить последние матчи из папки с последними матчами
        /// </summary>
        private void LoadRecentMatches() {
            lock(Locker) {
                foreach(var file in Directory.GetFiles (workDirectory)) {
                    recentMatches.Add (
                        JsonConvert.DeserializeObject<MatchInfo> (LoadMatchFromFile (file))
                    );
                }
                recentMatches = recentMatches.OrderByDescending (x => x.Timestamp).ToList ();
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
            lock(Locker) {
                s = JsonConvert.SerializeObject (recentMatches.Take(count));
            }
            return s;
        }

        #endregion
    }
}
