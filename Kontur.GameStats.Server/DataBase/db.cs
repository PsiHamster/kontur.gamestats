﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteDB;
using Newtonsoft.Json;
using NLog;

namespace Kontur.GameStats.Server.DataBase {

    /// <summary>
    /// Класс для обращения с базой данных.
    /// На вход принимает данные в json, возвращает тоже json
    /// Кидает RequestException при не правильных входных данных
    /// Ошибки базы данных также вызывают свои исключения.
    /// </summary>
    public partial class DataBase {
        private static string statsFileName = "statsbase.db";
        
        private string statsDBConn = "Filename=" + Directory.GetCurrentDirectory () + "\\" + statsFileName +
                ";Journal=false;Timeout=0:10:00;Cache Size=500000";

        public DateTime LastMatchTime = new DateTime (0).Date;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();

        #region Initializer

        #region LoadDefaults

        private void LoadLastMatchTime() {
            using(var db = new LiteDatabase (statsDBConn)) {
                var col = db.GetCollection<Match> ("matches");
                if(col.LongCount () > 0) {
                    var max = col.FindOne (Query.All ("TimeStamp", Query.Descending)).TimeStamp;
                    LastMatchTime = max;
                }
            }
        }

        #endregion

        /// <summary>
        /// Конструктор, возвращающий базу данных работающих с бд.
        /// </summary>
        /// <param name="deletePrev">Удалить ли старый файл, или открыть его</param>
        public DataBase(bool deletePrev) {
            logger.Info (string.Format("Initializing statsDB"));
            if(deletePrev && File.Exists (statsFileName)) {
                File.Delete (statsFileName);
            }
            if (!deletePrev && File.Exists (statsFileName)) {
                LoadLastMatchTime ();
            }
            StartListenUpdatedPlayers ();
            logger.Info (string.Format ("Success"));
        }

        /// <summary>
        /// Метод открывающий базу данных без удаления
        /// </summary>
        public DataBase() : this (false) { }

        #endregion
        
    }
}
