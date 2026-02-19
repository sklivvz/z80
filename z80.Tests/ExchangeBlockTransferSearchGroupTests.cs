using System;
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
            Assert.AreEqual(0x1122, en.HL);
            Assert.AreEqual(0x1942, en.DE);
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
            Assert.AreEqual(0x4219, en.AF);
            Assert.AreEqual(0x2211, en.AFp);
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
            Assert.AreEqual(0x1942, en.BC);
            Assert.AreEqual(0x2041, en.DE);
            Assert.AreEqual(0x2140, en.HL);
            Assert.AreEqual(0x1122, en.BCp);
            Assert.AreEqual(0x1223, en.DEp);
            Assert.AreEqual(0x1324, en.HLp);
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

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1122, en.HL);
            Assert.AreEqual(0x42, _ram[0x40]);
            Assert.AreEqual(0x19, _ram[0x41]);
        }

        [Test]
        public void Test_EX_at_SP_IX()
        {
            asm.LoadIxVal(0x1942);
            asm.LoadReg16Val(3, 0x0040);
            asm.LoadRegVal(7, 0x22);
            asm.LoadAddrA(0x0040);
            asm.LoadRegVal(7, 0x11);
            asm.LoadAddrA(0x0041);
            asm.ExAddrSpIx();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1122, en.IX);
            Assert.AreEqual(0x42, _ram[0x40]);
            Assert.AreEqual(0x19, _ram[0x41]);
        }

        [Test]
        public void Test_EX_at_SP_IY()
        {
            asm.LoadIyVal(0x1942);
            asm.LoadReg16Val(3, 0x0040);
            asm.LoadRegVal(7, 0x22);
            asm.LoadAddrA(0x0040);
            asm.LoadRegVal(7, 0x11);
            asm.LoadAddrA(0x0041);
            asm.ExAddrSpIy();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1122, en.IY);
            Assert.AreEqual(0x42, _ram[0x40]);
            Assert.AreEqual(0x19, _ram[0x41]);
        }

        [Test]
        [TestCase(7)]
        [TestCase(1)]
        public void Test_LDI(byte bc)
        {
            asm.LoadReg16Val(3, 0x3333);
            asm.LoadReg16Val(0, 0xFFFF);
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(0, bc);
            asm.LoadReg16Val(1, 0x2222);
            asm.LoadReg16Val(2, 0x1111);
            asm.LoadRegVal(7, 0x88);
            asm.LoadAddrA(0x1111);
            asm.LoadRegVal(7, 0x66);
            asm.LoadAddrA(0x2222);
            asm.Ldi();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(bc - 1, en.BC);
            Assert.AreEqual(0x2223, en.DE);
            Assert.AreEqual(0x1112, en.HL);
            Assert.AreEqual(0x88, _ram[0x1111]);
            Assert.AreEqual(0x88, _ram[0x2222]);
            Assert.IsFalse(en.FlagH);
            Assert.IsFalse(en.FlagN);
            Assert.AreEqual(bc != 1, en.FlagP, "Flag P contained the wrong value");
        }

        [Test]
        public void Test_LDIR()
        {
            Array.Copy(new byte[] { 0x88, 0x36, 0xA5, 0x42 }, 0, _ram, 0x1111, 4);
            Array.Copy(new byte[] { 0x66, 0x59, 0xC5, 0x24 }, 0, _ram, 0x2222, 4);

            asm.LoadReg16Val(3, 0x3333);
            asm.LoadReg16Val(0, 0xFFFF);
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(0, 0x0003);
            asm.LoadReg16Val(1, 0x2222);
            asm.LoadReg16Val(2, 0x1111);
            asm.Ldir();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x0000, en.BC);
            Assert.AreEqual(0x2225, en.DE);
            Assert.AreEqual(0x1114, en.HL);
            Assert.AreEqual(0x88, _ram[0x1111]);
            Assert.AreEqual(0x36, _ram[0x1112]);
            Assert.AreEqual(0xA5, _ram[0x1113]);
            Assert.AreEqual(0x42, _ram[0x1114]);
            Assert.AreEqual(0x88, _ram[0x2222]);
            Assert.AreEqual(0x36, _ram[0x2223]);
            Assert.AreEqual(0xA5, _ram[0x2224]);
            Assert.AreEqual(0x24, _ram[0x2225]);
            Assert.IsFalse(en.FlagH);
            Assert.IsFalse(en.FlagN);
            Assert.IsFalse(en.FlagP);
        }
        [Test]
        [TestCase(7)]
        [TestCase(1)]
        public void Test_LDD(byte bc)
        {
            asm.LoadReg16Val(3, 0x3333);
            asm.LoadReg16Val(0, 0xFFFF);
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(0, bc);
            asm.LoadReg16Val(1, 0x2222);
            asm.LoadReg16Val(2, 0x1111);
            asm.LoadRegVal(7, 0x88);
            asm.LoadAddrA(0x1111);
            asm.LoadRegVal(7, 0x66);
            asm.LoadAddrA(0x2222);
            asm.Ldd();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(bc - 1, en.BC);
            Assert.AreEqual(0x2221, en.DE);
            Assert.AreEqual(0x1110, en.HL);
            Assert.AreEqual(0x88, _ram[0x1111]);
            Assert.AreEqual(0x88, _ram[0x2222]);
            Assert.IsFalse(en.FlagH);
            Assert.IsFalse(en.FlagN);
            Assert.AreEqual(bc != 1, en.FlagP, "Flag P contained the wrong value");
        }

        [Test]
        public void Test_LDDR()
        {
            Array.Copy(new byte[] { 0x42, 0x88, 0x36, 0xA5 }, 0, _ram, 0x1111, 4);
            Array.Copy(new byte[] { 0x24, 0x66, 0x59, 0xC5 }, 0, _ram, 0x2222, 4);

            asm.LoadReg16Val(3, 0x3333);
            asm.LoadReg16Val(0, 0xFFFF);
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(0, 0x0003);
            asm.LoadReg16Val(1, 0x2225);
            asm.LoadReg16Val(2, 0x1114);
            asm.Lddr();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x0000, en.BC);
            Assert.AreEqual(0x2222, en.DE);
            Assert.AreEqual(0x1111, en.HL);
            Assert.AreEqual(0x88, _ram[0x1112]);
            Assert.AreEqual(0x36, _ram[0x1113]);
            Assert.AreEqual(0xA5, _ram[0x1114]);
            Assert.AreEqual(0x42, _ram[0x1111]);
            Assert.AreEqual(0x88, _ram[0x2223]);
            Assert.AreEqual(0x36, _ram[0x2224]);
            Assert.AreEqual(0xA5, _ram[0x2225]);
            Assert.AreEqual(0x24, _ram[0x2222]);
            Assert.IsFalse(en.FlagH);
            Assert.IsFalse(en.FlagN);
            Assert.IsFalse(en.FlagP);
        }

        [Test]
        [TestCase(7, 0x3F)]
        [TestCase(1, 0x3F)]
        [TestCase(7, 0x42)]
        [TestCase(1, 0x42)]
        [TestCase(7, 0x21)]
        [TestCase(1, 0x21)]
        public void Test_CPI(byte bc, byte a)
        {
            asm.LoadReg16Val(3, 0x3333);
            asm.LoadReg16Val(0, 0xFFFF);
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(0, bc);
            asm.LoadReg16Val(2, 0x1111);
            asm.LoadRegVal(7, 0x3F);
            asm.LoadAddrA(0x1111);
            asm.LoadRegVal(7, a);
            asm.Cpi();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(bc - 1, en.BC);
            Assert.AreEqual(0x1112, en.HL);
            Assert.AreEqual(a, en.A);
            Assert.AreEqual(0x3F, _ram[0x1111]);

            var result = (byte)(a - 0x3F);
            Assert.AreEqual((result & 0x80) != 0, en.FlagS, "Flag S should reflect result MSB");
            Assert.AreEqual(a == 0x3F, en.FlagZ, "Flag Z contained the wrong value");
            // H = half-borrow (borrow from bit 4)
            Assert.AreEqual((a & 0x0F) < (0x3F & 0x0F), en.FlagH, "Flag H should reflect half-borrow");
            Assert.AreEqual(bc != 1, en.FlagP, "Flag P contained the wrong value");
            Assert.IsTrue(en.FlagN);
        }

        [Test]
        [TestCase(7, 0xF3, 4)]
        [TestCase(1, 0xF3, 0)]
        [TestCase(7, 0x42, 0)]
        [TestCase(1, 0x42, 0)]
        [TestCase(7, 0x21, 0)]
        [TestCase(1, 0x21, 0)]
        public void Test_CPIR(byte bc, byte a, byte bc_res)
        {
            Array.Copy(new byte[] { 0x52, 0x00, 0xF3 }, 0, _ram, 0x1111, 3);
            asm.LoadReg16Val(3, 0x3333);
            asm.LoadReg16Val(0, 0xFFFF);
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(0, bc);
            asm.LoadReg16Val(2, 0x1111);
            asm.LoadRegVal(7, a);
            asm.Cpir();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(bc_res, en.BC);
            Assert.AreEqual(0x1111 + bc - bc_res, en.HL);
            Assert.AreEqual(a, en.A);

            var last = _ram[en.HL - 1];
            var result = (byte)(a - last);
            Assert.AreEqual((result & 0x80) != 0, en.FlagS, "Flag S should reflect result MSB");
            Assert.AreEqual(a == last, en.FlagZ, "Flag Z contained the wrong value");
            // H = half-borrow (borrow from bit 4)
            Assert.AreEqual((a & 0x0F) < (last & 0x0F), en.FlagH, "Flag H should reflect half-borrow");
            Assert.AreEqual(en.BC != 0, en.FlagP, "Flag P contained the wrong value");
            Assert.IsTrue(en.FlagN);
        }


        [Test]
        [TestCase(7, 0x3F)]
        [TestCase(1, 0x3F)]
        [TestCase(7, 0x42)]
        [TestCase(1, 0x42)]
        [TestCase(7, 0x21)]
        [TestCase(1, 0x21)]
        public void Test_CPD(byte bc, byte a)
        {
            asm.LoadReg16Val(3, 0x3333);
            asm.LoadReg16Val(0, 0xFFFF);
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(0, bc);
            asm.LoadReg16Val(2, 0x1111);
            asm.LoadRegVal(7, 0x3F);
            asm.LoadAddrA(0x1111);
            asm.LoadRegVal(7, a);
            asm.Cpd();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(bc - 1, en.BC);
            Assert.AreEqual(0x1110, en.HL);
            Assert.AreEqual(a, en.A);
            Assert.AreEqual(0x3F, _ram[0x1111]);

            var result = (byte)(a - 0x3F);
            Assert.AreEqual((result & 0x80) != 0, en.FlagS, "Flag S should reflect result MSB");
            Assert.AreEqual(a == 0x3F, en.FlagZ, "Flag Z contained the wrong value");
            // H = half-borrow (borrow from bit 4)
            Assert.AreEqual((a & 0x0F) < (0x3F & 0x0F), en.FlagH, "Flag H should reflect half-borrow");
            Assert.AreEqual(bc != 1, en.FlagP, "Flag P contained the wrong value");
            Assert.IsTrue(en.FlagN);
        }

        [Test]
        [TestCase(7, 0xF3, 4)]
        [TestCase(1, 0xF3, 0)]
        [TestCase(7, 0x42, 0)]
        [TestCase(1, 0x42, 0)]
        [TestCase(7, 0x21, 0)]
        [TestCase(1, 0x21, 0)]
        public void Test_CPDR(byte bc, byte a, byte bc_res)
        {
            Array.Copy(new byte[] { 0xF3, 0x00, 0x52 }, 0, _ram, 0x1116, 3);
            asm.LoadReg16Val(3, 0x3333);
            asm.LoadReg16Val(0, 0xFFFF);
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.LoadReg16Val(0, bc);
            asm.LoadReg16Val(2, 0x1118);
            asm.LoadRegVal(7, a);
            asm.Cpdr();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(bc_res, en.BC);
            Assert.AreEqual(0x1118 - bc + bc_res, en.HL);
            Assert.AreEqual(a, en.A);

            var last = _ram[en.HL + 1];
            var result = (byte)(a - last);
            Assert.AreEqual((result & 0x80) != 0, en.FlagS, "Flag S should reflect result MSB");
            Assert.AreEqual(a == last, en.FlagZ, "Flag Z contained the wrong value");
            // H = half-borrow (borrow from bit 4)
            Assert.AreEqual((a & 0x0F) < (last & 0x0F), en.FlagH, "Flag H should reflect half-borrow");
            Assert.AreEqual(en.BC != 0, en.FlagP, "Flag P contained the wrong value");
            Assert.IsTrue(en.FlagN);
        }

    }
}