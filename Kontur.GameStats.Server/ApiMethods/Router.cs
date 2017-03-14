using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kontur.GameStats.Server.DataBase;
using NLog;

namespace Kontur.GameStats.Server.ApiMethods {
    public partial class Router {
        private Logger logger = LogManager.GetCurrentClassLogger ();
        private DataBase.DataBase dataBase;

        public Router(DataBase.DataBase db) {
            dataBase = db;
        }

        public void Route(string[] url, HttpListenerRequest request, HttpListenerResponse response) {
            try {
                ParseUrl (url, request, response);
            } catch(DataBase.RequestException e) {
                logger.Debug (e);
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusDescription = e.Message;
            } catch (MethodNotFoundException e) {
                logger.Debug (e);
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusDescription = "No such method found";
            } catch (WrongParamsException e) {
                logger.Debug (e);
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusDescription = "Wrong params exception";
            } catch (Exception e) {
                logger.Error (e);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.StatusDescription = "Server error. Try later.";
            } finally {
                response.Close ();
            }
        }

        #region StreamWork

        /// <summary>
        /// Пишет полученные данные клиенту.
        /// </summary>
        private void WriteResponse(HttpListenerResponse response, string text) {
            byte[] bytesText = Encoding.Unicode.GetBytes (text);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.KeepAlive = false;
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.Unicode;
            response.ContentLength64 = bytesText.Length;
            Stream output = response.OutputStream;
            output.Write (bytesText, 0, bytesText.Length);
            output.Close ();
        }

        /// <summary>
        /// Считывает переданные запросом данные
        /// </summary>
        public string GetDataFromRequest(HttpListenerRequest request) {
            string data;
            using(var inputStream = request.InputStream) {
                var encoding = request.ContentEncoding;
                using(var reader = new StreamReader (inputStream, encoding)) {
                    data = reader.ReadToEnd ();
                }
            }
            return data;
        }

        #endregion

        #region MethodsParser

        /// <summary>
        /// Парс 1-й переменной в url[] и отправка на обработку следующим функциям
        /// </summary>
        private void ParseUrl(string[] url, HttpListenerRequest request, HttpListenerResponse response) {
            if(url.Length == 0) {
                throw new MethodNotFoundException ();
            }
            switch(url[0]) {
                case "servers":
                    ServersMethods (url, request, response);
                    break;
                case "players":
                    PlayersMethods (url, request, response);
                    break;
                case "reports":
                    ReportsMethods (url, request, response);
                    break;
                default:
                    throw new MethodNotFoundException ();
            }
        }

        private void ServersMethods (string[] url, HttpListenerRequest request, HttpListenerResponse response) {
            if(url.Length < 2) {
                throw new MethodNotFoundException ();
            }

            string text = null;

            if(url[1] == "info") {
                text = GetServersInfo ();
            } else if(url.Length > 2) {
                switch(url[2]) {
                    case "info":
                        if(request.HttpMethod == "PUT") {
                            var info = GetDataFromRequest (request);
                            PutServerInfo (url[1], info);
                        } else {
                            text = GetServerInfo (url[1]);
                        }
                        break;
                    case "stats":
                        text = GetServerStats (url[1]);
                        break;
                    case "matches":
                        if(url.Length > 3)
                            if(request.HttpMethod == "PUT") {
                                var info = GetDataFromRequest (request);
                                PutMatchInfo (url[1], url[3], info);
                            } else {
                                text = GetMatchInfo (url[1], url[3]);
                            }
                        else
                            throw new MethodNotFoundException ();
                        break;
                    default:
                        throw new MethodNotFoundException ();
                }
            } else {
                throw new MethodNotFoundException ();
            }

            if (text != null) {
                WriteResponse (response, text);
            }
        }

        private void PlayersMethods(string[] url, HttpListenerRequest request, HttpListenerResponse response) {
            if(url.Length != 3) {
                throw new MethodNotFoundException ();
            }
            string text = null;
            switch(url[2]) {
                case "stats":
                    text = GetPlayerStats (url[1]);
                    break;
                default:
                    throw new MethodNotFoundException ();
            }
            if(text != null) {
                WriteResponse (response, text);
            }
        }

        private void ReportsMethods(string[] url, HttpListenerRequest request, HttpListenerResponse response) {
            int count;
            if(url.Length == 3) {
                count = int.Parse (url[2]);
            } else if (url.Length == 2) {
                count = 5;
            } else {
                throw new MethodNotFoundException ();
            }
            string text = null;
            switch(url[1]) {
                case "stats":
                    text = GetRecentMatches (count);
                    break;
                case "best-players":
                    text = GetBestPlayers (count);
                    break;
                case "popular-servers":
                    text = GetPopularServers (count);
                    break;
                default:
                    throw new MethodNotFoundException ();
            }
            if(text != null) {
                WriteResponse (response, text);
            }
        }

        #endregion
    }
}
