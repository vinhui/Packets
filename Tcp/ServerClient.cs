using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Tcp
{
    public class ServerClient
    {
        public TcpClient Tcp;
        public bool IsListening;
        public bool IsConnected => Tcp?.Connected ?? false;
        public int Ping;

        public EndPoint EndPoint;

        internal Stream Stream;

        public ServerClient()
        {
            Ping = -1;
        }
    }
}