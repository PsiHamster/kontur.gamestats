using Newtonsoft.Json;
using System;
using System.IO;

namespace Kontur.GameStats.Server.DataBase {

    /// <summary>
    /// Класс реализующий хранение матчей в папке matches
    /// Делит матчи по endpoint серверов, которые нужно
    /// передавать в AddServer()
    /// </summary>
    class MatchesBase {

        private string workDirectory;

        /// <summary>
        /// Инициализирует базу матчей в папке matches
        /// </summary>
        public MatchesBase(string directory, bool deletePrev = false) {
            workDirectory = directory + "\\matches";
            if (deletePrev && Directory.Exists (workDirectory)) {
                 Directory.Delete (workDirectory, true);
            }
            Directory.CreateDirectory ("matches");
        }

        #region FileWriteGet

        #region Put

        /// <summary>
        /// Сохраняет матч в папку servers/{endPoint}/{timeStamp}.json,
        /// заменяя в таймштампе : на D
        /// </summary>
        public void PutMatch (string endPoint, string timeStamp, string matchInfo) {
            try {
                using(var file = new StreamWriter (
                    string.Format (workDirectory+"\\{0}\\{1}.json",
                        endPoint, timeStamp.Replace (":", "D")), false)) {
                    file.Write (matchInfo);
                }
            } catch (IOException e) {
                throw (new RequestException ("Match already added"));
            }
        }

        /// <summary>
        /// Сохраняет матч в папку servers/{endPoint}/{timeStamp}.json,
        /// заменяя в таймштампе : на D
        /// </summary>
        public void PutMatch (string endPoint, string timeStamp, MatchInfo match) {
            PutMatch (endPoint, timeStamp, match.GetJSON ());
        }

        #endregion

        #region Get

        /// <summary>
        /// Получить json результаты матча из базы данных по endPoint и timeStamp
        /// </summary>
        public string GetMatchJSON (string endPoint, string timeStamp) {
            string matchInfo;
            try {
                using(var file = new StreamReader (
                        string.Format (workDirectory+"\\{0}\\{1}.json",
                            endPoint, timeStamp.Replace (":", "D")))) {
                    matchInfo = file.ReadToEnd ();
                }
            } catch(FileNotFoundException e) {
                throw (new RequestException ("Match wasn't found"));
            }
            return matchInfo;
        }

        /// <summary>
        /// Получить матч из базы данных по endPoint и timeStamp
        /// </summary>
        public MatchInfo GetMatchInfo (string endPoint, string timeStamp) {
            return JsonConvert.DeserializeObject<MatchInfo>(GetMatchJSON (endPoint, timeStamp));
        }

        #endregion

        #endregion

        /// <summary>
        /// Добавляет серверу хранилище матчей.
        /// </summary>
        public void AddServer(string endPoint) {
            Directory.CreateDirectory (string.Format (workDirectory+"\\matches\\{0}", endPoint));
        }
    }
}
