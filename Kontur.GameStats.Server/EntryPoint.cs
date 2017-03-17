using System;
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
