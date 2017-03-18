using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.DataBase {
    [Serializable]
    public class Player {

        #region Fields

        /// <summary>
        /// Имя игрока с символами в нижнем регистре
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Имя игрока в том виде, в котором оно пришло последний раз
        /// </summary>
        public string RawName { get; set; }

        public int TotalMatches { get; set; } = 0;
        public int TotalMatchesWon { get; set; } = 0;

        public int TotalKills { get; set; } = 0;
        public int TotalDeaths { get; set; } = 0;
        public double KD { get; set; } = 0.0;

        public DateTime FirstMatchPlayed { get; set; }
        public DateTime LastMatchPlayed { get; set; }
        public double AverageScoreBoardPercent { get; set; } = 0;

        public int MaximumMatchesPerDay { get; set; } = 0;

        public Dictionary<string, int> ServerPlays { get; set; } = new Dictionary<string, int> ();
        public Dictionary<DateTime, int> Days { get; set; } = new Dictionary<DateTime, int> ();
        public Dictionary<string, int> GameModes { get; set; } = new Dictionary<string, int> ();

        #endregion

        #region Getters

        public BestPlayer FormatAsBestPlayer() {
            return new BestPlayer () {
                RawName = RawName,
                Name = Name,
                KillToDeathRatio = KD
            };
        }
        
        /// <summary>
        /// Получить статистику игрока в JSON формате
        /// </summary>
        /// <param name="LastMatchDate">Дата последнего сыгранного матча</param>
        public string GetJSONplayerStats(DateTime LastMatchDate) {
            return JsonConvert.SerializeObject (new {
                totalMatchesPlayed = TotalMatches,
                totalMatchesWon = TotalMatchesWon,
                favouriteServer = ServerPlays.Aggregate ((a, b) => a.Value > b.Value ? a : b).Key,
                uniqueServers = ServerPlays.Count,
                favouriteGameMode = GameModes.Aggregate ((a, b) => a.Value > b.Value ? a : b).Key,
                averageScoreBoardPercernt = AverageScoreBoardPercent,
                maximumMatchesPerDay = MaximumMatchesPerDay,
                averageMatchesPerDay = TotalMatches /
                    (LastMatchDate.Subtract (FirstMatchPlayed.Date).TotalDays + 1),
                lastMatchPlayed = LastMatchPlayed.ToUniversalTime (),
                killToDeathRatio = TotalKills / (double)TotalDeaths
            });
        }

        #endregion

        #region Updater

        public void Update(string endPoint, DateTime time, string gameMode,
                ScoreBoard score, int place, int totalPlayers) {
            // playersBelowCurrent / (totalPlayers - 1) * 100%​.
            double currentPer;
            if(totalPlayers > 1)
                currentPer = (totalPlayers - place) / (double)(totalPlayers - 1) * 100;
            else
                currentPer = 100.0;
            AverageScoreBoardPercent = (
                AverageScoreBoardPercent * TotalMatches + currentPer) /
                (TotalMatches + 1);
            TotalKills += score.Kills;
            TotalDeaths += score.Deaths;
            TotalMatches += 1;
            LastMatchPlayed = time;
            KD = TotalKills / (double)TotalDeaths;

            Days.IncDict (time.Date);
            GameModes.IncDict (gameMode);
            ServerPlays.IncDict (endPoint);

            MaximumMatchesPerDay = Math.Max (
                MaximumMatchesPerDay,
                Days[time.Date]
                );

            if(place == 1) {
                TotalMatchesWon += 1;
            }
        }

        #endregion

    }

    [Serializable]
    public class BestPlayer {
        [JsonIgnore]
        public ObjectId _id { get; set; }
        [BsonIndex (true)]
        [JsonProperty (PropertyName = "name")]
        public string RawName { get; set; }
        [JsonIgnore]
        public string Name { get; set; }
        [BsonIndex]
        [JsonProperty (PropertyName = "killToDeathRatio")]
        public double KillToDeathRatio { get; set; }
    }
}
