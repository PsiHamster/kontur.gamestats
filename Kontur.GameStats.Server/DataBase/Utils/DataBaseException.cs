using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server.DataBase {
    public class RequestException : Exception {
        public RequestException() { }

        public RequestException(string message) : base (message) { }
    }
}
