using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Kontur.GameStats.Server.DataBase;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Kontur.GameStats.Tests.DBtests {
    [TestClass]
    public class ServerMethodsTests {

        #region servers/{endpoint}/info

        [TestMethod]
        public void PutServerInfo() {
            var db = new DataBase ("PutServerInfo", true);
            var inputData = new {
                name = "MyServer001",
                gameModes = new string[] { "DM" } };
            
            db.PutServerInfo ("server1", JsonConvert.SerializeObject(inputData));
            Assert.AreEqual (JsonConvert.SerializeObject (inputData), db.GetServerInfo ("server1"));
        }

        [TestMethod]
        public void PutAndUpdateServerInfo() {
            var db = new DataBase ("PutAndUpdateServerInfo", true);
            var inputData = new {
                name = "MyServer001",
                gameModes = new string[] { "DM" }
            };

            db.PutServerInfo ("server1", JsonConvert.SerializeObject (inputData));
            Assert.AreEqual (JsonConvert.SerializeObject (inputData), db.GetServerInfo ("server1"));

            var inputData2 = new {
                name = "MyServer002",
                gameModes = new string[] { "DM" }
            };

            db.PutServerInfo ("server1", JsonConvert.SerializeObject (inputData2));
            Assert.AreEqual (JsonConvert.SerializeObject (inputData2), db.GetServerInfo ("server1"));
        }

        [TestMethod]
        public void PutWrongServerInfo() {
            var db = new DataBase ("PutWrongServerInfo", true);
            string data = JsonConvert.SerializeObject (new {
                lol = "AHAHAHAH"
            });

            try {
                db.PutServerInfo ("server1", data);
            } catch (RequestException) {
                return;
            } catch (Exception) {
                Assert.Fail ();
            }
            Assert.Fail ();
        }

        [TestMethod]
        public void PutExtraServerInfo() {
            var db = new DataBase ("PutExtraServerInfo", true);
            string data = JsonConvert.SerializeObject (new {
                name = "server1",
                gameModes = new string[] { "DM" },
                lol = "AHAHAHAH"
            });

            try {
                db.PutServerInfo ("server1", data);
            } catch(RequestException) {
                return;
            } catch(Exception) {
                Assert.Fail ();
            }
            Assert.Fail ();
        }

        #endregion

        #region servers/info

        [TestMethod]
        public void GetServersInfo() {
            var db = new DataBase ("GetServersInfo", true);
            var inputData = new ServerInfo {
                Name = "MyServer001",
                GameModes = new string[] { "DM" }
            };
            var inputData2 = new ServerInfo {
                Name = "MyServer002",
                GameModes = new string[] { "DM" }
            };
            var inputData3 = new ServerInfo {
                Name = "MyServer003",
                GameModes = new string[] { "DM", "HSDM" }
            };

            db.PutServerInfo ("server1", JsonConvert.SerializeObject (inputData));
            db.PutServerInfo ("server2", JsonConvert.SerializeObject (inputData2));
            db.PutServerInfo ("server3", JsonConvert.SerializeObject (inputData3));

            var serversInfo = db.GetServersInfo ();

            Assert.AreEqual (
                JsonConvert.SerializeObject (
                    new ServerInfoEndpoint[] {
                        new ServerInfoEndpoint { EndPoint = "server1", Info = inputData },
                        new ServerInfoEndpoint { EndPoint = "server2", Info = inputData2 },
                        new ServerInfoEndpoint { EndPoint = "server3", Info = inputData3 },
                }),
                serversInfo
            );
        }

        #endregion

        #region servers/{endpoint}/stats

        [TestMethod]
        public void GetServerStats() {
            var db = new DataBase ("GetServerStats", true);
            var inputData = new ServerInfo {
                Name = "MyServer001",
                GameModes = new string[] { "DM" }
            };
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
                    }
                }
                );
            db.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData);

            var expected = JsonConvert.SerializeObject (new {
                totalMatchesPlayed = 1,
                maximumMatchesPerDay = 1,
                averageMatchesPerDay = 1.0,
                maximumPopulation = 2,
                averagePopulation = 2.0,
                top5GameModes = new object[] { "DM" },
                top5Maps = new object[] { "DM-HelloWorld" }
            });

            Assert.AreEqual (expected, db.GetServerStatistics ("server1"));
        }

        #endregion

    }
}
