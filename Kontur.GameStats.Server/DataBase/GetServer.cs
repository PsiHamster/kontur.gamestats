using Newtonsoft.Json;
using System;
using System.Linq;

namespace Kontur.GameStats.Server.DataBase {
    public partial class DataBase {

        #region ServerInfo

        public string GetServerInfo(string endPoint) {
            Server server = servers.GetServer (endPoint);
            if(server == null) {
                throw new RequestException ("Server not found");
            }
            return server.GetJSONserverInfo ();
        }

        #endregion

        #region ServersInfo

        public string GetServersInfo() {
            string s = JsonConvert.SerializeObject (
                servers.GetServersInfo());
            return s;
        }

        #endregion

        #region ServerStatistics

        public string GetServerStatistics(string endpoint) {
            Server server = servers.GetServer(endpoint);
            if(server == null) {
                throw new RequestException ("Server not found");
            }
            return server.GetJSONserverStatistics (LastMatchTime.Date);
        }

        #endregion

        #region PopularServers

        public string GetPopularServers(int count) {
            return servers.GetPopularServers (LastMatchTime, count);
        }

        #endregion

    }
}
