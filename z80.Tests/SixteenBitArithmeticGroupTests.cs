using System;
using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class SixteenBitArithmeticGroupTests : OpCodeTestBase
    {
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

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = val + val2;
            var sixteenBitSum = (ushort)trueSum;
            Assert.AreEqual(sixteenBitSum, en.IY);
            Assert.AreEqual(halfcarry, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0, false, 0x1234, 0x4321, false, false, false)]
        [TestCase(0, true, 0x1234, 0x4321, false, false, false)]
        [TestCase(0, false, 0x0FFF, 0x0001, true, false, false)]
        [TestCase(0, false, 0x7FF0, 0x0234, true, false, true)]
        [TestCase(0, false, 0x9FFF, 0xAFFF, true, true, true)]
        [TestCase(1, false, 0x1234, 0x4321, false, false, false)]
        [TestCase(1, false, 0x0FFF, 0x0001, true, false, false)]
        [TestCase(1, false, 0x7FF0, 0x0234, true, false, true)]
        [TestCase(1, false, 0x9FFF, 0xAFFF, true, true, true)]
        [TestCase(2, false, 0x1234, 0x1234, false, false, false)]
        [TestCase(2, false, 0x6234, 0x6234, false, false, true)]
        [TestCase(2, false, 0x8234, 0x8234, false, true, true)]
        [TestCase(3, false, 0x1234, 0x4321, false, false, false)]
        [TestCase(3, false, 0x0FFF, 0x0001, true, false, false)]
        [TestCase(3, false, 0x7FF0, 0x0234, true, false, true)]
        [TestCase(3, false, 0x9FFF, 0xAFFF, true, true, true)]
        #endregion
        public void Test_ADC_HL_ss(byte reg, bool useCarry, int val, int val2, bool halfcarry, bool carry, bool overflow)
        {
            asm.LoadReg16Val(0, (ushort)(useCarry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(2, (ushort)val);
            asm.LoadReg16Val(reg, (ushort)val2);
            asm.AdcHlReg16(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = val + val2 + (useCarry ? 1 : 0);
            var sixteenBitSum = (ushort)trueSum;
            Assert.AreEqual(sixteenBitSum, en.HL);
            Assert.AreEqual((sixteenBitSum & 0x8000) > 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(sixteenBitSum == 0, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(halfcarry, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }
        [Test]
        #region testcases
        [TestCase(0, false, 0x0FFF, 0x0001, false, false, false)]
        [TestCase(0, true, 0x0FFF, 0x0001, false, false, false)]
        [TestCase(0, false, 0x1234, 0x4321, true, true, false)]
        [TestCase(0, false, 0x0123, 0x0234, true, true, false)]
        [TestCase(0, false, 0x0FFF, 0x0FFF, false, false, false)]
        [TestCase(1, false, 0x0FFF, 0x0001, false, false, false)]
        [TestCase(1, false, 0x1234, 0x4321, true, true, false)]
        [TestCase(1, false, 0x0123, 0x0234, true, true, false)]
        [TestCase(1, false, 0x0FFF, 0x0FFF, false, false, false)]
        [TestCase(2, false, 0x1234, 0x1234, false, false, false)]
        [TestCase(3, false, 0x0FFF, 0x0001, false, false, false)]
        [TestCase(3, false, 0x1234, 0x4321, true, true, false)]
        [TestCase(3, false, 0x0123, 0x0234, true, true, false)]
        [TestCase(3, false, 0x0FFF, 0x0FFF, false, false, false)]
        #endregion
        public void Test_SBC_HL_ss(byte reg, bool useCarry, int val, int val2, bool halfcarry, bool carry, bool overflow)
        {
            asm.LoadReg16Val(0, (ushort)(useCarry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(2, (ushort)val);
            asm.LoadReg16Val(reg, (ushort)val2);
            asm.SbcHlReg16(reg);
            asm.Halt();

            en.Run();

            var trueDiff = val - val2 - (useCarry ? 1 : 0);
            var sixteenBitDiff = (ushort)trueDiff;
            Assert.AreEqual(sixteenBitDiff, en.HL);
            Assert.AreEqual((sixteenBitDiff & 0x8000) > 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(sixteenBitDiff == 0, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(halfcarry, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }


        [Test]
        #region testcases
        [TestCase(0x00)]
        [TestCase(0x01)]
        [TestCase(0x02)]
        [TestCase(0x03)]
        #endregion
        public void Test_INC_ss(byte reg)
        {
            asm.LoadReg16Val(reg, 0x1942);
            asm.IncReg16(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            switch (reg)
            {
                case 0:
                    Assert.AreEqual(0x1943, en.BC);
                    break;
                case 1:
                    Assert.AreEqual(0x1943, en.DE);
                    break;
                case 2:
                    Assert.AreEqual(0x1943, en.HL);
                    break;
                case 3:
                    Assert.AreEqual(0x1943, en.SP);
                    break;
            }

        }

        [Test]
        public void Test_INC_IX()
        {
            asm.LoadIxVal(0x1942);
            asm.IncIx();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1943, en.IX);
        }
        [Test]
        public void Test_INC_IY()
        {
            asm.LoadIyVal(0x1942);
            asm.IncIy();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1943, en.IY);
        }

        [Test]
        #region testcases
        [TestCase(0x00)]
        [TestCase(0x01)]
        [TestCase(0x02)]
        [TestCase(0x03)]
        #endregion
        public void Test_DEC_ss(byte reg)
        {
            asm.LoadReg16Val(reg, 0x1942);
            asm.DecReg16(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            switch (reg)
            {
                case 0:
                    Assert.AreEqual(0x1941, en.BC);
                    break;
                case 1:
                    Assert.AreEqual(0x1941, en.DE);
                    break;
                case 2:
                    Assert.AreEqual(0x1941, en.HL);
                    break;
                case 3:
                    Assert.AreEqual(0x1941, en.SP);
                    break;
            }

        }

        [Test]
        public void Test_DEC_IX()
        {
            asm.LoadIxVal(0x1942);
            asm.DecIx();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1941, en.IX);
        }
        [Test]
        public void Test_DEC_IY()
        {
            asm.LoadIyVal(0x1942);
            asm.DecIy();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1941, en.IY);
        }


    }
}