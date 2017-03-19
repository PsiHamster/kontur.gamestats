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

            matches = new MatchesBase (workDirectory, deletePrev);
            players = new PlayersBase (workDirectory, deletePrev);
            servers = new ServersBase (workDirectory, deletePrev);
            
            if (!deletePrev) {
                LastMatchTime = matches.RecentMatches.GetLastMatchTime ();
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
    }
}
