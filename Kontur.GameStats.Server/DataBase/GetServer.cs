using LiteDB;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Kontur.GameStats.Server.DataBase {
    public partial class DataBase {

        #region ServerInfo

        public string GetServerInfo(string endPoint) {
            Server server;
            using(var db = new LiteDatabase (dbConn)) {
                var col = db.GetCollection<Server> ("servers");
                server = col.FindOne (x => x.EndPoint == endPoint);
            }
            if(server == null) {
                throw new RequestException ("Server not found");
            }
            return server.GetJSONserverInfo ();
        }

        #endregion

        #region ServersInfo

        public string GetServersInfo() {
            string s;
            using(var db = new LiteDatabase (dbConn)) {
                var col = db.GetCollection<Server> ("servers");
                s = JsonConvert.SerializeObject (col.FindAll ()
                    .Select (
                        server => server.GetServerInfoEndpoint ()
                    ));
            }
            return s;
        }

        #endregion

        #region ServerStatistics

        public string GetServerStatistics(string endpoint) {
            Server server;
            using(var db = new LiteDatabase (dbConn)) {
                var col = db.GetCollection<Server> ("servers");
                server = col.FindOne (x => x.EndPoint == endpoint);
            }
            if(server == null) {
                throw new RequestException ("Server not found");
            }
            return server.GetJSONserverStatistics (LastMatchTime.Date);
        }

        #endregion

        #region PopularServers

        public string GetPopularServers(int count) {
            string s;
            count = Math.Min (Math.Max (count, 0), 50);
            using(var db = new LiteDatabase (dbConn)) {
                var col = db.GetCollection<Server> ("servers");
                var ans = col.FindAll ()
                    .Select (server =>
                         new {
                             endpoint = server.EndPoint,
                             name = server.Name,
                             averageMatchesPerDay = server.TotalMatches /
                             ((LastMatchTime.Date.Subtract (server.FirstMatchPlayed.Date)).TotalDays + 1)
                         })
                    .OrderByDescending (
                        x => x.averageMatchesPerDay)
                    .Take (count);
                s = JsonConvert.SerializeObject (ans);
            }
            return s;
        }

        #endregion

    }
}
