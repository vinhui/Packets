using System.Collections;
using System.Collections.Generic;

namespace Packets
{
    public class PacketsFactory
    {
        private readonly HashSet<IPacket> registeredTypes = new HashSet<IPacket>();

        public void RegisterPacket<T>()
            where T : IPacket, new()
        {
            registeredTypes.Add(new T());
        }

        public bool TryGetPacket(byte[] bytes, int start, int count, out int used, out IPacket packet)
        {
            foreach (var type in registeredTypes)
            {
                if (!type.IsMatch(bytes, start, count))
                    continue;

                packet = type.Clone();
                packet.Deserialize(bytes, start, count, out used);
                return true;
            }

            used = 0;
            packet = default;
            return false;
        }

        public PacketCollection GetPackets(byte[] bytes, int start, int count)
        {
            var collection = new PacketCollection();
            while (start < count)
            {
                if (TryGetPacket(bytes, start, count, out var used, out var packet))
                {
                    collection.Packets.Add(packet);
                    start += used;
                }
                else
                {
                    break;
                }
            }

            collection.BytesUsed = start;
            return collection;
        }

        public class PacketCollection : IEnumerable<IPacket>
        {
            internal readonly List<IPacket> Packets = new List<IPacket>();
            public int BytesUsed;

            public IEnumerator<IPacket> GetEnumerator()
            {
                return Packets.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return Packets.GetEnumerator();
            }
        }
    }
}