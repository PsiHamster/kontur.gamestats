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

        private string dbConn = "Filename=database.db;Journal=false;Timeout=0:10:00;Cache Size=500000";

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
        /// <param name="name">Имя файла базы данных</param>
        public DataBase(bool deletePrev) {
            logger.Info (string.Format("Initializing statsDB"));
            if(deletePrev)
                DeleteFiles ();

            using(var db = new LiteDatabase (dbConn)) {
                var serversCol = db.GetCollection<Server> ("servers");
                serversCol.LongCount ();
            }

            matches = new MatchesBase (deletePrev);
            players = new PlayersBase (deletePrev);

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
        /// Метод открывающий базу данных без удаления
        /// </summary>
        public DataBase() : this (false) { }
        #endregion

        #region deleter

        private void DeleteFiles() {
            if (File.Exists ("database.db")) {
                File.Delete ("database.db");
            }
            if (File.Exists ("bestPlayers.dat")) {
                File.Delete ("bestPlayers.dat");
            }
            if (File.Exists ("recentMatches.dat")) {
                File.Delete ("recentMatches.dat");
            }
        }

        #endregion
    }
}
