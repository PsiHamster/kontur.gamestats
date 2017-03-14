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
    public class serverMethodsTests {
        [TestMethod]
        public void PutServerInfo() {
            
            DataBase.InitialazeDB ("testdb.db", true);
            var inputData = new ServerInfo {
                name = "MyServer001",
                gameModes = new string[] { "DM" } };
            
            DataBase.PutInfo ("server1", JsonConvert.SerializeObject(inputData));
            Assert.AreEqual (JsonConvert.SerializeObject (inputData), DataBase.GetServerInfo ("server1"));

            var inputData2 = new ServerInfo {
                name = "MyServer002",
                gameModes = new string[] { "DM" }
            };

            DataBase.PutInfo ("server1", JsonConvert.SerializeObject(inputData2));
            Assert.AreEqual (JsonConvert.SerializeObject (inputData2), DataBase.GetServerInfo ("server1"));
        }

        [TestMethod]
        public void GetServersInfo() {
            DataBase.InitialazeDB ("testdb.db", true);
            var inputData = new ServerInfo {
                name = "MyServer001",
                gameModes = new string[] { "DM" }
            };
            var inputData2 = new ServerInfo {
                name = "MyServer002",
                gameModes = new string[] { "DM" }
            };
            var inputData3 = new ServerInfo {
                name = "MyServer003",
                gameModes = new string[] { "DM", "HSDM" }
            };

            DataBase.PutInfo ("server1", JsonConvert.SerializeObject(inputData));
            DataBase.PutInfo ("server2", JsonConvert.SerializeObject(inputData2));
            DataBase.PutInfo ("server3", JsonConvert.SerializeObject(inputData3));

            var s = DataBase.GetServersInfo ();
            
            Assert.AreEqual (
                JsonConvert.SerializeObject(
                new ServerInfoEndpoint[] {
                    new ServerInfoEndpoint { endpoint = "server1", info = inputData },
                    new ServerInfoEndpoint { endpoint = "server2", info = inputData2 },
                    new ServerInfoEndpoint { endpoint = "server3", info = inputData3 },
                }),
                s);
        }
    }
}
