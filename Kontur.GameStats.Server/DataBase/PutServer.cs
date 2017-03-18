using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using LiteDB;

namespace Kontur.GameStats.Server.DataBase {
    public partial class DataBase {

        #region ServerInfo

        private ServerInfo DeserializeServerInfo(string serverInfo) {
            try {
                return JsonConvert.DeserializeObject<ServerInfo> (
                                    serverInfo,
                                    new JsonSerializerSettings {
                                        MissingMemberHandling = MissingMemberHandling.Error,
                                        CheckAdditionalContent = true
                                    });
            } catch {
                throw new RequestException ("Invalid server data");
            }
        }

        #endregion

        /// <summary>
        /// Добавить информацию о сервере в БД. В случае неверных данных кидает exception
        /// </summary>
        /// <param name="stringInfo">Информация о сервере в JSON</param>
        public void PutServerInfo(string endPoint, string stringInfo) {
            var info = DeserializeServerInfo (stringInfo);

            using(var db = new LiteDatabase (dbConn)) {
                var col = db.GetCollection<Server> ("servers");
                Server server;

                if((server = col.FindOne (x => x.EndPoint == endPoint)) != null) {
                    server.Name = info.Name;
                    server.GameModes = info.GameModes;

                    col.Update (server);
                } else {
                    matches.AddServer (endPoint);
                    server = new Server {
                        EndPoint = endPoint,
                        Name = info.Name,
                        GameModes = info.GameModes,
                    };

                    col.Insert (server);
                }
            }
        }
    }
    }
