namespace Kontur.GameStats.Server.DataBase {
    public partial class DataBase {

        #region PlayerStats

        public string GetPlayerStats(string playerName) {
            string name = playerName.ToLower ();
            var player = players.GetPlayer (name);

            if(player == null) {
                throw new RequestException ("Player not found");
            }

            return player.GetJSONplayerStats (LastMatchTime.Date);
        }

        #endregion

        #region BestPlayers

        public string GetBestPlayers(int count) {
            return bestPlayers.Take (count);
        }

        #endregion

    }
}
