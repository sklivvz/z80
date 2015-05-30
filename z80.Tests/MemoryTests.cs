using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var sut = new Memory(ram, 0);

            for (ushort i = 0; i < ram.Length; i++)
            {
                Assert.AreEqual(i, sut[i]);
            }
        }
        [Test]
        public void ReadInRom()
        {
            var ram = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var sut = new Memory(ram, 10);

            for (ushort i = 0; i < ram.Length; i++)
            {
                Assert.AreEqual(i, sut[i]);
            }
        }
        [Test]
        public void WriteInRam()
        {
            var ram = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var sut = new Memory(ram, 0);

            for (ushort i = 0; i < ram.Length; i++)
            {
                sut[i] =(byte) (0xFF ^ i);
                Assert.AreEqual((byte)(0xFF ^ i), sut[i]);
            }
        }
        [Test]
        public void WriteInRom()
        {
            var ram = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var sut = new Memory(ram, 10);

            for (ushort i = 0; i < ram.Length; i++)
            {
                sut[i] = (byte)(0xFF ^ i);
                Assert.AreEqual(i, sut[i]);
            }
        }
    }
}
