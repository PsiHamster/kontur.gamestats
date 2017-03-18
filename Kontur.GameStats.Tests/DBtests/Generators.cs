using Kontur.GameStats.Server.DataBase;
using Newtonsoft.Json;
using System;

namespace Kontur.GameStats.Tests.DBtests {
    static class MatchGenerator {
        static string[] maps = new string[] { "de_nuke","de_inferno","de_cobblestone","de_mirage","de_overpass","de_cache","de_train","de_dust2" };
        static string[] gameModes = new string[] { "DM", "HSDM", "THSDM", "TDM", "MM", "ARENA", "BP" };
        static Random randomizer = new Random ();

        public static object GetScore(int i) {
            return new {
                name = string.Format ("Player{0}", i),
                frags = randomizer.Next (0, 100),
                kills = randomizer.Next (0, 100),
                deaths = randomizer.Next (0, 100)
            };
        }

        public static string GetMatch() {
            object[] players = new object[randomizer.Next (1, 100)];
            int startPlayerID = randomizer.Next (0, 100);
            for (int i = 0; i < players.Length; i++) {
                players[i] = GetScore (startPlayerID + i);
            }

            return JsonConvert.SerializeObject (new {
                map = maps[randomizer.Next () % maps.Length],
                gameMode = gameModes[randomizer.Next() % gameModes.Length],
                fragLimit = 20,
                timeLimit = 20,
                timeElapsed = 12.345678,
                scoreboard = players
            });
        }
    }

    static class ServerGenerator {
        static Random randomize = new Random ();
        static int serverID = 0;

        public static ServerInfoEndpoint GetRandomServer() {
            var serverData = new ServerInfoEndpoint {
                EndPoint = string.Format ("server{0}", serverID++),
                Info = new ServerInfo {
                    Name = string.Format("MyServer{0}", randomize.Next()),
                    GameModes = new string[] { "DM" }
                }
            };

            return serverData;
        }
    }
}
