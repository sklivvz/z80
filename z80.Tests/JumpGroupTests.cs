using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace z80.Tests
{
    [TestFixture]
    internal class JumpGroupTests : OpCodeTestBase
    {
        [Test]
        public void Test_JP_nn()
        {
            asm.Jp(0x0005);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x06, en.PC);
        }
        [Test]
        [TestCase(0xFF, 0x09)]
        [TestCase(0x00, 0x07)]
        public void Test_JP_NZ_nn(byte val, short addr)
        {
            asm.LoadRegVal(7,val);
            asm.OrReg(7);
            asm.JpNz(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        [TestCase(0xFF, 0x07)]
        [TestCase(0x00, 0x09)]
        public void Test_JP_Z_nn(byte val, short addr)
        {
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.JpZ(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        [TestCase(0xFF, 0x07)]
        [TestCase(0x00, 0x09)]
        public void Test_JP_NC_nn(byte val, short addr)
        {
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.JpNc(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        [TestCase(0xFF, 0x09)]
        [TestCase(0x00, 0x07)]
        public void Test_JP_C_nn(byte val, short addr)
        {
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.JpC(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        [TestCase(0x7F, 0x07)]
        [TestCase(0x00, 0x09)]
        public void Test_JP_PO_nn(byte val, short addr)
        {
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.JpPo(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        [TestCase(0x7F, 0x09)]
        [TestCase(0x00, 0x07)]
        public void Test_JP_PE_nn(byte val, short addr)
        {
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.JpPe(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        [TestCase(0x01, 0x09)]
        [TestCase(0x80, 0x07)]
        public void Test_JP_P_nn(byte val, short addr)
        {
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.JpP(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        [TestCase(0x01, 0x07)]
        [TestCase(0x80, 0x09)]
        public void Test_JP_M_nn(byte val, short addr)
        {
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.JpM(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }

        [Test]
        public void Test_JR_nn()
        {
            asm.Jr(0x04);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x05, en.PC);
        }
        [Test]
        [TestCase(0xFF, 0x08)]
        [TestCase(0x00, 0x06)]
        public void Test_JR_NZ_nn(byte val, short addr)
        {
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.JrNz(0x04);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        [TestCase(0xFF, 0x06)]
        [TestCase(0x00, 0x08)]
        public void Test_JR_Z_nn(byte val, short addr)
        {
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.JrZ(0x04);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        [TestCase(0xFF, 0x06)]
        [TestCase(0x00, 0x08)]
        public void Test_JR_NC_nn(byte val, short addr)
        {
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.JrNc(0x04);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        [TestCase(0xFF, 0x08)]
        [TestCase(0x00, 0x06)]
        public void Test_JR_C_nn(byte val, short addr)
        {
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.JrC(0x04);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        public void Test_JP_HL()
        {
            asm.LoadReg16Val(2,0x0006);
            asm.JpHl();
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x07, en.PC);
        }
        [Test]
        public void Test_JP_IX()
        {
            asm.LoadIxVal(0x0008);
            asm.JpIx();
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x09, en.PC);
        }
        [Test]
        public void Test_JP_IY()
        {
            asm.LoadIyVal(0x0008);
            asm.JpIy();
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x09, en.PC);
        }
        [Test]
        [TestCase(0x01)]
        [TestCase(0x42)]
        public void Test_DJNZ_e(byte loops)
        {
            asm.LoadRegVal(0,loops);
            asm.XorReg(7);
            asm.IncReg(7);
            asm.Djnz(-1);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(loops, en.A);
            Assert.AreEqual(0, en.B);
        }
    }
}

