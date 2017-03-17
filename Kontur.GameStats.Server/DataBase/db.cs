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
    /// На вход принимает данные в json, возвращает тоже json
    /// Кидает RequestException при не правильных входных данных
    /// Ошибки базы данных также вызывают свои исключения.
    /// </summary>
    public partial class DataBase {
        public static string statsFileName = "statsbase.db";
        
        private string statsDBConn = "Filename=" + Directory.GetCurrentDirectory () + "\\" + statsFileName +
                ";Journal=false;Timeout=0:10:00;Cache Size=500000";

        public DateTime LastMatchTime = new DateTime (0).Date;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();

        private BestPlayers bestPlayers;
        private RecentMatches recentMatches;
        private PlayersBase players;

        #region Initializer

        #region LoadDefaults

        private void LoadLastMatchTime() {
            RecentMatchInfo[] a;
            if((a = JsonConvert.DeserializeObject<RecentMatchInfo[]> (recentMatches.Take (1))).Length > 0){
                LastMatchTime =  a[0].timestamp;
            };
        }

        #endregion

        /// <summary>
        /// Конструктор, возвращающий базу данных работающих с бд.
        /// </summary>
        /// <param name="deletePrev">Удалить ли старый файл, или открыть его</param>
        /// <param name="name">Имя файла базы данных</param>
        public DataBase(string name, bool deletePrev) {
            statsDBConn = "Filename=" + Directory.GetCurrentDirectory () + "\\" + name +
                ";Journal=false;Timeout=0:10:00;Cache Size=500000";
            logger.Info (string.Format("Initializing statsDB"));
            if(deletePrev)
                DeleteFiles (name);

            players = new PlayersBase ();
            bestPlayers = new BestPlayers ();
            recentMatches = new RecentMatches ();
            if (!deletePrev && File.Exists ("recentMatches.dat")) {
                LoadLastMatchTime ();
            }

            logger.Info (string.Format ("Success"));
        }

        /// <summary>
        /// Метод открывающий базу данных без удаления
        /// </summary>
        public DataBase() : this (statsFileName, false) { }
        #endregion

        #region deleter

        private void DeleteFiles(string name) {
            if (File.Exists (name)) {
                File.Delete (name);
            }
            if (File.Exists ("bestPlayers.dat")) {
                File.Delete ("bestPlayers.dat");
            }
            if (File.Exists ("recentMatches.dat")) {
                File.Delete ("recentMatches.dat");
            }
            if (Directory.Exists("servers")) {
                Directory.Delete ("servers", true);
            }
            if(Directory.Exists ("players")) {
                Directory.Delete ("servers", true);
            }
        }

        #endregion
    }
}
