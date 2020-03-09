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

            var logger = LogManager.GetLogger("Main");
            logger.Info("Starting server");
            var fileTransfer = new FileTransfer();
            fileTransfer.FileReceived += (sender, stream) => logger.Info("Received file, saved to {path}", stream.Name);

            var factory = new PacketsFactory();
            factory.RegisterPacket<ChunkedDataPacket>();

            var server = new Server(IPAddress.Any, 50505, 4, factory);
            server.PacketReceived += (sender, packetArgs) => fileTransfer.OnPacketReceived(packetArgs.Client.EndPoint, packetArgs.Packet);
            server.Start();
            Console.ReadLine();
            server.Stop();
        }
    }
}