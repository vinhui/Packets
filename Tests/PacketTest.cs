using System.Collections.Generic;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class Packet
    {
        [Test]
        public void Serialize()
        {
            const string text = "I am random data";
            var packet = new TestPacket(text);
            var bytes = packet.Serialize();
            Assert.True(packet.IsMatch(bytes, 0, bytes.Length));
            Assert.AreEqual(5 + 1 + text.Length, bytes.Length);
        }

        [Test]
        public void SerializeDeserialize()
        {
            const string text = "I am random data";
            var packetA = new TestPacket(text);
            var bytes = packetA.Serialize();

            var packetB = new TestPacket();
            Assert.IsTrue(packetB.IsMatch(bytes, 0, bytes.Length));
            packetB.Deserialize(bytes, 0, bytes.Length, out var used);
            Assert.AreEqual(bytes.Length, used);
            Assert.AreEqual(text, packetB.Data);

            var bytesList = new List<byte>(bytes);
            bytesList.Add(0);
            bytesList.Insert(0, 0);
            bytes = bytesList.ToArray();
            Assert.IsTrue(packetB.IsMatch(bytes, 1, bytes.Length));
            packetB.Deserialize(bytes, 1, bytes.Length, out used);
            Assert.AreEqual(bytesList.Count - 2, used);
            Assert.AreEqual(text, packetB.Data);
        }

        [Test]
        public void Clone()
        {
            var packetA = new TestPacket("I am groot");
            var packetB = packetA.Clone();
            Assert.AreNotSame(packetA, packetB);

            var packetC = new EmptyPacket();
            var packetD = packetA.Clone();
            Assert.AreNotSame(packetC, packetD);
        }
    }
}