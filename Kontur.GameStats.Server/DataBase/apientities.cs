using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.DataBase {
    public class ServerInfo {
        public string name;
        public string[] gameModes;
    }

    public class ServerInfoEndpoint {
        public string endpoint;
        public ServerInfo info;
    }

    public class MatchResults {
        public string map;
        public string gameMode;
        public int fragLimit;
        public int timeLimit;
        public double timeElapsed;
        public ScoreBoard[] scoreboard;
    }

    public class RecentMatchInfo {
        public string server;
        public DateTime timestamp;
        public MatchResults matchResult;
    }

    [Serializable]
    public class BestPlayer {
        [JsonProperty(PropertyName = "name")]
        public string RawName;
        [JsonProperty (IsReference = false)]
        public string Name;
        public double killToDeathRatio;
    }
}
