using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using LiteDB;
using System.IO;

namespace Kontur.GameStats.Server.DataBase {
    public partial class DataBase {
        
        #region ServerMethods

        public string GetServerInfo(string endPoint) {
            Server server;
            using(var db = new LiteDatabase (dbConn)) {
                var col = db.GetCollection<Server> ("servers");
                server = col.FindOne (x => x.EndPoint == endPoint);
            }
            if(server == null) {
                throw new RequestException ("Server not found");
            }
            return server.GetJSONserverInfo();
        }

        public string GetServersInfo() {
            string s;
            using(var db = new LiteDatabase (dbConn)) {
                var col = db.GetCollection<Server> ("servers");
                s = JsonConvert.SerializeObject (col.FindAll ()
                    .Select (
                        server => server.GetServerInfoEndpoint()
                    ));
            }
            return s;
        }

        public string GetServerStatistics(string endpoint) {
            Server server;
            using(var db = new LiteDatabase (dbConn)) {
                var col = db.GetCollection<Server> ("servers");
                server = col.FindOne (x => x.EndPoint == endpoint);
            }
            if(server == null) {
                throw new RequestException ("Server not found");
            }
            return server.GetJSONserverStatistics(LastMatchTime.Date);
        }

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
                             ((LastMatchTime.Date.Subtract(server.FirstMatchPlayed.Date)).TotalDays + 1)
                         })
                    .OrderByDescending (
                        x => x.averageMatchesPerDay)
                    .Take (count);
                s = JsonConvert.SerializeObject (ans);
            }
            return s;
        }

        #endregion

        #region MathesMethods

        public string GetMatchInfo(string endPoint, string timeStamp) {
            return matches.GetMatchJSON (endPoint, timeStamp);
        }

        public string GetRecentMatches(int count) {
            return recentMatches.Take (count);
        }

        #endregion

        #region PlayersMethods

        public string GetPlayerStats(string playerName) {
            string name = playerName.ToLower ();
            var player = players.GetPlayer (name);

            if(player == null) {
                throw new RequestException ("Player not found");
            }

            return player.GetJSONplayerStats (LastMatchTime.Date);
        }

        public string GetBestPlayers(int count) {
            return bestPlayers.Take (count);
        }

        #endregion
    }
}
