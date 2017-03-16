using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteDB;
using Newtonsoft.Json;
using NLog;

namespace Kontur.GameStats.Server.DataBase {

    /// <summary>
    /// Класс для обращения с базой данных.
    /// На вход принимает данные в json, возвращает тоже json
    /// Кидает RequestException при не правильных входных данных
    /// Ошибки базы данных также вызывают свои исключения.
    /// </summary>
    public class DataBase {
        private string dbName;
        public DateTime LastMatchTime = new DateTime (0).Date;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();

        #region Initializer

        /// <summary>
        /// Конструктор, возвращающий базу данных работающих с данным файлом.
        /// </summary>
        /// <param name="name">Имя файла БД</param>
        /// <param name="deletePrev">Удалить ли старый файл, или открыть его</param>
        public DataBase(string name, bool deletePrev) {
            logger.Info (string.Format("Initializing DB {0}", name));
            dbName = "Filename=" + Directory.GetCurrentDirectory () + @"\" + name +
                ";Journal=false;Timeout=0:01:00;Cache Size=500000";
            if(deletePrev && File.Exists (name)) {
                File.Delete (name);
            } else {
                using(var db = new LiteDatabase (dbName)) {
                    var col = db.GetCollection<Match> ("matches");
                    if(col.LongCount () > 0) {
                        var max = col.FindOne (Query.All ("TimeStamp", Query.Descending)).TimeStamp;
                        LastMatchTime = max;
                    }
                }
            }
            logger.Info (string.Format ("Success", name));
        }

        /// <summary>
        /// Метод открывающий базу данных с стандартным именем
        /// statisticsBase.sql
        /// </summary>
        public DataBase() : this ("statisticsBase.db", false) { }

        #endregion

        #region PutMethods

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

