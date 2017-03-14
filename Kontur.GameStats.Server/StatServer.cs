﻿using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Kontur.GameStats.Server.ApiMethods;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server
{
    internal class StatServer : IDisposable
    {
        private Router router;
        private DataBase.DataBase database;

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
                if (!isRunning)
                {
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
                        var context = listener.GetContext();
                        Task.Run(() => HandleContextAsync(context));
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    // TODO: log errors
                }
            }
        }

        private async Task HandleContextAsync(HttpListenerContext listenerContext) {
            var url = listenerContext.Request.RawUrl.Substring (1).Split ('/').Where(x => x!="").ToArray();
            var request = listenerContext.Request;
            var response = listenerContext.Response;

            router.Route (url, request, response);
    
            response.Close ();
        }

        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}