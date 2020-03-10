using Packets;

namespace Tcp
{
    public class PacketReceivedArgs
    {
        /// <summary>
        /// The packet that was received
        /// </summary>
        public readonly IPacket Packet;

        /// <summary>
        /// The client from whom it came
        /// </summary>
        public readonly ServerClient Client;

        public PacketReceivedArgs(IPacket packet, ServerClient client)
        {
            Packet = packet;
            Client = client;
        }

        public PacketReceivedArgs()
        {
        }
    }
}