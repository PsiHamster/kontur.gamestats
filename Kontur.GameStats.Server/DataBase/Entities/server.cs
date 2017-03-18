using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.GameStats.Server.DataBase {
    /// <summary>
    /// Базовый класс сервера, хранящий всю информацию и статистику
    /// одного сервера.
    /// </summary>
    [Serializable]
    public class Server {

        #region Fields

        public string EndPoint { get; set; }
        public string Name { get; set; }
        public string[] GameModes { get; set; }

        public DateTime FirstMatchPlayed { get; set; }
        public int TotalMatches { get; set; } = 0;
        public int TotalPopulation { get; set; } = 0;

        public int MaxPlaysPerDay { get; set; } = 0;
        public int MaxPopulation { get; set; } = 0;

        public Dictionary<string, int> GameModesPlays { get; set; } = new Dictionary<string, int> ();
        public Dictionary<string, int> MapsPlays { get; set; } = new Dictionary<string, int> ();
        public Dictionary<DateTime, int> DaysPlays { get; set; } = new Dictionary<DateTime, int> ();

        #endregion

        #region Getters

        #region GetSubClasses

        /// <summary>
        /// Получить информацию о сервере, присланную advertise запросом
        /// </summary>
        public ServerInfo GetServerInfo() {
            return new ServerInfo () {
                Name = Name,
                GameModes = GameModes
            };
        }

        /// <summary>
        /// Получить информацию о сервере с EndPoint
        /// </summary>
        public ServerInfoEndpoint GetServerInfoEndpoint() {
            return new ServerInfoEndpoint () {
                EndPoint = EndPoint,
                Info = GetServerInfo ()
            };
        }

        #endregion

        #region GetJson

        /// <summary>
        /// Получить информацию о сервере, присланную advertise запросом, в json формате
        /// </summary>
        public string GetJSONserverInfo() {
            return JsonConvert.SerializeObject (GetServerInfo ());
        }

        /// <summary>
        /// Получить информацию о сервере с EndPoint в json формате
        /// </summary>
        public string GetJSONserverInfoEndpoint() {
            return JsonConvert.SerializeObject (GetServerInfoEndpoint ());
        }

        /// <summary>
        /// Возвращает строку содержащую статистику сервера в JSON
        /// </summary>
        /// <param name="lastMatchDate">Дата последнего сыгранного матча</param>
        /// <returns></returns>
        public string GetJSONserverStatistics(DateTime lastMatchDate) {
            return JsonConvert.SerializeObject (new {
                totalMatchesPlayed = TotalMatches,
                maximumMatchesPerDay = MaxPlaysPerDay,
                averageMatchesPerDay = TotalMatches / (double)(lastMatchDate.Subtract (FirstMatchPlayed.Date).TotalDays + 1),
                maximumPopulation = MaxPopulation,
                averagePopulation = TotalPopulation / (double)TotalMatches,
                top5GameModes = GameModesPlays.OrderByDescending (x => x.Value).Take (5).Select (x => x.Key),
                top5Maps = MapsPlays.OrderByDescending (x => x.Value).Take (5).Select (x => x.Key)
            });
        }

        #endregion

        #endregion

        #region Updater

        public void Update(MatchInfo match) {
            if(TotalMatches == 0 || FirstMatchPlayed > match.Timestamp) {
                FirstMatchPlayed = match.Timestamp;
            }
            GameModesPlays.IncDict (match.MatchResult.GameMode);
            MapsPlays.IncDict (match.MatchResult.Map);
            DaysPlays.IncDict (match.Timestamp.Date);

            MaxPlaysPerDay = Math.Max (
                MaxPlaysPerDay,
                DaysPlays[match.Timestamp.Date]
            );
            MaxPopulation = Math.Max (
                MaxPopulation,
                match.MatchResult.ScoreBoard.Length);
            TotalPopulation += match.MatchResult.ScoreBoard.Length;
            TotalMatches += 1;
        }

        #endregion

    }

    public class ServerInfo {
        [JsonProperty (PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty (PropertyName = "gameModes")]
        public string[] GameModes { get; set; }
    }

    public class ServerInfoEndpoint {
        [JsonProperty (PropertyName = "endpoint")]
        public string EndPoint { get; set; }
        [JsonProperty (PropertyName = "info")]
        public ServerInfo Info { get; set; }
    }
}
