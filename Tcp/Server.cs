using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Packets;

namespace Tcp
{
    public class Server
    {
        private static readonly Logger Logger = LogManager.GetLogger("TcpServer");

        private TcpListener listener;
        private ServerClient[] clients;

        /// <summary>
        /// Get all the connected clients
        /// </summary>
        public IEnumerable<ServerClient> ConnectedClients => clients.Where(x => x.IsConnected);

        /// <summary>
        /// The maximum allowed clients to connect to this server
        /// </summary>
        public uint MaxClients { get; }

        private Thread connectionsThread;
        private bool isListening;
        private Thread[] listenThreads;
        private PacketsFactory packetsFactory;

        /// <summary>
        /// Interval at which to send a ping to all the clients
        /// Set to lower than 1 to disable sending pings
        /// </summary>
        public int PingIntervalMs { get; set; } = 5000;

        private Thread pingThread;

        /// <summary>
        /// Buffer size in bytes for receiving packets
        /// </summary>
        /// <remarks>This is a buffer that is per client, thus having <see cref="MaxClients"/> connections would result in this value x32 of memory used</remarks>
        public int RxBufferSize { get; set; } = 1024;

        private bool IsRunning => isListening && (listener?.Server?.IsBound ?? false);

        /// <summary>
        /// Fired whenever a packet was received
        /// </summary>
        public event EventHandler<PacketReceivedArgs> PacketReceived;

        /// <summary>
        /// Initialize a new server instance
        /// </summary>
        /// <param name="bindIp">IP to bind to, easiest is <see cref="IPAddress.Any"/></param>
        /// <param name="port">The port to bind to</param>
        /// <param name="maxClients">The maximum amount of clients that are allowed to connect</param>
        /// <param name="packetsFactory">The factory responsible for parsing received data</param>
        public Server(IPAddress bindIp, ushort port, uint maxClients, PacketsFactory packetsFactory)
        {
            listener = new TcpListener(bindIp, port);
            clients = new ServerClient[maxClients];
            for (var i = 0; i < clients.Length; i++)
                clients[i] = new ServerClient();
            MaxClients = maxClients;
            packetsFactory.RegisterPacket<PingPacket>();
            this.packetsFactory = packetsFactory;

            PacketReceived += async (sender, args) =>
            {
                if (!(args.Packet is PingPacket packet)) return;
                if (packet.didBounce)
                {
                    var diff = DateTime.UtcNow - packet.SendTime;
                    args.Client.Ping = (int) diff.TotalMilliseconds;
                    Logger.Debug("Ping to client {client}: {ping}ms", args.Client.EndPoint, args.Client.Ping);
                }
                else
                {
                    packet.didBounce = true;
                    await SendAsync(packet, args.Client);
                }
            };
        }

        /// <summary>
        /// Actually start the server
        /// </summary>
        public void Start()
        {
            Logger.Info("Starting server");
            listener.Start();

            Logger.Info("Waiting for clients");
            isListening = true;
            listenThreads = new Thread[MaxClients];

            connectionsThread = new Thread(WaitForConnections);
            connectionsThread.Start();

            if (PingIntervalMs > 0)
            {
                pingThread = new Thread(async () =>
                {
                    while (listener.Server.IsBound)
                    {
                        Thread.Sleep(PingIntervalMs);
                        if (clients.Any(x => x.IsConnected))
                        {
                            Logger.Debug("Sending ping to all connected clients");
                            await SendToAllAsync(new PingPacket {SendTime = DateTime.UtcNow});
                        }
                    }
                });
                pingThread.Start();
            }
        }

