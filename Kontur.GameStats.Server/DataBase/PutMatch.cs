using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Kontur.GameStats.Server.DataBase {
    public partial class DataBase {

        #region MatchInfo

        private MatchResult DeserializeMatchInfo(string matchInfo) {
            try {
                return JsonConvert.DeserializeObject<MatchResult> (
                                    matchInfo,
                                    new JsonSerializerSettings {
                                        MissingMemberHandling = MissingMemberHandling.Error,
                                        CheckAdditionalContent = true
                                    });
            } catch (Exception) {
                throw new RequestException ("Invalid match data");
            }
        }

        #endregion

        #region Updaters
        
        /// <summary>
        /// Вернуть обновленную информацию о всех игроках.
        /// </summary>
        private IEnumerable<Player> UpdatePlayers(MatchInfo match) {
            for(int i = 0; i < match.MatchResult.ScoreBoard.Length; i++) {
                var endpoint = match.Server;
                var timeStamp = match.Timestamp;
                var matchResult = match.MatchResult;
                var score = matchResult.ScoreBoard[i];
                var player = players.GetPlayer(score.Name.ToLower ());

                if(player == null) {
                    player = new Player {
                        Name = score.Name.ToLower (),
                        RawName = score.Name,
                        FirstMatchPlayed = timeStamp
                    };
                }

                player.Update (
                    endpoint, timeStamp, matchResult.GameMode, score, i + 1,
                    matchResult.ScoreBoard.Length);

                yield return player;
            }
        }

        #endregion

        /// <summary>
        /// Добавить информацию о матче в БД. В случае неверных данных кидает exception
        /// </summary>
        /// <param name="matchResult">Информация о матче в JSON</param>
        public void PutMatch(string endPoint, string timeStamp, string matchResult) {
            var endTime = DateTime.Parse (timeStamp).ToUniversalTime ();
            if(endTime > LastMatchTime) {
                LastMatchTime = endTime;
            }

            var matchInfo = new MatchInfo () {
                Server = endPoint,
                Timestamp = endTime,
                MatchResult = DeserializeMatchInfo (matchResult)
            };


            var server = servers.GetServer (endPoint);
            if(server == null) {
                throw new RequestException ("Server not found");
            }
            server.Update (matchInfo);
            servers.UpsertServer (server);

            matches.PutMatch (endPoint, timeStamp, matchInfo);
            
            foreach(var player in UpdatePlayers (matchInfo)) {
                players.AddPlayer (player);
            }
        }
    }
}
