using Kontur.GameStats.Server.DataBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace Kontur.GameStats.Tests.DBtests {
    [TestClass]
    public class MatchPutTests {

        [TestMethod]
        public void PutSingleMatch() {
            var db = new DataBase ("PutSingleMatch", true);
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
            var db = new DataBase ("PutLessMatchData", true);
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
            } catch (RequestException) {
                return;
            } catch (Exception) {
                Assert.Fail ();
            }
            Assert.Fail ();
        }

        [TestMethod]
        public void PutWrongMatchData() {
            var db = new DataBase ("PutWrongMatchData", true);
            var inputData = new ServerInfo { Name = "MyServer001", GameModes = new string[] { "DM" } };

            db.PutServerInfo ("server1", JsonConvert.SerializeObject (inputData));

            string matchData = JsonConvert.SerializeObject (
                new {
                    waifu = "Mio :3"
                }
                );
            try {
                db.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData);
            } catch(RequestException) {
                return;
            } catch(Exception) {
                Assert.Fail ();
            }
            Assert.Fail ();
        }

        [TestMethod]
        public void PutExtraMatchData() {
            var db = new DataBase ("PutExtraMatchData", true);
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
            } catch(RequestException) {
                return;
            } catch(Exception) {
                Assert.Fail ();
            }
            Assert.Fail ();
        }
        
    }
}