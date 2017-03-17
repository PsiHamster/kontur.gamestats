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

    [Serializable]
    public class BestPlayer {
        [JsonProperty(PropertyName = "name")]
        public string RawName;
        [JsonProperty (IsReference = false)]
        public string Name;
        public double killToDeathRatio;
    }
}
