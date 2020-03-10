using System.Collections;
using System.Collections.Generic;

namespace Packets
{
    public class PacketsFactory
    {
        private readonly HashSet<IPacket> registeredTypes = new HashSet<IPacket>();

        /// <summary>
        /// Register a new type of packet that can be received
        /// </summary>
        /// <typeparam name="T">Type of the packet</typeparam>
        public void RegisterPacket<T>()
            where T : IPacket, new()
        {
            registeredTypes.Add(new T());
        }

        /// <summary>
        /// Attempt to get a packet from the given buffer
        /// </summary>
        /// <param name="bytes">The buffer to use</param>
        /// <param name="start">The index from which to start reading from the buffer</param>
        /// <param name="count">The total amount of bytes in the buffer</param>
        /// <param name="used">The amount of bytes it used to get a packet</param>
        /// <param name="packet">The deserialized packet if there was a match</param>
        /// <returns>Returns if there was a packet deserialized or not</returns>
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

        /// <summary>
        /// Get a set of packets that are in the given buffer
        /// </summary>
        /// <param name="bytes">The buffer to use</param>
        /// <param name="start">The index from which to start reading</param>
        /// <param name="count">The total amount of bytes in the buffer</param>
        /// <returns>Returns a collection of packets and how many bytes it used in total</returns>
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

            /// <summary>
            /// Amount of packets in this collection
            /// </summary>
            public int Count => Packets.Count;

            /// <summary>
            /// The amount of bytes used in total
            /// </summary>
            public int BytesUsed { get; internal set; }

            /// <summary>
            /// Get a packet by index
            /// </summary>
            /// <param name="index">Index to look at</param>
            public IPacket this[int index] => Packets[index];

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