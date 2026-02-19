using NUnit.Framework;

namespace z80.Tests
{
    /// <summary>
    /// Regression tests for bug fixes:
    /// - 16-bit ADC HL: half-carry uses wrong carry value, wrong overflow detection
    /// - BIT instruction: missing S and P/V flags
    /// - CCF: missing H flag from previous C
    /// - CPI/CPD: wrong sign flag and half-carry calculations
    /// </summary>
    [TestFixture]
    public class BugFixTests_16BitAndMisc : OpCodeTestBase
    {
        #region ADC HL Tests

        [Test]
        public void Test_ADC_HL_BC_HalfCarry_WithCarryFlag()
        {
            // ADC HL, BC should include carry flag in half-carry calculation
            // HL = 0x0F00, BC = 0x0100, C = 1
            // 0x0F00 + 0x0100 + 1 = 0x1001
            // Half-carry: (0x0F00 & 0x0FFF) + (0x0100 & 0x0FFF) + 1 = 0x0F00 + 0x0100 + 1 = 0x1001 > 0x0FFF, so H = 1
            asm.LoadReg16Val(2, 0x0F00);  // HL = 0x0F00
            asm.LoadReg16Val(0, 0x0100);  // BC = 0x0100
            asm.Scf();                    // Set carry flag
            asm.AdcHlReg16(0);            // ADC HL, BC
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x1001, en.HL, "Result should be 0x1001");
            Assert.IsTrue(en.FlagH, "Half-carry should be set (0x0F00 + 0x0100 + 1 overflows bit 11)");
        }

