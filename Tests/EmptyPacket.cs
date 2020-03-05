using Packets;

namespace Tests
{
    public struct EmptyPacket : IPacket
    {
        private static readonly byte[] Header =
        {
            1, 2, 3, 4, 5
        };

        public byte[] Serialize()
        {
            var bytes = new byte[Header.Length];
            PacketUtils.WriteHeader(bytes, 0, Header);
            return bytes;
        }

        public bool IsMatch(byte[] bytes, int start, int count)
        {
            return PacketUtils.MatchesHeader(bytes, start, count, Header);
        }

        public void Deserialize(byte[] bytes, int start, int count, out int used)
        {
            used = Header.Length;
        }

        public IPacket Clone()
        {
            return this;
        }

        public override string ToString()
        {
            return "EmptyPacket";
        }
    }
}