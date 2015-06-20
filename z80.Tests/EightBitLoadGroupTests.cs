using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class EightBitLoadGroupTests: OpCodeTestBase
    {
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(7)]
        public void Test_LD_r_n(byte r)
        {
            asm.LoadRegVal(r, 42);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(42, en.Reg8(r));
        }

        [Test]
        #region testcases
        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(3, 0)]
        [TestCase(4, 0)]
        [TestCase(5, 0)]
        [TestCase(7, 0)]
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 1)]
        [TestCase(0, 2)]
        [TestCase(1, 2)]
        [TestCase(2, 2)]
        [TestCase(3, 2)]
        [TestCase(4, 2)]
        [TestCase(5, 2)]
        [TestCase(7, 2)]
        [TestCase(0, 3)]
        [TestCase(1, 3)]
        [TestCase(2, 3)]
        [TestCase(3, 3)]
        [TestCase(4, 3)]
        [TestCase(5, 3)]
        [TestCase(7, 3)]
        [TestCase(0, 4)]
        [TestCase(1, 4)]
        [TestCase(2, 4)]
        [TestCase(3, 4)]
        [TestCase(4, 4)]
        [TestCase(5, 4)]
        [TestCase(7, 4)]
        [TestCase(0, 5)]
        [TestCase(1, 5)]
        [TestCase(2, 5)]
        [TestCase(3, 5)]
        [TestCase(4, 5)]
        [TestCase(5, 5)]
        [TestCase(7, 5)]
        [TestCase(0, 7)]
        [TestCase(1, 7)]
        [TestCase(2, 7)]
        [TestCase(3, 7)]
        [TestCase(4, 7)]
        [TestCase(5, 7)]
        [TestCase(7, 7)]
        #endregion
        public void Test_LD_r1_r2(byte r, byte r2)
        {
            asm.LoadRegVal(r, 33);
            asm.LoadRegReg(r2, r);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(33, en.Reg8(r));
            Assert.AreEqual(33, en.Reg8(r2));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(7)]
        public void Test_LD_r_HL(byte r)
        {
            asm.LoadReg16Val(2, 5);
            asm.LoadRegAtHl(r);
            asm.Halt();
            asm.Data(123);

            en.Run();

            Assert.AreEqual(asm.Position - 1, en.PC);
            Assert.AreEqual(123, en.Reg8(r));
        }

        [Test]
        #region testcase
        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(3, 0)]
        [TestCase(4, 0)]
        [TestCase(5, 0)]
        [TestCase(7, 0)]
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 1)]
        [TestCase(0, 2)]
        [TestCase(1, 2)]
        [TestCase(2, 2)]
        [TestCase(3, 2)]
        [TestCase(4, 2)]
        [TestCase(5, 2)]
        [TestCase(7, 2)]
        #endregion
        public void Test_LD_r_at_IX(byte r, sbyte d)
        {
            asm.LoadIxVal(8);
            asm.LoadRegAddrIx(r, d);
            asm.Halt();
            asm.Data(123);
            asm.Data(42);
            asm.Data(66);

            en.Run();

            Assert.AreEqual(asm.Position - 3, en.PC);
            Assert.AreEqual(_ram[en.PC + d], en.Reg8(r));
        }

        [Test]
        #region testcase
        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(3, 0)]
        [TestCase(4, 0)]
        [TestCase(5, 0)]
        [TestCase(7, 0)]
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 1)]
        [TestCase(0, 2)]
        [TestCase(1, 2)]
        [TestCase(2, 2)]
        [TestCase(3, 2)]
        [TestCase(4, 2)]
        [TestCase(5, 2)]
        [TestCase(7, 2)]
        #endregion
        public void Test_LD_r_at_IY(byte r, sbyte d)
        {
            asm.LoadIyVal(8);
            asm.LoadRegAddrIy(r, d);
            asm.Halt();
            asm.Data(123);
            asm.Data(42);
            asm.Data(66);

            en.Run();

            Assert.AreEqual(asm.Position - 3, en.PC);
            Assert.AreEqual(_ram[en.PC + d], en.Reg8(r));
        }

        [Test]
        #region testcase
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(7)]
        #endregion
        public void Test_LD_at_HL_r(byte r)
        {
            asm.LoadReg16Val(2, 8);
            asm.LoadRegVal(r, 66);
            asm.LoadAtHLReg(r);
            asm.Noop();
            asm.Halt();
            asm.Data(123);

            en.Run();

            Assert.AreEqual(asm.Position - 1, en.PC);
            Assert.AreEqual(66, _ram[8]);
        }

        [Test]
        #region testcase
        [TestCase(0, -2)]
        [TestCase(1, -2)]
        [TestCase(2, -2)]
        [TestCase(3, -2)]
        [TestCase(4, -2)]
        [TestCase(5, -2)]
        [TestCase(7, -2)]
        [TestCase(0, -1)]
        [TestCase(1, -1)]
        [TestCase(2, -1)]
        [TestCase(3, -1)]
        [TestCase(4, -1)]
        [TestCase(5, -1)]
        [TestCase(7, -1)]
        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(3, 0)]
        [TestCase(4, 0)]
        [TestCase(5, 0)]
        [TestCase(7, 0)]
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 1)]
        [TestCase(0, 2)]
        [TestCase(1, 2)]
        [TestCase(2, 2)]
        [TestCase(3, 2)]
        [TestCase(4, 2)]
        [TestCase(5, 2)]
        [TestCase(7, 2)]
        #endregion
        public void Test_LD_at_IX_r(byte r, sbyte d)
        {
            asm.LoadIxVal(12);
            asm.LoadRegVal(r, 201);
            asm.LoadIxR(r, d);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);
            asm.Data(0x33);
            asm.Data(0x44);
            asm.Data(0x55);

            en.Run();

            Assert.AreEqual(asm.Position - 5, en.PC);
            Assert.AreEqual(201, _ram[12 + d]);
        }

        [Test]
        #region testcase
        [TestCase(0, -2)]
        [TestCase(1, -2)]
        [TestCase(2, -2)]
        [TestCase(3, -2)]
        [TestCase(4, -2)]
        [TestCase(5, -2)]
        [TestCase(7, -2)]
        [TestCase(0, -1)]
        [TestCase(1, -1)]
        [TestCase(2, -1)]
        [TestCase(3, -1)]
        [TestCase(4, -1)]
        [TestCase(5, -1)]
        [TestCase(7, -1)]
        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 0)]
        [TestCase(3, 0)]
        [TestCase(4, 0)]
        [TestCase(5, 0)]
        [TestCase(7, 0)]
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 1)]
        [TestCase(0, 2)]
        [TestCase(1, 2)]
        [TestCase(2, 2)]
        [TestCase(3, 2)]
        [TestCase(4, 2)]
        [TestCase(5, 2)]
        [TestCase(7, 2)]
        #endregion
        public void Test_LD_at_IY_r(byte r, sbyte d)
        {
            asm.LoadIyVal(12);
            asm.LoadRegVal(r, 201);
            asm.LoadIyReg(r, d);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);
            asm.Data(0x33);
            asm.Data(0x44);
            asm.Data(0x55);

            en.Run();

            Assert.AreEqual(asm.Position - 5, en.PC);
            Assert.AreEqual(201, _ram[12 + d]);
        }

        [Test]
        public void Test_LD_at_HL_n()
        {
            asm.LoadReg16Val(2, 8);
            asm.LoadAtHLVal(201);
            asm.Halt();
            asm.Data(123);

            en.Run();

            Assert.AreEqual(asm.Position - 1, en.PC);
            Assert.AreEqual(201, _ram[8]);
        }

        [Test]
        #region testcase
        [TestCase(-2)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        #endregion
        public void Test_LD_at_IX_n(sbyte d)
        {
            asm.LoadIxVal(11);
            asm.LoadAtIxVal(d, 201);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);
            asm.Data(0x33);
            asm.Data(0x44);
            asm.Data(0x55);

            en.Run();

            Assert.AreEqual(asm.Position - 5, en.PC);
            Assert.AreEqual(201, _ram[11 + d]);
        }

        [Test]
        #region testcase
        [TestCase(-2)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        #endregion
        public void Test_LD_at_IY_n(sbyte d)
        {
            asm.LoadIyVal(11);
            asm.LoadIyN(d, 201);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);
            asm.Data(0x33);
            asm.Data(0x44);
            asm.Data(0x55);

            en.Run();

            Assert.AreEqual(asm.Position - 5, en.PC);
            Assert.AreEqual(201, _ram[11 + d]);
        }

        [Test]
        public void Test_LD_A_at_BC()
        {
            asm.LoadReg16Val(0, 5);
            asm.LoadABc();
            asm.Halt();
            asm.Data(0x42);

            en.Run();

            Assert.AreEqual(asm.Position - 1, en.PC);
            Assert.AreEqual(66, en.A);
        }

        [Test]
        public void Test_LD_A_at_DE()
        {
            asm.LoadReg16Val(1, 5);
            asm.LoadADe();
            asm.Halt();
            asm.Data(0x42);

            en.Run();

            Assert.AreEqual(asm.Position - 1, en.PC);
            Assert.AreEqual(66, en.A);
        }

        [Test]
        public void Test_LD_A_at_nn()
        {
            asm.LoadAAddr(4);
            asm.Halt();
            asm.Data(0x42);

            en.Run();

            Assert.AreEqual(asm.Position - 1, en.PC);
            Assert.AreEqual(66, en.A);
        }

        [Test]
        public void Test_LD_at_BC_A()
        {
            asm.LoadRegVal(7, 0x42);
            asm.LoadReg16Val(0, 0x08);
            asm.LoadBcA();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(66, _ram[8]);
        }

        [Test]
        public void Test_LD_at_DE_A()
        {
            asm.LoadRegVal(7, 0x42);
            asm.LoadReg16Val(1, 0x08);
            asm.LoadDeA();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(66, _ram[8]);
        }

        [Test]
        public void Test_LD_at_nn_A()
        {
            asm.LoadRegVal(7, 0x42);
            asm.LoadAddrA(0x08);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(66, _ram[8]);
        }

        [Test]
        public void Test_LD_I_A()
        {
            asm.LoadRegVal(7, 0x42);
            asm.LoadIA();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(66, en.I);
        }

        [Test]
        [TestCase(23, false, false)]
        [TestCase(0, false, true)]
        [TestCase(-1, true, false)]
        public void Test_LD_A_I(sbyte val, bool sign, bool zero)
        {
            asm.LoadRegVal(7, (byte)val);
            asm.LoadIA();
            asm.LoadRegVal(7, 0xC9);
            asm.LoadAI();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual((byte)val, en.A);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
        }

        [Test]
        public void Test_LD_R_A()
        {
            asm.LoadRegVal(7, 0x42);
            asm.LoadRA();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            //R is incremented by 3 machine cycles by the end.
            Assert.AreEqual(69, en.R);
        }

        [Test]
        [TestCase(23, false, false)]
        [TestCase(-5, false, true)]
        [TestCase(-6, true, false)]
        public void Test_LD_A_R(sbyte val, bool sign, bool zero)
        {
            asm.LoadRegVal(7, (byte)val);
            asm.LoadRA();
            asm.LoadRegVal(7, 0xC9);
            asm.LoadAR();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            //R is incremented by 5 machine cycles by the end.
            Assert.AreEqual(val+5, (sbyte)en.A);
            Assert.AreEqual(sign, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
        }
    }
}