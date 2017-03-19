using Kontur.GameStats.Server.ApiMethods;
using NLog;
using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace Kontur.GameStats.Server {
    internal class StatServer : IDisposable {
        #region Fields

        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;

        private Router router;
        private DataBase.DataBase database;
        private string prefix;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();

        #endregion

        public StatServer() {
            database = new DataBase.DataBase ();
            router = new Router (database);

            listener = new HttpListener ();
        }

        #region Commands

        public void Start(string prefix) {
            Console.WriteLine ("Started");
            lock(listener) {
                if(!isRunning) {
                    this.prefix = prefix;
                    listener.Prefixes.Clear ();
                    listener.Prefixes.Add (prefix);
                    listener.Start ();

                    listenerThread = new Thread (Listen) {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    listenerThread.Start ();

                    isRunning = true;
                }
            }
        }

        public void Stop() {
            lock(listener) {
                if(!isRunning)
                    return;

                listener.Stop ();

                listenerThread.Abort ();
                listenerThread.Join ();

                isRunning = false;
            }
        }

        public void Dispose() {
            if(disposed)
                return;

            disposed = true;

            Stop ();

            listener.Close ();
        }

        #endregion

        #region Listener

        private void Listen() {
            while(true) {
                try {
                    if(listener.IsListening) {
                        ThreadPool.QueueUserWorkItem (Process, listener.GetContext ());
                        //var context = listener.GetContext ();
                        //Task.Run(() => Process(context));
                    } else Thread.Sleep (0);
                } catch(ThreadAbortException e) {
                    logger.Error (e);
                    return;
                } catch(Exception e) {
                    logger.Error (e);
                }
            }
        }
        private void Process(object o) {
            var listenerContext = o as HttpListenerContext;
            var uri = listenerContext.Request.Url.LocalPath.Split ('/').Skip (prefix.Count (x => x == '/') - 2).ToArray ();
            var request = listenerContext.Request;
            var response = listenerContext.Response;

            try {
                router.Route (uri, request, response);
            } catch(Exception e) {
                logger.Error (e);
            }
        }

        #endregion
        
    }
}