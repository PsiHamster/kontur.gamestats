﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kontur.GameStats.Server.DataBase;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Kontur.GameStats.Server.ApiMethods.PutMethods;

namespace Kontur.GameStats.Tests.DBtests {
    [TestClass]
    public class matchPutTests {
        [TestMethod]
        public void PutSingleMatch() {
            DataBase.InitialazeDB ("testdb.db", true);
            var inputData = new ServerInfo { name = "MyServer001", gameModes = new string[] { "DM" } };
            DataBase.PutInfo ("server1", JsonConvert.SerializeObject(inputData));

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
            DataBase.PutMatch ("server1", "2017-01-22T15:17:00Z", matchData);

            Assert.AreEqual (matchData, DataBase.GetMatchInfo ("server1", "2017-01-22T15:17:00Z"));
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
                DataBase.GetPlayerStats("Player1")
            );
        }
    }
}