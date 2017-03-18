using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Kontur.GameStats.Server.DataBase;
using NLog;

namespace Kontur.GameStats.Server.ApiMethods {
    public partial class Router {
        private Logger logger = LogManager.GetCurrentClassLogger ();
        private DataBase.DataBase dataBase;

        public Router(DataBase.DataBase db) {
            dataBase = db;
        }

        public void Route(string[] uri, HttpListenerRequest request, HttpListenerResponse response) {
            try {
                ParseUri (uri, request, response);
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

        #endregion

        #region MethodsParser

        /// <summary>
        /// Парс 1-й переменной в uri[] и отправка на обработку следующим функциям
        /// </summary>
        private void ParseUri(string[] uri, HttpListenerRequest request, HttpListenerResponse response) {
            if(uri.Length == 0) {
                throw new MethodNotFoundException ();
            }
            switch(uri[0]) {
                case "servers":
                    ServersMethods (uri, request, response);
                    break;
                case "players":
                    PlayersMethods (uri, request, response);
                    break;
                case "reports":
                    ReportsMethods (uri, request, response);
                    break;
                default:
                    throw new MethodNotFoundException ();
            }
        }

        private void ServersMethods (string[] uri, HttpListenerRequest request, HttpListenerResponse response) {
            if(uri.Length < 2) {
                throw new MethodNotFoundException ();
            }

            string text = null;

            if(uri[1] == "info") {
                text = GetServersInfo ();
            } else if(uri.Length > 2) {
                switch(uri[2]) {
                    case "info":
                        if(request.HttpMethod == "PUT") {
                            var info = GetDataFromRequest (request);
                            PutServerInfo (uri[1], info);
                        } else {
                            text = GetServerInfo (uri[1]);
                        }
                        break;
                    case "stats":
                        text = GetServerStats (uri[1]);
                        break;
                    case "matches":
                        if(uri.Length > 3)
                            if(request.HttpMethod == "PUT") {
                                var info = GetDataFromRequest (request);
                                PutMatchInfo (uri[1], uri[3], info);
                            } else {
                                text = GetMatchInfo (uri[1], uri[3]);
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

        private void PlayersMethods(string[] uri, HttpListenerRequest request, HttpListenerResponse response) {
            if(uri.Length != 3) {
                throw new MethodNotFoundException ();
            }
            string text = null;
            switch(uri[2]) {
                case "stats":
                    string playerName = HttpUtility.UrlDecode (uri[1]);
                    text = GetPlayerStats (playerName);
                    break;
                default:
                    throw new MethodNotFoundException ();
            }
            if(text != null) {
                WriteResponse (response, text);
            }
        }

        private void ReportsMethods(string[] uri, HttpListenerRequest request, HttpListenerResponse response) {
            int count;
            if(uri.Length == 3) {
                count = int.Parse (uri[2]);
            } else if (uri.Length == 2) {
                count = 5;
            } else {
                throw new MethodNotFoundException ();
            }
            string text = null;
            switch(uri[1]) {
                case "stats":
                    text = GetRecentMatches (count);
                    break;
                case "best-players":
                    text = GetBestPlayers (count);
                    break;
                case "popular-servers":
                    text = GetPopularServers (count);
                    break;
                case "recent-matches":
                    text = GetRecentMatches (count);
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
