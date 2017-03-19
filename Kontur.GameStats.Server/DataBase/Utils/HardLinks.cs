using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.DataBase {
    public static class HardLinks {
        /// <summary>
        /// Создает HardLink на файл.
        /// </summary>
        [DllImport ("Kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool CreateHardLink(
          string lpFileName,
          string lpExistingFileName,
          IntPtr lpSecurityAttributes
        );
    }
}