        [Test]
        public void Test_ADC_HL_BC_HalfCarry_WithoutCarryFlag()
        {
            // ADC HL, BC without carry should not set half-carry if sum doesn't overflow bit 11
            // HL = 0x0F00, BC = 0x00FF, C = 0
            // 0x0F00 + 0x00FF = 0x0FFF, no half-carry
            asm.LoadReg16Val(2, 0x0F00);  // HL = 0x0F00
            asm.LoadReg16Val(0, 0x00FF);  // BC = 0x00FF
            asm.AndVal(0xFF);             // Clear carry (AND sets C = 0)
            asm.AdcHlReg16(0);            // ADC HL, BC
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x0FFF, en.HL, "Result should be 0x0FFF");
            Assert.IsFalse(en.FlagH, "Half-carry should NOT be set");
        }

        [Test]
        public void Test_ADC_HL_Overflow_Positive_Plus_Positive_To_Negative()
        {
            // Signed overflow: 0x7F00 + 0x0100 = 0x8000 (positive + positive = negative)
            asm.LoadReg16Val(2, 0x7F00);  // HL = 0x7F00
            asm.LoadReg16Val(0, 0x0100);  // BC = 0x0100
            asm.AndVal(0xFF);             // Clear carry
            asm.AdcHlReg16(0);            // ADC HL, BC
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x8000, en.HL, "Result should be 0x8000");
            Assert.IsTrue(en.FlagP, "Overflow should be set (positive + positive = negative)");
        }

        [Test]
        public void Test_ADC_HL_No_Overflow_Different_Signs()
        {
            // No overflow: 0x8000 + 0x0100 = 0x8100 (negative + positive = negative)
            asm.LoadReg16Val(2, 0x8000);  // HL = 0x8000
            asm.LoadReg16Val(0, 0x0100);  // BC = 0x0100
            asm.AndVal(0xFF);             // Clear carry
            asm.AdcHlReg16(0);            // ADC HL, BC
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x8100, en.HL, "Result should be 0x8100");
            Assert.IsFalse(en.FlagP, "Overflow should NOT be set (different signs)");
        }

        [Test]
        public void Test_ADC_HL_Zero_Flag_On_16bit_Zero()
        {
            // 0xFFFF + 0x0001 = 0x10000, truncated to 0x0000
            asm.LoadReg16Val(2, 0xFFFF);  // HL = 0xFFFF
            asm.LoadReg16Val(0, 0x0001);  // BC = 0x0001
            asm.AndVal(0xFF);             // Clear carry
            asm.AdcHlReg16(0);            // ADC HL, BC
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x0000, en.HL, "Result should be 0x0000");
            Assert.IsTrue(en.FlagZ, "Zero flag should be set");
            Assert.IsTrue(en.FlagC, "Carry should be set");
        }

        #endregion

        #region BIT Instruction Tests

        [Test]
        public void Test_BIT_7_Sets_Sign_Flag_When_Bit7_Set()
        {
            // BIT 7, A with A = 0x80 should set S flag
            asm.LoadRegVal(7, 0x80);  // A = 0x80 (bit 7 set)
            asm.BitNReg(7, 7);        // BIT 7, A
            asm.Halt();

            en.Run();

            Assert.IsFalse(en.FlagZ, "Z should be clear (bit 7 is set)");
            Assert.IsTrue(en.FlagS, "S should be set when testing bit 7 that is set");
        }

        [Test]
        public void Test_BIT_7_Clears_Sign_Flag_When_Bit7_Clear()
        {
            // BIT 7, A with A = 0x7F should not set S flag
            asm.LoadRegVal(7, 0x7F);  // A = 0x7F (bit 7 clear)
            asm.BitNReg(7, 7);        // BIT 7, A
            asm.Halt();

            en.Run();

            Assert.IsTrue(en.FlagZ, "Z should be set (bit 7 is clear)");
            Assert.IsFalse(en.FlagS, "S should be clear when testing bit 7 that is clear");
        }

        [Test]
        public void Test_BIT_Sets_PV_Same_As_Z()
        {
            // BIT n, r sets P/V = Z (both reflect whether tested bit is 0)
            asm.LoadRegVal(7, 0x00);  // A = 0x00
            asm.BitNReg(3, 7);        // BIT 3, A - bit 3 is 0
            asm.Halt();

            en.Run();

            Assert.IsTrue(en.FlagZ, "Z should be set (bit is 0)");
            Assert.IsTrue(en.FlagP, "P/V should be set same as Z");
        }

        [Test]
        public void Test_BIT_Clears_PV_When_Bit_Set()
        {
            // BIT n, r with bit set: Z = 0, P/V = 0
            asm.LoadRegVal(7, 0x08);  // A = 0x08 (bit 3 set)
            asm.BitNReg(3, 7);        // BIT 3, A
            asm.Halt();

            en.Run();

            Assert.IsFalse(en.FlagZ, "Z should be clear (bit is set)");
            Assert.IsFalse(en.FlagP, "P/V should be clear same as Z");
        }

        #endregion

        #region CCF Tests

        [Test]
        public void Test_CCF_Sets_H_From_Previous_Carry()
        {
            // CCF with C=1 should set H=1 (H copies previous C)
            asm.Scf();      // Set carry
            asm.Ccf();      // Complement carry (C becomes 0, H should become 1)
            asm.Halt();

            en.Run();

            Assert.IsFalse(en.FlagC, "C should be 0 after CCF");
            Assert.IsTrue(en.FlagH, "H should be set (was previous C value)");
        }

        [Test]
        public void Test_CCF_Clears_H_When_Carry_Was_Clear()
        {
            // CCF with C=0 should set H=0
            asm.AndVal(0xFF);  // Clear carry
            asm.Ccf();         // Complement carry (C becomes 1, H should become 0)
            asm.Halt();

            en.Run();

            Assert.IsTrue(en.FlagC, "C should be 1 after CCF");
            Assert.IsFalse(en.FlagH, "H should be clear (was previous C value)");
        }

        #endregion

        #region CPI/CPD Tests

        [Test]
        public void Test_CPI_Sign_Flag_From_Result()
        {
            // CPI: A = 0x10, (HL) = 0x20, result = 0x10 - 0x20 = 0xF0 (negative)
            // S should be set based on result MSB, not A < B comparison
            asm.LoadRegVal(7, 0x10);      // A = 0x10
            asm.LoadReg16Val(2, 0x1000);  // HL = 0x1000
            _ram[0x1000] = 0x20;
            asm.Cpi();
            asm.Halt();

            en.Run();

            // 0x10 - 0x20 = 0xF0 (as byte), MSB is set, so S should be 1
            Assert.IsTrue(en.FlagS, "S flag should be set (result 0xF0 has MSB set)");
        }

        [Test]
        public void Test_CPI_Sign_Flag_Clear_When_Result_Positive()
        {
            // CPI: A = 0x80, (HL) = 0x10, result = 0x80 - 0x10 = 0x70 (positive)
            // Even though A > B, result is positive, so S should be clear
            asm.LoadRegVal(7, 0x80);      // A = 0x80
            asm.LoadReg16Val(2, 0x1000);  // HL = 0x1000
            _ram[0x1000] = 0x10;
            asm.Cpi();
            asm.Halt();

            en.Run();

            // 0x80 - 0x10 = 0x70, MSB is clear, so S should be 0
            Assert.IsFalse(en.FlagS, "S flag should be clear (result 0x70 has MSB clear)");
        }

        [Test]
        public void Test_CPI_HalfCarry_Correct_Calculation()
        {
            // CPI: A = 0x10, (HL) = 0x01, result = 0x0F
            // Half-carry: (0x10 & 0x0F) - (0x01 & 0x0F) = 0 - 1, borrow from bit 4, H = 1
            asm.LoadRegVal(7, 0x10);      // A = 0x10
            asm.LoadReg16Val(2, 0x1000);  // HL = 0x1000
            _ram[0x1000] = 0x01;
            asm.Cpi();
            asm.Halt();

            en.Run();

            Assert.IsTrue(en.FlagH, "H flag should be set (borrow from bit 4)");
        }

        [Test]
        public void Test_CPI_HalfCarry_Not_Set_When_No_Borrow()
        {
            // CPI: A = 0x18, (HL) = 0x01, result = 0x17
            // Half-carry: (0x18 & 0x0F) - (0x01 & 0x0F) = 8 - 1 = 7, no borrow, H = 0
            asm.LoadRegVal(7, 0x18);      // A = 0x18
            asm.LoadReg16Val(2, 0x1000);  // HL = 0x1000
            _ram[0x1000] = 0x01;
            asm.Cpi();
            asm.Halt();

            en.Run();

            Assert.IsFalse(en.FlagH, "H flag should be clear (no borrow from bit 4)");
        }

        #endregion
    }
}
