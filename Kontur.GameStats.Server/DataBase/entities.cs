using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Kontur.GameStats.Server.DataBase {

    #region Server

    public class Server {
        public ObjectId ServerID { get; set; }
        [BsonIndex (true)]
        public string EndPoint { get; set; }
        public string Name { get; set; }
        public string[] GameModes { get; set; }

        public DateTime FirstMatchPlayed { get; set; }
        public int TotalMatches { get; set; } = 0;
        public int TotalPopulation { get; set; } = 0;

        public int MaxPlaysPerDay { get; set; } = 0;
        public int MaxPopulation { get; set; } = 0;

        public Dictionary<string, int> GameModesPlays { get; set; } = new Dictionary<string, int> ();
        public Dictionary<string, int> MapsPlays { get; set; } = new Dictionary<string, int> ();
        public Dictionary<DateTime, int> DaysPlays { get; set; } = new Dictionary<DateTime, int> ();
    }

    #endregion

    #region Player

    [Serializable]
    public class Player {
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
    }

    #endregion

    #region Match

    [Serializable]
    public class ScoreBoard {
        [JsonProperty ("name")]
        public string Name { get; set; }
        [JsonProperty ("frags")]
        public int Frags { get; set; }
        [JsonProperty ("kills")]
        public int Kills { get; set; }
        [JsonProperty ("deaths")]
        public int Deaths { get; set; }
    }

    [Serializable]
    public class Match {
        [JsonProperty ("map")]
        public string Map { get; set; }
        [JsonProperty ("gameMode")]
        public string GameMode { get; set; }
        [JsonProperty ("fragLimit")]
        public int FragLimit { get; set; }
        [JsonProperty ("timeLimit")]
        public int TimeLimit { get; set; }
        [JsonProperty ("timeElapsed")]
        public double TimeElapsed { get; set; }
        [JsonProperty ("scoreboard")]
        public ScoreBoard[] ScoreBoard { get; set; }
    }

    [Serializable]
    public class MatchInfo {
        [JsonProperty ("server")]
        public string Server { get; set; }
        [JsonProperty ("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonProperty ("matchResult")]
        public Match MatchResult { get; set; }
    }
    
    #endregion

}
