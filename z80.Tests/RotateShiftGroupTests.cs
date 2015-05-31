using System;
using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class RotateShiftGroupTests : OpCodeTestBase
    {
        [Test]
        #region testcases
        [TestCase(0x01, 0x02, false)]
        [TestCase(0x81, 0x02, true)]
        [TestCase(0x42, 0x84, false)]
        [TestCase(0x84, 0x08, true)]
        #endregion
        public void Test_RLCA(byte reg, byte res, bool carry)
        {
            asm.Ccf();
            asm.LoadRegVal(7, reg);
            asm.Rlca();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0x01, 0x04, false)]
        [TestCase(0x81, 0x05, false)]
        [TestCase(0x42, 0x08, true)]
        [TestCase(0x84, 0x11, false)]
        #endregion
        public void Test_RLA(byte reg, byte res, bool carry)
        {
            asm.Ccf();
            asm.LoadRegVal(7, reg);
            asm.Rla();
            asm.Rla();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0x80, 0x40, false)]
        [TestCase(0x81, 0x40, true)]
        [TestCase(0x42, 0x21, false)]
        [TestCase(0x21, 0x10, true)]
        #endregion
        public void Test_RRCA(byte reg, byte res, bool carry)
        {
            asm.Ccf();
            asm.LoadRegVal(7, reg);
            asm.Rrca();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0x80, 0x20, false)]
        [TestCase(0x81, 0xA0, false)]
        [TestCase(0x42, 0x10, true)]
        [TestCase(0x21, 0x88, false)]
        #endregion
        public void Test_RRA(byte reg, byte res, bool carry)
        {
            asm.Ccf();
            asm.LoadRegVal(7, reg);
            asm.Rra();
            asm.Rra();
            asm.Halt();

            en.Run();

            en.DumpRam();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

    }
}