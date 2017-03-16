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
        public void PutInfo(string endPoint, string stringInfo) {
            var info = DeserializeServerInfo (stringInfo);

            using(var db = new LiteDatabase (statsDBConn)) {
                var col = db.GetCollection<Server> ("servers");
                Server server;

                if((server = col.FindOne (x => x.EndPoint == endPoint)) != null) {
                    server.Name = info.name;
                    server.GameModes = info.gameModes;

                    col.Update (server);
                } else {
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
        /// Обновить статистику игрока
        /// </summary>
        private void UpdatePlayer(Player player, string endPoint, Match match, DateTime time, ScoreBoard score, int place, int totalPlayers) {
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

            player.MaximumMatchesPerDay = Math.Max (player.MaximumMatchesPerDay, player.Days[time.ToUniversalTime ().Date]);

            if(place == 1) {
                player.TotalMatchesWon += 1;
            }

            toUpdatePlayers.Enqueue (player);
        }

        /// <summary>
        /// Вернуть обновленную информацию о всех игроках.
        /// </summary>
        private IEnumerable<Player> UpdatePlayers(LiteCollection<Player> playersCol, string endpoint, Match match, DateTime time) {
            for(int i = 0; i < match.ScoreBoard.Length; i++) {
                var score = match.ScoreBoard[i];
                var player = playersCol.FindOne (x => x.Name == score.Name.ToLower ());
                if(player == null) {
                    player = new Player {
                        Name = score.Name.ToLower (),
                        RawName = score.Name,
                        FirstMatchPlayed = time
                    };
                    UpdatePlayer (player, endpoint, match, time, score, i + 1, match.ScoreBoard.Length);
                    yield return player;
                } else {
                    UpdatePlayer (player, endpoint, match, time, score, i + 1, match.ScoreBoard.Length);
                    yield return player;
                }
            }
        }

        /// <summary>
        /// Обновить информацию о сервере без добавления в бд
        /// </summary>
        private void UpdateServer(Server server, Match match, DateTime endTime) {
            if(server.TotalMatches == 0 || server.FirstMatchPlayed > endTime) {
                server.FirstMatchPlayed = endTime;
            }
            IncDictionary (server.GameModesPlays, match.GameMode);
            IncDictionary (server.MapsPlays, match.Map);
            IncDictionary (server.DaysPlays, endTime.ToUniversalTime ().Date);

            server.MaxPlaysPerDay = Math.Max (server.MaxPlaysPerDay, server.DaysPlays[endTime.ToUniversalTime ().Date]);
            server.MaxPopulation = Math.Max (server.MaxPopulation, match.ScoreBoard.Length);
            server.TotalPopulation += match.ScoreBoard.Length;
            server.TotalMatches += 1;
        }

        #endregion

        /// <summary>
        /// Добавить информацию о матче в БД. В случае неверных данных кидает exception
        /// </summary>
        /// <param name="matchInfo">Информация о матче в JSON</param>
        public void PutMatch(string endPoint, string timeStamp, string matchInfo) {
            var endTime = DateTime.Parse (timeStamp);
            if(endTime > LastMatchTime) {
                LastMatchTime = endTime;
            }

            Match match = DeserializeMatchInfo (matchInfo);
            match.EndPoint = endPoint;
            match.TimeStamp = endTime;

            using(var db = new LiteDatabase (statsDBConn)) {
                var playersCol = db.GetCollection<Player> ("players");
                var serversCol = db.GetCollection<Server> ("servers");
                var matchesCol = db.GetCollection<Match> ("matches");

                Server server = serversCol.FindOne (x => x.EndPoint == endPoint);

                if(server == null) {
                    throw new RequestException ("Server not found");
                }

                using(var trans = db.BeginTrans ()) {
                    matchesCol.Insert (match);
                    foreach (var player in UpdatePlayers (playersCol, endPoint, match, endTime)) {
                        playersCol.Upsert (player);
                    }
                    UpdateServer (server, match, endTime);
                    serversCol.Update (server);
                    trans.Commit ();
                }
            }
        }
    }
}
