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
        private Thread pingThread;

        /// <summary>
        /// Fires when it failed to connect to the server
        /// </summary>
        public event EventHandler FailedToConnect;

        /// <summary>
        /// Fires if we got disconnected from the server
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Fired when a packet was received
        /// </summary>
        public event EventHandler<IPacket> PacketReceived;

        /// <summary>
        /// Buffer size in bytes for receiving packets
        /// </summary>
        /// <remarks>Make sure packets will fit in this buffer in their entirety</remarks>
        public int RxBufferSize { get; set; } = 1024;

        /// <summary>
        /// Ping time to the server in ms
        /// </summary>
        public int Ping { get; private set; } = -1;

        /// <summary>
        /// Interval at which to send a ping to the server
        /// Set to lower than 1 to disable sending pings
        /// </summary>
        public int PingIntervalMs { get; set; } = 5000;

        /// <summary>
        /// Initialize a new connection with the given server
        /// </summary>
        /// <param name="endPoint">Server to connect to</param>
        /// <param name="packetsFactory">The factory responsible for parsing received data</param>
        public Client(IPEndPoint endPoint, PacketsFactory packetsFactory)
            : this(endPoint.Address, endPoint.Port, packetsFactory)
        {
        }

        /// <summary>
        /// Initialize a new connection with the given server
        /// </summary>
        /// <param name="serverIp">IP of the server to connect to</param>
        /// <param name="serverPort">Port that the server is bound to</param>
        /// <param name="packetsFactory">The factory responsible for parsing received data</param>
        public Client(IPAddress serverIp, int serverPort, PacketsFactory packetsFactory)
        {
            this.serverIp = serverIp;
            this.serverPort = serverPort;
            packetsFactory.RegisterPacket<PingPacket>();
            this.packetsFactory = packetsFactory;
            tcpClient = new TcpClient();

            PacketReceived += (sender, args) =>
            {
                if (!(args is PingPacket packet)) return;
                if (packet.didBounce)
                {
                    var diff = DateTime.UtcNow - packet.SendTime;
                    Ping = (int) diff.TotalMilliseconds;
                    Logger.Debug("Ping to server: {ping}ms", Ping);
                }
                else
                {
                    packet.didBounce = true;
                    Send(packet);
                }
            };
        }

        /// <summary>
        /// Start connecting to the server
        /// </summary>
        /// <returns>Returns a task that you can wait on</returns>
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

                if (PingIntervalMs > 0)
                {
                    pingThread = new Thread(async () =>
                    {
                        while (tcpClient?.Connected ?? false)
                        {
                            Logger.Debug("Sending ping to server");
                            Send(new PingPacket {SendTime = DateTime.UtcNow});

                            Thread.Sleep(PingIntervalMs);
                        }
                    });
                    pingThread.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to connect");
                Logger.Error(ex);
                FailedToConnect?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Stop the connection with the server
        /// </summary>
        public void Stop()
        {
            Logger.Info("Stopping");
            stream?.Close();
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

        /// <summary>
        /// Send a packet to the server
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <remarks>The send method is not async on purpose because we don't want to get sent packets mangled</remarks>
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