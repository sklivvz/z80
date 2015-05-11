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
        public void Test_Halt()
        {
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.WritePointer, en.PC);
        }

        [Test]
        public void Test_Noop()
        {
            asm.Noop();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.WritePointer, en.PC);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(7)]
        public void Test_LD_R_N(byte r)
        {
            asm.LdR(r,42);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.WritePointer, en.PC);
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
        public void Test_LD_R1_R2(byte r, byte r2)
        {
            asm.LdR(r,33);
            asm.LdRR(r2, r);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.WritePointer, en.PC);
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
        public void Test_LD_R_HL(byte r)
        {
            asm.LdR16(2,5);
            asm.LdRHl(r);
            asm.Halt();
            asm.Data(123);

            en.Run();

            Assert.AreEqual(asm.WritePointer-1, en.PC);
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
        public void Test_LD_R_IX_D(byte r, sbyte d)
        {
            asm.LdIx(8);
            asm.LdRIxD(r,d);
            asm.Halt();
            asm.Data(123);
            asm.Data(42);
            asm.Data(66);

            en.Run();

            Assert.AreEqual(asm.WritePointer - 3, en.PC);
            Assert.AreEqual(_ram[en.PC+d], en.Reg8(r));
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
        public void Test_LD_R_IY_D(byte r, sbyte d)
        {
            asm.LdIy(8);
            asm.LdRIyD(r, d);
            asm.Halt();
            asm.Data(123);
            asm.Data(42);
            asm.Data(66);

            en.Run();

            Assert.AreEqual(asm.WritePointer - 3, en.PC);
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
        public void Test_LD_HL_r(byte r)
        {
            asm.LdR16(2, 8);
            asm.LdR(r, 66);
            asm.LdHLR(r);
            asm.Noop();
            asm.Halt();
            asm.Data(123);

            en.Run();

            Assert.AreEqual(asm.WritePointer - 1, en.PC);
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
        public void Test_LD_IX_d_r(byte r, sbyte d)
        {
            asm.LdIx(11);
            asm.LdR(r, 201);
            asm.LdIxDR(r,d);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);
            asm.Data(0x33);
            asm.Data(0x44);
            asm.Data(0x55);

            en.Run();

            Assert.AreEqual(asm.WritePointer - 5, en.PC);
            Assert.AreEqual(201, _ram[11+d]);
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
        public void Test_LD_IY_d_r(byte r, sbyte d)
        {
            asm.LdIy(11);
            asm.LdR(r, 201);
            asm.LdIyDR(r, d);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);
            asm.Data(0x33);
            asm.Data(0x44);
            asm.Data(0x55);

            en.Run();

            Assert.AreEqual(asm.WritePointer - 5, en.PC);
            Assert.AreEqual(201, _ram[11 + d]);
        }


        [Test]
        public void Test_LD_HL_n()
        {
            asm.LdR16(2, 8);
            asm.LdHLN(201);
            asm.Halt();
            asm.Data(123);

            en.Run();

            en.DumpRam();

            Assert.AreEqual(asm.WritePointer - 1, en.PC);
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
        public void Test_LD_IX_d_n(sbyte d)
        {
            asm.LdIx(11);
            asm.LdIxDN(d, 201);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);
            asm.Data(0x33);
            asm.Data(0x44);
            asm.Data(0x55);

            en.Run();

            Assert.AreEqual(asm.WritePointer - 5, en.PC);
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
        public void Test_LD_IY_d_n(sbyte d)
        {
            asm.LdIy(11);
            asm.LdIyDN(d, 201);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);
            asm.Data(0x33);
            asm.Data(0x44);
            asm.Data(0x55);

            en.Run();

            Assert.AreEqual(asm.WritePointer - 5, en.PC);
            Assert.AreEqual(201, _ram[11 + d]);
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
