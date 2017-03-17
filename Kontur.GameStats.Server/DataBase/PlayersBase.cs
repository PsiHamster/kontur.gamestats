using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.DataBase {
    class PlayersBase {
        private BinaryFormatter formatter = new BinaryFormatter ();

        public PlayersBase() {
            Directory.CreateDirectory ("players");
        }

        public void AddPlayer(Player player) {
            string name = player.Name;
            string md5 = ComputeMD5Checksum (name);
            string filePath = string.Format ("players\\{0}\\{1}\\{2}\\{3}.dat", md5[0], md5[1], md5[2], name);
            using(var file = new FileStream (filePath, System.IO.FileMode.Create, FileAccess.Write)) {
                formatter.Serialize (file, player);
            }
        }

        public Player GetPlayer() {

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
