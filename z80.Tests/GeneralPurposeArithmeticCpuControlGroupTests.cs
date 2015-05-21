using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class GeneralPurposeArithmeticCpuControlGroupTests: OpCodeTestBase
    {
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
    }
}