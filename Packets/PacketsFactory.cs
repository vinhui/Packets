using System;
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

        public IEnumerable<IPacket> GetPackets(byte[] bytes, int start, int count)
        {
            while (start < count)
            {
                if (TryGetPacket(bytes, start, count, out var used, out var packet))
                {
                    start += used;
                    yield return packet;
                }
                else
                {
                    yield break;
                }
            }
        }
    }
}