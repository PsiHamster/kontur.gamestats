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
