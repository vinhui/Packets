using System;
using System.Runtime.InteropServices;

namespace Packets
{
    public static class PacketUtils
    {
        /// <summary>
        /// Check if one byte array matches a sequence in another.
        /// Useful for quickly checking if a packet header matches. 
        /// </summary>
        /// <param name="bytes">The 'bigger' array</param>
        /// <param name="start">The index at which to start checking</param>
        /// <param name="count">Total amount of bytes</param>
        /// <param name="header">The sequence to look for</param>
        /// <returns>True if it matches</returns>
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

        /// <summary>
        /// Write a sequence of bytes to another array
        /// </summary>
        /// <param name="bytes">Array to write to</param>
        /// <param name="start">Index at which to start writing in the array</param>
        /// <param name="header">Sequence of bytes to write to the array</param>
        /// <exception cref="ArgumentException">Thrown if the sequence won't fit</exception>
        public static void WriteHeader(byte[] bytes, int start, byte[] header)
        {
            if (header.Length > bytes.Length - start)
                throw new ArgumentException("Header does not fit in bytes", nameof(header));

            Array.Copy(header, 0, bytes, start, header.Length);
        }

        /// <summary>
        /// Write a string to a byte array
        /// </summary>
        /// <param name="bytes">The array to write it in</param>
        /// <param name="start">The index at which to start writing it</param>
        /// <param name="text">Value to write</param>
        /// <exception cref="ArgumentException">Thrown if the value would not fit in the byte array</exception>
        public static void WriteString(byte[] bytes, int start, string text)
        {
            if (text == null)
                return;

            if (text.Length > bytes.Length - start)
                throw new ArgumentException("Text does not fit in bytes", nameof(text));

            for (var i = 0; i < text.Length; i++)
                bytes[i + start] = (byte) text[i];
        }

        /// <summary>
        /// Write an Int64 (long) value to a byte array
        /// </summary>
        /// <param name="bytes">The array to write it in</param>
        /// <param name="start">The index at which to start writing it</param>
        /// <param name="value">Value to write</param>
        /// <exception cref="ArgumentException">Thrown if the value would not fit in the byte array</exception>
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

        /// <summary>
        /// Read an Int64 (long) from the byte array
        /// </summary>
        /// <param name="bytes">Bytes to read from</param>
        /// <param name="start">Index to start reading from</param>
        /// <returns>Returns the read value</returns>
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

        /// <summary>
        /// Write an unsigned Int64 (ulong) value to a byte array
        /// </summary>
        /// <param name="bytes">The array to write it in</param>
        /// <param name="start">The index at which to start writing it</param>
        /// <param name="value">Value to write</param>
        /// <exception cref="ArgumentException">Thrown if the value would not fit in the byte array</exception>
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

        /// <summary>
        /// Read an unsigned Int64 (ulong) from the byte array
        /// </summary>
        /// <param name="bytes">Bytes to read from</param>
        /// <param name="start">Index to start reading from</param>
        /// <returns>Returns the read value</returns>
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

        /// <summary>
        /// Write an Int32 (int) value to a byte array
        /// </summary>
        /// <param name="bytes">The array to write it in</param>
        /// <param name="start">The index at which to start writing it</param>
        /// <param name="value">Value to write</param>
        /// <exception cref="ArgumentException">Thrown if the value would not fit in the byte array</exception>
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

        /// <summary>
        /// Read an Int32 (int) from the byte array
        /// </summary>
        /// <param name="bytes">Bytes to read from</param>
        /// <param name="start">Index to start reading from</param>
        /// <returns>Returns the read value</returns>
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

        /// <summary>
        /// Write an unsigned Int32 (uint) value to a byte array
        /// </summary>
        /// <param name="bytes">The array to write it in</param>
        /// <param name="start">The index at which to start writing it</param>
        /// <param name="value">Value to write</param>
        /// <exception cref="ArgumentException">Thrown if the value would not fit in the byte array</exception>
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

        /// <summary>
        /// Read an unsigned Int32 (uint) from the byte array
        /// </summary>
        /// <param name="bytes">Bytes to read from</param>
        /// <param name="start">Index to start reading from</param>
        /// <returns>Returns the read value</returns>
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

        /// <summary>
        /// Write an Int16 (short) value to a byte array
        /// </summary>
        /// <param name="bytes">The array to write it in</param>
        /// <param name="start">The index at which to start writing it</param>
        /// <param name="value">Value to write</param>
        /// <exception cref="ArgumentException">Thrown if the value would not fit in the byte array</exception>
        public static void WriteInt16(byte[] bytes, int start, short value)
        {
            if (2 > bytes.Length - start)
                throw new ArgumentException("The value does not fit in bytes", nameof(value));

            var b = new Int16Bytes {Int16 = value};
            bytes[start + 0] = b.B0;
            bytes[start + 1] = b.B1;
        }

        /// <summary>
        /// Read an Int16 (short) from the byte array
        /// </summary>
        /// <param name="bytes">Bytes to read from</param>
        /// <param name="start">Index to start reading from</param>
        /// <returns>Returns the read value</returns>
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

        /// <summary>
        /// Write an unsigned Int16 (ushort) value to a byte array
        /// </summary>
        /// <param name="bytes">The array to write it in</param>
        /// <param name="start">The index at which to start writing it</param>
        /// <param name="value">Value to write</param>
        /// <exception cref="ArgumentException">Thrown if the value would not fit in the byte array</exception>
        public static void WriteUInt16(byte[] bytes, int start, ushort value)
        {
            if (2 > bytes.Length - start)
                throw new ArgumentException("The value does not fit in bytes", nameof(value));

            var b = new Int16Bytes {UInt16 = value};
            bytes[start + 0] = b.B0;
            bytes[start + 1] = b.B1;
        }

        /// <summary>
        /// Read an unsigned Int16 (ushort) from the byte array
        /// </summary>
        /// <param name="bytes">Bytes to read from</param>
        /// <param name="start">Index to start reading from</param>
        /// <returns>Returns the read value</returns>
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