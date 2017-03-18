namespace Kontur.GameStats.Server.ApiMethods {
    public partial class Router {

        private string GetServersInfo() {
            return dataBase.GetServersInfo ();
        }

        private string GetMatchInfo(string endpoint, string timeStamp) {
            return dataBase.GetMatchInfo (endpoint, timeStamp);
        }

        private string GetServerStats(string endpoint) {
            return dataBase.GetServerStatistics (endpoint);
        }

        private string GetServerInfo(string endPoint) {
            return dataBase.GetServerInfo (endPoint);
        }

        private string GetPlayerStats(string playerName) {
            return dataBase.GetPlayerStats (playerName);
        }

        private string GetRecentMatches(int count) {
            return dataBase.GetRecentMatches (count);
        }

        private string GetBestPlayers(int count) {
            return dataBase.GetBestPlayers (count);
        }

        private string GetPopularServers(int count) {
            return dataBase.GetPopularServers (count);
        }
    }
}
