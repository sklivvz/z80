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

        [Test]
        [TestCase(0x01, 0x99, 0x100)]
        [TestCase(0x01, 0x98, 0x99)]
        [TestCase(0x10, 0x89, 0x99)]
        [TestCase(0x01, 0x89, 0x90)]
        [TestCase(0x10, 0x90, 0x100)]
        public void Test_DAA_Add(byte a, byte val, int correct)
        {
            asm.LoadRegVal(7, a);
            asm.AddAVal(val);
            asm.Daa();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = correct;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var overflow = trueSum > 256;
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        [TestCase(0x00, '0')]
        [TestCase(0x01, '1')]
        [TestCase(0x02, '2')]
        [TestCase(0x03, '3')]
        [TestCase(0x04, '4')]
        [TestCase(0x05, '5')]
        [TestCase(0x06, '6')]
        [TestCase(0x07, '7')]
        [TestCase(0x08, '8')]
        [TestCase(0x09, '9')]
        [TestCase(0x0A, 'A')]
        [TestCase(0x0B, 'B')]
        [TestCase(0x0C, 'C')]
        [TestCase(0x0D, 'D')]
        [TestCase(0x0E, 'E')]
        [TestCase(0x0F, 'F')]
        public void Test_DAA_ByteToHex(byte a, char val)
        {
            asm.LoadRegVal(7, a);
            asm.AndVal(0x0F);
            asm.AddAVal(0x90);
            asm.Daa();
            asm.AdcAVal(0x40);
            asm.Daa();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual((byte)val, en.A);
        }

        [Test]
        [TestCase(1, 1, 0x00)]
        [TestCase(2, 1, 0x01)]
        [TestCase(10, 1, 0x09)]
        [TestCase(16, 1, 0x15)]
        [TestCase(0xA0, 0x10, 0x90)]
        [TestCase(0xAA, 0x11, 0x99)]
        [TestCase(10, 0, 0x10)]
        [TestCase(100, 1, 99)]
        public void Test_DAA_Sub(byte a, byte val, int correct)
        {
            asm.LoadRegVal(7, a);
            asm.SubVal(val);
            asm.Daa();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            var trueSum = correct;
            var byteSum = trueSum % 256;
            var sbyteSum = (sbyte)byteSum;
            Assert.AreEqual(byteSum, en.A);
            Assert.AreEqual(sbyteSum < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(en.A == 0x00, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(false, en.FlagH, "Flag H contained the wrong value");
            var overflow = trueSum > 256;
            Assert.AreEqual(overflow, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(trueSum > 0xFF, en.FlagC, "Flag C contained the wrong value");
        }

        [Test]
        [TestCase(0x00)]
        [TestCase(0x08)]
        [TestCase(0x80)]
        [TestCase(0xFF)]
        public void Test_Cpl(byte a)
        {
            asm.LoadRegVal(7, a);
            asm.Cpl();
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual((byte)(a^0xFF), en.A);
            Assert.AreEqual(true, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(true, en.FlagN, "Flag N contained the wrong value");
        }

        [Test]
        [TestCase(0x00)]
        [TestCase(0x08)]
        [TestCase(0x80)]
        [TestCase(0xFF)]
        public void Test_Neg(byte a)
        {
            asm.LoadRegVal(7, a);
            asm.Neg();
            asm.Halt();

            en.Run();

            var exp = -a;
            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual((sbyte)exp, (sbyte)en.A);
            Assert.AreEqual((sbyte)exp < 0, en.FlagS, "Flag S contained the wrong value");
            Assert.AreEqual(exp == 0, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual((a&15)>0, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(a == 0x80, en.FlagP, "Flag P contained the wrong value");
            Assert.AreEqual(true, en.FlagN, "Flag N contained the wrong value");
            Assert.AreEqual(a != 0, en.FlagC, "Flag C contained the wrong value");
        }

    }
}