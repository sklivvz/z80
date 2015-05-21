using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class ExchangeBlockTransferSearchGroupTests : OpCodeTestBase
    {
        [Test]
        public void Test_EX_DE_HL()
        {
            asm.LoadReg16Val(1, 0x1122);
            asm.LoadReg16Val(2, 0x1942);
            asm.ExDeHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1122, en.Reg16(4));
            Assert.AreEqual(0x1942, en.Reg16(2));
        }

        [Test]
        public void Test_EX_AF_AFp()
        {
            asm.LoadReg16Val(0, 0x1942);
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.ExAfAfp();
            asm.LoadReg16Val(0, 0x1122);
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.ExAfAfp();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x4219, en.Reg16(6));
            Assert.AreEqual(0x2211, en.Reg16(14));
        }

        [Test]
        public void Test_EXX()
        {
            asm.LoadReg16Val(0, 0x1942);
            asm.LoadReg16Val(1, 0x2041);
            asm.LoadReg16Val(2, 0x2140);
            asm.Exx();
            asm.LoadReg16Val(0, 0x1122);
            asm.LoadReg16Val(1, 0x1223);
            asm.LoadReg16Val(2, 0x1324);
            asm.Exx();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1942, en.Reg16(0));
            Assert.AreEqual(0x2041, en.Reg16(2));
            Assert.AreEqual(0x2140, en.Reg16(4));
            Assert.AreEqual(0x1122, en.Reg16(8));
            Assert.AreEqual(0x1223, en.Reg16(10));
            Assert.AreEqual(0x1324, en.Reg16(12));
        }

        [Test]
        public void Test_EX_at_SP_HL()
        {
            asm.LoadReg16Val(2, 0x1942);
            asm.LoadReg16Val(3, 0x0040);
            asm.LoadRegVal(7, 0x22);
            asm.LoadAddrA(0x0040);
            asm.LoadRegVal(7, 0x11);
            asm.LoadAddrA(0x0041);
            asm.ExAddrSpHl();
            asm.Halt();

            en.Run();
            en.DumpCpu();
            en.DumpRam();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1122, en.Reg16(4));
            Assert.AreEqual(0x42, _ram[0x40]);
            Assert.AreEqual(0x19, _ram[0x41]);
        }

    }
}