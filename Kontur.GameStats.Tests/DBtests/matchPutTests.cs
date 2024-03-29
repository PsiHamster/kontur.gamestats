﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kontur.GameStats.Server.DataBase;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kontur.GameStats.Tests.DBtests {
    [TestClass]
    public class matchPutTests {
        [TestMethod]
        public void PutSingleMatch() {
            var db = new DataBase ("testdb.db", true);
            var inputData = new ServerInfo { name = "MyServer001", gameModes = new string[] { "DM" } };
            
            db.PutInfo ("server1", JsonConvert.SerializeObject(inputData));

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
                db.GetPlayerStats("Player1")
            );
        }

        [TestMethod]
        public void GetRecentMatches() {
            var db = new DataBase ("testdb.db", true);
            var inputData = new ServerInfo { name = "MyServer001", gameModes = new string[] { "DM" } };
            db.PutInfo ("server1", JsonConvert.SerializeObject (inputData));

            string matchData1 = MatchGenerator.GetMatch ();
            string matchData2 = MatchGenerator.GetMatch ();
            string matchData3 = MatchGenerator.GetMatch ();
            db.PutMatch ("server1", "2017-01-22T15:14:00Z", matchData3);
            db.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData1);
            db.PutMatch ("server1", "2017-01-22T15:16:00Z", matchData2);

            var recentMatches = db.GetRecentMatches (5);
            var excepted = "[" + matchData1 + "," + matchData2 + "," + matchData3 + "]";
            Assert.AreEqual (
                excepted,
                recentMatches
                );
        }

        [TestMethod]
        public void GetPopularServers() {
            var db = new DataBase ("testdb.db", true);
            var inputData = new ServerInfo { name = "MyServer001", gameModes = new string[] { "DM" } };
            var inputData2 = new ServerInfo { name = "MyServer002", gameModes = new string[] { "DM" } };
            db.PutInfo ("server1", JsonConvert.SerializeObject (inputData));
            db.PutInfo ("server2", JsonConvert.SerializeObject (inputData2));

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
    }
}