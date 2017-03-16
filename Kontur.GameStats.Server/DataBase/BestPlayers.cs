using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LiteDB;

namespace Kontur.GameStats.Server.DataBase {
    public partial class DataBase {
        private Queue<Player> toUpdatePlayers;

        private List<Player> bestPlayers = new List<Player> (50);

        private Thread listenerThread;

        private void StartListenUpdatedPlayers() {
            listenerThread = new Thread (ListenUpdatedPlayers) {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
        }

        private void ListenUpdatedPlayers() {
            while(true) {
                try {
                    while (toUpdatePlayers.Count > 0) {
                        var p = toUpdatePlayers.Dequeue ();
                        UpdateBestPlayers (p);
                    }
                    Thread.Sleep (30 * 1000); // Sleep 30 seconds
                } catch(Exception e) {
                    logger.Error (e);
                }
            }
        }

        private void LoadBestPlayers() {
            var count = 50;
            using(var db = new LiteDatabase (statsDBConn)) {
                var col = db.GetCollection<Player> ("players");
                var results = col.Find (
                    Query.And (
                        Query.And (
                            Query.All ("KD", Query.Descending),
                            Query.GTE ("TotalMatches", 10)),
                        Query.Not ("TotalDeaths", 0)), limit: count
                    );
                bestPlayers = results.ToList ();
            }
        }

        private void UpdateBestPlayers(Player player) {
            if(player.TotalMatches < 10 || player.TotalDeaths == 0)
                return;
            bool isInserted = false;
            for (int i = 0; i < bestPlayers.Count; i++) {
                if(bestPlayers[i].Name == player.Name) {
                    bestPlayers.RemoveAt (i);
                    i--;
                } else if (bestPlayers[i].KD < player.KD) {
                    bestPlayers.Insert (i, player);
                    isInserted = true;
                }
            }
            if (!isInserted && bestPlayers.Count < 50) {
                bestPlayers.Add (player);
            } else if (bestPlayers.Count > 50) {
                bestPlayers.RemoveAt(50);
            }
        }
    }
}
