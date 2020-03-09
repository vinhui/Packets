using System;
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
        public uint MaxClients { get; }

        private Thread connectionsThread;
        private bool isListening;
        private Thread[] listenThreads;
        private PacketsFactory packetsFactory;

        public int PingIntervalMs { get; set; } = 5000;
        private Thread pingThread;

        private bool IsRunning => isListening && (listener?.Server?.IsBound ?? false);

        public event EventHandler<PacketReceivedArgs> PacketReceived;

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
                    await Send(packet, args.Client);
                }
            };
        }

        public void Start()
        {
            Logger.Info("Starting server");
            listener.Start();

            Logger.Info("Waiting for clients");
            isListening = true;
            listenThreads = new Thread[MaxClients];

            connectionsThread = new Thread(WaitForConnections);
            connectionsThread.Start();

            pingThread = new Thread(async () =>
            {
                while (listener.Server.IsBound)
                {
                    Thread.Sleep(PingIntervalMs);
                    if (clients.Any(x => x.IsConnected))
                    {
                        Logger.Debug("Sending ping to all connected clients");
                        await SendToAll(new PingPacket {SendTime = DateTime.UtcNow});
                    }
                }
            });
            pingThread.Start();
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
                var buffer = new byte[1024];
                while (client.IsConnected)
                {
                    if (client.Tcp?.Available >= 0)
                    {
                        var read = client.Stream.Read(buffer, 0, buffer.Length);
                        foreach (var packet in packetsFactory.GetPackets(buffer, 0, read))
                        {
                            Logger.Debug("Received packet from {client}: {packet}", client.EndPoint, packet);
                            PacketReceived?.Invoke(this, new PacketReceivedArgs(packet, client));
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
                Logger.Error("Got an exception while listening to client {client}, disconnecting", client.EndPoint);
                Logger.Error(ex);
            }

            clients[index].IsListening = false;
            clients[index].Ping = -1;
            clients[index].Tcp?.Close();
            clients[index].Tcp = null;

            Logger.Info("Client {client} on slot {slot} disconnected", client.EndPoint, index);
        }

        private async Task Send(byte[] bytes, ServerClient client)
        {
            try
            {
                var stream = client.Stream;
                await stream.WriteAsync(bytes, 0, bytes.Length);
                await stream.FlushAsync();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send packet to {client}", client.EndPoint);
                Logger.Error(ex);
            }
        }

        private Task Send(IPacket packet, ServerClient client)
        {
            if (!IsRunning)
            {
                Logger.Warn("The server is not running, not sending packet");
                return Task.CompletedTask;
            }

            Logger.Debug("Sending packet to {client}: ", client.EndPoint, packet);
            return Send(packet.Serialize(), client);
        }

        private async Task SendToAll(IPacket packet)
        {
            if (!IsRunning)
            {
                Logger.Warn("The server is not running, not sending packet");
                return;
            }

            var bytes = packet.Serialize();
            var tasks = new Task[clients.Length];
            for (var i = 0; i < clients.Length; i++)
            {
                if (!clients[i].IsConnected)
                {
                    tasks[i] = Task.CompletedTask;
                    continue;
                }

                Logger.Debug("Sending packet to {client}: {packet}", clients[i].EndPoint, packet);
                tasks[i] = Send(bytes, clients[i]);
            }

            await Task.WhenAll(tasks);
        }
    }
}