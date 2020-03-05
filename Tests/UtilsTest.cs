using System;
using System.Linq;
using NUnit.Framework;
using Packets;

namespace Tests
{
    [TestFixture]
    public class Utils
    {
        [Test]
        public void HeaderCorrect()
        {
            byte[] testHeader =
            {
                (byte) 'A',
                (byte) 'b',
                (byte) 'c',
                (byte) 'D',
                (byte) 'e',
            };

            byte[] bytes =
            {
                (byte) 'A',
                (byte) 'b',
                (byte) 'c',
                (byte) 'D',
                (byte) 'e',
            };

            Assert.IsTrue(PacketUtils.MatchesHeader(bytes, 0, bytes.Length, testHeader));
        }

        [Test]
        public void HeaderIncorrect()
        {
            byte[] testHeader =
            {
                (byte) 'A',
                (byte) 'b',
                (byte) 'c',
                (byte) 'D',
                (byte) 'e',
            };

            byte[] bytes =
            {
                (byte) 'A',
                (byte) 'b',
            };

            Assert.IsFalse(PacketUtils.MatchesHeader(bytes, 0, bytes.Length, testHeader));
        }

        [Test]
        public void WriteHeader()
        {
            byte[] testHeader =
            {
                (byte) 'A',
                (byte) 'b',
                (byte) 'c',
                (byte) 'D',
                (byte) 'e',
            };
            var bytes = new byte[testHeader.Length];
            PacketUtils.WriteHeader(bytes, 0, testHeader);
            Assert.True(bytes.SequenceEqual(testHeader));

            Assert.Throws<ArgumentException>(() => PacketUtils.WriteHeader(bytes, 1, testHeader));
            bytes = new byte[testHeader.Length - 1];
            Assert.Throws<ArgumentException>(() => PacketUtils.WriteHeader(bytes, 0, testHeader));
        }

        [Test]
        public void WriteString()
        {
            const string text = "This is some text";
            var bytes = new byte[text.Length];
            PacketUtils.WriteString(bytes, 0, text);
            Assert.IsTrue(bytes.SequenceEqual(text.Select(x => (byte) x)));
            
            Assert.Throws<ArgumentException>(() => PacketUtils.WriteString(bytes, 1, text));
            bytes = new byte[text.Length - 1];
            Assert.Throws<ArgumentException>(() => PacketUtils.WriteString(bytes, 0, text));
        }

        [Test]
        public void UInt64Conversion()
        {
            var bytes = new byte[8];
            const ulong value = 638265452;
            PacketUtils.WriteUInt64(bytes, 0, value);
            var readValue = PacketUtils.ReadUInt64(bytes, 0);
            Assert.AreEqual(value, readValue);
        }

        [Test]
        public void Int64Conversion()
        {
            var bytes = new byte[8];
            const long value = -638265452;
            PacketUtils.WriteInt64(bytes, 0, value);
            var readValue = PacketUtils.ReadInt64(bytes, 0);
            Assert.AreEqual(value, readValue);
        }
        
        [Test]
        public void UInt32Conversion()
        {
            var bytes = new byte[8];
            const int value = 638265452;
            PacketUtils.WriteUInt32(bytes, 0, value);
            var readValue = PacketUtils.ReadUInt32(bytes, 0);
            Assert.AreEqual(value, readValue);
        }

        [Test]
        public void Int32Conversion()
        {
            var bytes = new byte[8];
            const int value = -638265452;
            PacketUtils.WriteInt32(bytes, 0, value);
            var readValue = PacketUtils.ReadInt32(bytes, 0);
            Assert.AreEqual(value, readValue);
        }
        
        [Test]
        public void UInt16Conversion()
        {
            var bytes = new byte[8];
            const ushort value = 65452;
            PacketUtils.WriteUInt16(bytes, 0, value);
            var readValue = PacketUtils.ReadUInt16(bytes, 0);
            Assert.AreEqual(value, readValue);
        }

        [Test]
        public void Int16Conversion()
        {
            var bytes = new byte[8];
            const short value = -6545;
            PacketUtils.WriteInt16(bytes, 0, value);
            var readValue = PacketUtils.ReadInt16(bytes, 0);
            Assert.AreEqual(value, readValue);
        }
    }
}