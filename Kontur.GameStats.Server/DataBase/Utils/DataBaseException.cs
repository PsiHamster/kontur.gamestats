using System;

namespace Kontur.GameStats.Server.DataBase {
    public class RequestException : Exception {
        public RequestException() { }

        public RequestException(string message) : base (message) { }
    }
}
