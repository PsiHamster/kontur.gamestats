using Kontur.GameStats.Server.DataBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace Kontur.GameStats.Tests.DBtests {
    [TestClass]
    public class MatchPutTests {

        #region /servers/{endpoint}/matches/{timestamp}

        [TestMethod]
        public void PutSingleMatch() {
            var db = new DataBase (true);
            var inputData = new ServerInfo { Name = "MyServer001", GameModes = new string[] { "DM" } };
            
            db.PutServerInfo ("server1", JsonConvert.SerializeObject(inputData));

            string matchData = JsonConvert.SerializeObject (
                new {
                    map = "DM-HelloWorld",
                    gameMode = "DM",
                    fragLimit = 20,
                    timeLimit = 20,
                    timeElapsed = 12.345678,
                    scoreboard = new object[] {
                        new { name = "Player1", frags = 20, kills = 20, deaths = 4 },
                        new { name = "Player2", frags = 2, kills = 2, deaths = 21 }
                    }
                }
                );
            db.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData);

            Assert.AreEqual (matchData, db.GetMatchInfo ("server1", "2017-01-22T15:17:00Z"));
        }

        [TestMethod]
        public void PutLessMatchData() {
            var db = new DataBase (true);
            var inputData = new ServerInfo { Name = "MyServer001", GameModes = new string[] { "DM" } };

            db.PutServerInfo ("server1", JsonConvert.SerializeObject (inputData));

            string matchData = JsonConvert.SerializeObject (
                new {
                    map = "DM-HelloWorld",
                    gameMode = "DM",
                }
                );
            try {
                db.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData);
            } catch (RequestException e) {
                return;
            } catch (Exception e) {
                Assert.Fail ();
            }
            Assert.Fail ();
        }

        [TestMethod]
        public void PutWrongMatchData() {
            var db = new DataBase (true);
            var inputData = new ServerInfo { Name = "MyServer001", GameModes = new string[] { "DM" } };

            db.PutServerInfo ("server1", JsonConvert.SerializeObject (inputData));

            string matchData = JsonConvert.SerializeObject (
                new {
                    waifu = "Mio :3"
                }
                );
            try {
                db.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData);
            } catch(RequestException e) {
                return;
            } catch(Exception e) {
                Assert.Fail ();
            }
            Assert.Fail ();
        }

        [TestMethod]
        public void PutExtraMatchData() {
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
                        new { name = "Player2", frags = 2, kills = 2, deaths = 21 }
                    },
                    lol = "lollololololol"
                }
                );
            try {
                db.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData);
            } catch(RequestException e) {
                return;
            } catch(Exception e) {
                Assert.Fail ();
            }
            Assert.Fail ();
        }

        #endregion

        
        [TestMethod]
        public void GetPopularServers() {
            var db = new DataBase (true);
            var inputData = new ServerInfo { Name = "MyServer001", GameModes = new string[] { "DM" } };
            var inputData2 = new ServerInfo { Name = "MyServer002", GameModes = new string[] { "DM" } };
            db.PutServerInfo ("server1", JsonConvert.SerializeObject (inputData));
            db.PutServerInfo ("server2", JsonConvert.SerializeObject (inputData2));

            string matchData1 = MatchGenerator.GetMatch ();
            string matchData2 = MatchGenerator.GetMatch ();
            string matchData3 = MatchGenerator.GetMatch ();
            db.PutMatch ("server1", "2017-01-22T15:14:00Z", matchData3);
            db.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData1);
            db.PutMatch ("server2", "2017-01-22T15:16:00Z", matchData2);

            var popularServers = db.GetPopularServers (5);
            var excepted = JsonConvert.SerializeObject (new object[] {
                new{
                    endpoint = "server1",
                    name = "MyServer001",
                    averageMatchesPerDay = 2.0
                },
                new {
                    endpoint = "server2",
                    name = "MyServer002",
                    averageMatchesPerDay = 1.0
                }
            });
            Assert.AreEqual (
                excepted,
                popularServers
            );
        }

        [TestMethod]
        public void GetBestPlayers() {
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
                        new { name = "Player2", frags = 2, kills = 2, deaths = 2 }
                    }
                }
                );

            string matchData2 = JsonConvert.SerializeObject (
                new {
                    map = "DM-HelloWorld",
                    gameMode = "DM",
                    fragLimit = 20,
                    timeLimit = 20,
                    timeElapsed = 12.345678,
                    scoreboard = new object[] {
                        new { name = "Player1", frags = 20, kills = 20, deaths = 4 },
                        new { name = "Player2", frags = 2, kills = 2, deaths = 2 },
                        new { name = "Player3", frags = 2, kills = 2, deaths = 2 }
                    }
                }
                );

            db.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData);
            db.PutMatch ("server1", "2017-01-22T15:18:00Z", matchData);
            db.PutMatch ("server1", "2017-01-22T15:19:00Z", matchData);
            db.PutMatch ("server1", "2017-01-22T15:20:00Z", matchData2);
            db.PutMatch ("server1", "2017-01-22T15:21:00Z", matchData2);
            db.PutMatch ("server1", "2017-01-22T15:22:00Z", matchData2);
            db.PutMatch ("server1", "2017-01-22T15:23:00Z", matchData2);
            db.PutMatch ("server1", "2017-01-22T15:24:00Z", matchData2);
            db.PutMatch ("server1", "2017-01-22T15:25:00Z", matchData2);
            db.PutMatch ("server1", "2017-01-22T15:26:00Z", matchData2);

            Thread.Sleep (22 * 1000);

            var expected = JsonConvert.SerializeObject (
                new object[] {
                    new { name = "Player1", killToDeathRatio = 5.0 },
                    new { name = "Player2", killToDeathRatio = 1.0 }
                });

            Assert.AreEqual (
                "[]",
                db.GetBestPlayers (-1)
            );
            Assert.AreEqual (
                expected,
                db.GetBestPlayers (2)
            );
            Assert.AreEqual (
                expected,
                db.GetBestPlayers (5));
        }
    }
}