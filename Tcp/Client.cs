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
        private readonly int serverPort;
        private readonly PacketsFactory packetsFactory;

        private readonly TcpClient tcpClient;
        private Stream stream;

        private Thread listenThread;

        public EventHandler FailedToConnect;
        public EventHandler Disconnected;

        public event EventHandler<IPacket> PacketReceived;

        public int RxBufferSize { get; set; } = 1024;

        public Client(IPEndPoint endPoint, PacketsFactory packetsFactory)
            : this(endPoint.Address, endPoint.Port, packetsFactory)
        {
        }

        public Client(IPAddress serverIp, int serverPort, PacketsFactory packetsFactory)
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
                FailedToConnect?.Invoke(this, EventArgs.Empty);
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
                var buffer = new byte[RxBufferSize];
                var bufferOffset = 0;
                while (tcpClient?.Connected ?? false)
                {
                    if (tcpClient?.Available > 0)
                    {
                        var read = stream.Read(buffer, bufferOffset, buffer.Length - bufferOffset);
                        var packetCollection = packetsFactory.GetPackets(buffer, 0, read + bufferOffset);
                        foreach (var packet in packetCollection)
                        {
                            Logger.Debug("Received packet from server: {packet}", packet);
                            PacketReceived?.Invoke(this, packet);
                        }

                        var leftover = read + bufferOffset - packetCollection.BytesUsed;
                        if (leftover > 0)
                        {
                            Array.Copy(buffer, packetCollection.BytesUsed, buffer, 0, leftover);
                            bufferOffset = leftover;
                        }
                        else
                            bufferOffset = 0;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromTicks(100));
                    }
                }

                stream = null;
                if (tcpClient != null && tcpClient.Connected)
                    tcpClient.Close();
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.Error("Got an exceptions while listening to the server");
                Logger.Error(ex);
                Logger.Warn("Disconnecting");
                stream?.Close();
                stream = null;
                if (tcpClient != null && tcpClient.Connected)
                {
                    tcpClient.Close();
                    Disconnected?.Invoke(this, EventArgs.Empty);
                }
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