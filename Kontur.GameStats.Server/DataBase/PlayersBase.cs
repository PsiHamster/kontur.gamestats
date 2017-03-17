using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kontur.GameStats.Server.DataBase {
    class PlayersBase {
        private BinaryFormatter formatter = new BinaryFormatter ();

        public PlayersBase() {
            Directory.CreateDirectory ("players");

            var symbols = new char[] {
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
            };

            foreach(var first in symbols) {
                foreach(var second in symbols) {
                    foreach(var third in symbols) {
                        Directory.CreateDirectory (string.Format ("players\\{0}\\{1}\\{2}", first, second, third));
                    }
                }
            }
        }

        /// <summary>
        /// Добавляет игрока в базу данных.
        /// </summary>
        public void AddPlayer(Player player) {
            string name = HttpUtility.UrlEncode(player.Name);
            string md5 = ComputeMD5Checksum (name);

            string directoryPath = string.Format("players\\{0}\\{1}\\{2}", md5[0], md5[1], md5[2]);
            //Directory.CreateDirectory (directoryPath);
            string filePath = string.Format ("{0}\\{1}.dat", directoryPath, name);

            using(var file = new FileStream (filePath, System.IO.FileMode.Create, FileAccess.Write)) {
                formatter.Serialize (file, player);
            }
        }

        /// <summary>
        /// Возвращает игрока из базы данных.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Player GetPlayer(string playerName) {
            string name = HttpUtility.UrlEncode (playerName);
            string md5 = ComputeMD5Checksum (name);
            Player player;

            string directoryPath = string.Format ("players\\{0}\\{1}\\{2}", md5[0], md5[1], md5[2]);
            //Directory.CreateDirectory (directoryPath);
            string filePath = string.Format ("{0}\\{1}.dat", directoryPath, name);

            try {
                using(var file = new FileStream (filePath, System.IO.FileMode.Open, FileAccess.Read)) {
                    player = (Player)formatter.Deserialize (file);
                }
            } catch (FileNotFoundException e) {
                return null;
            }
            return player;
        }

        private static string ComputeMD5Checksum(string name) {
                MD5 md5 = new MD5CryptoServiceProvider ();
                var fileData = Encoding.Unicode.GetBytes (name);
                var checkSum = md5.ComputeHash (fileData);
                var result = BitConverter.ToString (checkSum).Replace ("-", String.Empty);
                return result;
        }
        
    }
}
