﻿using System;
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
            return JsonConvert.SerializeObject (new { name = server.Name, gameModes = server.GameModes });
        }

        public string GetServersInfo() {
            string s;
            using(var db = new LiteDatabase (dbConn)) {
                var col = db.GetCollection<Server> ("servers");

                s = JsonConvert.SerializeObject (col.FindAll ()
                    .Select (server => new ServerInfoEndpoint {
                        endpoint = server.EndPoint,
                        info = new ServerInfo {
                            name = server.Name,
                            gameModes = server.GameModes
                        }
                    }));
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
            var s = JsonConvert.SerializeObject (new {
                totalMatchesPlayed = server.TotalMatches,
                maximumMatchesPerDay = server.MaxPlaysPerDay,
                averageMatchesPerDay = server.TotalMatches / (double)(LastMatchTime.ToUniversalTime ().Date.Subtract (server.FirstMatchPlayed.ToUniversalTime ().Date).TotalDays + 1),
                maximumPopulation = server.MaxPopulation,
                averagePopulation = server.TotalPopulation / (double)server.TotalMatches,
                top5GameModes = server.GameModesPlays.OrderByDescending (x => x.Value).Take (5).Select (x => x.Key),
                top5Maps = server.MapsPlays.OrderByDescending (x => x.Value).Take (5).Select (x => x.Key)
            });

            return s;
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
                             averageMatchesPerDay = server.TotalMatches / ((LastMatchTime.ToUniversalTime ().Date.Subtract (server.FirstMatchPlayed.ToUniversalTime ().Date)).TotalDays + 1)
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
            var match = matches.GetMatch (endPoint, timeStamp);
            var s = JsonConvert.SerializeObject (match);
            return s;
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
            var s = JsonConvert.SerializeObject (new {
                totalMatchesPlayed = player.TotalMatches,
                totalMatchesWon = player.TotalMatchesWon,
                favouriteServer = player.ServerPlays.Aggregate ((a, b) => a.Value > b.Value ? a : b).Key,
                uniqueServers = player.ServerPlays.Count,
                favouriteGameMode = player.GameModes.Aggregate ((a, b) => a.Value > b.Value ? a : b).Key,
                averageScoreBoardPercernt = player.AverageScoreBoardPercent,
                maximumMatchesPerDay = player.MaximumMatchesPerDay,
                averageMatchesPerDay = player.TotalMatches / 
                    (LastMatchTime.ToUniversalTime ().Date.Subtract (
                        player.FirstMatchPlayed.ToUniversalTime ().Date).TotalDays + 1),
                lastMatchPlayed = player.LastMatchPlayed.ToUniversalTime (),
                killToDeathRatio = player.TotalKills / (double)player.TotalDeaths
            });

            return s;
        }

        public string GetBestPlayers(int count) {
            return bestPlayers.Take (count);
        }

        #endregion
    }
}
