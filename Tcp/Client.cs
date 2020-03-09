using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Packets;

namespace Tcp
{
    public class Client
    {
        private static readonly Logger Logger = LogManager.GetLogger("TcpClient");
        private readonly IPAddress serverIp;
        private readonly ushort serverPort;
        private readonly PacketsFactory packetsFactory;

        private readonly TcpClient tcpClient;
        private Stream stream;

        private Thread listenThread;

        public event EventHandler<IPacket> PacketReceived;

        public Client(IPAddress serverIp, ushort serverPort, PacketsFactory packetsFactory)
        {
            this.serverIp = serverIp;
            this.serverPort = serverPort;
            packetsFactory.RegisterPacket<PingPacket>();
            this.packetsFactory = packetsFactory;
            tcpClient = new TcpClient();

            PacketReceived += (sender, packet) =>
            {
                if (!(packet is PingPacket p)) return;
                if (p.didBounce) return;

                p.didBounce = true;
                Send(p);
            };
        }

        public async Task Start()
        {
            Logger.Info("Starting clients");
            try
            {
                await tcpClient.ConnectAsync(serverIp, serverPort);

                Logger.Info("Connected to server");
                stream = tcpClient.GetStream();

                listenThread = new Thread(StartListening);
                listenThread.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to connect");
                Logger.Error(ex);
            }
        }

        public void Stop()
        {
            Logger.Info("Stopping");
            tcpClient.Close();
        }

        private void StartListening()
        {
            Logger.Debug("Starting with listening");
            try
            {
                while (tcpClient?.Connected ?? false)
                {
                    if (tcpClient?.Available > 0)
                    {
                        var buffer = new byte[1024];
                        var read = stream.Read(buffer, 0, buffer.Length);
                        foreach (var packet in packetsFactory.GetPackets(buffer, 0, read))
                        {
                            Logger.Debug("Received packet from server: {packet}", packet);
                            PacketReceived?.Invoke(this, packet);
                        }
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromTicks(100));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Got an exceptions while listening to the server");
                Logger.Error(ex);
            }
        }

        public void Send(IPacket packet)
        {
            if (tcpClient == null || !tcpClient.Connected)
            {
                Logger.Warn("Can't send message, not connected to the server");
                return;
            }

            Logger.Debug("Sending packet to server: {packet}", packet);
            try
            {
                var bytes = packet.Serialize();
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send packet to the server");
                Logger.Error(ex);
            }
        }
    }
}