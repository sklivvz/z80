using NUnit.Framework;

namespace z80.Tests
{
    /// <summary>
    /// Regression tests for rotation and DAA instruction bugs.
    /// </summary>
    [TestFixture]
    public class BugFixTests_RotationDAA : OpCodeTestBase
    {
        // ============================================================
        // Bug: RLCA acts as shift-left instead of rotate-left
        // Bit 7 should go to BOTH carry AND bit 0
        // ============================================================

        [Test]
        public void Test_RLCA_Bit7_Goes_To_Bit0()
        {
            // A = 0x81 (10000001): rotate left → bit 7 (1) goes to bit 0 and carry
            // Result should be 0x03 (00000011), C=1
            asm.LoadRegVal(7, 0x81);
            asm.Rlca();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x03, en.A, "RLCA: 0x81 rotated left should be 0x03 (bit 7 goes to bit 0)");
            Assert.IsTrue(en.FlagC, "Carry should be set (old bit 7 was 1)");
        }

        [Test]
        public void Test_RLCA_No_Carry()
        {
            // A = 0x01 (00000001): rotate left → bit 7 (0) goes to bit 0 and carry
            // Result should be 0x02, C=0
            asm.LoadRegVal(7, 0x01);
            asm.Rlca();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x02, en.A, "RLCA: 0x01 rotated left should be 0x02");
            Assert.IsFalse(en.FlagC, "Carry should be clear (old bit 7 was 0)");
        }

        [Test]
        public void Test_RLCA_All_Ones()
        {
            // A = 0xFF: rotate left → should stay 0xFF, C=1
            asm.LoadRegVal(7, 0xFF);
            asm.Rlca();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0xFF, en.A, "RLCA: 0xFF rotated left should be 0xFF");
            Assert.IsTrue(en.FlagC, "Carry should be set");
        }

        // ============================================================
        // Bug: RRCA acts as shift-right instead of rotate-right
        // Bit 0 should go to BOTH carry AND bit 7
        // ============================================================

        [Test]
        public void Test_RRCA_Bit0_Goes_To_Bit7()
        {
            // A = 0x81 (10000001): rotate right → bit 0 (1) goes to bit 7 and carry
            // Result should be 0xC0 (11000000), C=1
            asm.LoadRegVal(7, 0x81);
            asm.Rrca();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0xC0, en.A, "RRCA: 0x81 rotated right should be 0xC0 (bit 0 goes to bit 7)");
            Assert.IsTrue(en.FlagC, "Carry should be set (old bit 0 was 1)");
        }

        [Test]
        public void Test_RRCA_No_Carry()
        {
            // A = 0x80 (10000000): rotate right → bit 0 (0) goes to bit 7 and carry
            // Result should be 0x40, C=0
            asm.LoadRegVal(7, 0x80);
            asm.Rrca();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x40, en.A, "RRCA: 0x80 rotated right should be 0x40");
            Assert.IsFalse(en.FlagC, "Carry should be clear (old bit 0 was 0)");
        }

        // ============================================================
        // Bug: DAA doesn't handle subtraction (N flag)
        // After SUB, DAA should subtract correction values, not add them
        // ============================================================

        [Test]
        public void Test_DAA_After_Addition()
        {
            // 0x15 + 0x27 in BCD: 15 + 27 = 42
            // 0x15 + 0x27 = 0x3C → DAA corrects lower nibble (C > 9) → 0x42
            asm.LoadRegVal(7, 0x15);
            asm.AddAVal(0x27);
            asm.Daa();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x42, en.A, "DAA after ADD: 15+27=42 in BCD");
        }

        [Test]
        public void Test_DAA_After_Subtraction()
        {
            // 0x42 - 0x15 in BCD: 42 - 15 = 27
            // 0x42 - 0x15 = 0x2D → DAA corrects lower nibble (D > 9) → 0x27
            asm.LoadRegVal(7, 0x42);
            asm.SubVal(0x15);
            asm.Daa();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x27, en.A, "DAA after SUB: 42-15=27 in BCD");
        }

        [Test]
        public void Test_DAA_After_Subtraction_With_Borrow()
        {
            // 0x10 - 0x01 in BCD: 10 - 01 = 09
            // 0x10 - 0x01 = 0x0F → DAA corrects: N=1, H=1 (borrow from bit 4), → 0x09
            asm.LoadRegVal(7, 0x10);
            asm.SubVal(0x01);
            asm.Daa();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x09, en.A, "DAA after SUB: 10-01=09 in BCD");
        }

        [Test]
        public void Test_DAA_Carry_Flag_Set_When_Upper_Correction()
        {
            // 0x99 + 0x01 in BCD: 99 + 01 = 100
            // 0x99 + 0x01 = 0x9A → DAA corrects both nibbles → 0x00, C=1
            asm.LoadRegVal(7, 0x99);
            asm.AddAVal(0x01);
            asm.Daa();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x00, en.A, "DAA: 99+01=00 with carry in BCD");
            Assert.IsTrue(en.FlagC, "Carry should be set for BCD overflow");
        }
    }
}
