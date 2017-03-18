using Kontur.GameStats.Server.DataBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Tests.DBtests {
    [TestClass]
    public class PlayerStatsTests {
        [TestMethod]
        public void PlayerStats() {
            var db = new DataBase (true);
            var inputData = new ServerInfo { Name = "MyServer001", GameModes = new string[] { "DM" } };

            db.PutServerInfo ("server1", JsonConvert.SerializeObject (inputData));

            string matchData = JsonConvert.SerializeObject (
                new {
                    map = "DM-HelloWorld",
                    gameMode = "DM",
                    fragLimit = 20,
                    timeLimit = 20,
                    timeElapsed = 12.345678,
                    scoreboard = new object[] {
                        new { name = "Player1", frags = 20, kills = 20, deaths = 4 },
                        new { name = "Player2", frags = 2, kills = 2, deaths = 20 }
                    }
                }
            );
            db.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData);
            
            Assert.AreEqual (
                JsonConvert.SerializeObject (new {
                    totalMatchesPlayed = 1,
                    totalMatchesWon = 1,
                    favouriteServer = "server1",
                    uniqueServers = 1,
                    favouriteGameMode = "DM",
                    averageScoreBoardPercernt = 100.0,
                    maximumMatchesPerDay = 1,
                    averageMatchesPerDay = 1.0,
                    lastMatchPlayed = "2017-01-22T15:17:00Z",
                    killToDeathRatio = 5.0
                }),
                db.GetPlayerStats ("player1")
            );
            Assert.AreEqual (
                JsonConvert.SerializeObject (new {
                    totalMatchesPlayed = 1,
                    totalMatchesWon = 0,
                    favouriteServer = "server1",
                    uniqueServers = 1,
                    favouriteGameMode = "DM",
                    averageScoreBoardPercernt = 0.0,
                    maximumMatchesPerDay = 1,
                    averageMatchesPerDay = 1.0,
                    lastMatchPlayed = "2017-01-22T15:17:00Z",
                    killToDeathRatio = 0.1
                }),
                db.GetPlayerStats ("player2")
            );
        }
    
        [TestMethod]
        public void PlayerStatsWithZeroDeathsAndZeroKills() {
            var db = new DataBase (true);
            var inputData = new ServerInfo { Name = "MyServer001", GameModes = new string[] { "DM" } };

            db.PutServerInfo ("server1", JsonConvert.SerializeObject (inputData));

            string matchData = JsonConvert.SerializeObject (
                new {
                    map = "DM-HelloWorld",
                    gameMode = "DM",
                    fragLimit = 20,
                    timeLimit = 20,
                    timeElapsed = 12.345678,
                    scoreboard = new object[] {
                        new { name = "Player1", frags = 20, kills = 20, deaths = 0 },
                        new { name = "Player2", frags = 0, kills = 0, deaths = 20 }
                    }
                }
            );
            db.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData);

            Assert.AreEqual (
                JsonConvert.SerializeObject (new {
                    totalMatchesPlayed = 1,
                    totalMatchesWon = 1,
                    favouriteServer = "server1",
                    uniqueServers = 1,
                    favouriteGameMode = "DM",
                    averageScoreBoardPercernt = 100.0,
                    maximumMatchesPerDay = 1,
                    averageMatchesPerDay = 1.0,
                    lastMatchPlayed = "2017-01-22T15:17:00Z",
                    killToDeathRatio = double.PositiveInfinity
                }),
                db.GetPlayerStats ("player1")
            );
            Assert.AreEqual (
                JsonConvert.SerializeObject (new {
                    totalMatchesPlayed = 1,
                    totalMatchesWon = 0,
                    favouriteServer = "server1",
                    uniqueServers = 1,
                    favouriteGameMode = "DM",
                    averageScoreBoardPercernt = 0.0,
                    maximumMatchesPerDay = 1,
                    averageMatchesPerDay = 1.0,
                    lastMatchPlayed = "2017-01-22T15:17:00Z",
                    killToDeathRatio = 0
                }),
                db.GetPlayerStats ("player2")
            );
        }
    }
}
