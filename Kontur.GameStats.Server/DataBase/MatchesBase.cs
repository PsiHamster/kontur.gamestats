using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.DataBase {
    class MatchesBase {
        public MatchesBase() {
            Directory.CreateDirectory ("matches");

        }

        /// <summary>
        /// Сохраняет матч в папку servers/{endPoint}/{timeStamp}.json,
        /// заменяя в таймштампе : на D
        /// </summary>
        public void PutMatch (string endPoint, string timeStamp, string matchInfo) {
            try {
                using(var file = new FileStream (
                    string.Format ("matches/{0}/{1}.json",
                        endPoint, timeStamp.Replace (":", "D")),
                    System.IO.FileMode.CreateNew, FileAccess.Write)) {
                    var bytes = Encoding.Unicode.GetBytes (matchInfo);
                    file.Write (bytes, 0, bytes.Length);
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
                using(var file = new FileStream (
                        string.Format ("matches/{0}/{1}.json",
                            endPoint, timeStamp.Replace (":", "D")),
                        System.IO.FileMode.Open, FileAccess.Read)) {
                    byte[] bytes = new byte[(int)file.Length];
                    file.Read (bytes, 0, (int)file.Length);
                    matchInfo = Encoding.Unicode.GetString (bytes);
                }
            } catch(FileNotFoundException e) {
                throw (new RequestException ("Match wasn't found"));
            }
            return matchInfo;
        }

        public void AddServer(string endPoint) {
            Directory.CreateDirectory (string.Format ("matches/{0}", endPoint));
        }
    }
}
