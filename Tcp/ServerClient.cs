using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Tcp
{
    public class ServerClient
    {
        internal TcpClient Tcp;
        public bool IsListening { get; internal set; }
        public bool IsConnected => Tcp?.Connected ?? false;
        public int Ping { get; internal set; }

        public EndPoint EndPoint { get; internal set; }

        internal Stream Stream;

        public ServerClient()
        {
            Ping = -1;
        }
    }
}