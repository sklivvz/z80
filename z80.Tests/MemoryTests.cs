using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    class MemoryTests
    {
        [Test]
        public void ReadInRam()
        {
            var ram = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var sut = new SimpleMemory(ram);

            for (ushort i = 0; i < ram.Length; i++)
                Assert.AreEqual(i, sut[i]);
        }

        [Test]
        public void WriteInRam()
        {
            var ram = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var sut = new SimpleMemory(ram);

            for (ushort i = 0; i < ram.Length; i++)
            {
                sut[i] = (byte)(0xFF ^ i);
                Assert.AreEqual((byte)(0xFF ^ i), sut[i]);
            }
        }
    }
}
