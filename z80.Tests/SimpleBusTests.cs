using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class SimpleBusTests
    {
        [Test]
        public void IoRead_ReturnsSetInputValue()
        {
            var bus = new SimpleBus();
            bus.SetInput(0x42, 0xAB);
            Assert.AreEqual(0xAB, bus.IoRead(0x42));
        }

        [Test]
        public void IoWrite_IsReadableViaGetOutput()
        {
            var bus = new SimpleBus();
            bus.IoWrite(0x42, 0xCD);
            Assert.AreEqual(0xCD, bus.GetOutput(0x42));
        }

        [Test]
        public void NMI_AutoClearsOnRead()
        {
            var bus = new SimpleBus();
            bus.NMI = true;
            Assert.IsTrue(bus.NMI);
            Assert.IsFalse(bus.NMI);
        }

        [Test]
        public void INT_AutoClearsOnRead()
        {
            var bus = new SimpleBus();
            bus.INT = true;
            Assert.IsTrue(bus.INT);
            Assert.IsFalse(bus.INT);
        }

        [Test]
        public void StubbedSignals_ReturnFalse()
        {
            var bus = new SimpleBus();
            Assert.IsFalse(bus.WAIT);
            Assert.IsFalse(bus.BUSRQ);
            Assert.IsFalse(bus.RESET);
        }
    }
}
