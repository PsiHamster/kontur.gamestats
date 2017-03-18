using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using LiteDB;
using System.IO;

namespace Kontur.GameStats.Server.DataBase {
    public partial class DataBase {

        #region MatchInfo

        public string GetMatchInfo(string endPoint, string timeStamp) {
            return matches.GetMatchJSON (endPoint, timeStamp);
        }

        #endregion

        #region RecentMatches

        public string GetRecentMatches(int count) {
            return recentMatches.Take (count);
        }

        #endregion

    }
}
