using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Packets;

namespace Tests
{
    [TestFixture]
    public class Factory
    {
        private PacketsFactory _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new PacketsFactory();
            _factory.RegisterPacket<EmptyPacket>();
            _factory.RegisterPacket<TestPacket>();
        }

        [Test]
        public void GetNoPacket()
        {
            Assert.IsFalse(_factory.TryGetPacket(new byte[10], 0, 10, out var used, out var packet));
            Assert.AreEqual(0, used);
            Assert.AreEqual(default(IPacket), packet);
        }

        [Test]
        public void GetSinglePacket()
        {
            var packet = new TestPacket("Some data");
            var bytes = packet.Serialize();
            Assert.IsTrue(_factory.TryGetPacket(bytes, 0, bytes.Length, out var used, out var newPacket));
            Assert.AreEqual(bytes.Length, used);
            Assert.IsInstanceOf<TestPacket>(newPacket);
            var testPacket = (TestPacket) newPacket;
            Assert.AreEqual(packet.Data, testPacket.Data);
        }

        [Test]
        public void GetMultiplePackets()
        {
            var random = Randomizer.CreateRandomizer();
            var packets = new List<TestPacket>();
            for (var i = 0; i < 10; i++)
            {
                packets.Add(new TestPacket(random.GetString()));
            }

            var bytes = packets.SelectMany(x => x.Serialize()).ToArray();
            var returnedPackets = _factory.GetPackets(bytes, 0, bytes.Length).ToArray();
            Assert.NotNull(returnedPackets);
            Assert.NotZero(returnedPackets.Length);
            Assert.AreEqual(packets.Count, returnedPackets.Length);
            for (var i = 0; i < returnedPackets.Length; i++)
            {
                Assert.IsInstanceOf<TestPacket>(returnedPackets[i]);
                var p = (TestPacket) returnedPackets[i];
                Assert.AreEqual(packets[i].Data, p.Data);
            }
        }

        [Test]
        public void NotSame()
        {
            var packets = new[]
            {
                new TestPacket(),
                new TestPacket(),
                new TestPacket()
            };

            var bytes = packets.SelectMany(x => x.Serialize()).ToArray();
            var returnedPackets = _factory.GetPackets(bytes, 0, bytes.Length).ToArray();

            IPacket lastPacket = null;
            foreach (var packet in returnedPackets)
            {
                Assert.AreNotSame(lastPacket, packet);
                lastPacket = packet;
            }
        }
    }
}