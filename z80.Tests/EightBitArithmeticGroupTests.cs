using System;
using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class EightBitArithmeticGroupTests : OpCodeTestBase
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

        // Useful ref: http://stackoverflow.com/questions/8034566/overflow-and-carry-flags-on-z80

        [Test]
        #region testcases
        [TestCase(0, 0x44, 0x11)]
        [TestCase(0, 0x44, 0x0F)]
        [TestCase(0, 0x44, 0xFF)]
        [TestCase(0, 0x44, 0x01)]
        [TestCase(0, 0xF4, 0x11)]
        [TestCase(0, 0xF4, 0x0F)]
        [TestCase(0, 0xF4, 0xFF)]
        [TestCase(0, 0xF4, 0x01)]
        [TestCase(1, 0x44, 0x11)]
        [TestCase(1, 0x44, 0x0F)]
        [TestCase(1, 0x44, 0xFF)]
        [TestCase(1, 0x44, 0x01)]
        [TestCase(2, 0x44, 0x11)]
        [TestCase(2, 0x44, 0x0F)]
        [TestCase(2, 0x44, 0xFF)]
        [TestCase(2, 0x44, 0x01)]
        [TestCase(3, 0x44, 0x11)]
        [TestCase(3, 0x44, 0x0F)]
        [TestCase(3, 0x44, 0xFF)]
        [TestCase(3, 0x44, 0x01)]
        [TestCase(4, 0x44, 0x11)]
        [TestCase(4, 0x44, 0x0F)]
        [TestCase(4, 0x44, 0xFF)]
        [TestCase(4, 0x44, 0x01)]
        [TestCase(5, 0x44, 0x11)]
        [TestCase(5, 0x44, 0x0F)]
        [TestCase(5, 0x44, 0xFF)]
        [TestCase(5, 0x44, 0x01)]
        [TestCase(7, 0x44, 0x44)]
        #endregion
        public void Test_ADD_A_r(byte reg, byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.LoadRegVal(reg, val2);
            asm.AddAReg(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (val + val2);
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Console.WriteLine("{0:X4}", trueSum);
            Console.WriteLine("{0:X4}", byteSum);
            Console.WriteLine("{0}", sbyteSum);
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(((val2 & 0x0F) + (val & 0x0F)) > 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_ADD_A_n(byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.AddAVal(val2);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (val + val2);
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(((val2 & 0x0F) + (val & 0x0F)) > 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_ADD_A_at_HL(byte val, byte val2)
        {
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.AddAAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (val + val2);
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(((val2 & 0x0F) + (val & 0x0F)) > 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_ADD_A_at_IX(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIxVal(0x0040);
            asm.AddAAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (val + val2);
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(((val2 & 0x0F) + (val & 0x0F)) > 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_ADD_A_at_IY(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIyVal(0x0040);
            asm.AddAAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (val + val2);
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(((val2 & 0x0F) + (val & 0x0F)) > 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0, 0x44, 0x11, false)]
        [TestCase(0, 0x44, 0x0F, false)]
        [TestCase(0, 0x44, 0xFF, false)]
        [TestCase(0, 0x44, 0x01, false)]
        [TestCase(0, 0xF4, 0x11, false)]
        [TestCase(0, 0xF4, 0x0F, false)]
        [TestCase(0, 0xF4, 0xFF, false)]
        [TestCase(0, 0xF4, 0x01, false)]
        [TestCase(1, 0x44, 0x11, false)]
        [TestCase(1, 0x44, 0x0F, false)]
        [TestCase(1, 0x44, 0xFF, false)]
        [TestCase(1, 0x44, 0x01, false)]
        [TestCase(2, 0x44, 0x11, false)]
        [TestCase(2, 0x44, 0x0F, false)]
        [TestCase(2, 0x44, 0xFF, false)]
        [TestCase(2, 0x44, 0x01, false)]
        [TestCase(3, 0x44, 0x11, false)]
        [TestCase(3, 0x44, 0x0F, false)]
        [TestCase(3, 0x44, 0xFF, false)]
        [TestCase(3, 0x44, 0x01, false)]
        [TestCase(4, 0x44, 0x11, false)]
        [TestCase(4, 0x44, 0x0F, false)]
        [TestCase(4, 0x44, 0xFF, false)]
        [TestCase(4, 0x44, 0x01, false)]
        [TestCase(5, 0x44, 0x11, false)]
        [TestCase(5, 0x44, 0x0F, false)]
        [TestCase(5, 0x44, 0xFF, false)]
        [TestCase(5, 0x44, 0x01, false)]
        [TestCase(7, 0x44, 0x44, false)]
        [TestCase(0, 0x44, 0x11, true)]
        [TestCase(0, 0x44, 0x0F, true)]
        [TestCase(0, 0x44, 0xFF, true)]
        [TestCase(0, 0x44, 0x01, true)]
        [TestCase(0, 0xF4, 0x11, true)]
        [TestCase(0, 0xF4, 0x0F, true)]
        [TestCase(0, 0xF4, 0xFF, true)]
        [TestCase(0, 0xF4, 0x01, true)]
        [TestCase(1, 0x44, 0x11, true)]
        [TestCase(1, 0x44, 0x0F, true)]
        [TestCase(1, 0x44, 0xFF, true)]
        [TestCase(1, 0x44, 0x01, true)]
        [TestCase(2, 0x44, 0x11, true)]
        [TestCase(2, 0x44, 0x0F, true)]
        [TestCase(2, 0x44, 0xFF, true)]
        [TestCase(2, 0x44, 0x01, true)]
        [TestCase(3, 0x44, 0x11, true)]
        [TestCase(3, 0x44, 0x0F, true)]
        [TestCase(3, 0x44, 0xFF, true)]
        [TestCase(3, 0x44, 0x01, true)]
        [TestCase(4, 0x44, 0x11, true)]
        [TestCase(4, 0x44, 0x0F, true)]
        [TestCase(4, 0x44, 0xFF, true)]
        [TestCase(4, 0x44, 0x01, true)]
        [TestCase(5, 0x44, 0x11, true)]
        [TestCase(5, 0x44, 0x0F, true)]
        [TestCase(5, 0x44, 0xFF, true)]
        [TestCase(5, 0x44, 0x01, true)]
        [TestCase(7, 0x44, 0x44, true)]
        #endregion
        public void Test_ADC_A_r(byte reg, byte val, byte val2, bool carry)
        {
            asm.LoadReg16Val(3, 0x0060);
            asm.LoadReg16Val(0, (ushort)(carry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadRegVal(7, val);
            asm.LoadRegVal(reg, val2);
            asm.AdcAReg(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (val + val2);
            if (carry) trueSum++;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(((val2 & 0x0F) + (val & 0x0F)) > 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, true)]
        [TestCase(0x44, 0x0F, true)]
        [TestCase(0x44, 0xFF, true)]
        [TestCase(0x44, 0x01, true)]
        [TestCase(0xF4, 0x11, true)]
        [TestCase(0xF4, 0x0F, true)]
        [TestCase(0xF4, 0xFF, true)]
        [TestCase(0xF4, 0x01, true)]
        [TestCase(0x44, 0x11, false)]
        [TestCase(0x44, 0x0F, false)]
        [TestCase(0x44, 0xFF, false)]
        [TestCase(0x44, 0x01, false)]
        [TestCase(0xF4, 0x11, false)]
        [TestCase(0xF4, 0x0F, false)]
        [TestCase(0xF4, 0xFF, false)]
        [TestCase(0xF4, 0x01, false)]
        #endregion
        public void Test_ADC_A_n(byte val, byte val2, bool carry)
        {
            asm.LoadReg16Val(3, 0x0060);
            asm.LoadReg16Val(0, (ushort)(carry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadRegVal(7, val);
            asm.AdcAVal(val2);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (val + val2);
            if (carry) trueSum++;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(((val2 & 0x0F) + (val & 0x0F)) > 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, true)]
        [TestCase(0x44, 0x0F, true)]
        [TestCase(0x44, 0xFF, true)]
        [TestCase(0x44, 0x01, true)]
        [TestCase(0xF4, 0x11, true)]
        [TestCase(0xF4, 0x0F, true)]
        [TestCase(0xF4, 0xFF, true)]
        [TestCase(0xF4, 0x01, true)]
        [TestCase(0x44, 0x11, false)]
        [TestCase(0x44, 0x0F, false)]
        [TestCase(0x44, 0xFF, false)]
        [TestCase(0x44, 0x01, false)]
        [TestCase(0xF4, 0x11, false)]
        [TestCase(0xF4, 0x0F, false)]
        [TestCase(0xF4, 0xFF, false)]
        [TestCase(0xF4, 0x01, false)]
        #endregion
        public void Test_ADC_A_at_HL(byte val, byte val2, bool carry)
        {
            asm.LoadReg16Val(3, 0x0060);
            asm.LoadReg16Val(0, (ushort)(carry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.AdcAAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (val + val2);
            if (carry) trueSum++;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(((val2 & 0x0F) + (val & 0x0F)) > 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0, true)]
        [TestCase(0x44, 0x0F, 0, true)]
        [TestCase(0x44, 0xFF, 0, true)]
        [TestCase(0x44, 0x01, 0, true)]
        [TestCase(0xF4, 0x11, 0, true)]
        [TestCase(0xF4, 0x0F, 0, true)]
        [TestCase(0xF4, 0xFF, 0, true)]
        [TestCase(0xF4, 0x01, 0, true)]
        [TestCase(0x44, 0x11, 1, true)]
        [TestCase(0x44, 0x0F, 1, true)]
        [TestCase(0x44, 0xFF, 1, true)]
        [TestCase(0x44, 0x01, 1, true)]
        [TestCase(0xF4, 0x11, 1, true)]
        [TestCase(0xF4, 0x0F, 1, true)]
        [TestCase(0xF4, 0xFF, 1, true)]
        [TestCase(0xF4, 0x01, 1, true)]
        [TestCase(0x44, 0x11, -1, true)]
        [TestCase(0x44, 0x0F, -1, true)]
        [TestCase(0x44, 0xFF, -1, true)]
        [TestCase(0x44, 0x01, -1, true)]
        [TestCase(0xF4, 0x11, -1, true)]
        [TestCase(0xF4, 0x0F, -1, true)]
        [TestCase(0xF4, 0xFF, -1, true)]
        [TestCase(0xF4, 0x01, -1, true)]
        [TestCase(0x44, 0x11, 0, false)]
        [TestCase(0x44, 0x0F, 0, false)]
        [TestCase(0x44, 0xFF, 0, false)]
        [TestCase(0x44, 0x01, 0, false)]
        [TestCase(0xF4, 0x11, 0, false)]
        [TestCase(0xF4, 0x0F, 0, false)]
        [TestCase(0xF4, 0xFF, 0, false)]
        [TestCase(0xF4, 0x01, 0, false)]
        [TestCase(0x44, 0x11, 1, false)]
        [TestCase(0x44, 0x0F, 1, false)]
        [TestCase(0x44, 0xFF, 1, false)]
        [TestCase(0x44, 0x01, 1, false)]
        [TestCase(0xF4, 0x11, 1, false)]
        [TestCase(0xF4, 0x0F, 1, false)]
        [TestCase(0xF4, 0xFF, 1, false)]
        [TestCase(0xF4, 0x01, 1, false)]
        [TestCase(0x44, 0x11, -1, false)]
        [TestCase(0x44, 0x0F, -1, false)]
        [TestCase(0x44, 0xFF, -1, false)]
        [TestCase(0x44, 0x01, -1, false)]
        [TestCase(0xF4, 0x11, -1, false)]
        [TestCase(0xF4, 0x0F, -1, false)]
        [TestCase(0xF4, 0xFF, -1, false)]
        [TestCase(0xF4, 0x01, -1, false)]
        #endregion
        public void Test_ADC_A_at_IX(byte val, byte val2, sbyte disp, bool carry)
        {
            asm.LoadReg16Val(3, 0x0060);
            asm.LoadReg16Val(0, (ushort)(carry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIxVal(0x0040);
            asm.AdcAAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (val + val2);
            if (carry) trueSum++;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(((val2 & 0x0F) + (val & 0x0F)) > 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0, true)]
        [TestCase(0x44, 0x0F, 0, true)]
        [TestCase(0x44, 0xFF, 0, true)]
        [TestCase(0x44, 0x01, 0, true)]
        [TestCase(0xF4, 0x11, 0, true)]
        [TestCase(0xF4, 0x0F, 0, true)]
        [TestCase(0xF4, 0xFF, 0, true)]
        [TestCase(0xF4, 0x01, 0, true)]
        [TestCase(0x44, 0x11, 1, true)]
        [TestCase(0x44, 0x0F, 1, true)]
        [TestCase(0x44, 0xFF, 1, true)]
        [TestCase(0x44, 0x01, 1, true)]
        [TestCase(0xF4, 0x11, 1, true)]
        [TestCase(0xF4, 0x0F, 1, true)]
        [TestCase(0xF4, 0xFF, 1, true)]
        [TestCase(0xF4, 0x01, 1, true)]
        [TestCase(0x44, 0x11, -1, true)]
        [TestCase(0x44, 0x0F, -1, true)]
        [TestCase(0x44, 0xFF, -1, true)]
        [TestCase(0x44, 0x01, -1, true)]
        [TestCase(0xF4, 0x11, -1, true)]
        [TestCase(0xF4, 0x0F, -1, true)]
        [TestCase(0xF4, 0xFF, -1, true)]
        [TestCase(0xF4, 0x01, -1, true)]
        [TestCase(0x44, 0x11, 0, false)]
        [TestCase(0x44, 0x0F, 0, false)]
        [TestCase(0x44, 0xFF, 0, false)]
        [TestCase(0x44, 0x01, 0, false)]
        [TestCase(0xF4, 0x11, 0, false)]
        [TestCase(0xF4, 0x0F, 0, false)]
        [TestCase(0xF4, 0xFF, 0, false)]
        [TestCase(0xF4, 0x01, 0, false)]
        [TestCase(0x44, 0x11, 1, false)]
        [TestCase(0x44, 0x0F, 1, false)]
        [TestCase(0x44, 0xFF, 1, false)]
        [TestCase(0x44, 0x01, 1, false)]
        [TestCase(0xF4, 0x11, 1, false)]
        [TestCase(0xF4, 0x0F, 1, false)]
        [TestCase(0xF4, 0xFF, 1, false)]
        [TestCase(0xF4, 0x01, 1, false)]
        [TestCase(0x44, 0x11, -1, false)]
        [TestCase(0x44, 0x0F, -1, false)]
        [TestCase(0x44, 0xFF, -1, false)]
        [TestCase(0x44, 0x01, -1, false)]
        [TestCase(0xF4, 0x11, -1, false)]
        [TestCase(0xF4, 0x0F, -1, false)]
        [TestCase(0xF4, 0xFF, -1, false)]
        [TestCase(0xF4, 0x01, -1, false)]
        #endregion
        public void Test_ADC_A_at_IY(byte val, byte val2, sbyte disp, bool carry)
        {
            asm.LoadReg16Val(3, 0x0060);
            asm.LoadReg16Val(0, (ushort)(carry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIyVal(0x0040);
            asm.AdcAAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (val + val2);
            if (carry) trueSum++;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(((val2 & 0x0F) + (val & 0x0F)) > 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0, 0x44, 0x11)]
        [TestCase(0, 0x44, 0x0F)]
        [TestCase(0, 0x44, 0xFF)]
        [TestCase(0, 0x44, 0x01)]
        [TestCase(0, 0xF4, 0x11)]
        [TestCase(0, 0xF4, 0x0F)]
        [TestCase(0, 0xF4, 0xFF)]
        [TestCase(0, 0xF4, 0x01)]
        [TestCase(1, 0x44, 0x11)]
        [TestCase(1, 0x44, 0x0F)]
        [TestCase(1, 0x44, 0xFF)]
        [TestCase(1, 0x44, 0x01)]
        [TestCase(2, 0x44, 0x11)]
        [TestCase(2, 0x44, 0x0F)]
        [TestCase(2, 0x44, 0xFF)]
        [TestCase(2, 0x44, 0x01)]
        [TestCase(3, 0x44, 0x11)]
        [TestCase(3, 0x44, 0x0F)]
        [TestCase(3, 0x44, 0xFF)]
        [TestCase(3, 0x44, 0x01)]
        [TestCase(4, 0x44, 0x11)]
        [TestCase(4, 0x44, 0x0F)]
        [TestCase(4, 0x44, 0xFF)]
        [TestCase(4, 0x44, 0x01)]
        [TestCase(5, 0x44, 0x11)]
        [TestCase(5, 0x44, 0x0F)]
        [TestCase(5, 0x44, 0xFF)]
        [TestCase(5, 0x44, 0x01)]
        [TestCase(7, 0x44, 0x44)]
        #endregion
        public void Test_SUB_A_r(byte reg, byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.LoadRegVal(reg, val2);
            asm.SubReg(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(byteDiff, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_SUB_A_n(byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.SubVal(val2);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(byteDiff, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_SUB_A_at_HL(byte val, byte val2)
        {
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.SubAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(byteDiff, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_SUB_A_at_IX(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIxVal(0x0040);
            asm.SubAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(byteDiff, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_SUB_A_at_IY(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIyVal(0x0040);
            asm.SubAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(byteDiff, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0, 0x44, 0x11, false)]
        [TestCase(0, 0x44, 0x0F, false)]
        [TestCase(0, 0x44, 0xFF, false)]
        [TestCase(0, 0x44, 0x01, false)]
        [TestCase(0, 0xF4, 0x11, false)]
        [TestCase(0, 0xF4, 0x0F, false)]
        [TestCase(0, 0xF4, 0xFF, false)]
        [TestCase(0, 0xF4, 0x01, false)]
        [TestCase(1, 0x44, 0x11, false)]
        [TestCase(1, 0x44, 0x0F, false)]
        [TestCase(1, 0x44, 0xFF, false)]
        [TestCase(1, 0x44, 0x01, false)]
        [TestCase(2, 0x44, 0x11, false)]
        [TestCase(2, 0x44, 0x0F, false)]
        [TestCase(2, 0x44, 0xFF, false)]
        [TestCase(2, 0x44, 0x01, false)]
        [TestCase(3, 0x44, 0x11, false)]
        [TestCase(3, 0x44, 0x0F, false)]
        [TestCase(3, 0x44, 0xFF, false)]
        [TestCase(3, 0x44, 0x01, false)]
        [TestCase(4, 0x44, 0x11, false)]
        [TestCase(4, 0x44, 0x0F, false)]
        [TestCase(4, 0x44, 0xFF, false)]
        [TestCase(4, 0x44, 0x01, false)]
        [TestCase(5, 0x44, 0x11, false)]
        [TestCase(5, 0x44, 0x0F, false)]
        [TestCase(5, 0x44, 0xFF, false)]
        [TestCase(5, 0x44, 0x01, false)]
        [TestCase(7, 0x44, 0x44, false)]
        [TestCase(0, 0x44, 0x11, true)]
        [TestCase(0, 0x44, 0x0F, true)]
        [TestCase(0, 0x44, 0xFF, true)]
        [TestCase(0, 0x44, 0x01, true)]
        [TestCase(0, 0xF4, 0x11, true)]
        [TestCase(0, 0xF4, 0x0F, true)]
        [TestCase(0, 0xF4, 0xFF, true)]
        [TestCase(0, 0xF4, 0x01, true)]
        [TestCase(1, 0x44, 0x11, true)]
        [TestCase(1, 0x44, 0x0F, true)]
        [TestCase(1, 0x44, 0xFF, true)]
        [TestCase(1, 0x44, 0x01, true)]
        [TestCase(2, 0x44, 0x11, true)]
        [TestCase(2, 0x44, 0x0F, true)]
        [TestCase(2, 0x44, 0xFF, true)]
        [TestCase(2, 0x44, 0x01, true)]
        [TestCase(3, 0x44, 0x11, true)]
        [TestCase(3, 0x44, 0x0F, true)]
        [TestCase(3, 0x44, 0xFF, true)]
        [TestCase(3, 0x44, 0x01, true)]
        [TestCase(4, 0x44, 0x11, true)]
        [TestCase(4, 0x44, 0x0F, true)]
        [TestCase(4, 0x44, 0xFF, true)]
        [TestCase(4, 0x44, 0x01, true)]
        [TestCase(5, 0x44, 0x11, true)]
        [TestCase(5, 0x44, 0x0F, true)]
        [TestCase(5, 0x44, 0xFF, true)]
        [TestCase(5, 0x44, 0x01, true)]
        [TestCase(7, 0x44, 0x44, true)]
        #endregion
        public void Test_SBC_A_r(byte reg, byte val, byte val2, bool carry)
        {
            asm.LoadReg16Val(3, 0x0060);
            asm.LoadReg16Val(0, (ushort)(carry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadRegVal(7, val);
            asm.LoadRegVal(reg, val2);
            asm.SbcAReg(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            if (carry) trueDiff--;
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(byteDiff, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) + (carry ? 1 : 0) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, true)]
        [TestCase(0x44, 0x0F, true)]
        [TestCase(0x44, 0xFF, true)]
        [TestCase(0x44, 0x01, true)]
        [TestCase(0xF4, 0x11, true)]
        [TestCase(0xF4, 0x0F, true)]
        [TestCase(0xF4, 0xFF, true)]
        [TestCase(0xF4, 0x01, true)]
        [TestCase(0x44, 0x11, false)]
        [TestCase(0x44, 0x0F, false)]
        [TestCase(0x44, 0xFF, false)]
        [TestCase(0x44, 0x01, false)]
        [TestCase(0xF4, 0x11, false)]
        [TestCase(0xF4, 0x0F, false)]
        [TestCase(0xF4, 0xFF, false)]
        [TestCase(0xF4, 0x01, false)]
        #endregion
        public void Test_SBC_A_n(byte val, byte val2, bool carry)
        {
            asm.LoadReg16Val(3, 0x0060);
            asm.LoadReg16Val(0, (ushort)(carry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadRegVal(7, val);
            asm.SbcAVal(val2);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            if (carry) trueDiff--;
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(byteDiff, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) + (carry ? 1 : 0) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, true)]
        [TestCase(0x44, 0x0F, true)]
        [TestCase(0x44, 0xFF, true)]
        [TestCase(0x44, 0x01, true)]
        [TestCase(0xF4, 0x11, true)]
        [TestCase(0xF4, 0x0F, true)]
        [TestCase(0xF4, 0xFF, true)]
        [TestCase(0xF4, 0x01, true)]
        [TestCase(0x44, 0x11, false)]
        [TestCase(0x44, 0x0F, false)]
        [TestCase(0x44, 0xFF, false)]
        [TestCase(0x44, 0x01, false)]
        [TestCase(0xF4, 0x11, false)]
        [TestCase(0xF4, 0x0F, false)]
        [TestCase(0xF4, 0xFF, false)]
        [TestCase(0xF4, 0x01, false)]
        #endregion
        public void Test_SBC_A_at_HL(byte val, byte val2, bool carry)
        {
            asm.LoadReg16Val(3, 0x0060);
            asm.LoadReg16Val(0, (ushort)(carry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.SbcAAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            if (carry) trueDiff--;
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(byteDiff, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) + (carry ? 1 : 0) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0, true)]
        [TestCase(0x44, 0x0F, 0, true)]
        [TestCase(0x44, 0xFF, 0, true)]
        [TestCase(0x44, 0x01, 0, true)]
        [TestCase(0xF4, 0x11, 0, true)]
        [TestCase(0xF4, 0x0F, 0, true)]
        [TestCase(0xF4, 0xFF, 0, true)]
        [TestCase(0xF4, 0x01, 0, true)]
        [TestCase(0x44, 0x11, 1, true)]
        [TestCase(0x44, 0x0F, 1, true)]
        [TestCase(0x44, 0xFF, 1, true)]
        [TestCase(0x44, 0x01, 1, true)]
        [TestCase(0xF4, 0x11, 1, true)]
        [TestCase(0xF4, 0x0F, 1, true)]
        [TestCase(0xF4, 0xFF, 1, true)]
        [TestCase(0xF4, 0x01, 1, true)]
        [TestCase(0x44, 0x11, -1, true)]
        [TestCase(0x44, 0x0F, -1, true)]
        [TestCase(0x44, 0xFF, -1, true)]
        [TestCase(0x44, 0x01, -1, true)]
        [TestCase(0xF4, 0x11, -1, true)]
        [TestCase(0xF4, 0x0F, -1, true)]
        [TestCase(0xF4, 0xFF, -1, true)]
        [TestCase(0xF4, 0x01, -1, true)]
        [TestCase(0x44, 0x11, 0, false)]
        [TestCase(0x44, 0x0F, 0, false)]
        [TestCase(0x44, 0xFF, 0, false)]
        [TestCase(0x44, 0x01, 0, false)]
        [TestCase(0xF4, 0x11, 0, false)]
        [TestCase(0xF4, 0x0F, 0, false)]
        [TestCase(0xF4, 0xFF, 0, false)]
        [TestCase(0xF4, 0x01, 0, false)]
        [TestCase(0x44, 0x11, 1, false)]
        [TestCase(0x44, 0x0F, 1, false)]
        [TestCase(0x44, 0xFF, 1, false)]
        [TestCase(0x44, 0x01, 1, false)]
        [TestCase(0xF4, 0x11, 1, false)]
        [TestCase(0xF4, 0x0F, 1, false)]
        [TestCase(0xF4, 0xFF, 1, false)]
        [TestCase(0xF4, 0x01, 1, false)]
        [TestCase(0x44, 0x11, -1, false)]
        [TestCase(0x44, 0x0F, -1, false)]
        [TestCase(0x44, 0xFF, -1, false)]
        [TestCase(0x44, 0x01, -1, false)]
        [TestCase(0xF4, 0x11, -1, false)]
        [TestCase(0xF4, 0x0F, -1, false)]
        [TestCase(0xF4, 0xFF, -1, false)]
        [TestCase(0xF4, 0x01, -1, false)]
        #endregion
        public void Test_SBC_A_at_IX(byte val, byte val2, sbyte disp, bool carry)
        {
            asm.LoadReg16Val(3, 0x0060);
            asm.LoadReg16Val(0, (ushort)(carry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIxVal(0x0040);
            asm.SbcAAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            if (carry) trueDiff--;
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(byteDiff, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) + (carry ? 1 : 0) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0, true)]
        [TestCase(0x44, 0x0F, 0, true)]
        [TestCase(0x44, 0xFF, 0, true)]
        [TestCase(0x44, 0x01, 0, true)]
        [TestCase(0xF4, 0x11, 0, true)]
        [TestCase(0xF4, 0x0F, 0, true)]
        [TestCase(0xF4, 0xFF, 0, true)]
        [TestCase(0xF4, 0x01, 0, true)]
        [TestCase(0x44, 0x11, 1, true)]
        [TestCase(0x44, 0x0F, 1, true)]
        [TestCase(0x44, 0xFF, 1, true)]
        [TestCase(0x44, 0x01, 1, true)]
        [TestCase(0xF4, 0x11, 1, true)]
        [TestCase(0xF4, 0x0F, 1, true)]
        [TestCase(0xF4, 0xFF, 1, true)]
        [TestCase(0xF4, 0x01, 1, true)]
        [TestCase(0x44, 0x11, -1, true)]
        [TestCase(0x44, 0x0F, -1, true)]
        [TestCase(0x44, 0xFF, -1, true)]
        [TestCase(0x44, 0x01, -1, true)]
        [TestCase(0xF4, 0x11, -1, true)]
        [TestCase(0xF4, 0x0F, -1, true)]
        [TestCase(0xF4, 0xFF, -1, true)]
        [TestCase(0xF4, 0x01, -1, true)]
        [TestCase(0x44, 0x11, 0, false)]
        [TestCase(0x44, 0x0F, 0, false)]
        [TestCase(0x44, 0xFF, 0, false)]
        [TestCase(0x44, 0x01, 0, false)]
        [TestCase(0xF4, 0x11, 0, false)]
        [TestCase(0xF4, 0x0F, 0, false)]
        [TestCase(0xF4, 0xFF, 0, false)]
        [TestCase(0xF4, 0x01, 0, false)]
        [TestCase(0x44, 0x11, 1, false)]
        [TestCase(0x44, 0x0F, 1, false)]
        [TestCase(0x44, 0xFF, 1, false)]
        [TestCase(0x44, 0x01, 1, false)]
        [TestCase(0xF4, 0x11, 1, false)]
        [TestCase(0xF4, 0x0F, 1, false)]
        [TestCase(0xF4, 0xFF, 1, false)]
        [TestCase(0xF4, 0x01, 1, false)]
        [TestCase(0x44, 0x11, -1, false)]
        [TestCase(0x44, 0x0F, -1, false)]
        [TestCase(0x44, 0xFF, -1, false)]
        [TestCase(0x44, 0x01, -1, false)]
        [TestCase(0xF4, 0x11, -1, false)]
        [TestCase(0xF4, 0x0F, -1, false)]
        [TestCase(0xF4, 0xFF, -1, false)]
        [TestCase(0xF4, 0x01, -1, false)]
        #endregion
        public void Test_SBC_A_at_IY(byte val, byte val2, sbyte disp, bool carry)
        {
            asm.LoadReg16Val(3, 0x0060);
            asm.LoadReg16Val(0, (ushort)(carry ? 1 : 0));
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIyVal(0x0040);
            asm.SbcAAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            if (carry) trueDiff--;
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(byteDiff, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) + (carry ? 1 : 0) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0, 0x44, 0x11)]
        [TestCase(0, 0x44, 0x0F)]
        [TestCase(0, 0x44, 0xFF)]
        [TestCase(0, 0x44, 0x01)]
        [TestCase(0, 0xF4, 0x11)]
        [TestCase(0, 0xF4, 0x0F)]
        [TestCase(0, 0xF4, 0xFF)]
        [TestCase(0, 0xF4, 0x01)]
        [TestCase(1, 0x44, 0x11)]
        [TestCase(1, 0x44, 0x0F)]
        [TestCase(1, 0x44, 0xFF)]
        [TestCase(1, 0x44, 0x01)]
        [TestCase(2, 0x44, 0x11)]
        [TestCase(2, 0x44, 0x0F)]
        [TestCase(2, 0x44, 0xFF)]
        [TestCase(2, 0x44, 0x01)]
        [TestCase(3, 0x44, 0x11)]
        [TestCase(3, 0x44, 0x0F)]
        [TestCase(3, 0x44, 0xFF)]
        [TestCase(3, 0x44, 0x01)]
        [TestCase(4, 0x44, 0x11)]
        [TestCase(4, 0x44, 0x0F)]
        [TestCase(4, 0x44, 0xFF)]
        [TestCase(4, 0x44, 0x01)]
        [TestCase(5, 0x44, 0x11)]
        [TestCase(5, 0x44, 0x0F)]
        [TestCase(5, 0x44, 0xFF)]
        [TestCase(5, 0x44, 0x01)]
        [TestCase(7, 0x44, 0x44)]
        #endregion
        public void Test_AND_A_r(byte reg, byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.LoadRegVal(reg, val2);
            asm.AndReg(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val & val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(true, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }


        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_AND_A_n(byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.AndVal(val2);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val & val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(true, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_AND_A_at_HL(byte val, byte val2)
        {
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.AndAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val & val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(true, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_AND_A_at_IX(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIxVal(0x0040);
            asm.AndAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val & val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(true, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_AND_A_at_IY(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIyVal(0x0040);
            asm.AndAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val & val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(true, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0, 0x44, 0x11)]
        [TestCase(0, 0x44, 0x0F)]
        [TestCase(0, 0x44, 0xFF)]
        [TestCase(0, 0x44, 0x01)]
        [TestCase(0, 0xF4, 0x11)]
        [TestCase(0, 0xF4, 0x0F)]
        [TestCase(0, 0xF4, 0xFF)]
        [TestCase(0, 0xF4, 0x01)]
        [TestCase(1, 0x44, 0x11)]
        [TestCase(1, 0x44, 0x0F)]
        [TestCase(1, 0x44, 0xFF)]
        [TestCase(1, 0x44, 0x01)]
        [TestCase(2, 0x44, 0x11)]
        [TestCase(2, 0x44, 0x0F)]
        [TestCase(2, 0x44, 0xFF)]
        [TestCase(2, 0x44, 0x01)]
        [TestCase(3, 0x44, 0x11)]
        [TestCase(3, 0x44, 0x0F)]
        [TestCase(3, 0x44, 0xFF)]
        [TestCase(3, 0x44, 0x01)]
        [TestCase(4, 0x44, 0x11)]
        [TestCase(4, 0x44, 0x0F)]
        [TestCase(4, 0x44, 0xFF)]
        [TestCase(4, 0x44, 0x01)]
        [TestCase(5, 0x44, 0x11)]
        [TestCase(5, 0x44, 0x0F)]
        [TestCase(5, 0x44, 0xFF)]
        [TestCase(5, 0x44, 0x01)]
        [TestCase(7, 0x44, 0x44)]
        #endregion
        public void Test_OR_A_r(byte reg, byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.LoadRegVal(reg, val2);
            asm.OrReg(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val | val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }


        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_OR_A_n(byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.OrVal(val2);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val | val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_OR_A_at_HL(byte val, byte val2)
        {
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.OrAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val | val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_OR_A_at_IX(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIxVal(0x0040);
            asm.OrAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val | val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_OR_A_at_IY(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIyVal(0x0040);
            asm.OrAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val | val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0, 0x44, 0x11)]
        [TestCase(0, 0x44, 0x0F)]
        [TestCase(0, 0x44, 0xFF)]
        [TestCase(0, 0x44, 0x01)]
        [TestCase(0, 0xF4, 0x11)]
        [TestCase(0, 0xF4, 0x0F)]
        [TestCase(0, 0xF4, 0xFF)]
        [TestCase(0, 0xF4, 0x01)]
        [TestCase(1, 0x44, 0x11)]
        [TestCase(1, 0x44, 0x0F)]
        [TestCase(1, 0x44, 0xFF)]
        [TestCase(1, 0x44, 0x01)]
        [TestCase(2, 0x44, 0x11)]
        [TestCase(2, 0x44, 0x0F)]
        [TestCase(2, 0x44, 0xFF)]
        [TestCase(2, 0x44, 0x01)]
        [TestCase(3, 0x44, 0x11)]
        [TestCase(3, 0x44, 0x0F)]
        [TestCase(3, 0x44, 0xFF)]
        [TestCase(3, 0x44, 0x01)]
        [TestCase(4, 0x44, 0x11)]
        [TestCase(4, 0x44, 0x0F)]
        [TestCase(4, 0x44, 0xFF)]
        [TestCase(4, 0x44, 0x01)]
        [TestCase(5, 0x44, 0x11)]
        [TestCase(5, 0x44, 0x0F)]
        [TestCase(5, 0x44, 0xFF)]
        [TestCase(5, 0x44, 0x01)]
        [TestCase(7, 0x44, 0x44)]
        #endregion
        public void Test_XOR_A_r(byte reg, byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.LoadRegVal(reg, val2);
            asm.XorReg(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val ^ val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }


        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_XOR_A_n(byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.XorVal(val2);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val ^ val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_XOR_A_at_HL(byte val, byte val2)
        {
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.XorAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val ^ val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_XOR_A_at_IX(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIxVal(0x0040);
            asm.XorAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val ^ val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_XOR_A_at_IY(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIyVal(0x0040);
            asm.XorAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);

            var res = (byte)(val ^ val2);
            var sres = (sbyte)res;
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(sres < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var parity = Countbits(res) % 2 == 0;
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagC, "Flag C contained the wrong value");

        }


        [Test]
        #region testcases
        [TestCase(0, 0x44, 0x11)]
        [TestCase(0, 0x44, 0x0F)]
        [TestCase(0, 0x44, 0xFF)]
        [TestCase(0, 0x44, 0x01)]
        [TestCase(0, 0xF4, 0x11)]
        [TestCase(0, 0xF4, 0x0F)]
        [TestCase(0, 0xF4, 0xFF)]
        [TestCase(0, 0xF4, 0x01)]
        [TestCase(1, 0x44, 0x11)]
        [TestCase(1, 0x44, 0x0F)]
        [TestCase(1, 0x44, 0xFF)]
        [TestCase(1, 0x44, 0x01)]
        [TestCase(2, 0x44, 0x11)]
        [TestCase(2, 0x44, 0x0F)]
        [TestCase(2, 0x44, 0xFF)]
        [TestCase(2, 0x44, 0x01)]
        [TestCase(3, 0x44, 0x11)]
        [TestCase(3, 0x44, 0x0F)]
        [TestCase(3, 0x44, 0xFF)]
        [TestCase(3, 0x44, 0x01)]
        [TestCase(4, 0x44, 0x11)]
        [TestCase(4, 0x44, 0x0F)]
        [TestCase(4, 0x44, 0xFF)]
        [TestCase(4, 0x44, 0x01)]
        [TestCase(5, 0x44, 0x11)]
        [TestCase(5, 0x44, 0x0F)]
        [TestCase(5, 0x44, 0xFF)]
        [TestCase(5, 0x44, 0x01)]
        [TestCase(7, 0x44, 0x44)]
        #endregion
        public void Test_CP_A_r(byte reg, byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.LoadRegVal(reg, val2);
            asm.CpReg(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(val, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(val == val2, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_CP_A_n(byte val, byte val2)
        {
            asm.LoadRegVal(7, val);
            asm.CpVal(val2);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(val, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(val == val2, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11)]
        [TestCase(0x44, 0x0F)]
        [TestCase(0x44, 0xFF)]
        [TestCase(0x44, 0x01)]
        [TestCase(0xF4, 0x11)]
        [TestCase(0xF4, 0x0F)]
        [TestCase(0xF4, 0xFF)]
        [TestCase(0xF4, 0x01)]
        #endregion
        public void Test_CP_A_at_HL(byte val, byte val2)
        {
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.CpAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(val, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(val == val2, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_CP_A_at_IX(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIxVal(0x0040);
            asm.CpAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(val, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(val == val2, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0x44, 0x11, 0)]
        [TestCase(0x44, 0x0F, 0)]
        [TestCase(0x44, 0xFF, 0)]
        [TestCase(0x44, 0x01, 0)]
        [TestCase(0xF4, 0x11, 0)]
        [TestCase(0xF4, 0x0F, 0)]
        [TestCase(0xF4, 0xFF, 0)]
        [TestCase(0xF4, 0x01, 0)]
        [TestCase(0x44, 0x11, 1)]
        [TestCase(0x44, 0x0F, 1)]
        [TestCase(0x44, 0xFF, 1)]
        [TestCase(0x44, 0x01, 1)]
        [TestCase(0xF4, 0x11, 1)]
        [TestCase(0xF4, 0x0F, 1)]
        [TestCase(0xF4, 0xFF, 1)]
        [TestCase(0xF4, 0x01, 1)]
        [TestCase(0x44, 0x11, -1)]
        [TestCase(0x44, 0x0F, -1)]
        [TestCase(0x44, 0xFF, -1)]
        [TestCase(0x44, 0x01, -1)]
        [TestCase(0xF4, 0x11, -1)]
        [TestCase(0xF4, 0x0F, -1)]
        [TestCase(0xF4, 0xFF, -1)]
        [TestCase(0xF4, 0x01, -1)]
        #endregion
        public void Test_CP_A_at_IY(byte val, byte val2, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val2);
            asm.LoadRegVal(7, val);
            asm.LoadIyVal(0x0040);
            asm.CpAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueDiff = (val - val2);
            var byteDiff = (byte)trueDiff % 256;
            var sbyteDiff = (sbyte)byteDiff;
            Assert.AreEqual(val, en.A);
            Assert.AreEqual(sbyteDiff < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH, "Flag H contained the wrong value");
            var overflow = (val >= 0x80 && val2 < 0x80 && sbyteDiff >= 0) || (val < 0x80 && val2 >= 0x80 && sbyteDiff < 0); // subtraction overflow: sign mismatch between operands and result differs from minuend
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueDiff < 0, en.FlagC, "Flag C contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0, 0x28)]
        [TestCase(0, 0x7F)]
        [TestCase(1, 0x28)]
        [TestCase(1, 0x7F)]
        [TestCase(2, 0x28)]
        [TestCase(2, 0x7F)]
        [TestCase(3, 0x28)]
        [TestCase(3, 0x7F)]
        [TestCase(4, 0x28)]
        [TestCase(4, 0x7F)]
        [TestCase(5, 0x28)]
        [TestCase(5, 0x7F)]
        [TestCase(7, 0x28)]
        [TestCase(7, 0x7F)]
        #endregion
        public void Test_INC_r(byte reg, byte val)
        {
            asm.LoadRegVal(reg, val);
            asm.IncReg(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = val + 1;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.Reg8(reg));
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.Reg8(reg) == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((val & 0x0F) == 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = val == 0x7F;
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.IsFalse(en.FlagN, "INC should reset N flag");
            // INC does not affect carry flag (not checked here)

        }

        [Test]
        #region testcases
        [TestCase(0x28)]
        [TestCase(0x7F)]
        #endregion
        public void Test_INC_at_HL(byte val)
        {
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(val);
            asm.IncAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (1 + val);
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, _ram[0x0040]);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(_ram[0x0040] == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((val & 0x0F) == 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = val == 0x7F;
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.IsFalse(en.FlagN, "INC should reset N flag");
            // INC does not affect carry flag

        }

        [Test]
        #region testcases
        [TestCase(0x28, 0)]
        [TestCase(0x7F, 0)]
        [TestCase(0x28, 1)]
        [TestCase(0x7F, 1)]
        [TestCase(0x28, -1)]
        [TestCase(0x7F, -1)]
        #endregion
        public void Test_INC_at_IX(byte val, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val);
            asm.LoadIxVal(0x0040);
            asm.IncAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (1 + val);
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, _ram[0x0040 + disp]);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(byteSum == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((val & 0x0F) == 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = val == 0x7F;
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.IsFalse(en.FlagN, "INC should reset N flag");
            // INC does not affect carry flag

        }
        [Test]
        #region testcases
        [TestCase(0x28, 0)]
        [TestCase(0x7F, 0)]
        [TestCase(0x28, 1)]
        [TestCase(0x7F, 1)]
        [TestCase(0x28, -1)]
        [TestCase(0x7F, -1)]
        #endregion
        public void Test_INC_at_IY(byte val, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val);
            asm.LoadIyVal(0x0040);
            asm.IncAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = (1 + val);
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, _ram[0x0040 + disp]);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(byteSum == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((val & 0x0F) == 0x0F, en.FlagH, "Flag H contained the wrong value");
            var overflow = val == 0x7F;
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.IsFalse(en.FlagN, "INC should reset N flag");
            // INC does not affect carry flag

        }

        [Test]
        #region testcases
        [TestCase(0, 0x28)]
        [TestCase(0, 0x80)]
        [TestCase(1, 0x28)]
        [TestCase(1, 0x80)]
        [TestCase(2, 0x28)]
        [TestCase(2, 0x80)]
        [TestCase(3, 0x28)]
        [TestCase(3, 0x80)]
        [TestCase(4, 0x28)]
        [TestCase(4, 0x80)]
        [TestCase(5, 0x28)]
        [TestCase(5, 0x80)]
        [TestCase(7, 0x28)]
        [TestCase(7, 0x80)]
        #endregion
        public void Test_DEC_r(byte reg, byte val)
        {
            asm.LoadRegVal(reg, val);
            asm.DecReg(reg);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = val - 1;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.Reg8(reg));
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.Reg8(reg) == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val) == 0, en.FlagH, "Flag H contained the wrong value");
            var overflow = val == 0x80;
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.IsTrue(en.FlagN, "DEC should set N flag");
            // DEC does not affect carry flag

        }

        [Test]
        #region testcases
        [TestCase(0x28)]
        [TestCase(0x80)]
        #endregion
        public void Test_DEC_at_HL(byte val)
        {
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(val);
            asm.DecAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = val - 1;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, _ram[0x0040]);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(_ram[0x0040] == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val) == 0, en.FlagH, "Flag H contained the wrong value");
            var overflow = val == 0x80;
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.IsTrue(en.FlagN, "DEC should set N flag");
            // DEC does not affect carry flag

        }

        [Test]
        #region testcases
        [TestCase(0x28, 0)]
        [TestCase(0x80, 0)]
        [TestCase(0x28, 1)]
        [TestCase(0x80, 1)]
        [TestCase(0x28, -1)]
        [TestCase(0x80, -1)]
        #endregion
        public void Test_DEC_at_IX(byte val, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val);
            asm.LoadIxVal(0x0040);
            asm.DecAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = val - 1;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, _ram[0x0040 + disp]);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(byteSum == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val) == 0, en.FlagH, "Flag H contained the wrong value");
            var overflow = val == 0x80;
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.IsTrue(en.FlagN, "DEC should set N flag");
            // DEC does not affect carry flag

        }
        [Test]
        #region testcases
        [TestCase(0x28, 0)]
        [TestCase(0x80, 0)]
        [TestCase(0x28, 1)]
        [TestCase(0x80, 1)]
        [TestCase(0x28, -1)]
        [TestCase(0x80, -1)]
        #endregion
        public void Test_DEC_at_IY(byte val, sbyte disp)
        {
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(val);
            asm.LoadIyVal(0x0040);
            asm.DecAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = val - 1;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, _ram[0x0040 + disp]);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(byteSum == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((0x0F & val) == 0, en.FlagH, "Flag H contained the wrong value");
            var overflow = val == 0x80;
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.IsTrue(en.FlagN, "DEC should set N flag");
            // DEC does not affect carry flag

        }
    }
}