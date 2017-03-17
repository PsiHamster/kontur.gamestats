using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using LiteDB;
using System.IO;

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
        public void PutInfo(string endPoint, string stringInfo) {
            var info = DeserializeServerInfo (stringInfo);

            using(var db = new LiteDatabase (dbConn)) {
                var col = db.GetCollection<Server> ("servers");
                Server server;

                if((server = col.FindOne (x => x.EndPoint == endPoint)) != null) {
                    server.Name = info.name;
                    server.GameModes = info.gameModes;

                    col.Update (server);
                } else {
                    matches.AddServer (endPoint);
                    server = new Server {
                        EndPoint = endPoint,
                        Name = info.name,
                        GameModes = info.gameModes,
                    };

                    col.Insert (server);
                }
            }
        }

        #region MatchInfo

        private Match DeserializeMatchInfo(string matchInfo) {
            try {
                return JsonConvert.DeserializeObject<Match> (
                                    matchInfo,
                                    new JsonSerializerSettings {
                                        CheckAdditionalContent = true
                                    });
            } catch {
                throw new RequestException ("Invalid match data");
            }
        }

        #endregion

        #region Updaters

        /// <summary>
        /// Увеличить значение в словаре по ключу
        /// на 1/создать с значением 1, если не существует
        /// </summary>
        private void IncDictionary<T>(Dictionary<T, int> dict, T key) {
            if(dict.ContainsKey (key)) {
                dict[key] += 1;
            } else {
                dict[key] = 1;
            }
        }

        /// <summary>
        /// Обновить статистику игрока и добавить его в очередь на добавление к лучшим.
        /// </summary>
        private void UpdatePlayer( Player player, string endPoint, Match match, DateTime time,
                ScoreBoard score, int place, int totalPlayers) {
            // playersBelowCurrent / (totalPlayers - 1) * 100%​.
            double currentPer;
            if(totalPlayers > 1)
                currentPer = (totalPlayers - place) / (double)(totalPlayers - 1) * 100;
            else
                currentPer = 100.0;
            player.AverageScoreBoardPercent = (
                player.AverageScoreBoardPercent * player.TotalMatches + currentPer) /
                (player.TotalMatches + 1);
            player.TotalKills += score.Kills;
            player.TotalDeaths += score.Deaths;
            player.TotalMatches += 1;
            player.LastMatchPlayed = time;
            if(player.TotalDeaths != 0)
                player.KD = player.TotalKills / (double)player.TotalDeaths;

            IncDictionary (player.Days, time.ToUniversalTime ().Date);
            IncDictionary (player.GameModes, match.GameMode);
            IncDictionary (player.ServerPlays, endPoint);

            player.MaximumMatchesPerDay = Math.Max (
                player.MaximumMatchesPerDay,
                player.Days[time.ToUniversalTime ().Date]
                );

            if(place == 1) {
                player.TotalMatchesWon += 1;
            }

            if (player.KD > bestPlayers.minKD && player.TotalMatches >= 10 && player.TotalDeaths > 0)
                bestPlayers.toUpdatePlayers.Enqueue (
                    new BestPlayer () {
                        RawName = player.RawName,
                        Name = player.Name,
                        killToDeathRatio = player.KD
                    });
        }

        /// <summary>
        /// Вернуть обновленную информацию о всех игроках.
        /// </summary>
        private IEnumerable<Player> UpdatePlayers(MatchInfo match) {
            for(int i = 0; i < match.MatchResult.ScoreBoard.Length; i++) {
                var score = match.MatchResult.ScoreBoard[i];
                var player = players.GetPlayer(score.Name.ToLower ());
                if(player == null) {
                    player = new Player {
                        Name = score.Name.ToLower (),
                        RawName = score.Name,
                        FirstMatchPlayed = match.Timestamp
                    };
                }
                UpdatePlayer (player, match.Server, match.MatchResult, match.Timestamp,
                    score, i + 1, match.MatchResult.ScoreBoard.Length);
                yield return player;
            }
        }

        /// <summary>
        /// Обновить информацию о сервере без добавления в бд
        /// </summary>
        private void UpdateServer(Server server, MatchInfo match) {
            if(server.TotalMatches == 0 || server.FirstMatchPlayed > match.Timestamp) {
                server.FirstMatchPlayed = match.Timestamp;
            }
            IncDictionary (server.GameModesPlays, match.MatchResult.GameMode);
            IncDictionary (server.MapsPlays, match.MatchResult.Map);
            IncDictionary (server.DaysPlays, match.Timestamp.Date);

            server.MaxPlaysPerDay = Math.Max (
                server.MaxPlaysPerDay,
                server.DaysPlays[match.Timestamp.Date]
                );
            server.MaxPopulation = Math.Max (
                server.MaxPopulation,
                match.MatchResult.ScoreBoard.Length);
            server.TotalPopulation += match.MatchResult.ScoreBoard.Length;
            server.TotalMatches += 1;
        }

        #endregion

        /// <summary>
        /// Добавить информацию о матче в БД. В случае неверных данных кидает exception
        /// </summary>
        /// <param name="matchResult">Информация о матче в JSON</param>
        public void PutMatch(string endPoint, string timeStamp, string matchResult) {
            var endTime = DateTime.Parse (timeStamp).ToUniversalTime ();
            if(endTime > LastMatchTime) {
                LastMatchTime = endTime;
            }

            var matchInfo = new MatchInfo () {
                Server = endPoint,
                Timestamp = endTime,
                MatchResult = DeserializeMatchInfo (matchResult)
            };

            using(var db = new LiteDatabase (dbConn)) {
                var serversCol = db.GetCollection<Server> ("servers");

                var server = serversCol.FindOne (x => x.EndPoint == endPoint);
                if(server == null) {
                    throw new RequestException ("Server not found");
                }
                UpdateServer (server, matchInfo);
                serversCol.Update (server);
            }

            matches.PutMatch (endPoint, timeStamp, matchResult);
            recentMatches.newMatches.Enqueue (matchInfo);

            foreach(var player in UpdatePlayers (matchInfo)) {
                players.AddPlayer (player);
            }
        }
    }
}
