using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Kontur.GameStats.Server.DataBase {

    [Serializable]
    public class MatchInfo {

        #region fields
        
        [JsonProperty ("server")]
        public string Server { get; set; }
        [BsonIndex]
        [JsonProperty ("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonProperty ("matchResult")]
        public MatchResult MatchResult { get; set; }

        #endregion

        #region methods

        /// <summary>
        /// Возвращает информацию о матче в JSON
        /// </summary>
        public string GetJSON() {
            return JsonConvert.SerializeObject (this);
        }

        #endregion
    }

    [Serializable]
    public class MatchResult {

        #region fields

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

        #endregion

        #region methods

        /// <summary>
        /// Возвращает результаты матча в JSON
        /// </summary>
        public string GetJSON() {
            return JsonConvert.SerializeObject (this);
        }

        #endregion
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
