using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;
using Kontur.GameStats.Server.DataBase;

namespace Kontur.GameStats.Server.ApiMethods {
    public static class GetMethods {
        private static Logger logger = LogManager.GetCurrentClassLogger ();

        public static string GetServersInfo(HttpListenerRequest request, HttpListenerResponse response) {
            try {
                var info = db.GetServersInfo ();
                return info;
            } catch(RequestException e) {
                logger.Debug (e);
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusDescription = e.Message;
                return null;
            } catch(Exception e) {
                logger.Error (e);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.StatusDescription = "Error";
                return null;
            }
        }

        public static string GetMatchInfo(string endpoint, string timeStamp, HttpListenerRequest request, HttpListenerResponse response) {
            try {
                var info = db.GetMatchInfo (endpoint, timeStamp);
                return info;
            } catch(RequestException e) {
                logger.Debug (e);
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusDescription = e.Message;
                return null;
            } catch(Exception e) {
                logger.Error (e);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.StatusDescription = "Error";
                return null;
            }
        }

        public static string GetServerStats(string endpoint, HttpListenerRequest request, HttpListenerResponse response) {
            try {
                var info = db.GetServerStatistics (endpoint);
                return info;
            } catch(RequestException e) {
                logger.Debug (e);
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusDescription = e.Message;
                return null;
            } catch(Exception e) {
                logger.Error (e);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.StatusDescription = "Error";
                return null;
            }
        }

        public static string GetServerInfo(string endPoint, HttpListenerRequest request, HttpListenerResponse response) {
            try {
                var info = db.GetServerInfo (endPoint);
                return info;
            } catch(RequestException e) {
                logger.Debug (e);
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusDescription = e.Message;
                return null;
            } catch(Exception e) {
                logger.Error (e);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.StatusDescription = "Error";
                return null;
            }
        }

        public static string GetPlayerStats(string playerName, HttpListenerRequest request, HttpListenerResponse response) {
            try {
                var info = db.GetPlayerStats(playerName);
                return info;
            } catch(RequestException e) {
                logger.Debug (e);
                response.StatusCode = 400;
                response.StatusDescription = e.Message;
                return null;
            } catch(Exception e) {
                logger.Error (e);
                response.StatusCode = 400;
                response.StatusDescription = "Error";
                return null;
            }
        }
    }
}
