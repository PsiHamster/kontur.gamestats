using System;

namespace Kontur.GameStats.Server.ApiMethods {

    /// <summary>
    /// Класс-исключение для неверно введенных пользователем данных.
    /// </summary>
    public class MethodNotFoundException : Exception {
        public MethodNotFoundException() { }
        public MethodNotFoundException(string message) : base (message) { }
    }

    /// <summary>
    /// Класс-исключение для неверно введенных пользователем параметров.
    /// </summary>
    public class WrongParamsException : Exception {
        public WrongParamsException() { }
        public WrongParamsException(string message) : base (message) { }
    }
}

