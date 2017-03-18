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
                killToDeathRatio = KD
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

    }

    [Serializable]
    public class BestPlayer {
        [JsonProperty (PropertyName = "name")]
        public string RawName;
        [JsonIgnore]
        public string Name;
        public double killToDeathRatio;
    }
}
