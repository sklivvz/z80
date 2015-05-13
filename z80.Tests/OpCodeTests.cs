using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace z80.Tests
{
    [TestFixture]
    public class OpCodeTests
    {
        private System en;
        private Z80Asm asm;
        private byte[] _ram;

        [Test]
        public void Test_HALT()
        {
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
        }

        [Test]
        public void Test_NOOP()
        {
            asm.Noop();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
        }

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
            asm.LoadR16Val(2, 5);
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
        public void Test_LD_r_ᐸIX_dᐳ(byte r, sbyte d)
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
        public void Test_LD_r_ᐸIY_dᐳ(byte r, sbyte d)
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
            asm.LoadR16Val(2, 8);
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
        public void Test_LD_ᐸIX_dᐳ_r(byte r, sbyte d)
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
        public void Test_LD_ᐸIY_dᐳ_r(byte r, sbyte d)
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
            asm.LoadR16Val(2, 8);
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
        public void Test_LD_ᐸIX_dᐳ_n(sbyte d)
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
        public void Test_LD_ᐸIY_dᐳ_n(sbyte d)
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
            asm.LoadR16Val(0, 5);
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
            asm.LoadR16Val(1, 5);
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

            en.DumpRam();

            Assert.AreEqual(asm.Position - 1, en.PC);
            Assert.AreEqual(66, en.A);
        }

        [Test]
        public void Test_LD_at_BC_A()
        {
            asm.LoadRegVal(7, 0x42);
            asm.LoadR16Val(0, 0x08);
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
            asm.LoadR16Val(1, 0x08);
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
            Assert.AreEqual(sign, en.FlagS);
            Assert.AreEqual(zero, en.FlagZ);
            Assert.AreEqual(false, en.FlagH);
            Assert.AreEqual(false, en.FlagN);
        }

        [Test]
        public void Test_LD_R_A()
        {
            asm.LoadRegVal(7, 0x42);
            asm.LoadRA();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(66, en.R);
        }

        [Test]
        [TestCase(23, false, false)]
        [TestCase(0, false, true)]
        [TestCase(-1, true, false)]
        public void Test_LD_A_R(sbyte val, bool sign, bool zero)
        {
            asm.LoadRegVal(7, (byte)val);
            asm.LoadRA();
            asm.LoadRegVal(7, 0xC9);
            asm.LoadAR();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual((byte)val, en.A);
            Assert.AreEqual(sign, en.FlagS);
            Assert.AreEqual(zero, en.FlagZ);
            Assert.AreEqual(false, en.FlagH);
            Assert.AreEqual(false, en.FlagN);
        }

        [Test]
        public void Test_LD_BC_nn()
        {
            asm.LoadR16Val(0, 0x1942);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x19, en.B);
            Assert.AreEqual(0x42, en.C);
        }

        [Test]
        public void Test_LD_DE_nn()
        {
            asm.LoadR16Val(1, 0x1942);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x19, en.D);
            Assert.AreEqual(0x42, en.E);
        }

        [Test]
        public void Test_LD_HL_nn()
        {
            asm.LoadR16Val(2, 0x1942);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x19, en.H);
            Assert.AreEqual(0x42, en.L);
        }

        [Test]
        public void Test_LD_SP_nn()
        {
            asm.LoadR16Val(3, 0x1942);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1942, en.SP);
        }

        [Test]
        public void Test_LD_IX_nn()
        {
            asm.LoadIxVal(0x1942);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1942, en.IX);
        }

        [Test]
        public void Test_LD_IY_nn()
        {
            asm.LoadIyVal(0x1942);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1942, en.IY);
        }

        [Test]
        public void Test_LD_HL_at_nn()
        {
            asm.LoadHLAddr(0x04);
            asm.Halt();
            asm.Data(0x42);
            asm.Data(0x19);

            en.Run();

            Assert.AreEqual(asm.Position-2, en.PC);
            Assert.AreEqual(0x19, en.H);
            Assert.AreEqual(0x42, en.L);
        }

        [Test]
        public void Test_LD_BC_at_nn()
        {
            asm.LoadReg16Addr(0, 0x05);
            asm.Halt();
            asm.Data(0x42);
            asm.Data(0x19);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x19, en.B);
            Assert.AreEqual(0x42, en.C);
        }

        [Test]
        public void Test_LD_DE_at_nn()
        {
            asm.LoadReg16Addr(1, 0x05);
            asm.Halt();
            asm.Data(0x42);
            asm.Data(0x19);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x19, en.D);
            Assert.AreEqual(0x42, en.E);
        }


        [Test]
        public void Test_LD_HL_at_nn_alt()
        {
            asm.LoadReg16Addr(2, 0x05);
            asm.Halt();
            asm.Data(0x42);
            asm.Data(0x19);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x19, en.H);
            Assert.AreEqual(0x42, en.L);
        }

        [Test]
        public void Test_LD_SP_at_nn()
        {
            asm.LoadReg16Addr(3, 0x05);
            asm.Halt();
            asm.Data(0x42);
            asm.Data(0x19);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x1942, en.SP);
        }

        [Test]
        public void Test_LD_IX_at_nn()
        {
            asm.LoadIXAddr(0x05);
            asm.Halt();
            asm.Data(0x42);
            asm.Data(0x19);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x1942, en.IX);
        }

        [Test]
        public void Test_LD_IY_at_nn()
        {
            asm.LoadIYAddr(0x05);
            asm.Halt();
            asm.Data(0x42);
            asm.Data(0x19);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x1942, en.IY);
        }

        [Test]
        public void Test_EI()
        {
            asm.Di();
            asm.Ei();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(true, en.Iff1);
            Assert.AreEqual(true, en.Iff2);
        }

        [Test]
        public void Test_DI()
        {
            asm.Ei();
            asm.Di();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(false, en.Iff1);
            Assert.AreEqual(false, en.Iff2);
        }

        //////////////////////////
        [SetUp]
        public void TestSetup()
        {
            _ram = new byte[0xFFFF];
            en = new System(_ram);
            asm = new Z80Asm(_ram);
            en.Reset();
            asm.Reset();
        }
    }
}
