using System;
using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class SixteenBitArithmeticGroupTests : OpCodeTestBase
    {

        private int Countbits(int value)
        {
            int count = 0;
            while (value != 0)
            {
                count++;
                value &= value - 1;
            }
            return count;
        }

        [Test]
        #region testcases
        [TestCase(0, 0x1234, 0x4321, false, false)]
        [TestCase(0, 0x0FFF, 0x0001, true, false)]
        [TestCase(0, 0x0001, 0x0FFF, true, false)]
        [TestCase(0, 0x0001, 0xFFFF, true, true)]
        [TestCase(1, 0x1234, 0x4321, false, false)]
        [TestCase(1, 0x0FFF, 0x0001, true, false)]
        [TestCase(1, 0x0001, 0x0FFF, true, false)]
        [TestCase(1, 0x0001, 0xFFFF, true, true)]
        [TestCase(2, 0x1234, 0x1234, false, false)]
        [TestCase(2, 0x0FFF, 0x0FFF, true, false)]
        [TestCase(2, 0xFFFF, 0xFFFF, true, true)]
        [TestCase(3, 0x1234, 0x4321, false, false)]
        [TestCase(3, 0x0FFF, 0x0001, true, false)]
        [TestCase(3, 0x0001, 0x0FFF, true, false)]
        [TestCase(3, 0x0001, 0xFFFF, true, true)]
        #endregion
        public void Test_ADD_HL_ss(byte reg, int val, int val2, bool halfcarry, bool carry)
        {
            asm.LoadReg16Val(2, (ushort)val);
            asm.LoadReg16Val(reg, (ushort)val2);
            asm.AddHlReg16(reg);
            asm.Halt();

            en.Run();
            en.DumpCpu();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = val + val2;
            var sixteenBitSum = (ushort)trueSum;
            Assert.AreEqual(sixteenBitSum, en.HL);
            Assert.AreEqual(halfcarry, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0, 0x1234, 0x4321, false, false)]
        [TestCase(0, 0x0FFF, 0x0001, true, false)]
        [TestCase(0, 0x0001, 0x0FFF, true, false)]
        [TestCase(0, 0x0001, 0xFFFF, true, true)]
        [TestCase(1, 0x1234, 0x4321, false, false)]
        [TestCase(1, 0x0FFF, 0x0001, true, false)]
        [TestCase(1, 0x0001, 0x0FFF, true, false)]
        [TestCase(1, 0x0001, 0xFFFF, true, true)]
        [TestCase(2, 0x1234, 0x1234, false, false)]
        [TestCase(2, 0x0FFF, 0x0FFF, true, false)]
        [TestCase(2, 0xFFFF, 0xFFFF, true, true)]
        [TestCase(3, 0x1234, 0x4321, false, false)]
        [TestCase(3, 0x0FFF, 0x0001, true, false)]
        [TestCase(3, 0x0001, 0x0FFF, true, false)]
        [TestCase(3, 0x0001, 0xFFFF, true, true)]
        #endregion
        public void Test_ADD_IX_ss(byte reg, int val, int val2, bool halfcarry, bool carry)
        {
            asm.LoadIxVal((ushort)val);
            asm.LoadReg16Val(reg, (ushort)val2);
            asm.AddIxReg16(reg);
            asm.Halt();

            en.Run();
            en.DumpCpu();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = val + val2;
            var sixteenBitSum = (ushort)trueSum;
            Assert.AreEqual(sixteenBitSum, en.IX);
            Assert.AreEqual(halfcarry, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0, 0x1234, 0x4321, false, false)]
        [TestCase(0, 0x0FFF, 0x0001, true, false)]
        [TestCase(0, 0x0001, 0x0FFF, true, false)]
        [TestCase(0, 0x0001, 0xFFFF, true, true)]
        [TestCase(1, 0x1234, 0x4321, false, false)]
        [TestCase(1, 0x0FFF, 0x0001, true, false)]
        [TestCase(1, 0x0001, 0x0FFF, true, false)]
        [TestCase(1, 0x0001, 0xFFFF, true, true)]
        [TestCase(2, 0x1234, 0x1234, false, false)]
        [TestCase(2, 0x0FFF, 0x0FFF, true, false)]
        [TestCase(2, 0xFFFF, 0xFFFF, true, true)]
        [TestCase(3, 0x1234, 0x4321, false, false)]
        [TestCase(3, 0x0FFF, 0x0001, true, false)]
        [TestCase(3, 0x0001, 0x0FFF, true, false)]
        [TestCase(3, 0x0001, 0xFFFF, true, true)]
        #endregion
        public void Test_ADD_IY_ss(byte reg, int val, int val2, bool halfcarry, bool carry)
        {
            asm.LoadIyVal((ushort)val);
            asm.LoadReg16Val(reg, (ushort)val2);
            asm.AddIyReg16(reg);
            asm.Halt();

            en.Run();
            en.DumpCpu();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = val + val2;
            var sixteenBitSum = (ushort)trueSum;
            Assert.AreEqual(sixteenBitSum, en.IY);
            Assert.AreEqual(halfcarry, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }


    }
}