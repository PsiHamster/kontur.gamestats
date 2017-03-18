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
        [TestMethod]
        public void PutServerInfo() {
            var db = new DataBase (true);
            var inputData = new ServerInfo {
                Name = "MyServer001",
                GameModes = new string[] { "DM" } };
            
            db.PutInfo ("server1", JsonConvert.SerializeObject(inputData));
            Assert.AreEqual (JsonConvert.SerializeObject (inputData), db.GetServerInfo ("server1"));
        }

        [TestMethod]
        public void PutAndUpdateServerInfo() {
            var db = new DataBase (true);
            var inputData = new ServerInfo {
                Name = "MyServer001",
                GameModes = new string[] { "DM" }
            };

            db.PutInfo ("server1", JsonConvert.SerializeObject (inputData));
            Assert.AreEqual (JsonConvert.SerializeObject (inputData), db.GetServerInfo ("server1"));

            var inputData2 = new ServerInfo {
                Name = "MyServer002",
                GameModes = new string[] { "DM" }
            };

            db.PutInfo ("server1", JsonConvert.SerializeObject (inputData2));
            Assert.AreEqual (JsonConvert.SerializeObject (inputData2), db.GetServerInfo ("server1"));
        }

        [TestMethod]
        public void GetServerStats() {
            var db = new DataBase (true);
            var inputData = new ServerInfo {
                Name = "MyServer001",
                GameModes = new string[] { "DM" }
            };
            db.PutInfo ("server1", JsonConvert.SerializeObject (inputData));


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

        [TestMethod]
        public void GetServersInfo() {
            var db = new DataBase (true);
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

            db.PutInfo ("server1", JsonConvert.SerializeObject(inputData));
            db.PutInfo ("server2", JsonConvert.SerializeObject(inputData2));
            db.PutInfo ("server3", JsonConvert.SerializeObject(inputData3));

            var serversInfo = db.GetServersInfo ();
            
            Assert.AreEqual (
                JsonConvert.SerializeObject(
                    new ServerInfoEndpoint[] {
                        new ServerInfoEndpoint { EndPoint = "server1", Info = inputData },
                        new ServerInfoEndpoint { EndPoint = "server2", Info = inputData2 },
                        new ServerInfoEndpoint { EndPoint = "server3", Info = inputData3 },
                }),
                serversInfo
            );
        }
    }
}
