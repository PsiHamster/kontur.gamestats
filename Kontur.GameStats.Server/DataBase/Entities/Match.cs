using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Kontur.GameStats.Server.DataBase {

    /// <summary>
    /// Информация о матче в формате для recent-matches,
    /// содержащая всю информацию, включая адрес сервера и
    /// таймштамп
    /// </summary>
    [Serializable]
    public class MatchInfo {

        #region fields
        
        [JsonProperty ("server"), JsonRequired()]
        public string Server { get; set; }
        [JsonProperty ("timestamp"), JsonRequired ()]
        public DateTime Timestamp { get; set; }
        [JsonProperty ("matchResult"), JsonRequired ()]
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

    /// <summary>
    /// Класс хранящий результаты матча в том формате,
    /// который приходит на
    /// servers/{endpoint}/matches/{timestamp}
    /// </summary>
    [Serializable]
    public class MatchResult {

        #region fields

        [JsonProperty ("map"), JsonRequired ()]
        public string Map { get; set; }
        [JsonProperty ("gameMode"), JsonRequired ()]
        public string GameMode { get; set; }
        [JsonProperty ("fragLimit"), JsonRequired ()]
        public int FragLimit { get; set; }
        [JsonProperty ("timeLimit"), JsonRequired ()]
        public int TimeLimit { get; set; }
        [JsonProperty ("timeElapsed"), JsonRequired ()]
        public double TimeElapsed { get; set; }
        [JsonProperty ("scoreboard"), JsonRequired ()]
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
        [JsonProperty ("name"), JsonRequired ()]
        public string Name { get; set; }
        [JsonProperty ("frags"), JsonRequired ()]
        public int Frags { get; set; }
        [JsonProperty ("kills"), JsonRequired ()]
        public int Kills { get; set; }
        [JsonProperty ("deaths"), JsonRequired ()]
        public int Deaths { get; set; }
    }
}
