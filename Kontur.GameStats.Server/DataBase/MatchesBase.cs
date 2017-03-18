using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.DataBase {

    /// <summary>
    /// Класс реализующий хранение матчей в папке matches
    /// Делит матчи по endpoint серверов, которые нужно
    /// передавать в AddServer()
    /// </summary>
    class MatchesBase {
        public MatchesBase() {
            Directory.CreateDirectory ("matches");
        }

        #region FileWriteGet

        /// <summary>
        /// Сохраняет матч в папку servers/{endPoint}/{timeStamp}.json,
        /// заменяя в таймштампе : на D
        /// </summary>
        public void PutMatch (string endPoint, string timeStamp, string matchInfo) {
            try {
                using(var file = new StreamWriter (
                    string.Format ("matches/{0}/{1}.json",
                        endPoint, timeStamp.Replace (":", "D")), false)) {
                    file.Write (matchInfo);
                }
            } catch (IOException e) {
                throw (new RequestException ("Match already added"));
            }
        }

        /// <summary>
        /// Получить матч из базы данных по endPoint и timeStamp
        /// </summary>
        public string GetMatch (string endPoint, string timeStamp) {
            string matchInfo;
            try {
                using(var file = new StreamReader (
                        string.Format ("matches/{0}/{1}.json",
                            endPoint, timeStamp.Replace (":", "D")))) {
                    matchInfo = file.ReadToEnd ();
                }
            } catch(FileNotFoundException e) {
                throw (new RequestException ("Match wasn't found"));
            }
            return matchInfo;
        }

        #endregion

        /// <summary>
        /// Добавляет серверу хранилище матчей.
        /// </summary>
        public void AddServer(string endPoint) {
            Directory.CreateDirectory (string.Format ("matches/{0}", endPoint));
        }
    }
}
