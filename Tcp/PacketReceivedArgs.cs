using Packets;

namespace Tcp
{
    public class PacketReceivedArgs
    {
        public IPacket Packet;
        public ServerClient Client;

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