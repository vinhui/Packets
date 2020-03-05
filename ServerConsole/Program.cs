using System;
using System.Net;
using NLog;
using NLog.Config;
using NLog.Targets;
using Packets;
using Tcp;

namespace ServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget();
            config.AddRuleForAllLevels(consoleTarget);
            LogManager.Configuration = config;

            LogManager.GetLogger("Main").Info("Starting server");

            var factory = new PacketsFactory();

            var server = new Server(IPAddress.Any, 50505, 4, factory);
            server.Start();
            Console.ReadLine();
            server.Stop();
        }
    }
}