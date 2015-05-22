using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class SixteenBitLoadGroupTests : OpCodeTestBase
    {
        [Test]
        public void Test_LD_BC_nn()
        {
            asm.LoadReg16Val(0, 0x1942);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x19, en.B);
            Assert.AreEqual(0x42, en.C);
        }

        [Test]
        public void Test_LD_DE_nn()
        {
            asm.LoadReg16Val(1, 0x1942);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x19, en.D);
            Assert.AreEqual(0x42, en.E);
        }

        [Test]
        public void Test_LD_HL_nn()
        {
            asm.LoadReg16Val(2, 0x1942);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x19, en.H);
            Assert.AreEqual(0x42, en.L);
        }

        [Test]
        public void Test_LD_SP_nn()
        {
            asm.LoadReg16Val(3, 0x1942);
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

            Assert.AreEqual(asm.Position - 2, en.PC);
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
        public void Test_LD_at_nn_HL()
        {
            asm.LoadReg16Val(2, 0x1942);
            asm.LoadAddrHl(0x0008);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x42, _ram[asm.Position - 1]);
            Assert.AreEqual(0x19, _ram[asm.Position]);
        }

        [Test]
        public void Test_LD_at_nn_BC()
        {
            asm.LoadReg16Val(0, 0x1942);
            asm.LoadAddrReg16(0, 0x0009);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x42, _ram[asm.Position - 1]);
            Assert.AreEqual(0x19, _ram[asm.Position]);
        }

        [Test]
        public void Test_LD_at_nn_DE()
        {
            asm.LoadReg16Val(1, 0x1942);
            asm.LoadAddrReg16(1, 0x0009);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x42, _ram[asm.Position - 1]);
            Assert.AreEqual(0x19, _ram[asm.Position]);
        }

        [Test]
        public void Test_LD_at_nn_HL_alt()
        {
            asm.LoadReg16Val(2, 0x1942);
            asm.LoadAddrReg16(2, 0x0009);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x42, _ram[asm.Position - 1]);
            Assert.AreEqual(0x19, _ram[asm.Position]);
        }

        [Test]
        public void Test_LD_at_nn_SP()
        {
            asm.LoadReg16Val(3, 0x1942);
            asm.LoadAddrReg16(3, 0x0009);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x42, _ram[asm.Position - 1]);
            Assert.AreEqual(0x19, _ram[asm.Position]);
        }

        [Test]
        public void Test_LD_at_nn_IX()
        {
            asm.LoadIxVal(0x1942);
            asm.LoadAddrIx(0x000A);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x42, _ram[asm.Position - 1]);
            Assert.AreEqual(0x19, _ram[asm.Position]);
        }

        [Test]
        public void Test_LD_at_nn_IY()
        {
            asm.LoadIyVal(0x1942);
            asm.LoadAddrIy(0x000A);
            asm.Halt();
            asm.Data(0x11);
            asm.Data(0x22);

            en.Run();

            Assert.AreEqual(asm.Position - 2, en.PC);
            Assert.AreEqual(0x42, _ram[asm.Position - 1]);
            Assert.AreEqual(0x19, _ram[asm.Position]);
        }

        [Test]
        public void Test_LD_SP_HL()
        {
            asm.LoadReg16Val(2, 0x1942);
            asm.LoadSpHl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1942, en.SP);
        }

        [Test]
        public void Test_LD_SP_IX()
        {
            asm.LoadIxVal(0x1942);
            asm.LoadSpIx();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1942, en.SP);
        }

        [Test]
        public void Test_LD_SP_IY()
        {
            asm.LoadIyVal(0x1942);
            asm.LoadSpIy();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x1942, en.SP);
        }

        [Test]
        public void Test_PUSH_BC()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadReg16Val(0, 0x1942);
            asm.PushReg16(0);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x000E, en.SP);
            Assert.AreEqual(en.B, _ram[en.SP + 1]);
            Assert.AreEqual(en.C, _ram[en.SP]);
        }

        [Test]
        public void Test_PUSH_DE()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadReg16Val(1, 0x1942);
            asm.PushReg16(1);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x000E, en.SP);
            Assert.AreEqual(en.D, _ram[en.SP + 1]);
            Assert.AreEqual(en.E, _ram[en.SP]);
        }

        [Test]
        public void Test_PUSH_HL()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadReg16Val(2, 0x1942);
            asm.PushReg16(2);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x000E, en.SP);
            Assert.AreEqual(en.H, _ram[en.SP + 1]);
            Assert.AreEqual(en.L, _ram[en.SP]);
        }

        [Test]
        public void Test_PUSH_AF()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadRegVal(7, 0x42);
            asm.PushReg16(3);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x000E, en.SP);
            Assert.AreEqual(en.A, _ram[en.SP + 1]);
            Assert.AreEqual(en.F, _ram[en.SP]);
        }

        [Test]
        public void Test_PUSH_IX()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadIxVal(0x1942);
            asm.PushIx();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x000E, en.SP);
            Assert.AreEqual(en.IX, _ram[en.SP + 1] * 256 + _ram[en.SP]);
        }

        [Test]
        public void Test_PUSH_IY()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadIxVal(0x1942);
            asm.PushIy();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x000E, en.SP);
            Assert.AreEqual(en.IY, _ram[en.SP + 1] * 256 + _ram[en.SP]);
        }

        [Test]
        public void Test_POP_BC()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadReg16Val(1, 0x1942);
            asm.PushReg16(1);
            asm.PopReg16(0);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x0010, en.SP);
            Assert.AreEqual(0x1942, en.BC);
        }

        [Test]
        public void Test_POP_DE()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadReg16Val(0, 0x1942);
            asm.PushReg16(0);
            asm.PopReg16(1);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x0010, en.SP);
            Assert.AreEqual(0x1942, en.DE);
        }

        [Test]
        public void Test_POP_HL()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadReg16Val(0, 0x1942);
            asm.PushReg16(0);
            asm.PopReg16(2);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x0010, en.SP);
            Assert.AreEqual(0x1942, en.HL);
        }

        [Test]
        public void Test_POP_AF()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadReg16Val(0, 0x1942);
            asm.PushReg16(0);
            asm.PopReg16(3);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x0010, en.SP);
            Assert.AreEqual(0x4219, en.AF);
        }

        [Test]
        public void Test_POP_IX()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadReg16Val(0, 0x1942);
            asm.PushReg16(0);
            asm.PopIx();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x0010, en.SP);
            Assert.AreEqual(0x1942, en.IX);
        }

        [Test]
        public void Test_POP_IY()
        {
            asm.LoadReg16Val(3, 0x0010);
            asm.LoadReg16Val(0, 0x1942);
            asm.PushReg16(0);
            asm.PopIy();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(0x0010, en.SP);
            Assert.AreEqual(0x1942, en.IY);
        }
    }
}