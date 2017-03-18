using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteDB;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;

namespace Kontur.GameStats.Server.DataBase {

    /// <summary>
    /// Класс для обращения с базой данных.
    /// На вход принимает данные в json, возвращает json
    /// Кидает RequestException при не правильных входных данных
    /// Ошибки базы данных также вызывают свои исключения.
    /// </summary>
    public partial class DataBase {

        #region Fields

        public DateTime LastMatchTime = new DateTime (0).Date;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();

        private BestPlayers bestPlayers;
        private RecentMatches recentMatches;

        private PlayersBase players;
        private MatchesBase matches;

        private string workDirectory;
        private string dbConn;

        #endregion

        #region Constructor

        #region LoadDefaults

        private void LoadLastMatchTime() {
            MatchInfo[] a;
            if((a = JsonConvert.DeserializeObject<MatchInfo[]> (recentMatches.Take (1))).Length > 0){
                LastMatchTime =  a[0].Timestamp;
            };
        }

        #endregion

        /// <summary>
        /// Конструктор, возвращающий базу данных работающих с бд.
        /// </summary>
        /// <param name="deletePrev">Удалить ли старый файл, или открыть его</param>
        /// <param name="path">Директория в которой будет находится база данных</param>
        public DataBase(string path, bool deletePrev) {
            logger.Info (string.Format("Initializing statsDB"));

            workDirectory = path;
            Directory.CreateDirectory (workDirectory);

            dbConn = string.Format(
                "Filename={0}database.db;Journal=false;Timeout=0:10:00;Cache Size=500000",
                path+@"\");

            if(deletePrev)
                DeleteFiles ();

            using(var db = new LiteDatabase (dbConn)) {
                var serversCol = db.GetCollection<Server> ("servers");
                serversCol.LongCount ();
            }

            matches = new MatchesBase (workDirectory, deletePrev);
            players = new PlayersBase (workDirectory, deletePrev);

            bestPlayers = new BestPlayers ();
            recentMatches = new RecentMatches ();

            bestPlayers.StartListen ();
            recentMatches.StartListen ();

            if (!deletePrev && File.Exists ("recentMatches.dat")) {
                LoadLastMatchTime ();
            }

            logger.Info (string.Format ("Success"));
        }

        /// <summary>
        /// Метод открывающий базу данных без удаления в текущей директории
        /// </summary>
        public DataBase() : this ("database", false) { }

        /// <summary>
        /// Метод открывающий базу данных в текущей директории
        /// </summary>
        public DataBase(bool deletePrev) : this ("database", deletePrev) { }

        #endregion

        #region deleter

        private void DeleteFiles() {
            if (File.Exists (workDirectory+"\\database.db")) {
                File.Delete (workDirectory+"\\database.db");
            }
            if (File.Exists (workDirectory+"\\bestPlayers.dat")) {
                File.Delete (workDirectory+"\\bestPlayers.dat");
            }
            if (File.Exists (workDirectory+"\\recentMatches.dat")) {
                File.Delete (workDirectory+"\\recentMatches.dat");
            }
        }

        #endregion
    }
}
