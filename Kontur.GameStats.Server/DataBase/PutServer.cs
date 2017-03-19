using Newtonsoft.Json;

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
            Server server;

            if((server = servers.GetServer (endPoint)) != null) {
                server.Name = info.Name;
                server.GameModes = info.GameModes;
            } else {
                matches.AddServer (endPoint);
                server = new Server {
                    EndPoint = endPoint,
                    Name = info.Name,
                    GameModes = info.GameModes,
                };
            }

            servers.UpsertServer (server);
        }
    }
    }
