using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace z80.Tests
{
    [TestFixture]
    internal class BitSetResetTestGroupTests : OpCodeTestBase
    {

        [Test]

        #region testcases
        [TestCase(0, 0, 0x55, false)]
        [TestCase(0, 1, 0x57, false)]
        [TestCase(0, 2, 0xA9, false)]
        [TestCase(0, 3, 0xA1, false)]
        [TestCase(0, 4, 0xD3, false)]
        [TestCase(0, 5, 0xF5, false)]
        [TestCase(0, 7, 0xD4, true)]
        [TestCase(1, 0, 0x7D, true)]
        [TestCase(1, 1, 0xD8, true)]
        [TestCase(1, 2, 0xF6, false)]
        [TestCase(1, 3, 0x5E, false)]
        [TestCase(1, 4, 0x08, true)]
        [TestCase(1, 5, 0xE4, true)]
        [TestCase(1, 7, 0x3D, true)]
        [TestCase(2, 0, 0x5F, false)]
        [TestCase(2, 1, 0x1E, false)]
        [TestCase(2, 2, 0x04, false)]
        [TestCase(2, 3, 0x90, true)]
        [TestCase(2, 4, 0x1B, true)]
        [TestCase(2, 5, 0x97, false)]
        [TestCase(2, 7, 0xE5, false)]
        [TestCase(3, 0, 0x68, false)]
        [TestCase(3, 1, 0x55, true)]
        [TestCase(3, 2, 0x0F, false)]
        [TestCase(3, 3, 0x97, true)]
        [TestCase(3, 4, 0x06, true)]
        [TestCase(3, 5, 0x1B, false)]
        [TestCase(3, 7, 0xEE, false)]
        [TestCase(4, 0, 0x48, true)]
        [TestCase(4, 1, 0x36, false)]
        [TestCase(4, 2, 0xEF, true)]
        [TestCase(4, 3, 0xE1, true)]
        [TestCase(4, 4, 0xA3, true)]
        [TestCase(4, 5, 0xE0, true)]
        [TestCase(4, 7, 0x11, false)]
        [TestCase(5, 0, 0x15, true)]
        [TestCase(5, 1, 0xF8, false)]
        [TestCase(5, 2, 0xC1, true)]
        [TestCase(5, 3, 0x06, true)]
        [TestCase(5, 4, 0x9D, true)]
        [TestCase(5, 5, 0x1C, true)]
        [TestCase(5, 7, 0xD1, true)]
        [TestCase(6, 0, 0x6A, false)]
        [TestCase(6, 1, 0x66, false)]
        [TestCase(6, 2, 0x38, true)]
        [TestCase(6, 3, 0x9D, true)]
        [TestCase(6, 4, 0x3A, true)]
        [TestCase(6, 5, 0x0C, true)]
        [TestCase(6, 7, 0x72, false)]
        [TestCase(7, 0, 0x44, true)]
        [TestCase(7, 1, 0x7F, true)]
        [TestCase(7, 2, 0x47, true)]
        [TestCase(7, 3, 0xE0, false)]
        [TestCase(7, 4, 0xE7, false)]
        [TestCase(7, 5, 0x44, true)]
        [TestCase(7, 7, 0xEC, false)]
        #endregion

        public void Test_BIT_B_R(byte bit, byte register, byte set, bool zero)
        {
            asm.LoadRegVal(register, set);
            asm.BitNReg(bit, register);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(true, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0, 0x60, true)]
        [TestCase(1, 0x22, false)]
        [TestCase(2, 0x11, true)]
        [TestCase(3, 0x87, true)]
        [TestCase(4, 0xB9, false)]
        [TestCase(5, 0x11, true)]
        [TestCase(6, 0x11, true)]
        [TestCase(7, 0x90, false)]

        #endregion
        public void Test_BIT_B_HL(byte bit,  byte set, bool zero)
        {
            asm.LoadReg16Val(2,0x0040);
            asm.LoadAtHLVal(set);
            asm.BitNAtHl(bit);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(true, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");

        }

        [Test]
        #region testcases
        [TestCase(0, -70, 0x55, false)]
        [TestCase(1, 75, 0xA7, false)]
        [TestCase(2, -43, 0x35, false)]
        [TestCase(3, 26, 0x7C, false)]
        [TestCase(4, -77, 0x26, true)]
        [TestCase(5, -18, 0x57, true)]
        [TestCase(6, -6, 0xDC, false)]
        [TestCase(7, -101, 0xDE, false)]
        #endregion
        public void Test_BIT_B_IX_d(byte bit, sbyte d, byte set, bool zero)
        {
            asm.Reset();
            asm.LoadReg16Val(2, (ushort)(0x0140+d));
            asm.LoadAtHLVal(set);
            asm.LoadIxVal(0x0140);
            asm.BitNAtIxd(bit, d);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(true, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");

        }

        [Test]

        #region testcases

        [TestCase(0, 37, 0x72, true)]
        [TestCase(1, -33, 0xB4, true)]
        [TestCase(2, -80, 0x16, false)]
        [TestCase(3, 62, 0x33, true)]
        [TestCase(4, -87, 0x16, false)]
        [TestCase(5, -94, 0x50, true)]
        [TestCase(6, 50, 0x94, true)]
        [TestCase(7, -117, 0x05, true)]

        #endregion

        public void Test_BIT_B_IY_d(byte bit, sbyte d, byte set, bool zero)
        {
            asm.Reset();
            asm.LoadReg16Val(2, (ushort) (0x0140 + d));
            asm.LoadAtHLVal(set);
            asm.LoadIyVal(0x0140);
            asm.BitNAtIyd(bit, d);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(zero, en.FlagZ, "Flag Z contained the wrong value");
            Assert.AreEqual(true, en.FlagH, "Flag H contained the wrong value");
            Assert.AreEqual(false, en.FlagN, "Flag N contained the wrong value");
        }

        private void TestCaseGenerator()
        {
            Random r = new Random();
            for (int i = 0; i < 8; i++)
            {
                //if ((i & 7) == 6) continue;
                var val = r.Next(256);
                var disp = (sbyte)r.Next(256);
                var bit = i;
                Console.WriteLine("[TestCase({0}, {1}, 0x{2:X2}, {3})]", bit, disp, val, ((val & (1 << bit)) != 0).ToString().ToLower());
            }
        }

        private void TestCaseTester(byte bit, byte set, bool zero)
        {
            Console.WriteLine("Bit {0}, zero {1}", bit, zero);
            var padLeft = Convert.ToString(set, 2).PadLeft(8, '0');
            Console.WriteLine(padLeft);
            Console.WriteLine("76543210");
            Console.WriteLine(new string('-', 7 - bit) + "^");
            Console.WriteLine(new string(' ', 7 - bit) + padLeft[7 - bit]);

            if (padLeft[7 - bit] != (zero ? '0' : '1'))
                Assert.Fail("TestCase Fails");
        }
    }
    }
