using System;
using System.IO;
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

            var logger = LogManager.GetLogger("Main");
            logger.Info("Starting client");
            var fileTransfer = new FileTransfer();

            var factory = new PacketsFactory();
            factory.RegisterPacket<ChunkedDataPacket>();

            var server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50505);
            var client = new Client(server, factory);
            client.PacketReceived += (sender, packet) => fileTransfer.OnPacketReceived(server, packet);
            client.FailedToConnect += (sender, eventArgs) => client.Start();
            client.Disconnected += (sender, eventArgs) => client.Start();
            client.Start();
            Console.ReadLine();
            using (var fileStream = new FileStream("img.jpg", FileMode.Open))
            {
                fileTransfer.SendFile(fileStream, client.Send);
            }

            Console.ReadLine();
            client.Stop();
        }
    }
}