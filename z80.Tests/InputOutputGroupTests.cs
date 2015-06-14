using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    internal class InputOutputTests : OpCodeTestBase
    {
        [Test]
        [TestCase(0x00)]
        [TestCase(0x42)]
        public void Test_IN_A_n(byte val)
        {
            asm.LoadRegVal(7, val);
            asm.InAPort(0x34);
            asm.Halt();

            en.SetInput((ushort)(val * 256 + 0x34), 0x56);
            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x56, en.A);
        }
        [Test]
        [TestCase(2, 0x3C, false, false, true)]
        [TestCase(3, 0xBB, true, false, true)]
        [TestCase(2, 0xEB, true, false, true)]
        [TestCase(0, 0x38, false, false, false)]
        [TestCase(7, 0x9A, true, false, true)]
        [TestCase(2, 0x47, false, false, true)]
        [TestCase(3, 0x8D, true, false, true)]
        [TestCase(5, 0x71, false, false, true)]
        [TestCase(2, 0x58, false, false, false)]
        [TestCase(7, 0x45, false, false, false)]
        [TestCase(3, 0x56, false, false, true)]
        [TestCase(1, 0x91, true, false, false)]
        [TestCase(1, 0x00, false, true, true)]
        [TestCase(2, 0xC0, true, false, true)]
        [TestCase(1, 0x79, false, false, false)]
        [TestCase(7, 0x5A, false, false, true)]
        [TestCase(4, 0x9A, true, false, true)]
        [TestCase(0, 0x07, false, false, false)]
        public void Test_IN_r_BC(byte reg, byte val, bool sign, bool zero, bool parity)
        {
            asm.LoadReg16Val(0, 0x1234);
            asm.InRegBc(reg);
            asm.Halt();

            en.SetInput(0x1234, val);
            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(val, en.Reg8(reg));
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
        }

        private void generator()
        {
            Func<bool, string> l = b => b.ToString().ToLower();
            var r = new Random();
            for (int i = 0; i < 20; i++)
            {
                var reg = r.Next(7);
                var val = r.Next(256);
                if (reg == 6) reg++;
                var p =
                    ((val & 0x80) >> 7) +
                    ((val & 0x40) >> 6) +
                    ((val & 0x20) >> 5) +
                    ((val & 0x10) >> 4) +
                    ((val & 0x08) >> 3) +
                    ((val & 0x04) >> 2) +
                    ((val & 0x02) >> 1) +
                    (val & 0x01);
                Console.WriteLine($"[TestCase({reg}, 0x{val:X2}, {l(val > 127)}, {l(val == 0)}, {l(p%2==0)})]");
            }
            Assert.Fail();
        }
    }
}
