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
        private ServersBase servers;

        private string workDirectory;

        #endregion

        #region Constructor

        /// <summary>
        /// Конструктор, возвращающий базу данных работающих с бд.
        /// </summary>
        /// <param name="deletePrev">Удалить ли старый файл, или открыть его</param>
        /// <param name="path">Директория в которой будет находится база данных</param>
        public DataBase(string path, bool deletePrev) {
            logger.Info (string.Format("Initializing statsDB"));

            workDirectory = path;
            Directory.CreateDirectory (workDirectory);

            if(deletePrev)
                DeleteFiles ();

            var dbConnPlayers = string.Format (
                "Filename={0}bestplayers.db;Journal=false;Timeout=0:00:10;Cache Size=15000",
                path + @"\");
            var dbConnMatches = string.Format (
                "Filename={0}recentmatches.db;Journal=false;Timeout=0:00:10;Cache Size=15000",
                path + @"\");

            matches = new MatchesBase (workDirectory, deletePrev);
            players = new PlayersBase (workDirectory, deletePrev);
            servers = new ServersBase (workDirectory, deletePrev);

            bestPlayers = new BestPlayers (dbConnPlayers);
            recentMatches = new RecentMatches (dbConnMatches);

            bestPlayers.StartCleanThread ();
            recentMatches.StartCleanThread ();

            if (!deletePrev) {
                LastMatchTime = recentMatches.GetLastMatchTime ();
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
            if(File.Exists (workDirectory + "\\bestplayers.db")) {
                File.Delete (workDirectory + "\\bestplayers.db");
            }
            if(File.Exists (workDirectory + "\\recentmatches.db")) {
                File.Delete (workDirectory + "\\recentmatches.db");
            }
        }

        #endregion
    }
}
