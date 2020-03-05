using System;
using System.Net;
using NLog;
using NLog.Config;
using NLog.Targets;
using Packets;
using Tcp;

namespace ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget();
            config.AddRuleForAllLevels(consoleTarget);
            LogManager.Configuration = config;

            LogManager.GetLogger("Main").Info("Starting client");

            var factory = new PacketsFactory();

            var client = new Client(IPAddress.Parse("127.0.0.1"), 50505, factory);
            client.Start();
            Console.ReadLine();
            client.Stop();
        }
    }
}