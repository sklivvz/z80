using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class SimpleMemoryTests
    {
        [Test]
        public void Read_ReturnsValue()
        {
            var ram = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var mem = new SimpleMemory(ram);

            for (ushort i = 0; i < ram.Length; i++)
                Assert.AreEqual(i, mem[i]);
        }

        [Test]
        public void Write_StoresValue()
        {
            var ram = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var mem = new SimpleMemory(ram);

            for (ushort i = 0; i < ram.Length; i++)
            {
                mem[i] = (byte)(0xFF ^ i);
                Assert.AreEqual((byte)(0xFF ^ i), mem[i]);
            }
        }

        [Test]
        public void Write_ModifiesBackingArray()
        {
            var ram = new byte[10];
            var mem = new SimpleMemory(ram);

            mem[5] = 0x42;
            Assert.AreEqual(0x42, ram[5]);
        }
    }
}
