using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using LiteDB;
using System.IO;

namespace Kontur.GameStats.Server.DataBase {
    public partial class DataBase {

        #region MatchInfo

        private MatchResult DeserializeMatchInfo(string matchInfo) {
            try {
                return JsonConvert.DeserializeObject<MatchResult> (
                                    matchInfo,
                                    new JsonSerializerSettings {
                                        CheckAdditionalContent = true
                                    });
            } catch {
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

                if (player.KD > bestPlayers.minKD && player.TotalMatches >= 10 && player.TotalDeaths > 0)
                    bestPlayers.toUpdatePlayers.Enqueue (player.FormatAsBestPlayer());

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

            using(var db = new LiteDatabase (dbConn)) {
                var serversCol = db.GetCollection<Server> ("servers");

                var server = serversCol.FindOne (x => x.EndPoint == endPoint);
                if(server == null) {
                    throw new RequestException ("Server not found");
                }
                server.Update (matchInfo);
                serversCol.Update (server);
            }

            matches.PutMatch (endPoint, timeStamp, matchResult);
            recentMatches.Add(matchInfo);

            foreach(var player in UpdatePlayers (matchInfo)) {
                players.AddPlayer (player);
            }
        }
    }
}
