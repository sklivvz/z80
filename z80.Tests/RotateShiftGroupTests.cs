using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace z80.Tests
{
    [TestFixture]
    public class RotateShiftGroupTests : OpCodeTestBase
    {
        [Test]
        #region testcases
        [TestCase(0x01, 0x02, false)]
        [TestCase(0x81, 0x03, true)]  // Rotation: bit 7 goes to both carry AND bit 0
        [TestCase(0x42, 0x84, false)]
        [TestCase(0x84, 0x09, true)]  // Rotation: bit 7 goes to both carry AND bit 0
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
        [TestCase(0x81, 0xC0, true)]  // Rotation: bit 0 goes to both carry AND bit 7
        [TestCase(0x42, 0x21, false)]
        [TestCase(0x21, 0x90, true)]  // Rotation: bit 0 goes to both carry AND bit 7
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



            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.A);
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x01, 0x02, false, false, false, false)]
        [TestCase(0, 0x81, 0x03, true, false, false, true)]
        [TestCase(0, 0x42, 0x84, false, false, true, true)]
        [TestCase(1, 0x84, 0x09, true, false, false, true)]
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x01, 0x02, false, false, false, false)]
        [TestCase(1, 0x81, 0x03, true, false, false, true)]
        [TestCase(2, 0x42, 0x84, false, false, true, true)]
        [TestCase(2, 0x84, 0x09, true, false, false, true)]
        [TestCase(2, 0x00, 0x00, false, true, false, true)]
        [TestCase(2, 0x01, 0x02, false, false, false, false)]
        [TestCase(3, 0x81, 0x03, true, false, false, true)]
        [TestCase(3, 0x42, 0x84, false, false, true, true)]
        [TestCase(3, 0x84, 0x09, true, false, false, true)]
        [TestCase(3, 0x00, 0x00, false, true, false, true)]
        [TestCase(4, 0x01, 0x02, false, false, false, false)]
        [TestCase(4, 0x81, 0x03, true, false, false, true)]
        [TestCase(4, 0x42, 0x84, false, false, true, true)]
        [TestCase(4, 0x84, 0x09, true, false, false, true)]
        [TestCase(5, 0x00, 0x00, false, true, false, true)]
        [TestCase(5, 0x01, 0x02, false, false, false, false)]
        [TestCase(5, 0x81, 0x03, true, false, false, true)]
        [TestCase(5, 0x42, 0x84, false, false, true, true)]
        [TestCase(5, 0x84, 0x09, true, false, false, true)]
        [TestCase(7, 0x00, 0x00, false, true, false, true)]
        [TestCase(7, 0x01, 0x02, false, false, false, false)]
        [TestCase(7, 0x81, 0x03, true, false, false, true)]
        [TestCase(7, 0x42, 0x84, false, false, true, true)]
        [TestCase(7, 0x84, 0x09, true, false, false, true)]
        #endregion
        public void Test_RLC_r(byte register, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadRegVal(register, reg);
            asm.RlcReg(register);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.Reg8(register));
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0x00, 0x00, false, true, false, true)]
        [TestCase(0x01, 0x02, false, false, false, false)]
        [TestCase(0x81, 0x03, true, false, false, true)]
        [TestCase(0x42, 0x84, false, false, true, true)]
        [TestCase(0x84, 0x09, true, false, false, true)]
        #endregion
        public void Test_RLC_HL(byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(reg);
            asm.RlcAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x01, 0x02, false, false, false, false)]
        [TestCase(-1, 0x81, 0x03, true, false, false, true)]
        [TestCase(-1, 0x42, 0x84, false, false, true, true)]
        [TestCase(-1, 0x84, 0x09, true, false, false, true)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x01, 0x02, false, false, false, false)]
        [TestCase(0, 0x81, 0x03, true, false, false, true)]
        [TestCase(0, 0x42, 0x84, false, false, true, true)]
        [TestCase(0, 0x84, 0x09, true, false, false, true)]
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x01, 0x02, false, false, false, false)]
        [TestCase(1, 0x81, 0x03, true, false, false, true)]
        [TestCase(1, 0x42, 0x84, false, false, true, true)]
        [TestCase(1, 0x84, 0x09, true, false, false, true)]
        #endregion
        public void Test_RLC_IX_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIxVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.RlcAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }
        [Test]
        #region testcases
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x01, 0x02, false, false, false, false)]
        [TestCase(-1, 0x81, 0x03, true, false, false, true)]
        [TestCase(-1, 0x42, 0x84, false, false, true, true)]
        [TestCase(-1, 0x84, 0x09, true, false, false, true)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x01, 0x02, false, false, false, false)]
        [TestCase(0, 0x81, 0x03, true, false, false, true)]
        [TestCase(0, 0x42, 0x84, false, false, true, true)]
        [TestCase(0, 0x84, 0x09, true, false, false, true)]
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x01, 0x02, false, false, false, false)]
        [TestCase(1, 0x81, 0x03, true, false, false, true)]
        [TestCase(1, 0x42, 0x84, false, false, true, true)]
        [TestCase(1, 0x84, 0x09, true, false, false, true)]
        #endregion
        public void Test_RLC_IY_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIyVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.RlcAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x01, 0x02, false, false, false, false)]
        [TestCase(0, 0x81, 0x02, true, false, false, false)]
        [TestCase(0, 0x42, 0x84, false, false, true, true)]
        [TestCase(1, 0x84, 0x08, true, false, false, false)]
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x01, 0x02, false, false, false, false)]
        [TestCase(1, 0x81, 0x02, true, false, false, false)]
        [TestCase(2, 0x42, 0x84, false, false, true, true)]
        [TestCase(2, 0x84, 0x08, true, false, false, false)]
        [TestCase(2, 0x00, 0x00, false, true, false, true)]
        [TestCase(2, 0x01, 0x02, false, false, false, false)]
        [TestCase(3, 0x81, 0x02, true, false, false, false)]
        [TestCase(3, 0x42, 0x84, false, false, true, true)]
        [TestCase(3, 0x84, 0x08, true, false, false, false)]
        [TestCase(3, 0x00, 0x00, false, true, false, true)]
        [TestCase(4, 0x01, 0x02, false, false, false, false)]
        [TestCase(4, 0x81, 0x02, true, false, false, false)]
        [TestCase(4, 0x42, 0x84, false, false, true, true)]
        [TestCase(4, 0x84, 0x08, true, false, false, false)]
        [TestCase(5, 0x00, 0x00, false, true, false, true)]
        [TestCase(5, 0x01, 0x02, false, false, false, false)]
        [TestCase(5, 0x81, 0x02, true, false, false, false)]
        [TestCase(5, 0x42, 0x84, false, false, true, true)]
        [TestCase(5, 0x84, 0x08, true, false, false, false)]
        [TestCase(7, 0x00, 0x00, false, true, false, true)]
        [TestCase(7, 0x01, 0x02, false, false, false, false)]
        [TestCase(7, 0x81, 0x02, true, false, false, false)]
        [TestCase(7, 0x42, 0x84, false, false, true, true)]
        [TestCase(7, 0x84, 0x08, true, false, false, false)]
        #endregion
        public void Test_RL_r(byte register, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadRegVal(register, reg);
            asm.RlReg(register);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.Reg8(register));
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0x00, 0x01, false, false, false, false)]
        [TestCase(0x01, 0x03, false, false, false, true)]
        [TestCase(0x81, 0x03, true, false, false, true)]
        [TestCase(0x42, 0x85, false, false, true, false)]
        [TestCase(0x84, 0x09, true, false, false, true)]
        #endregion
        public void Test_RL_HL(byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Scf();
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(reg);
            asm.RlAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(-1, 0x00, 0x01, false, false, false, false)]
        [TestCase(-1, 0x01, 0x03, false, false, false, true)]
        [TestCase(-1, 0x81, 0x03, true, false, false, true)]
        [TestCase(-1, 0x42, 0x85, false, false, true, false)]
        [TestCase(-1, 0x84, 0x09, true, false, false, true)]
        [TestCase(0, 0x00, 0x01, false, false, false, false)]
        [TestCase(0, 0x01, 0x03, false, false, false, true)]
        [TestCase(0, 0x81, 0x03, true, false, false, true)]
        [TestCase(0, 0x42, 0x85, false, false, true, false)]
        [TestCase(0, 0x84, 0x09, true, false, false, true)]
        [TestCase(1, 0x00, 0x01, false, false, false, false)]
        [TestCase(1, 0x01, 0x03, false, false, false, true)]
        [TestCase(1, 0x81, 0x03, true, false, false, true)]
        [TestCase(1, 0x42, 0x85, false, false, true, false)]
        [TestCase(1, 0x84, 0x09, true, false, false, true)]
        #endregion
        public void Test_RL_IX_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Scf();
            asm.LoadIxVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.RlAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }
        [Test]
        #region testcases
        [TestCase(-1, 0x00, 0x01, false, false, false, false)]
        [TestCase(-1, 0x01, 0x03, false, false, false, true)]
        [TestCase(-1, 0x81, 0x03, true, false, false, true)]
        [TestCase(-1, 0x42, 0x85, false, false, true, false)]
        [TestCase(-1, 0x84, 0x09, true, false, false, true)]
        [TestCase(0, 0x00, 0x01, false, false, false, false)]
        [TestCase(0, 0x01, 0x03, false, false, false, true)]
        [TestCase(0, 0x81, 0x03, true, false, false, true)]
        [TestCase(0, 0x42, 0x85, false, false, true, false)]
        [TestCase(0, 0x84, 0x09, true, false, false, true)]
        [TestCase(1, 0x00, 0x01, false, false, false, false)]
        [TestCase(1, 0x01, 0x03, false, false, false, true)]
        [TestCase(1, 0x81, 0x03, true, false, false, true)]
        [TestCase(1, 0x42, 0x85, false, false, true, false)]
        [TestCase(1, 0x84, 0x09, true, false, false, true)]
        #endregion
        public void Test_RL_IY_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Scf();
            asm.LoadIyVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.RlAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0x20, false, false, false, false)]
        [TestCase(1, 0x81, 0x60, false, false, false, true)]
        [TestCase(1, 0x42, 0x90, true, false, true, true)]
        [TestCase(1, 0x21, 0x48, false, false, false, true)]
        [TestCase(2, 0x00, 0x00, false, true, false, true)]
        [TestCase(2, 0x80, 0x20, false, false, false, false)]
        [TestCase(2, 0x81, 0x60, false, false, false, true)]
        [TestCase(2, 0x42, 0x90, true, false, true, true)]
        [TestCase(2, 0x21, 0x48, false, false, false, true)]
        [TestCase(3, 0x00, 0x00, false, true, false, true)]
        [TestCase(3, 0x80, 0x20, false, false, false, false)]
        [TestCase(3, 0x81, 0x60, false, false, false, true)]
        [TestCase(3, 0x42, 0x90, true, false, true, true)]
        [TestCase(3, 0x21, 0x48, false, false, false, true)]
        [TestCase(4, 0x00, 0x00, false, true, false, true)]
        [TestCase(4, 0x80, 0x20, false, false, false, false)]
        [TestCase(4, 0x81, 0x60, false, false, false, true)]
        [TestCase(4, 0x42, 0x90, true, false, true, true)]
        [TestCase(4, 0x21, 0x48, false, false, false, true)]
        [TestCase(5, 0x00, 0x00, false, true, false, true)]
        [TestCase(5, 0x80, 0x20, false, false, false, false)]
        [TestCase(5, 0x81, 0x60, false, false, false, true)]
        [TestCase(5, 0x42, 0x90, true, false, true, true)]
        [TestCase(5, 0x21, 0x48, false, false, false, true)]
        [TestCase(7, 0x00, 0x00, false, true, false, true)]
        [TestCase(7, 0x80, 0x20, false, false, false, false)]
        [TestCase(7, 0x81, 0x60, false, false, false, true)]
        [TestCase(7, 0x42, 0x90, true, false, true, true)]
        [TestCase(7, 0x21, 0x48, false, false, false, true)]
        #endregion
        public void Test_RRC_r(byte register, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadRegVal(register, reg);
            asm.RrcReg(register);
            asm.RrcReg(register);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.Reg8(register));
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0x00, 0x00, false, true, false, true)]
        [TestCase(0x80, 0x20, false, false, false, false)]
        [TestCase(0x81, 0x60, false, false, false, true)]
        [TestCase(0x42, 0x90, true, false, true, true)]
        [TestCase(0x21, 0x48, false, false, false, true)]
        #endregion
        public void Test_RRC_HL(byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(reg);
            asm.RrcAddrHl();
            asm.RrcAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0x20, false, false, false, false)]
        [TestCase(1, 0x81, 0x60, false, false, false, true)]
        [TestCase(1, 0x42, 0x90, true, false, true, true)]
        [TestCase(1, 0x21, 0x48, false, false, false, true)]
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x80, 0x20, false, false, false, false)]
        [TestCase(-1, 0x81, 0x60, false, false, false, true)]
        [TestCase(-1, 0x42, 0x90, true, false, true, true)]
        [TestCase(-1, 0x21, 0x48, false, false, false, true)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x80, 0x20, false, false, false, false)]
        [TestCase(0, 0x81, 0x60, false, false, false, true)]
        [TestCase(0, 0x42, 0x90, true, false, true, true)]
        [TestCase(0, 0x21, 0x48, false, false, false, true)]
        #endregion
        public void Test_RRC_IX_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIxVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.RrcAddrIx(disp);
            asm.RrcAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }
        [Test]
        #region testcases
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0x20, false, false, false, false)]
        [TestCase(1, 0x81, 0x60, false, false, false, true)]
        [TestCase(1, 0x42, 0x90, true, false, true, true)]
        [TestCase(1, 0x21, 0x48, false, false, false, true)]
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x80, 0x20, false, false, false, false)]
        [TestCase(-1, 0x81, 0x60, false, false, false, true)]
        [TestCase(-1, 0x42, 0x90, true, false, true, true)]
        [TestCase(-1, 0x21, 0x48, false, false, false, true)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x80, 0x20, false, false, false, false)]
        [TestCase(0, 0x81, 0x60, false, false, false, true)]
        [TestCase(0, 0x42, 0x90, true, false, true, true)]
        [TestCase(0, 0x21, 0x48, false, false, false, true)]
        #endregion
        public void Test_RRC_IY_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIyVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.RrcAddrIy(disp);
            asm.RrcAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x80, 0x20, false, false, false, false)]
        [TestCase(0, 0x81, 0xA0, false, false, true, true)]
        [TestCase(0, 0x42, 0x10, true, false, false, false)]
        [TestCase(0, 0x21, 0x88, false, false, true, true)]
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0x20, false, false, false, false)]
        [TestCase(1, 0x81, 0xA0, false, false, true, true)]
        [TestCase(1, 0x42, 0x10, true, false, false, false)]
        [TestCase(1, 0x21, 0x88, false, false, true, true)]
        [TestCase(2, 0x00, 0x00, false, true, false, true)]
        [TestCase(2, 0x80, 0x20, false, false, false, false)]
        [TestCase(2, 0x81, 0xA0, false, false, true, true)]
        [TestCase(2, 0x42, 0x10, true, false, false, false)]
        [TestCase(2, 0x21, 0x88, false, false, true, true)]
        [TestCase(3, 0x00, 0x00, false, true, false, true)]
        [TestCase(3, 0x80, 0x20, false, false, false, false)]
        [TestCase(3, 0x81, 0xA0, false, false, true, true)]
        [TestCase(3, 0x42, 0x10, true, false, false, false)]
        [TestCase(3, 0x21, 0x88, false, false, true, true)]
        [TestCase(4, 0x00, 0x00, false, true, false, true)]
        [TestCase(4, 0x80, 0x20, false, false, false, false)]
        [TestCase(4, 0x81, 0xA0, false, false, true, true)]
        [TestCase(4, 0x42, 0x10, true, false, false, false)]
        [TestCase(4, 0x21, 0x88, false, false, true, true)]
        [TestCase(5, 0x00, 0x00, false, true, false, true)]
        [TestCase(5, 0x80, 0x20, false, false, false, false)]
        [TestCase(5, 0x81, 0xA0, false, false, true, true)]
        [TestCase(5, 0x42, 0x10, true, false, false, false)]
        [TestCase(5, 0x21, 0x88, false, false, true, true)]
        [TestCase(7, 0x00, 0x00, false, true, false, true)]
        [TestCase(7, 0x80, 0x20, false, false, false, false)]
        [TestCase(7, 0x81, 0xA0, false, false, true, true)]
        [TestCase(7, 0x42, 0x10, true, false, false, false)]
        [TestCase(7, 0x21, 0x88, false, false, true, true)]
        #endregion
        public void Test_RR_r(byte register, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadRegVal(register, reg);
            asm.RrReg(register);
            asm.RrReg(register);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.Reg8(register));
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0x00, 0x00, false, true, false, true)]
        [TestCase(0x80, 0x20, false, false, false, false)]
        [TestCase(0x81, 0xA0, false, false, true, true)]
        [TestCase(0x42, 0x10, true, false, false, false)]
        [TestCase(0x21, 0x88, false, false, true, true)]
        #endregion
        public void Test_RR_HL(byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(reg);
            asm.RrAddrHl();
            asm.RrAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0x20, false, false, false, false)]
        [TestCase(1, 0x81, 0xA0, false, false, true, true)]
        [TestCase(1, 0x42, 0x10, true, false, false, false)]
        [TestCase(1, 0x21, 0x88, false, false, true, true)]
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x80, 0x20, false, false, false, false)]
        [TestCase(-1, 0x81, 0xA0, false, false, true, true)]
        [TestCase(-1, 0x42, 0x10, true, false, false, false)]
        [TestCase(-1, 0x21, 0x88, false, false, true, true)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x80, 0x20, false, false, false, false)]
        [TestCase(0, 0x81, 0xA0, false, false, true, true)]
        [TestCase(0, 0x42, 0x10, true, false, false, false)]
        [TestCase(0, 0x21, 0x88, false, false, true, true)]
        #endregion
        public void Test_RR_IX_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIxVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.RrAddrIx(disp);
            asm.RrAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }
        [Test]
        #region testcases
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0x20, false, false, false, false)]
        [TestCase(1, 0x81, 0xA0, false, false, true, true)]
        [TestCase(1, 0x42, 0x10, true, false, false, false)]
        [TestCase(1, 0x21, 0x88, false, false, true, true)]
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x80, 0x20, false, false, false, false)]
        [TestCase(-1, 0x81, 0xA0, false, false, true, true)]
        [TestCase(-1, 0x42, 0x10, true, false, false, false)]
        [TestCase(-1, 0x21, 0x88, false, false, true, true)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x80, 0x20, false, false, false, false)]
        [TestCase(0, 0x81, 0xA0, false, false, true, true)]
        [TestCase(0, 0x42, 0x10, true, false, false, false)]
        [TestCase(0, 0x21, 0x88, false, false, true, true)]
        #endregion
        public void Test_RR_IY_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIyVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.RrAddrIy(disp);
            asm.RrAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }
        [Test]
        #region testcases
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x01, 0x02, false, false, false, false)]
        [TestCase(0, 0x81, 0x02, true, false, false, false)]
        [TestCase(0, 0x42, 0x84, false, false, true, true)]
        [TestCase(1, 0x84, 0x08, true, false, false, false)]
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x01, 0x02, false, false, false, false)]
        [TestCase(1, 0x81, 0x02, true, false, false, false)]
        [TestCase(2, 0x42, 0x84, false, false, true, true)]
        [TestCase(2, 0x84, 0x08, true, false, false, false)]
        [TestCase(2, 0x00, 0x00, false, true, false, true)]
        [TestCase(2, 0x01, 0x02, false, false, false, false)]
        [TestCase(3, 0x81, 0x02, true, false, false, false)]
        [TestCase(3, 0x42, 0x84, false, false, true, true)]
        [TestCase(3, 0x84, 0x08, true, false, false, false)]
        [TestCase(3, 0x00, 0x00, false, true, false, true)]
        [TestCase(4, 0x01, 0x02, false, false, false, false)]
        [TestCase(4, 0x81, 0x02, true, false, false, false)]
        [TestCase(4, 0x42, 0x84, false, false, true, true)]
        [TestCase(4, 0x84, 0x08, true, false, false, false)]
        [TestCase(5, 0x00, 0x00, false, true, false, true)]
        [TestCase(5, 0x01, 0x02, false, false, false, false)]
        [TestCase(5, 0x81, 0x02, true, false, false, false)]
        [TestCase(5, 0x42, 0x84, false, false, true, true)]
        [TestCase(5, 0x84, 0x08, true, false, false, false)]
        [TestCase(7, 0x00, 0x00, false, true, false, true)]
        [TestCase(7, 0x01, 0x02, false, false, false, false)]
        [TestCase(7, 0x81, 0x02, true, false, false, false)]
        [TestCase(7, 0x42, 0x84, false, false, true, true)]
        [TestCase(7, 0x84, 0x08, true, false, false, false)]
        #endregion
        public void Test_SLA_r(byte register, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadRegVal(register, reg);
            asm.SlaReg(register);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.Reg8(register));
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0x00, 0x00, false, true, false, true)]
        [TestCase(0x01, 0x02, false, false, false, false)]
        [TestCase(0x81, 0x02, true, false, false, false)]
        [TestCase(0x42, 0x84, false, false, true, true)]
        [TestCase(0x84, 0x08, true, false, false, false)]
        #endregion
        public void Test_SLA_HL(byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(reg);
            asm.SlaAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x01, 0x02, false, false, false, false)]
        [TestCase(-1, 0x81, 0x02, true, false, false, false)]
        [TestCase(-1, 0x42, 0x84, false, false, true, true)]
        [TestCase(-1, 0x84, 0x08, true, false, false, false)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x01, 0x02, false, false, false, false)]
        [TestCase(0, 0x81, 0x02, true, false, false, false)]
        [TestCase(0, 0x42, 0x84, false, false, true, true)]
        [TestCase(0, 0x84, 0x08, true, false, false, false)]
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x01, 0x02, false, false, false, false)]
        [TestCase(1, 0x81, 0x02, true, false, false, false)]
        [TestCase(1, 0x42, 0x84, false, false, true, true)]
        [TestCase(1, 0x84, 0x08, true, false, false, false)]
        #endregion
        public void Test_SLA_IX_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIxVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.SlaAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }
        [Test]
        #region testcases
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x01, 0x02, false, false, false, false)]
        [TestCase(-1, 0x81, 0x02, true, false, false, false)]
        [TestCase(-1, 0x42, 0x84, false, false, true, true)]
        [TestCase(-1, 0x84, 0x08, true, false, false, false)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x01, 0x02, false, false, false, false)]
        [TestCase(0, 0x81, 0x02, true, false, false, false)]
        [TestCase(0, 0x42, 0x84, false, false, true, true)]
        [TestCase(0, 0x84, 0x08, true, false, false, false)]
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x01, 0x02, false, false, false, false)]
        [TestCase(1, 0x81, 0x02, true, false, false, false)]
        [TestCase(1, 0x42, 0x84, false, false, true, true)]
        [TestCase(1, 0x84, 0x08, true, false, false, false)]
        #endregion
        public void Test_SLA_IY_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIyVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.SlaAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0xE0, false, false, true, false)]
        [TestCase(1, 0x81, 0xE0, false, false, true, false)]
        [TestCase(1, 0x42, 0x10, true, false, false, false)]
        [TestCase(1, 0x21, 0x08, false, false, false, false)]
        [TestCase(2, 0x00, 0x00, false, true, false, true)]
        [TestCase(2, 0x80, 0xE0, false, false, true, false)]
        [TestCase(2, 0x81, 0xE0, false, false, true, false)]
        [TestCase(2, 0x42, 0x10, true, false, false, false)]
        [TestCase(2, 0x21, 0x08, false, false, false, false)]
        [TestCase(3, 0x00, 0x00, false, true, false, true)]
        [TestCase(3, 0x80, 0xE0, false, false, true, false)]
        [TestCase(3, 0x81, 0xE0, false, false, true, false)]
        [TestCase(3, 0x42, 0x10, true, false, false, false)]
        [TestCase(3, 0x21, 0x08, false, false, false, false)]
        [TestCase(4, 0x00, 0x00, false, true, false, true)]
        [TestCase(4, 0x80, 0xE0, false, false, true, false)]
        [TestCase(4, 0x81, 0xE0, false, false, true, false)]
        [TestCase(4, 0x42, 0x10, true, false, false, false)]
        [TestCase(4, 0x21, 0x08, false, false, false, false)]
        [TestCase(5, 0x00, 0x00, false, true, false, true)]
        [TestCase(5, 0x80, 0xE0, false, false, true, false)]
        [TestCase(5, 0x81, 0xE0, false, false, true, false)]
        [TestCase(5, 0x42, 0x10, true, false, false, false)]
        [TestCase(5, 0x21, 0x08, false, false, false, false)]
        [TestCase(7, 0x00, 0x00, false, true, false, true)]
        [TestCase(7, 0x80, 0xE0, false, false, true, false)]
        [TestCase(7, 0x81, 0xE0, false, false, true, false)]
        [TestCase(7, 0x42, 0x10, true, false, false, false)]
        [TestCase(7, 0x21, 0x08, false, false, false, false)]
        #endregion
        public void Test_SRA_r(byte register, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadRegVal(register, reg);
            asm.SraReg(register);
            asm.SraReg(register);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.Reg8(register));
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0x00, 0x00, false, true, false, true)]
        [TestCase(0x80, 0xE0, false, false, true, false)]
        [TestCase(0x81, 0xE0, false, false, true, false)]
        [TestCase(0x42, 0x10, true, false, false, false)]
        [TestCase(0x21, 0x08, false, false, false, false)]
        #endregion
        public void Test_SRA_HL(byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(reg);
            asm.SraAddrHl();
            asm.SraAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0xE0, false, false, true, false)]
        [TestCase(1, 0x81, 0xE0, false, false, true, false)]
        [TestCase(1, 0x42, 0x10, true, false, false, false)]
        [TestCase(1, 0x21, 0x08, false, false, false, false)]
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x80, 0xE0, false, false, true, false)]
        [TestCase(-1, 0x81, 0xE0, false, false, true, false)]
        [TestCase(-1, 0x42, 0x10, true, false, false, false)]
        [TestCase(-1, 0x21, 0x08, false, false, false, false)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x80, 0xE0, false, false, true, false)]
        [TestCase(0, 0x81, 0xE0, false, false, true, false)]
        [TestCase(0, 0x42, 0x10, true, false, false, false)]
        [TestCase(0, 0x21, 0x08, false, false, false, false)]
        #endregion
        public void Test_SRA_IX_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIxVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.SraAddrIx(disp);
            asm.SraAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }
        [Test]
        #region testcases
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0xE0, false, false, true, false)]
        [TestCase(1, 0x81, 0xE0, false, false, true, false)]
        [TestCase(1, 0x42, 0x10, true, false, false, false)]
        [TestCase(1, 0x21, 0x08, false, false, false, false)]
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x80, 0xE0, false, false, true, false)]
        [TestCase(-1, 0x81, 0xE0, false, false, true, false)]
        [TestCase(-1, 0x42, 0x10, true, false, false, false)]
        [TestCase(-1, 0x21, 0x08, false, false, false, false)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x80, 0xE0, false, false, true, false)]
        [TestCase(0, 0x81, 0xE0, false, false, true, false)]
        [TestCase(0, 0x42, 0x10, true, false, false, false)]
        [TestCase(0, 0x21, 0x08, false, false, false, false)]
        #endregion
        public void Test_SRA_IY_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIyVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.SraAddrIy(disp);
            asm.SraAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }


        [Test]
        #region testcases
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0x20, false, false, false, false)]
        [TestCase(1, 0x81, 0x20, false, false, false, false)]
        [TestCase(1, 0x42, 0x10, true, false, false, false)]
        [TestCase(1, 0x21, 0x08, false, false, false, false)]
        [TestCase(2, 0x00, 0x00, false, true, false, true)]
        [TestCase(2, 0x80, 0x20, false, false, false, false)]
        [TestCase(2, 0x81, 0x20, false, false, false, false)]
        [TestCase(2, 0x42, 0x10, true, false, false, false)]
        [TestCase(2, 0x21, 0x08, false, false, false, false)]
        [TestCase(3, 0x00, 0x00, false, true, false, true)]
        [TestCase(3, 0x80, 0x20, false, false, false, false)]
        [TestCase(3, 0x81, 0x20, false, false, false, false)]
        [TestCase(3, 0x42, 0x10, true, false, false, false)]
        [TestCase(3, 0x21, 0x08, false, false, false, false)]
        [TestCase(4, 0x00, 0x00, false, true, false, true)]
        [TestCase(4, 0x80, 0x20, false, false, false, false)]
        [TestCase(4, 0x81, 0x20, false, false, false, false)]
        [TestCase(4, 0x42, 0x10, true, false, false, false)]
        [TestCase(4, 0x21, 0x08, false, false, false, false)]
        [TestCase(5, 0x00, 0x00, false, true, false, true)]
        [TestCase(5, 0x80, 0x20, false, false, false, false)]
        [TestCase(5, 0x81, 0x20, false, false, false, false)]
        [TestCase(5, 0x42, 0x10, true, false, false, false)]
        [TestCase(5, 0x21, 0x08, false, false, false, false)]
        [TestCase(7, 0x00, 0x00, false, true, false, true)]
        [TestCase(7, 0x80, 0x20, false, false, false, false)]
        [TestCase(7, 0x81, 0x20, false, false, false, false)]
        [TestCase(7, 0x42, 0x10, true, false, false, false)]
        [TestCase(7, 0x21, 0x08, false, false, false, false)]
        #endregion
        public void Test_SRL_r(byte register, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadRegVal(register, reg);
            asm.SrlReg(register);
            asm.SrlReg(register);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.Reg8(register));
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(0x00, 0x00, false, true, false, true)]
        [TestCase(0x80, 0x20, false, false, false, false)]
        [TestCase(0x81, 0x20, false, false, false, false)]
        [TestCase(0x42, 0x10, true, false, false, false)]
        [TestCase(0x21, 0x08, false, false, false, false)]
        #endregion
        public void Test_SRL_HL(byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(reg);
            asm.SrlAddrHl();
            asm.SrlAddrHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        #region testcases
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0x20, false, false, false, false)]
        [TestCase(1, 0x81, 0x20, false, false, false, false)]
        [TestCase(1, 0x42, 0x10, true, false, false, false)]
        [TestCase(1, 0x21, 0x08, false, false, false, false)]
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x80, 0x20, false, false, false, false)]
        [TestCase(-1, 0x81, 0x20, false, false, false, false)]
        [TestCase(-1, 0x42, 0x10, true, false, false, false)]
        [TestCase(-1, 0x21, 0x08, false, false, false, false)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x80, 0x20, false, false, false, false)]
        [TestCase(0, 0x81, 0x20, false, false, false, false)]
        [TestCase(0, 0x42, 0x10, true, false, false, false)]
        [TestCase(0, 0x21, 0x08, false, false, false, false)]
        #endregion
        public void Test_SRL_IX_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIxVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.SrlAddrIx(disp);
            asm.SrlAddrIx(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }
        [Test]
        #region testcases
        [TestCase(1, 0x00, 0x00, false, true, false, true)]
        [TestCase(1, 0x80, 0x20, false, false, false, false)]
        [TestCase(1, 0x81, 0x20, false, false, false, false)]
        [TestCase(1, 0x42, 0x10, true, false, false, false)]
        [TestCase(1, 0x21, 0x08, false, false, false, false)]
        [TestCase(-1, 0x00, 0x00, false, true, false, true)]
        [TestCase(-1, 0x80, 0x20, false, false, false, false)]
        [TestCase(-1, 0x81, 0x20, false, false, false, false)]
        [TestCase(-1, 0x42, 0x10, true, false, false, false)]
        [TestCase(-1, 0x21, 0x08, false, false, false, false)]
        [TestCase(0, 0x00, 0x00, false, true, false, true)]
        [TestCase(0, 0x80, 0x20, false, false, false, false)]
        [TestCase(0, 0x81, 0x20, false, false, false, false)]
        [TestCase(0, 0x42, 0x10, true, false, false, false)]
        [TestCase(0, 0x21, 0x08, false, false, false, false)]
        #endregion
        public void Test_SRL_IY_d(sbyte disp, byte reg, byte res, bool carry, bool zero, bool sign, bool parity)
        {
            asm.Ccf();
            asm.LoadIyVal(0x0040);
            asm.LoadReg16Val(2, (ushort)(0x0040 + disp));
            asm.LoadAtHLVal(reg);
            asm.SrlAddrIy(disp);
            asm.SrlAddrIy(disp);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[0x0040 + disp]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(carry, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        [TestCase(0x7A, 0x31, 0x73, 0x1A, false, false, false)]
        public void Test_RLD(byte a, byte b, byte ra, byte rb, bool zero, bool sign, bool parity)
        {
            asm.LoadReg16Val(2,0x0040);
            asm.LoadAtHLVal(b);
            asm.LoadRegVal(7,a);
            asm.Rld();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(ra, en.A);
            Assert.AreEqual(rb, _ram[0x0040]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
        }

        [Test]
        [TestCase(0x84, 0x20, 0x80, 0x42, false, true, false)]
        public void Test_RRD(byte a, byte b, byte ra, byte rb, bool zero, bool sign, bool parity)
        {
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(b);
            asm.LoadRegVal(7, a);
            asm.Rrd();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(ra, en.A);
            Assert.AreEqual(rb, _ram[0x0040]);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(parity, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
        }

    }

}
