using System;
using Fclp;
using System.Linq;

namespace Kontur.GameStats.Server
{
    public class EntryPoint
    {
        public static void Main(string[] args)
        {
            HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize ();
            /*
            var commandLineParser = new FluentCommandLineParser<Options>();

            commandLineParser
                .Setup(options => options.Prefix)
                .As("prefix")
                .SetDefault("http://+:8080/")
                .WithDescription("HTTP prefix to listen on");

            commandLineParser
                .SetupHelp("h", "help")
                .WithHeader($"{AppDomain.CurrentDomain.FriendlyName} [--prefix <prefix>]")
                .Callback(text => Console.WriteLine(text));

            if (commandLineParser.Parse(args).HelpCalled)
                return;

            DataBase.DataBaseInitializer.InitializeDataBase ();
            RunServer(commandLineParser.Object);
            */

            var context = new StatisticbaseContext ();

            IQueryable<Server> query = context.Server;
            var servers = query.ToList ();
            foreach(var a in servers) {
                Console.WriteLine (a.ServerId);
            }
        }

        private static void RunServer(Options options)
        {
            using(var server = new StatServer ()) {
                server.Start (options.Prefix);

                Console.ReadKey (true);
            }
        }

        private class Options
        {
            public string Prefix { get; set; }
        }
    }
}
