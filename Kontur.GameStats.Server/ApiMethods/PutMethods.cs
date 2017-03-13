using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kontur.GameStats.Server.DataBase;

namespace Kontur.GameStats.Server.ApiMethods {
    public static class PutMethods {
        /// <summary>
        /// Считывает переданные запросом данные
        /// </summary>
        public static string GetDataFromRequest(HttpListenerRequest request) {
            string data;
            using(var inputStream = request.InputStream) {
                var encoding = request.ContentEncoding;
                using(var reader = new StreamReader (inputStream, encoding)) {
                    data = reader.ReadToEnd ();
                }
            }
            return data;
        }

        /// <summary>
        /// Достает входящую строку и отправляет её на обработку классу БД
        /// </summary>
        public static void PutServerInfo(string endpoint, HttpListenerRequest request, HttpListenerResponse response) {
            try {
                var info = GetDataFromRequest (request);
                db.PutInfo (endpoint, info);
                response.StatusCode = 200;
            } catch (Exception e) {
                Console.WriteLine (e.Message);
                response.StatusCode = 400;
                response.StatusDescription = "Invalid arguments";
            }
        }

        /// <summary>
        /// Достает входящую строку и отправляет её на обработку классу БД
        /// </summary>
        public static void PutMatchInfo(string endpoint, string timeStamp, HttpListenerRequest request, HttpListenerResponse response) {
            try {
                var info = GetDataFromRequest (request);
                db.PutMatch (endpoint, timeStamp, info);
                response.StatusCode = 200;
            } catch (Exception e) {
                Console.WriteLine (e.Message);
                response.StatusCode = 400;
                response.StatusDescription = "Invalid arguments";
            }
        }
    }
}
