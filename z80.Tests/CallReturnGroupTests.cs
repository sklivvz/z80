using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    internal class CallReturnGroupTests : OpCodeTestBase
    {
        [Test]
        public void Test_CALL_nn()
        {
            asm.Call(0x0005);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x06, en.PC);
            Assert.AreEqual(0xFFFD, en.SP);
            Assert.AreEqual(0x03, _ram[0xFFFD]);
            Assert.AreEqual(0x00, _ram[0xFFFE]);
        }

        [Test]
        [TestCase(0xFF, 0x09, true)]
        [TestCase(0x00, 0x07, false)]
        public void Test_CALL_NZ_nn(byte val, short addr, bool branch)
        {
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.CallNz(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x06, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
            else
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
        }
        [Test]
        [TestCase(0xFF, 0x07, false)]
        [TestCase(0x00, 0x09, true)]
        public void Test_CALL_Z_nn(byte val, short addr, bool branch)
        {
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.CallZ(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x06, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
            else
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
        }
        [Test]
        [TestCase(0xFF, 0x07, false)]
        [TestCase(0x00, 0x09, true)]
        public void Test_CALL_NC_nn(byte val, short addr, bool branch)
        {
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.CallNc(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x06, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
            else
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
        }
        [Test]
        [TestCase(0xFF, 0x09, true)]
        [TestCase(0x00, 0x07, false)]
        public void Test_CALL_C_nn(byte val, short addr, bool branch)
        {
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.CallC(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x06, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
            else
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
        }
        [Test]
        [TestCase(0x7F, 0x07, false)]
        [TestCase(0x00, 0x09, true)]
        public void Test_CALL_PO_nn(byte val, short addr, bool branch)
        {
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.CallPo(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x06, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
            else
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
        }
        [Test]
        [TestCase(0x7F, 0x09, true)]
        [TestCase(0x00, 0x07, false)]
        public void Test_CALL_PE_nn(byte val, short addr, bool branch)
        {
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.CallPe(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
        }
        [Test]
        [TestCase(0x01, 0x09, true)]
        [TestCase(0x80, 0x07, false)]
        public void Test_CALL_P_nn(byte val, short addr, bool branch)
        {
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.CallP(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x06, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
            else
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
        }
        [Test]
        [TestCase(0x01, 0x07, false)]
        [TestCase(0x80, 0x09, true)]
        public void Test_CALL_M_nn(byte val, short addr, bool branch)
        {
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.CallM(0x0008);
            asm.Halt();
            asm.Halt();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x06, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
            else
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
        }

        [Test]
        public void Test_RET_NZ_nn()
        {
            asm.Call(0x0004);
            asm.Halt();
            asm.Ret();
            asm.Halt();

            en.Run();

            Assert.AreEqual(0x04, en.PC);
            Assert.AreEqual(0xFFFF, en.SP);
        }
        [Test]
        [TestCase(0xFF, 0x04, true)]
        [TestCase(0x00, 0x09, false)]
        public void Test_RET_NZ_nn(byte val, short addr, bool branch)
        {
            asm.Call(0x0004);
            asm.Halt();
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.RetNz();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
            else
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x03, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
        }
        [Test]
        [TestCase(0xFF, 0x09, false)]
        [TestCase(0x00, 0x04, true)]
        public void Test_RET_Z_nn(byte val, short addr, bool branch)
        {
            asm.Call(0x0004);
            asm.Halt();
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.RetZ();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
            else
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x03, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
        }
        [Test]
        [TestCase(0xFF, 0x09, false)]
        [TestCase(0x00, 0x04, true)]
        public void Test_RET_NC_nn(byte val, short addr, bool branch)
        {
            asm.Call(0x0004);
            asm.Halt();
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.RetNc();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
            else
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x03, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
        }
        [Test]
        [TestCase(0xFF, 0x04, true)]
        [TestCase(0x00, 0x09, false)]
        public void Test_RET_C_nn(byte val, short addr, bool branch)
        {
            asm.Call(0x0004);
            asm.Halt();
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.RetC();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
            else
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x03, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
        }
        [Test]
        [TestCase(0x00, 0x04, true)]
        [TestCase(0x7F, 0x09, false)]
        public void Test_RET_PO_nn(byte val, short addr, bool branch)
        {
            asm.Call(0x0004);
            asm.Halt();
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.RetPo();
            asm.Halt();

            en.Run();

            en.DumpCpu();
            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
            else
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x03, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
        }
        [Test]
        [TestCase(0x7F, 0x04, true)]
        [TestCase(0x00, 0x09, false)]
        public void Test_RET_PE_nn(byte val, short addr, bool branch)
        {
            asm.Call(0x0004);
            asm.Halt();
            asm.LoadRegVal(7, val);
            asm.IncReg(7);
            asm.RetPe();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
            else
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x03, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
        }
        [Test]
        [TestCase(0xFF, 0x09, false)]
        [TestCase(0x00, 0x04, true)]
        public void Test_RET_P_nn(byte val, short addr, bool branch)
        {
            asm.Call(0x0004);
            asm.Halt();
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.RetP();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
            else
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x03, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
        }
        [Test]
        [TestCase(0xFF, 0x04, true)]
        [TestCase(0x00, 0x09, false)]
        public void Test_RET_M_nn(byte val, short addr, bool branch)
        {
            asm.Call(0x0004);
            asm.Halt();
            asm.LoadRegVal(7, val);
            asm.OrReg(7);
            asm.RetM();
            asm.Halt();

            en.Run();

            Assert.AreEqual(addr, en.PC);
            if (branch)
            {
                Assert.AreEqual(0xFFFF, en.SP);
            }
            else
            {
                Assert.AreEqual(0xFFFD, en.SP);
                Assert.AreEqual(0x03, _ram[0xFFFD]);
                Assert.AreEqual(0x00, _ram[0xFFFE]);
            }
        }

    }
}