            using(var db = new LiteDatabase (dbName)) {
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
        private void IncDictionary<T>(Dictionary<T,int> dict, T key) {
            if (dict.ContainsKey(key)) {
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
            if (player.TotalDeaths != 0)
                player.KD = player.TotalKills / (double)player.TotalDeaths;

            IncDictionary (player.Days, time.Date);
            IncDictionary (player.GameModes, match.GameMode);
            IncDictionary (player.ServerPlays, endPoint);

            player.MaximumMatchesPerDay = Math.Max (player.MaximumMatchesPerDay, player.Days[time.Date]);

            if(place == 1) {
                player.TotalMatchesWon += 1;
            }
        }

        /// <summary>
        /// Записать информацию в бд о всех игроках, участвовавших в матче.
        /// </summary>
        private void UpdatePlayersInDB(LiteCollection<Player> playersCol, string endpoint, Match match, DateTime time) {
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
                    playersCol.Insert (player);
                } else {
                    UpdatePlayer (player, endpoint, match, time, score, i + 1, match.ScoreBoard.Length);
                    playersCol.Update (player);
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
            IncDictionary (server.DaysPlays, endTime.Date);

            server.MaxPlaysPerDay = Math.Max (server.MaxPlaysPerDay, server.DaysPlays[endTime.Date]);
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
            using(var db = new LiteDatabase (dbName)) {
                var endTime = DateTime.Parse (timeStamp);
                if(endTime.Date > LastMatchTime) {
                    LastMatchTime = endTime.Date;
                }
                var matchesCol = db.GetCollection<Match> ("matches");
                var playersCol = db.GetCollection<Player> ("players");
                var serversCol = db.GetCollection<Server> ("servers");
                if(matchesCol.Exists (x => x.EndPoint == endPoint && x.TimeStamp == endTime)) {
                    throw new RequestException ("Match was already added");
                }

                Match match = DeserializeMatchInfo (matchInfo);
                match.EndPoint = endPoint;
                match.TimeStamp = endTime;

                Server server = serversCol.FindOne (x => x.EndPoint == endPoint);

                if(server == null) {
                    throw new RequestException ("Server not found");
                }

                using(var trans = db.BeginTrans ()) {
                    matchesCol.Insert (match);
                    UpdatePlayersInDB (playersCol, endPoint, match, endTime);
                    UpdateServer (server, match, endTime);
                    serversCol.Update (server);
                    trans.Commit ();
                }
            }
        }

        #endregion

        #region GetMethods

        #region ServerMethods

        public string GetServerInfo(string endPoint) {
            Server server;
            using(var db = new LiteDatabase (dbName)) {
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
            using(var db = new LiteDatabase (dbName)) {
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
            using(var db = new LiteDatabase (dbName)) {
                var col = db.GetCollection<Server> ("servers");
                server = col.FindOne (x => x.EndPoint == endpoint);
            }
            if(server == null) {
                throw new RequestException ("Server not found");
            }
            var s = JsonConvert.SerializeObject (new {
                totalMatchesPlayed = server.TotalMatches,
                maximumMatchesPerDay = server.MaxPlaysPerDay,
                averageMatchesPerDay = server.TotalMatches / (double)(LastMatchTime.Date.Subtract (server.FirstMatchPlayed.Date).TotalDays + 1),
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
            using(var db = new LiteDatabase (dbName)) {
                var col = db.GetCollection<Server> ("servers");
                var ans = col.FindAll ()
                    .Select (server =>
                         new {
                             endpoint = server.EndPoint,
                             name = server.Name,
                             averageMatchesPerDay = server.TotalMatches / ((LastMatchTime.Subtract (server.FirstMatchPlayed)).TotalDays + 1)
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
            Match match;
            using(var db = new LiteDatabase (dbName)) {
                var col = db.GetCollection<Match> ("matches");
                match = col.FindOne (x => x.EndPoint == endPoint && x.TimeStamp == DateTime.Parse (timeStamp));
            }
            if(match == null) {
                throw new RequestException ("Match not found");
            }
            var s = JsonConvert.SerializeObject (new {
                map = match.Map,
                gameMode = match.GameMode,
                fragLimit = match.FragLimit,
                timeLimit = match.TimeLimit,
                timeElapsed = match.TimeElapsed,
                scoreboard = match.ScoreBoard
            });
            return s;
        }

        public string GetRecentMatches(int count) {
            string s;
            count = Math.Min (Math.Max (count, 0), 50);
            using(var db = new LiteDatabase (dbName)) {
                var col = db.GetCollection<Match> ("matches");
                var results = col.Find (Query.All ("TimeStamp", Query.Descending), limit: count)
                    .Select (
                        match => new {
                            map = match.Map,
                            gameMode = match.GameMode,
                            fragLimit = match.FragLimit,
                            timeLimit = match.TimeLimit,
                            timeElapsed = match.TimeElapsed,
                            scoreboard = match.ScoreBoard
                        });
                s = JsonConvert.SerializeObject (results);
            }
            return s;
        }

        #endregion

        #region PlayersMethods

        public string GetPlayerStats(string playerName) {
            string name = playerName.ToLower ();
            Player player;
            using(var db = new LiteDatabase (dbName)) {
                var col = db.GetCollection<Player> ("players");
                player = col.FindOne (x => x.Name == name);
            }
            if(player == null) {
                throw new RequestException ("Player not found");
            }
            var s = JsonConvert.SerializeObject (new {
                totalMatchesPlayed = player.TotalMatches,
                totalMatchesWon = player.TotalMatchesWon,
                favouriteServer = player.ServerPlays.Aggregate((a,b) => a.Value > b.Value ? a : b).Key,
                uniqueServers = player.ServerPlays.Count,
                favouriteGameMode = player.GameModes.Aggregate ((a, b) => a.Value > b.Value ? a : b).Key,
                averageScoreBoardPercernt = player.AverageScoreBoardPercent,
                maximumMatchesPerDay = player.MaximumMatchesPerDay,
                averageMatchesPerDay = player.TotalMatches / (double)(LastMatchTime.Subtract (player.FirstMatchPlayed.Date).TotalDays + 1),
                lastMatchPlayed = player.LastMatchPlayed.ToUniversalTime (),
                killToDeathRatio = player.TotalKills / (double)player.TotalDeaths
            });

            return s;
        }

        public string GetBestPlayers(int count) {
            string s;
            count = Math.Min (Math.Max (count, 0), 50);
            using(var db = new LiteDatabase (dbName)) {
                var col = db.GetCollection<Player> ("players");
                var results = col.Find (
                    Query.And (
                        Query.And (
                            Query.All ("KD", Query.Descending),
                            Query.GTE ("TotalMatches", 10)),
                        Query.Not ("TotalDeaths", 0))
                    ).Select
                    (
                        player => new {
                            name = player.RawName,
                            killToDeathRatio = player.KD
                        }
                    ).OrderByDescending(x=>x.killToDeathRatio).Take(count);
                s = JsonConvert.SerializeObject (results);
            }
            return s;
        }

        #endregion

        #endregion

    }
}
