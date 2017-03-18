namespace Kontur.GameStats.Server.ApiMethods {
    public partial class Router {
        private void PutServerInfo(string endpoint, string serverInfo) {
            dataBase.PutServerInfo (endpoint, serverInfo);
        }

        private void PutMatchInfo(string endpoint, string timeStamp, string matchInfo) {
            dataBase.PutMatch (endpoint, timeStamp, matchInfo);
        }
    }
}
