using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kontur.GameStats.Server.DataBase;

namespace Kontur.GameStats.Server.ApiMethods {
    public static class Router {
        public static void Route(string[] url, HttpListenerRequest request, HttpListenerResponse response) {
            if(url.Length == 0) {
                response.StatusCode = 400;
                response.Close ();
                return;
            }
            switch(url[0]) {
                case "servers":
                    ServersMethods (url, request, response);
                    break;
                case "players":
                    PlayersMethods (url, request, response);
                    break;
                case "reports":
                    break;
                default:
                    NoSuchMethod (response);
                    break;
            }
            
        }
        private static void NoSuchMethod(HttpListenerResponse response) {
            response.StatusCode = 400;
            response.StatusDescription = "No such method found";
            response.Close ();
        }
        
        private static void WriteResponse(HttpListenerResponse response, string text) {
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

        private static void ServersMethods (string[] url, HttpListenerRequest request, HttpListenerResponse response) {
            if(url.Length < 2) {
                NoSuchMethod (response);
                return;
            }

            string text = null;

            if(url[1] == "info") {
                text = GetMethods.GetServersInfo (request, response);
            } else if(url.Length > 2) {
                switch(url[2]) {
                    case "info":
                        if(request.HttpMethod == "PUT") {
                            PutMethods.PutServerInfo (url[1], request, response);
                        } else {
                            text = GetMethods.GetServerInfo (url[1], request, response);
                        }
                        break;
                    case "stats":
                        text = GetMethods.GetServerStats (url[1], request, response);
                        break;
                    case "matches":
                        if(url.Length > 3)
                            if(request.HttpMethod == "PUT") {
                                PutMethods.PutMatchInfo (url[1], url[3], request, response);
                            } else {
                                text = GetMethods.GetMatchInfo (url[1], url[3], request, response);
                            } else
                            NoSuchMethod (response);
                        break;
                    default:
                        NoSuchMethod (response);
                        break;
                }
            } else {
                NoSuchMethod (response);
            }

            if (text != null) {
                WriteResponse (response, text);
            }
        }

        private static void PlayersMethods(string[] url, HttpListenerRequest request, HttpListenerResponse response) {
            if(url.Length != 3) {
                NoSuchMethod (response);
                return;
            }
            string text = null;
            switch(url[2]) {
                case "stats":
                    text = GetMethods.GetPlayerStats (url[1], request, response);
                    break;
                default:
                    NoSuchMethod (response);
                    break;
            }
            if(text != null) {
                WriteResponse (response, text);
            }
        }
    }
}
