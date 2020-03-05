using System;
using System.Runtime.InteropServices;

namespace Packets
{
    public static class PacketUtils
    {
        public static bool MatchesHeader(byte[] bytes, int start, int count, byte[] header)
        {
            if (header.Length > count - start)
                return false;

            for (var i = 0; i < header.Length; i++)
            {
                var b = bytes[i + start];
                if (b != header[i])
                    return false;
            }

            return true;
        }

        public static void WriteHeader(byte[] bytes, int start, byte[] header)
        {
            if (header.Length > bytes.Length - start)
                throw new ArgumentException("Header does not fit in bytes", nameof(header));

            for (var i = 0; i < header.Length; i++)
                bytes[i + start] = header[i];
        }

        public static void WriteString(byte[] bytes, int start, string text)
        {
            if (text == null)
                return;

            if (text.Length > bytes.Length - start)
                throw new ArgumentException("Text does not fit in bytes", nameof(text));

            for (var i = 0; i < text.Length; i++)
                bytes[i + start] = (byte) text[i];
        }

        public static void WriteInt64(byte[] bytes, int start, long value)
        {
            if (8 > bytes.Length - start)
                throw new ArgumentException("The value does not fit in bytes", nameof(value));

            var b = new Int64Bytes {Int64 = value};
            bytes[start + 0] = b.B0;
            bytes[start + 1] = b.B1;
            bytes[start + 2] = b.B2;
            bytes[start + 3] = b.B3;
            bytes[start + 4] = b.B4;
            bytes[start + 5] = b.B5;
            bytes[start + 6] = b.B6;
            bytes[start + 7] = b.B7;
        }

        public static long ReadInt64(byte[] bytes, int start)
        {
            if (8 > bytes.Length - start)
                return default;

            var b = new Int64Bytes
            {
                B0 = bytes[start + 0],
                B1 = bytes[start + 1],
                B2 = bytes[start + 2],
                B3 = bytes[start + 3],
                B4 = bytes[start + 4],
                B5 = bytes[start + 5],
                B6 = bytes[start + 6],
                B7 = bytes[start + 7],
            };
            return b.Int64;
        }

        public static void WriteUInt64(byte[] bytes, int start, ulong value)
        {
            if (8 > bytes.Length - start)
                throw new ArgumentException("The value does not fit in bytes", nameof(value));

            var b = new Int64Bytes {UInt64 = value};
            bytes[start + 0] = b.B0;
            bytes[start + 1] = b.B1;
            bytes[start + 2] = b.B2;
            bytes[start + 3] = b.B3;
            bytes[start + 4] = b.B4;
            bytes[start + 5] = b.B5;
            bytes[start + 6] = b.B6;
            bytes[start + 7] = b.B7;
        }

        public static ulong ReadUInt64(byte[] bytes, int start)
        {
            if (8 > bytes.Length - start)
                return default;

            var b = new Int64Bytes
            {
                B0 = bytes[start + 0],
                B1 = bytes[start + 1],
                B2 = bytes[start + 2],
                B3 = bytes[start + 3],
                B4 = bytes[start + 4],
                B5 = bytes[start + 5],
                B6 = bytes[start + 6],
                B7 = bytes[start + 7],
            };
            return b.UInt64;
        }

        public static void WriteInt32(byte[] bytes, int start, int value)
        {
            if (4 > bytes.Length - start)
                throw new ArgumentException("The value does not fit in bytes", nameof(value));

            var b = new Int32Bytes {Int32 = value};
            bytes[start + 0] = b.B0;
            bytes[start + 1] = b.B1;
            bytes[start + 2] = b.B2;
            bytes[start + 3] = b.B3;
        }

        public static int ReadInt32(byte[] bytes, int start)
        {
            if (4 > bytes.Length - start)
                return default;

            var b = new Int32Bytes
            {
                B0 = bytes[start + 0],
                B1 = bytes[start + 1],
                B2 = bytes[start + 2],
                B3 = bytes[start + 3],
            };
            return b.Int32;
        }

        public static void WriteUInt32(byte[] bytes, int start, uint value)
        {
            if (4 > bytes.Length - start)
                throw new ArgumentException("The value does not fit in bytes", nameof(value));

            var b = new Int32Bytes {UInt32 = value};
            bytes[start + 0] = b.B0;
            bytes[start + 1] = b.B1;
            bytes[start + 2] = b.B2;
            bytes[start + 3] = b.B3;
        }

        public static uint ReadUInt32(byte[] bytes, int start)
        {
            if (4 > bytes.Length - start)
                return default;

            var b = new Int32Bytes
            {
                B0 = bytes[start + 0],
                B1 = bytes[start + 1],
                B2 = bytes[start + 2],
                B3 = bytes[start + 3],
            };
            return b.UInt32;
        }

        public static void WriteInt16(byte[] bytes, int start, short value)
        {
            if (2 > bytes.Length - start)
                throw new ArgumentException("The value does not fit in bytes", nameof(value));

            var b = new Int16Bytes {Int16 = value};
            bytes[start + 0] = b.B0;
            bytes[start + 1] = b.B1;
        }

        public static short ReadInt16(byte[] bytes, int start)
        {
            if (2 > bytes.Length - start)
                return default;

            var b = new Int16Bytes
            {
                B0 = bytes[start + 0],
                B1 = bytes[start + 1],
            };
            return b.Int16;
        }

        public static void WriteUInt16(byte[] bytes, int start, ushort value)
        {
            if (2 > bytes.Length - start)
                throw new ArgumentException("The value does not fit in bytes", nameof(value));

            var b = new Int16Bytes {UInt16 = value};
            bytes[start + 0] = b.B0;
            bytes[start + 1] = b.B1;
        }

        public static ushort ReadUInt16(byte[] bytes, int start)
        {
            if (2 > bytes.Length - start)
                return default;

            var b = new Int16Bytes
            {
                B0 = bytes[start + 0],
                B1 = bytes[start + 1],
            };
            return b.UInt16;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Int64Bytes
        {
            [FieldOffset(0)] public byte B0;
            [FieldOffset(1)] public byte B1;
            [FieldOffset(2)] public byte B2;
            [FieldOffset(3)] public byte B3;
            [FieldOffset(4)] public byte B4;
            [FieldOffset(5)] public byte B5;
            [FieldOffset(6)] public byte B6;
            [FieldOffset(7)] public byte B7;

            [FieldOffset(0)] public ulong UInt64;
            [FieldOffset(0)] public long Int64;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Int32Bytes
        {
            [FieldOffset(0)] public byte B0;
            [FieldOffset(1)] public byte B1;
            [FieldOffset(2)] public byte B2;
            [FieldOffset(3)] public byte B3;

            [FieldOffset(0)] public uint UInt32;
            [FieldOffset(0)] public int Int32;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Int16Bytes
        {
            [FieldOffset(0)] public byte B0;
            [FieldOffset(1)] public byte B1;

            [FieldOffset(0)] public ushort UInt16;
            [FieldOffset(0)] public short Int16;
        }
    }
}