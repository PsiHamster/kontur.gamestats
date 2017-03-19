using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kontur.GameStats.Server.DataBase {

    /// <summary>
    /// Класс реализующий хранение матчей в папке matches
    /// Делит матчи по endpoint серверов, которые нужно
    /// передавать в AddServer()
    /// </summary>
    class MatchesBase {

        private string workDirectory;

        public RecentMatches RecentMatches { get; private set; }

        /// <summary>
        /// Инициализирует базу матчей в папке matches
        /// </summary>
        public MatchesBase(string directory, bool deletePrev = false) {
            workDirectory = directory + "\\matches";
            RecentMatches = new RecentMatches (directory, deletePrev);

            if (deletePrev && Directory.Exists (workDirectory)) {
                 Directory.Delete (workDirectory, true);
            }
            Directory.CreateDirectory (workDirectory);
        }

        
        #region FileWriteGet

        #region Put

        /// <summary>
        /// Сохраняет матч в папку servers/{endPoint}/{timeStamp}.json,
        /// заменяя в таймштампе : на D
        /// </summary>
        private string PutMatch (string endPoint, string timeStamp, string matchInfo) {
            var matchAdress = string.Format (workDirectory + "\\{0}\\{1}.json",
                        endPoint, timeStamp.Replace (":", "D"));
            try {
                using(var file = new StreamWriter (matchAdress, false)) {
                    file.Write (matchInfo);
                }
            } catch (IOException) {
                throw (new RequestException ("Match already added"));
            }
            return matchAdress;
        }

        /// <summary>
        /// Сохраняет матч в папку servers/{endPoint}/{timeStamp}.json,
        /// заменяя в таймштампе : на D
        /// </summary>
        public void PutMatch (string endPoint, string timeStamp, MatchInfo match) {
            string adress = PutMatch (endPoint, timeStamp, match.GetJSON ());
            RecentMatches.Add (match, adress);
        }

        #endregion

        #region Get

        private string LoadMatchFromFile (string adress) {
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
        /// Получить json результаты матча из базы данных по endPoint и timeStamp
        /// </summary>
        public string GetMatchInfoJSON (string endPoint, string timeStamp) {
            string matchInfo;
            matchInfo = LoadMatchFromFile(
                string.Format (workDirectory + "\\{0}\\{1}.json",
                            endPoint, timeStamp.Replace (":", "D")));
            return matchInfo;
        }

        /// <summary>
        /// Получить матч из базы данных по endPoint и timeStamp
        /// </summary>
        public MatchInfo GetMatchInfo (string endPoint, string timeStamp) {
            return JsonConvert.DeserializeObject<MatchInfo>(GetMatchInfoJSON (endPoint, timeStamp));
        }

        /// <summary>
        /// Получить результаты матча в том формате, в котором они пришли на сервер
        /// </summary>
        public string GetMatchResultsJSON(string endPoint, string timeStamp) {
            return JsonConvert.SerializeObject (GetMatchInfo (endPoint, timeStamp).MatchResult);
        }

        #endregion

        #endregion

        /// <summary>
        /// Добавляет серверу хранилище матчей.
        /// </summary>
        public void AddServer(string endPoint) {
            Directory.CreateDirectory (string.Format (workDirectory+"\\{0}", endPoint));
        }
    }
}
