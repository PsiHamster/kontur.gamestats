using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Kontur.GameStats.Server.DataBase {

    /// <summary>
    /// Класс работающий с базой данных игроков, 
    /// игроки делятся по папкам /0/1/2,
    /// где 0, 1, 2 - первые три символа MD5 хэша
    /// от имени игрока
    /// </summary>
    class PlayersBase {

        public string workDirectory { get; private set; }
        public BestPlayers BestPlayers { get; private set; }

        #region Constructor

        public PlayersBase(string directory, bool deletePrev = false) {
            workDirectory = directory+ "\\players";
            BestPlayers = new BestPlayers (directory, deletePrev);
            if (deletePrev && Directory.Exists (workDirectory)) {
                Directory.Delete (workDirectory, true);
            }
            Directory.CreateDirectory (workDirectory);
            foreach(var first in symbols) {
                foreach(var second in symbols) {
                    foreach(var third in symbols) {
                        Directory.CreateDirectory (
                            string.Format (workDirectory + "\\{0}\\{1}\\{2}", first, second, third)
                            );
                    }
                }
            }
        }

        #endregion

        #region FileWriteGet

        private BinaryFormatter formatter = new BinaryFormatter ();

        /// <summary>
        /// Добавляет игрока в базу данных.
        /// </summary>
        public void AddPlayer(Player player) {
            string name = HttpUtility.UrlEncode(player.Name);
            string md5 = ComputeMD5Checksum (name);

            string directoryPath = string.Format(workDirectory + "\\{0}\\{1}\\{2}", md5[0], md5[1], md5[2]);
            string filePath = string.Format ("{0}\\{1}.dat", directoryPath, name);

            using(var file = new FileStream (filePath, System.IO.FileMode.Create, FileAccess.Write)) {
                formatter.Serialize (file, player);
            }
            BestPlayers.Add (player);
        }

        /// <summary>
        /// Возвращает игрока из базы данных.
        /// </summary>
        public Player GetPlayer(string playerName) {
            string name = HttpUtility.UrlEncode (playerName);
            string md5 = ComputeMD5Checksum (name);
            Player player;

            string directoryPath = string.Format (workDirectory + "\\{0}\\{1}\\{2}", md5[0], md5[1], md5[2]);
            string filePath = string.Format ("{0}\\{1}.dat", directoryPath, name);

            try {
                using(var file = new FileStream (filePath, System.IO.FileMode.Open, FileAccess.Read)) {
                    player = (Player)formatter.Deserialize (file);
                }
            } catch (FileNotFoundException) {
                return null;
            }
            return player;
        }

        #endregion

        #region MD5

        private char[] symbols = new char[] {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
        };

        /// <summary>
        /// Функция считающая MD5 хэш строки
        /// </summary>
        private static string ComputeMD5Checksum(string name) {
                MD5 md5 = new MD5CryptoServiceProvider ();
                var fileData = Encoding.Unicode.GetBytes (name);
                var checkSum = md5.ComputeHash (fileData);
                var result = BitConverter.ToString (checkSum).Replace ("-", String.Empty);
                return result;
        }

        #endregion

    }
}
