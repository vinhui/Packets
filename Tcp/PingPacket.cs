using System;
using Packets;

namespace Tcp
{
    public struct PingPacket : IPacket
    {
        private static readonly byte[] Header =
        {
            (byte) 'P',
            (byte) '1',
            (byte) 'n',
            (byte) 'G',
            (byte) 'p',
            (byte) '0',
            (byte) 'N',
            (byte) 'g',
        };

        /// <summary>
        /// The UTC time at which it was sent before bouncing
        /// </summary>
        public DateTime SendTime;

        /// <summary>
        /// Did the packet sent and back
        /// </summary>
        public bool didBounce;

        private static int Size => Header.Length + 8 + 1;

        public byte[] Serialize()
        {
            var bytes = new byte[Size];
            PacketUtils.WriteHeader(bytes, 0, Header);

            var timeTicks = SendTime.Ticks;
            PacketUtils.WriteInt64(bytes, Header.Length, timeTicks);

            bytes[Header.Length + 8] = (byte) (didBounce ? 1 : 0);

            return bytes;
        }

        public bool IsMatch(byte[] bytes, int start, int count)
        {
            return count - start >= Size && PacketUtils.MatchesHeader(bytes, start, count, Header);
        }

        public void Deserialize(byte[] bytes, int start, int count, out int used)
        {
            var ticks = PacketUtils.ReadInt64(bytes, start + Header.Length);
            SendTime = new DateTime(ticks);
            didBounce = bytes[Header.Length + 8] != 0;
            used = Size;
        }

        public IPacket Clone()
        {
            return this;
        }

        public override string ToString()
        {
            return $"PingPacket: time: {SendTime}; didBounce: {didBounce}";
        }
    }
}