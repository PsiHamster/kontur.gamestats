using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LiteDB;
using System.Collections.Concurrent;

namespace Kontur.GameStats.Server.DataBase {
    public partial class DataBase {
        private ConcurrentQueue<Player> toUpdatePlayers = new ConcurrentQueue<Player>();

        private SynchronizedCollection<Player> bestPlayers = new SynchronizedCollection<Player> (50);
        private double minKD = -1;

        private Thread listenerThread;

        private void StartListenUpdatedPlayers() {
            LoadBestPlayers ();
            listenerThread = new Thread (ListenUpdatedPlayers) {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            listenerThread.Start ();
        }

        private void ListenUpdatedPlayers() {
            while(true) {
                try {
                    while (!toUpdatePlayers.IsEmpty) {
                        Player p;
                        if (toUpdatePlayers.TryDequeue(out p))
                            UpdateBestPlayers (p);
                    }
                    Thread.Sleep (20 * 1000); // Sleep 20 seconds
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
                foreach (var r in results) {
                    bestPlayers.Add (r);
                }
            }
            if (bestPlayers.Count > 0)
                minKD = bestPlayers.Last ().KD;
        }

        private void UpdateBestPlayers(Player player) {
            if(player.TotalMatches < 10 || player.TotalDeaths == 0)
                return;
            if(player.KD < minKD)
                return;
            var newList = new SynchronizedCollection<Player> ();
            var count = 0;
            var inserted = false;
            foreach(var elem in bestPlayers) {
                if(count >= 50)
                    return;
                if(elem.Name == player.Name)
                    continue;
                if(player.KD > elem.KD && !inserted) {
                    newList.Add (player);
                    inserted = true;
                } else {
                    newList.Add (elem);
                }
                count+=1;
                if (count == 50) {
                    minKD = elem.KD;
                }
            }
            if(count < 50) {
                newList.Add (player);
                minKD = player.KD;
            }
            minKD = newList.Last ().KD;
            bestPlayers = newList;
        }
    }
}
