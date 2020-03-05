using System;
using Packets;

namespace Tests
{
    public class TestPacket : IPacket
    {
        private static readonly byte[] Header =
        {
            (byte) 'A',
            (byte) 'B',
            (byte) 'C',
            (byte) 'D',
            (byte) 'E',
        };

        public string Data { get; private set; }

        private int Size => Header.Length + 1 + (Data?.Length ?? 0);

        public TestPacket(string data)
        {
            Data = data;
        }

        public TestPacket()
        {
        }

        public byte[] Serialize()
        {
            if (Data?.Length > 255)
            {
                throw new ArgumentException("Data is too long", nameof(Data));
            }

            var bytes = new byte[Size];

            PacketUtils.WriteHeader(bytes, 0, Header);
            bytes[Header.Length] = (byte) (Data?.Length ?? 0);
            PacketUtils.WriteString(bytes, Header.Length + 1, Data);

            return bytes;
        }

        public bool IsMatch(byte[] bytes, int start, int count)
        {
            if (count - start < Header.Length + 1)
            {
                return false;
            }

            if (!PacketUtils.MatchesHeader(bytes, start, count, Header))
            {
                return false;
            }

            return true;
        }

        public void Deserialize(byte[] bytes, int start, int count, out int used)
        {
            var dataLength = bytes[Header.Length + start];
            Data = "";
            for (var i = 0; i < dataLength; i++)
            {
                Data += (char) bytes[start + Header.Length + 1 + i];
            }

            used = Size;
        }

        public IPacket Clone()
        {
            var clone = new TestPacket(Data);
            return clone;
        }

        public override string ToString()
        {
            return $"TestPacket: {Data}";
        }
    }
}