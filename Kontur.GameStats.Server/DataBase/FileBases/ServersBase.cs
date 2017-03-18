using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kontur.GameStats.Server.DataBase {
    /// <summary>
    /// Класс, хранящий все сервера в оперативной памяти и
    /// сохраняющий их на диск.
    /// </summary>
    public class ServersBase {
        public string workDirectory { get; private set; }
        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();
        private Dictionary<string, Server> servers;
        
        #region Constructor

        /// <param name="directory">Путь к папке базы данных</param>
        /// <param name="deletePrev">Удалять ли старые данные</param>
        public ServersBase(string directory, bool deletePrev = false) {
            workDirectory = directory + "\\servers";
            if(deletePrev && Directory.Exists (workDirectory)) {
                Directory.Delete (workDirectory, true);
            }
            Directory.CreateDirectory (workDirectory);
            if(!deletePrev) {
                LoadServers ();
            }
            servers = new Dictionary<string, Server> ();
        }

        #endregion

        #region Methods

        private BinaryFormatter formatter = new BinaryFormatter ();

        /// <summary>
        /// Добавляет сервер в базу данных.
        /// </summary>
        public void UpsertServer(Server server) {
            string name = HttpUtility.UrlEncode (server.EndPoint);
            
            string filePath = string.Format ("{0}\\{1}.dat", workDirectory, name);
            using(var file = new FileStream (filePath, System.IO.FileMode.Create, FileAccess.Write)) {
                formatter.Serialize (file, server);
            }
            lock(servers) {
                servers[server.EndPoint] = server;
            }
        }

        /// <summary>
        /// Возвращает сервер из базы данных.
        /// </summary>
        public Server GetServer(string endPoint) {
            if (servers.ContainsKey(endPoint)) {
                return servers[endPoint];
            } else {
                return null;
            }
        }

        /// <summary>
        /// Возвращает массив информации о серверах
        /// </summary>
        public ServerInfoEndpoint[] GetServersInfo() {
            var answer = new List<ServerInfoEndpoint> ();
            lock(servers) {
                foreach(var server in servers.Values) {
                    answer.Add (server.GetServerInfoEndpoint ());
                }
            }
            return answer.ToArray ();
        }

        /// <summary>
        /// Возвращает популярные сервера в JSON формате
        /// </summary>
        public string GetPopularServers(DateTime lastMatchTime, int count) {
            string s;
            count = Math.Min (Math.Max (count, 0), Math.Min(servers.Count, 50));
            lock (servers) {
                var ans = servers.Values.Select (server =>
                         new {
                             endpoint = server.EndPoint,
                             name = server.Name,
                             averageMatchesPerDay = server.TotalMatches /
                             ((lastMatchTime.Date.Subtract (server.FirstMatchPlayed.Date)).TotalDays + 1)
                         })
                    .OrderByDescending (
                        x => x.averageMatchesPerDay)
                    .Take (count);
                s = JsonConvert.SerializeObject (ans);
            }
            return s;
        }

        #region FileLoad

        /// <summary>
        /// Загрузка сервера из файла
        /// </summary>
        public void LoadServer(string fileName) {
            Server server;

            try {
                using(var file = new FileStream (fileName, System.IO.FileMode.Open, FileAccess.Read)) {
                    server = (Server)formatter.Deserialize (file);
                }
            } catch (Exception e) {
                logger.Error (e.Message);
                logger.Error ("Deleting player data");
                File.Delete (fileName);
                return;
            }

            servers[server.EndPoint] = server;
        }
        
        /// <summary>
        /// Загружает все сервера из файлов в оперативную память.
        /// </summary>
        private void LoadServers() {
            foreach(var file in Directory.EnumerateFiles (workDirectory)) {
                LoadServer (workDirectory + "\\" + file);
            }
        }

        #endregion

        #endregion
    }
}
