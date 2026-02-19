using NUnit.Framework;

namespace z80.Tests
{
    /// <summary>
    /// Regression tests for 8-bit arithmetic flag bugs.
    /// Each test targets a specific bug found against Z80 specifications.
    /// </summary>
    [TestFixture]
    public class BugFixTests_8BitArithmetic : OpCodeTestBase
    {
        // ============================================================
        // Bug: ADD half-carry operator precedence
        // The expression (a & 0xF + b & 0xF) evaluates as a & (0xF + b) & 0xF
        // due to + having higher precedence than &.
        // Fix: ((a & 0xF) + (b & 0xF)) > 0xF
        // ============================================================

        [Test]
        public void Test_ADD_HalfCarry_Should_Be_Set_When_Low_Nibbles_Overflow()
        {
            // A = 0x08, B = 0x09: low nibbles 8+9=17 > 15, H should be set
            asm.LoadRegVal(7, 0x08);  // A = 0x08
            asm.LoadRegVal(0, 0x09);  // B = 0x09
            asm.AddAReg(0);           // ADD A, B
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x11, en.A, "Result should be 0x11");
            Assert.IsTrue(en.FlagH, "H flag should be set: low nibble 8+9=17 overflows");
        }

        [Test]
        public void Test_ADD_HalfCarry_Should_Be_Clear_When_Low_Nibbles_Dont_Overflow()
        {
            // A = 0x12, B = 0x03: low nibbles 2+3=5 <= 15, H should be clear
            asm.LoadRegVal(7, 0x12);  // A = 0x12
            asm.LoadRegVal(0, 0x03);  // B = 0x03
            asm.AddAReg(0);           // ADD A, B
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x15, en.A, "Result should be 0x15");
            Assert.IsFalse(en.FlagH, "H flag should be clear: low nibble 2+3=5 doesn't overflow");
        }

        [Test]
        public void Test_ADD_HalfCarry_0x0F_Plus_0x01()
        {
            // A = 0x0F, B = 0x01: low nibbles F+1=16 > 15, H should be set
            asm.LoadRegVal(7, 0x0F);
            asm.LoadRegVal(0, 0x01);
            asm.AddAReg(0);
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x10, en.A);
            Assert.IsTrue(en.FlagH, "H flag should be set: F+1=16 overflows nibble");
        }

        // ============================================================
        // Bug: ADC half-carry operator precedence + missing carry
        // Same precedence bug as ADD, plus carry not included in half-carry calc.
        // Fix: ((a & 0xF) + (b & 0xF) + c) > 0xF
        // ============================================================

        [Test]
        public void Test_ADC_HalfCarry_With_Carry_Set()
        {
            // Set carry flag first via SCF, then ADC
            // A = 0x0E, B = 0x01, C=1: low nibbles E+1+1=16 > 15, H should be set
            asm.LoadRegVal(7, 0x0E);  // A = 0x0E
            asm.LoadRegVal(0, 0x01);  // B = 0x01
            asm.Scf();                // Set carry
            asm.AdcAReg(0);           // ADC A, B (with carry)
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x10, en.A, "Result should be 0x0E + 0x01 + 1 = 0x10");
            Assert.IsTrue(en.FlagH, "H flag should be set: E+1+1=16 overflows nibble");
        }

        // ============================================================
        // Bug: SUB overflow uses addition logic instead of subtraction
        // For SUB, overflow occurs when: pos - neg = neg, or neg - pos = pos
        // The code uses: (a >= 0x80 && b >= 0x80 && result > 0) || (a < 0x80 && b < 0x80 && result < 0)
        // which is the ADD overflow formula.
        // ============================================================

        [Test]
        public void Test_SUB_Overflow_Positive_Minus_Negative_Equals_Negative()
        {
            // A = 0x44 (68, positive), B = 0xFF (-1, negative)
            // 68 - (-1) = 69, but wraps to 0x45 (positive, no overflow)
            // Wait... 0x44 - 0xFF = 0x44 - 0xFF = -187 = 0x45 with borrow
            // Signed: 68 - (-1) = 69 = 0x45, fits in signed range, NO overflow
            // Actually let me use a clearer case:
            // A = 0x50 (80), B = 0xB0 (-80): 80 - (-80) = 160 = 0xA0 (negative!) = overflow
            asm.LoadRegVal(7, 0x50);  // A = 0x50 (+80)
            asm.LoadRegVal(0, 0xB0);  // B = 0xB0 (-80)
            asm.SubReg(0);            // SUB B
            asm.Halt();

            en.Run();

            Assert.AreEqual(0xA0, en.A, "0x50 - 0xB0 = 0xA0");
            Assert.IsTrue(en.FlagP, "Overflow: positive - negative = negative (80 - (-80) = 160, overflows signed byte)");
        }

        [Test]
        public void Test_SUB_Overflow_Negative_Minus_Positive_Equals_Positive()
        {
            // A = 0x80 (-128), B = 0x01 (1): -128 - 1 = -129, overflows
            // Result: 0x7F (+127)
            asm.LoadRegVal(7, 0x80);  // A = 0x80 (-128)
            asm.LoadRegVal(0, 0x01);  // B = 0x01
            asm.SubReg(0);            // SUB B
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x7F, en.A, "0x80 - 0x01 = 0x7F");
            Assert.IsTrue(en.FlagP, "Overflow: -128 - 1 = -129 doesn't fit in signed byte");
        }

        [Test]
        public void Test_SUB_No_Overflow_Same_Signs()
        {
            // A = 0x44 (68), B = 0x11 (17): 68 - 17 = 51, no overflow
            asm.LoadRegVal(7, 0x44);
            asm.LoadRegVal(0, 0x11);
            asm.SubReg(0);
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x33, en.A, "0x44 - 0x11 = 0x33");
            Assert.IsFalse(en.FlagP, "No overflow when subtracting same-sign values within range");
        }

        // ============================================================
        // Bug: SBC carry flag should be set when diff < 0 (borrow)
        // Code has: if (diff > 0xFF) which is wrong for subtraction
        // ============================================================

        [Test]
        public void Test_SBC_Carry_Set_On_Borrow()
        {
            // A = 0x10, B = 0x20, C=0: 0x10 - 0x20 = -16 (borrow), C should be set
            asm.LoadRegVal(7, 0x10);
            asm.LoadRegVal(0, 0x20);
            asm.AndVal(0xFF);   // Clear carry by doing AND 0xFF (resets C)
            asm.LoadRegVal(7, 0x10);  // Reload A since AND modified it
            asm.SbcAReg(0);
            asm.Halt();

            en.Run();

            Assert.AreEqual(0xF0, en.A, "0x10 - 0x20 = 0xF0");
            Assert.IsTrue(en.FlagC, "Carry should be set when borrow occurs (result < 0)");
        }

        [Test]
        public void Test_SBC_Carry_Clear_No_Borrow()
        {
            // A = 0x20, B = 0x10, C=0: 0x20 - 0x10 = 0x10 (no borrow)
            asm.LoadRegVal(7, 0x20);
            asm.LoadRegVal(0, 0x10);
            asm.AndVal(0xFF);   // Clear carry
            asm.LoadRegVal(7, 0x20);  // Reload A
            asm.SbcAReg(0);
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x10, en.A, "0x20 - 0x10 = 0x10");
            Assert.IsFalse(en.FlagC, "Carry should be clear when no borrow occurs");
        }

        // ============================================================
        // Bug: INC sets N flag (should be reset, INC is addition)
        // Bug: INC clears carry flag (should preserve it)
        // ============================================================

        [Test]
        public void Test_INC_N_Flag_Should_Be_Clear()
        {
            // INC is an addition operation, N should be 0
            asm.LoadRegVal(0, 0x05);  // B = 5
            asm.IncReg(0);            // INC B
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x06, en.B);
            Assert.IsFalse(en.FlagN, "N flag should be clear after INC (addition operation)");
        }

        [Test]
        public void Test_INC_Should_Preserve_Carry_Flag()
        {
            // Set carry first, then INC should not affect it
            asm.LoadRegVal(7, 0xFF);  // A = 0xFF
            asm.AddAVal(0x01);        // ADD A, 1 -> sets carry (0xFF + 1 = 0x100)
            asm.LoadRegVal(0, 0x05);  // B = 5
            asm.IncReg(0);            // INC B -> should preserve carry
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x06, en.B);
            Assert.IsTrue(en.FlagC, "Carry flag should be preserved by INC");
        }

        [Test]
        public void Test_INC_Should_Not_Set_Carry_On_Wrap()
        {
            // INC 0xFF = 0x00, carry should NOT be set (INC doesn't affect carry)
            asm.LoadRegVal(7, 0x00);
            asm.LoadRegVal(0, 0x00);
            asm.SubReg(0);            // SUB B to clear carry (0-0=0, C=0)
            asm.LoadRegVal(0, 0xFF);  // B = 0xFF
            asm.IncReg(0);            // INC B = 0x00
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x00, en.B);
            Assert.IsFalse(en.FlagC, "Carry should not be affected by INC, even on 0xFF->0x00 wrap");
        }

        // ============================================================
        // Bug: DEC clears carry flag (should preserve it)
        // ============================================================

        [Test]
        public void Test_DEC_Should_Preserve_Carry_Flag()
        {
            // Set carry first, then DEC should not affect it
            asm.LoadRegVal(7, 0xFF);
            asm.AddAVal(0x01);        // Sets carry
            asm.LoadRegVal(0, 0x05);
            asm.DecReg(0);            // DEC B -> should preserve carry
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x04, en.B);
            Assert.IsTrue(en.FlagC, "Carry flag should be preserved by DEC");
        }

        // ============================================================
        // Bug: CP overflow uses > 0x80 instead of >= 0x80
        // 0x80 is -128, a negative number, so it should be included
        // ============================================================

        [Test]
        public void Test_CP_Overflow_With_0x80()
        {
            // A = 0x80 (-128), compare with 0x01 (1)
            // -128 - 1 = -129, overflows signed byte, P/V should be set
            asm.LoadRegVal(7, 0x80);  // A = 0x80 (-128)
            asm.LoadRegVal(0, 0x01);  // B = 0x01
            asm.CpReg(0);            // CP B
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x80, en.A, "CP should not modify A");
            Assert.IsTrue(en.FlagP, "Overflow: -128 - 1 = -129 overflows signed byte");
        }

        [Test]
        public void Test_CP_Overflow_With_Both_0x80()
        {
            // A = 0x80 (-128), compare with 0x80 (-128)
            // -128 - (-128) = 0, no overflow
            asm.LoadRegVal(7, 0x80);
            asm.CpReg(7);            // CP A (compare A with itself)
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x80, en.A);
            Assert.IsFalse(en.FlagP, "No overflow: -128 - (-128) = 0");
            Assert.IsTrue(en.FlagZ, "Zero: A == A");
        }
    }
}
