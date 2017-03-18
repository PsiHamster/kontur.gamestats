using Kontur.GameStats.Server.DataBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Kontur.GameStats.Tests.DBtests {
    [TestClass]
    public class ReportsTests {
        
        private void SendMatches() {

        }

        [TestMethod]
        public void GetRecentMatches() {
            var db = new DataBase (true);
            var inputData = new ServerInfo { Name = "MyServer001", GameModes = new string[] { "DM" } };
            db.PutServerInfo ("server1", JsonConvert.SerializeObject (inputData));

            string matchData1 = MatchGenerator.GetMatch ();
            string matchData2 = MatchGenerator.GetMatch ();
            string matchData3 = MatchGenerator.GetMatch ();
            db.PutMatch ("server1", "2017-01-23T15:14:00Z", matchData3);
            db.PutMatch ("server1", "2017-01-23T15:17:00Z", matchData1);
            db.PutMatch ("server1", "2017-01-23T15:16:00Z", matchData2);

            var recentMatches = db.GetRecentMatches (5);

            var mas = new object[3];

            mas[0] = new {
                server = "server1",
                timestamp = "2017-01-23T15:17:00Z",
                matchResult = JsonConvert.DeserializeObject (matchData1)
            };
            mas[1] = new {
                server = "server1",
                timestamp = "2017-01-23T15:16:00Z",
                matchResult = JsonConvert.DeserializeObject (matchData2)
            };
            mas[2] = new {
                server = "server1",
                timestamp = "2017-01-23T15:14:00Z",
                matchResult = JsonConvert.DeserializeObject (matchData3)
            };

            var excepted = JsonConvert.SerializeObject (mas);

            Assert.AreEqual (
                excepted,
                recentMatches
                );
        }

    }
}