        private void WaitForConnections()
        {
            while (IsRunning)
            {
                if (listener.Pending())
                {
                    Logger.Debug("Got a pending connection");
                    var client = listener.AcceptTcpClient();
                    var foundSlot = false;
                    for (var i = 0; i < clients.Length; i++)
                    {
                        if (clients[i].IsConnected) continue;

                        foundSlot = true;
                        var index = i;
                        clients[i].Tcp = client;
                        clients[i].EndPoint = client.Client.RemoteEndPoint;
                        clients[i].Stream = client.GetStream();
                        Logger.Info("Client connected from {ip} on slot {index}", clients[i].EndPoint, index);
                        listenThreads[i] = new Thread(() => ListenToClient(index, clients[index]));
                        listenThreads[i].Start();
                        break;
                    }

                    if (!foundSlot)
                    {
                        Logger.Debug("There are no open slots for the pending connection");
                        client.GetStream().Close();
                        client.Close();
                    }
                }

                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Stop the running server
        /// </summary>
        public void Stop()
        {
            Logger.Info("Stopping");
            listener.Stop();
            isListening = false;
        }

        private void ListenToClient(int index, ServerClient client)
        {
            Logger.Debug("Starting with listening to client on slot {index}", index);
            clients[index].IsListening = true;

            try
            {
                var buffer = new byte[RxBufferSize];
                var bufferOffset = 0;
                while (client.IsConnected)
                {
                    if (client.Tcp?.Available >= 0)
                    {
                        var read = client.Stream.Read(buffer, bufferOffset, buffer.Length - bufferOffset);
                        var packetCollection = packetsFactory.GetPackets(buffer, 0, read + bufferOffset);
                        foreach (var packet in packetCollection)
                        {
                            Logger.Debug("Received packet from {client}: {packet}", client.EndPoint, packet);
                            PacketReceived?.Invoke(this, new PacketReceivedArgs(packet, client));
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
            }
            catch (Exception ex)
            {
                Logger.Error("Got an exception while listening to client {client}, disconnecting", client.EndPoint);
                Logger.Error(ex);
            }

            clients[index].IsListening = false;
            clients[index].Ping = -1;
            clients[index].Tcp?.Close();
            clients[index].Tcp = null;

            Logger.Info("Client {client} on slot {slot} disconnected", client.EndPoint, index);
        }

        private async Task<bool> SendAsync(byte[] bytes, ServerClient client)
        {
            try
            {
                var stream = client.Stream;
                await stream.WriteAsync(bytes, 0, bytes.Length);
                await stream.FlushAsync();
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send packet to {client}", client.EndPoint);
                Logger.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Send a packet to a specific client
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="client">Client to send the packet to</param>
        /// <returns>Returns a task that completes when the packet is sent with true if successful</returns>
        public Task<bool> SendAsync(IPacket packet, ServerClient client)
        {
            if (!IsRunning)
            {
                Logger.Warn("The server is not running, not sending packet");
                return Task.FromResult(false);
            }

            Logger.Debug("Sending packet to {client}: ", client.EndPoint, packet);
            return SendAsync(packet.Serialize(), client);
        }

        /// <summary>
        /// Send a packet to all the connected clients
        /// </summary>
        /// <param name="packet">Packet to send to everyone</param>
        /// <returns>Returns a task that you can wait on with a dictionary containing success per client</returns>
        /// <remarks>This uses more memory/processing than the very similar <see cref="SendToAllAsync"/>. Use that if you don't care about the results</remarks>
        public async Task<Dictionary<ServerClient, bool>> SendToAllWithResultAsync(IPacket packet)
        {
            if (!IsRunning)
            {
                Logger.Warn("The server is not running, not sending packet");
                return null;
            }

            var bytes = packet.Serialize();
            var connectedClients1 = ConnectedClients.ToArray();
            var tasks = new Task<bool>[connectedClients1.Length];
            for (var i = 0; i < connectedClients1.Length; i++)
            {
                Logger.Debug("Sending packet to {client}: {packet}", clients[i].EndPoint, packet);
                tasks[i] = SendAsync(bytes, clients[i]);
            }

            var connectedClients = connectedClients1;

            var result = await Task.WhenAll(tasks);
            return connectedClients
                .Zip(result, (k, v) => new {k, v})
                .ToDictionary(x => x.k, x => x.v);
        }

        /// <summary>
        /// Send a packet to all the connected clients
        /// </summary>
        /// <param name="packet">Packet to send to everyone</param>
        /// <returns>Returns a task that completes when the package is sent to all</returns>
        public async Task SendToAllAsync(IPacket packet)
        {
            if (!IsRunning)
            {
                Logger.Warn("The server is not running, not sending packet");
                return;
            }

            var bytes = packet.Serialize();
            var tasks = new Task<bool>[clients.Length];
            for (var i = 0; i < clients.Length; i++)
            {
                if (!clients[i].IsConnected)
                {
                    tasks[i] = Task.FromResult(true);
                    break;
                }

                Logger.Debug("Sending packet to {client}: {packet}", clients[i].EndPoint, packet);
                tasks[i] = SendAsync(bytes, clients[i]);
            }

            await Task.WhenAll(tasks);
        }
    }
}