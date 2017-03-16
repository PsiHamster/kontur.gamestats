﻿using System;
using System.Web;
using Fclp;

using Kontur.GameStats.Server.DataBase;
using System.Diagnostics;
using NLog;

namespace Kontur.GameStats.Server
{
    public class EntryPoint {
        private static Logger logger = LogManager.GetCurrentClassLogger ();

        public static void Main(string[] args)
        {
            Console.ReadKey ();
            var db = new DataBase.DataBase ();
            /*
            var FirstMatchPlayed = DateTime.Parse ("2017-01-15T21:51:04.0000000Z");
            var LastMatchTime = db.LastMatchTime;
            Console.WriteLine (LastMatchTime.Date);
            Console.WriteLine (FirstMatchPlayed.Date);
            Console.WriteLine (db.GetServerStatistics ("kappa7806"));
            var a = new Stopwatch ();
            Console.WriteLine (db.GetBestPlayers (50));
            Console.WriteLine (a.ElapsedMilliseconds);
            a.Restart ();
            db.GetServersInfo ();
            Console.WriteLine (a.ElapsedMilliseconds);
            a.Restart ();
            Console.WriteLine (db.GetPopularServers(50));
            Console.WriteLine (a.ElapsedMilliseconds);*/
            logger.Info (string.Format ("Parsing Command Line"));
            var commandLineParser = new FluentCommandLineParser<Options>();

            commandLineParser
                .Setup(options => options.Prefix)
                .As("prefix")
                .SetDefault("http://+:25566/")
                .WithDescription("HTTP prefix to listen on");

            commandLineParser
                .SetupHelp("h", "help")
                .WithHeader($"{AppDomain.CurrentDomain.FriendlyName} [--prefix <prefix>]")
                .Callback(text => Console.WriteLine(text));

            if (commandLineParser.Parse(args).HelpCalled)
                return;

            RunServer(commandLineParser.Object);
        }

        private static void RunServer(Options options)
        {
            using (var server = new StatServer()) {

                logger.Info (string.Format ("Starting Server on {0}", options.Prefix));
                server.Start(options.Prefix);

                Console.ReadKey(true);
            }
        }

        private class Options
        {
            public string Prefix { get; set; }
        }
    }
}
