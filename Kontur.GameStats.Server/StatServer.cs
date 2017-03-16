using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Kontur.GameStats.Server.ApiMethods;
using NLog;

namespace Kontur.GameStats.Server
{
    internal class StatServer : IDisposable
    {
        private Router router;
        private DataBase.DataBase database;
        private string prefix;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger ();

        public StatServer(string databaseName, bool deletePrev)
        {
            database = new DataBase.DataBase (databaseName, deletePrev);
            router = new Router (database);

            listener = new HttpListener();
        }

        public StatServer() {
            database = new DataBase.DataBase ();
            router = new Router (database);

            listener = new HttpListener ();
        }
        
        public void Start(string prefix)
        {
            lock (listener)
            {
                if (!isRunning) {
                    this.prefix = prefix;
                    listener.Prefixes.Clear();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    listenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    listenerThread.Start();
                    
                    isRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;

                listener.Stop();

                listenerThread.Abort();
                listenerThread.Join();
                
                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            listener.Close();
        }
        
        private void Listen()
        {
            while (true)
            {
                try
                {
                    if (listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem (Process, listener.GetContext());
                        //var context = listener.GetContext ();
                        //Task.Run(() => HandleContextAsync(context));
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException e) {
                    logger.Error (e);
                    return;
                }
                catch (Exception e) {
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
            } catch (Exception e) {
                logger.Error (e);
            }
        }

        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}