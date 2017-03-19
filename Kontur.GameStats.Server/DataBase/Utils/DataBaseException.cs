using System;

namespace Kontur.GameStats.Server.DataBase {
    /// <summary>
    /// Exception кидающийся, когда ошибка в
    /// пользовательских данных
    /// </summary>
    public class RequestException : Exception {
        public RequestException() { }

        public RequestException(string message) : base (message) { }
    }
}
