using System;
using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class EightBitArithmeticGroupTests : OpCodeTestBase
    {
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
            Assert.AreEqual(sbyteSum < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2 + 0x0F & val) > 0x0F, en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueSum > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteSum < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2 + 0x0F & val) > 0x0F, en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueSum > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteSum < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2 + 0x0F & val) > 0x0F, en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueSum > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteSum < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2 + 0x0F & val) > 0x0F, en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueSum > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteSum < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2 + 0x0F & val) > 0x0F, en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueSum > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteSum < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2 + 0x0F & val) > 0x0F, en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueSum > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteSum < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2 + 0x0F & val) > 0x0F, en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueSum > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteSum < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2 + 0x0F & val) > 0x0F, en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueSum > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteSum < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2 + 0x0F & val) > 0x0F, en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueSum > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteSum < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2 + 0x0F & val) > 0x0F, en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteSum < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueSum > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteDiff < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteDiff < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueDiff > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteDiff < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteDiff < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueDiff > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteDiff < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteDiff < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueDiff > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteDiff < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteDiff < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueDiff > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteDiff < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2) > (0x0F & val), en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteDiff < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueDiff > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteDiff < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2) + (carry ? 1 : 0) > (0x0F & val), en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteDiff < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueDiff > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteDiff < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2) + (carry ? 1 : 0) > (0x0F & val), en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteDiff < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueDiff > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteDiff < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2) + (carry ? 1 : 0) > (0x0F & val), en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteDiff < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueDiff > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteDiff < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2) + (carry ? 1 : 0) > (0x0F & val), en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteDiff < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueDiff > 0xFF, en.FlagC);

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
            Assert.AreEqual(sbyteDiff < 0, en.FlagS);
            Assert.AreEqual(en.A == 0x00, en.FlagZ);
            Assert.AreEqual((0x0F & val2) + (carry ? 1 : 0) > (0x0F & val), en.FlagH);
            var overflow = (val < 0x7F == val2 < 0x7F) && (val < 0x7F == sbyteDiff < 0); // if both operands are positive and result is negative or if both are negative and result is positive
            Assert.AreEqual(overflow, en.FlagP);
            Assert.AreEqual(trueDiff > 0xFF, en.FlagC);

        }
    }
}