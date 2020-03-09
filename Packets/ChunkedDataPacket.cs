using System;

namespace Packets
{
    public class ChunkedDataPacket : IPacket
    {
        private static readonly byte[] Header =
        {
            (byte) 'c',
            (byte) 'H',
            (byte) '0',
            (byte) 'n',
            (byte) 'k',
            (byte) 'y',
            (byte) 'b',
            (byte) '0',
            (byte) '1',
        };

        public ulong UniqueId;
        public uint Offset;
        public uint TotalChunks;
        public byte[] Data;
        public int DataLength;

        public byte[] Serialize()
        {
            var bytes = new byte[Header.Length + 8 + 4 + 4 + 4 + Data.Length]; // Header + id + index + offset + total chunks + data length + data
            var offset = 0;
            PacketUtils.WriteHeader(bytes, 0, Header);
            offset += Header.Length;
            PacketUtils.WriteUInt64(bytes, offset, UniqueId);
            offset += 8;
            PacketUtils.WriteUInt32(bytes, offset, Offset);
            offset += 4;
            PacketUtils.WriteUInt32(bytes, offset, TotalChunks);
            offset += 4;
            PacketUtils.WriteInt32(bytes, offset, DataLength);
            offset += 4;
            Array.Copy(Data, 0, bytes, offset, DataLength);
            return bytes;
        }

        public bool IsMatch(byte[] bytes, int start, int count)
        {
            if (!PacketUtils.MatchesHeader(bytes, start, count, Header))
                return false;

            if (count - start < Header.Length + 8 + 4 + 4 + 4)
                return false;

            DataLength = PacketUtils.ReadInt32(bytes, start + Header.Length + 8 + 4 + 4);
            if (count - start < Header.Length + 8 + 4 + 4 + 4 + DataLength)
                return false;

            return true;
        }

        public void Deserialize(byte[] bytes, int start, int count, out int used)
        {
            var offset = start + Header.Length;
            UniqueId = PacketUtils.ReadUInt64(bytes, offset);
            offset += 8;
            Offset = PacketUtils.ReadUInt32(bytes, offset);
            offset += 4;
            TotalChunks = PacketUtils.ReadUInt32(bytes, offset);
            offset += 4;
            DataLength = PacketUtils.ReadInt32(bytes, offset);
            offset += 4;
            Data = new byte[DataLength];
            Array.Copy(bytes, offset, Data, 0, DataLength);
            offset += DataLength;

            used = offset - start;
        }

        public IPacket Clone()
        {
            var packet = new ChunkedDataPacket
            {
                UniqueId = UniqueId,
                Offset = Offset,
                TotalChunks = TotalChunks,
                Data = new byte[Data?.Length ?? 0],
                DataLength = DataLength
            };
            if (Data != null) 
                Array.Copy(Data, packet.Data, Data.Length);
            return packet;
        }

        public override string ToString()
        {
            return $"ChunkedDataPacket: offset: {Offset}";
        }
    }
}