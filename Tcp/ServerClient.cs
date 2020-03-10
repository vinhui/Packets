using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Tcp
{
    public class ServerClient
    {
        internal TcpClient Tcp;

        /// <summary>
        /// Is the client being listened to
        /// </summary>
        public bool IsListening { get; internal set; }

        /// <summary>
        /// Is the client connected
        /// </summary>
        public bool IsConnected => Tcp?.Connected ?? false;

        /// <summary>
        /// Ping to the client
        /// </summary>
        /// <remarks>Is -1 if the client is not connected</remarks>
        public int Ping { get; internal set; }

        /// <summary>
        /// Endpoint of the client
        /// </summary>
        public EndPoint EndPoint { get; internal set; }

        internal Stream Stream;

        public ServerClient()
        {
            Ping = -1;
        }
    }
}