using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Kontur.GameStats.Server.DataBase {
    
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
}
