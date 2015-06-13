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

        [Test]
        #region testcases
        [TestCase(0, 0, 0x4D, 0x4D)]
        [TestCase(0, 1, 0x94, 0x96)]
        [TestCase(0, 2, 0x69, 0x6D)]
        [TestCase(0, 3, 0x23, 0x2B)]
        [TestCase(0, 4, 0xCC, 0xDC)]
        [TestCase(0, 5, 0x40, 0x60)]
        [TestCase(0, 6, 0xB5, 0xF5)]
        [TestCase(0, 7, 0xF3, 0xF3)]
        [TestCase(1, 0, 0xD4, 0xD5)]
        [TestCase(1, 1, 0xB7, 0xB7)]
        [TestCase(1, 2, 0x9E, 0x9E)]
        [TestCase(1, 3, 0x39, 0x39)]
        [TestCase(1, 4, 0x79, 0x79)]
        [TestCase(1, 5, 0x6B, 0x6B)]
        [TestCase(1, 6, 0xDB, 0xDB)]
        [TestCase(1, 7, 0x8D, 0x8D)]
        [TestCase(2, 0, 0x6A, 0x6B)]
        [TestCase(2, 1, 0xAC, 0xAE)]
        [TestCase(2, 2, 0xC6, 0xC6)]
        [TestCase(2, 3, 0x25, 0x2D)]
        [TestCase(2, 4, 0x16, 0x16)]
        [TestCase(2, 5, 0xDA, 0xFA)]
        [TestCase(2, 6, 0x8C, 0xCC)]
        [TestCase(2, 7, 0x25, 0xA5)]
        [TestCase(3, 0, 0xA9, 0xA9)]
        [TestCase(3, 1, 0xA0, 0xA2)]
        [TestCase(3, 2, 0x8C, 0x8C)]
        [TestCase(3, 3, 0x9C, 0x9C)]
        [TestCase(3, 4, 0xF2, 0xF2)]
        [TestCase(3, 5, 0x57, 0x77)]
        [TestCase(3, 6, 0x50, 0x50)]
        [TestCase(3, 7, 0x97, 0x97)]
        [TestCase(4, 0, 0xA9, 0xA9)]
        [TestCase(4, 1, 0x1A, 0x1A)]
        [TestCase(4, 2, 0xDA, 0xDE)]
        [TestCase(4, 3, 0x0C, 0x0C)]
        [TestCase(4, 4, 0xF7, 0xF7)]
        [TestCase(4, 5, 0x78, 0x78)]
        [TestCase(4, 6, 0x3A, 0x7A)]
        [TestCase(4, 7, 0xA3, 0xA3)]
        [TestCase(5, 0, 0xF5, 0xF5)]
        [TestCase(5, 1, 0xF6, 0xF6)]
        [TestCase(5, 2, 0x44, 0x44)]
        [TestCase(5, 3, 0x90, 0x98)]
        [TestCase(5, 4, 0xB3, 0xB3)]
        [TestCase(5, 5, 0x4B, 0x6B)]
        [TestCase(5, 6, 0x59, 0x59)]
        [TestCase(5, 7, 0x85, 0x85)]
        [TestCase(7, 0, 0xB9, 0xB9)]
        [TestCase(7, 1, 0x6C, 0x6E)]
        [TestCase(7, 2, 0x33, 0x37)]
        [TestCase(7, 3, 0x68, 0x68)]
        [TestCase(7, 4, 0x89, 0x99)]
        [TestCase(7, 5, 0x9F, 0xBF)]
        [TestCase(7, 6, 0x60, 0x60)]
        [TestCase(7, 7, 0x72, 0xF2)]
        #endregion
        public void Test_SET_B_R(byte register, byte bit, byte set, byte res)
        {
            TestCaseGenerator();
            asm.LoadRegVal(register, set);
            asm.SetNReg(bit, register);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.Reg8(register), $"Expected 0x{res:X2} but was 0x{en.Reg8(register):X2}");

        }

        [Test]
        #region testcases
        [TestCase(0, 0xB0, 0xB1)]
        [TestCase(1, 0xCB, 0xCB)]
        [TestCase(2, 0x3C, 0x3C)]
        [TestCase(3, 0xBF, 0xBF)]
        [TestCase(4, 0xCB, 0xDB)]
        [TestCase(5, 0x23, 0x23)]
        [TestCase(6, 0xF7, 0xF7)]
        [TestCase(7, 0x56, 0xD6)]


        #endregion
        public void Test_SET_B_HL(byte bit, byte set, byte res)
        {
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(set);
            asm.SetNAtHl(bit);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[en.HL], $"Expected 0x{res:X2} but was 0x{_ram[en.HL]:X2}");
        }

        [Test]
        #region testcases
        [TestCase(0, -78, 0x29, 0x29)]
        [TestCase(1, -29, 0x27, 0x27)]
        [TestCase(2, -54, 0xC7, 0xC7)]
        [TestCase(3, -56, 0x31, 0x39)]
        [TestCase(4, 124, 0xCE, 0xDE)]
        [TestCase(5, -94, 0x02, 0x22)]
        [TestCase(6, 12, 0x2C, 0x6C)]
        [TestCase(7, -8, 0x83, 0x83)]

        #endregion
        public void Test_SET_B_IX_d(byte bit, sbyte d, byte set, byte res)
        {
            asm.LoadReg16Val(2, (ushort)(0x0140 + d));
            asm.LoadAtHLVal(set);
            asm.LoadIxVal(0x0140);
            asm.SetNAtIxd(bit, d);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[en.IX+d], $"Expected 0x{res:X2} but was 0x{_ram[en.IX+d]:X2}");
        }

        [Test]
        #region testcases
        [TestCase(0, -31, 0x26, 0x27)]
        [TestCase(1, 24, 0x3B, 0x3B)]
        [TestCase(2, -68, 0x47, 0x47)]
        [TestCase(3, 110, 0x69, 0x69)]
        [TestCase(4, 43, 0x52, 0x52)]
        [TestCase(5, 3, 0x04, 0x24)]
        [TestCase(6, -76, 0xFF, 0xFF)]
        [TestCase(7, 54, 0x52, 0xD2)]
        #endregion
        public void Test_SET_B_IY_d(byte bit, sbyte d, byte set, byte res)
        {
            asm.LoadReg16Val(2, (ushort)(0x0140 + d));
            asm.LoadAtHLVal(set);
            asm.LoadIyVal(0x0140);
            asm.SetNAtIyd(bit, d);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[en.IY + d], $"Expected 0x{res:X2} but was 0x{_ram[en.IY + d]:X2}");
        }

        [Test]
        #region testcases
        [TestCase(0, 0, 0x17, 0x16)]
        [TestCase(0, 1, 0xFC, 0xFC)]
        [TestCase(0, 2, 0x73, 0x73)]
        [TestCase(0, 3, 0xBD, 0xB5)]
        [TestCase(0, 4, 0x19, 0x09)]
        [TestCase(0, 5, 0xF5, 0xD5)]
        [TestCase(0, 6, 0xFD, 0xBD)]
        [TestCase(0, 7, 0xC7, 0x47)]
        [TestCase(1, 0, 0xD1, 0xD0)]
        [TestCase(1, 1, 0xD9, 0xD9)]
        [TestCase(1, 2, 0x59, 0x59)]
        [TestCase(1, 3, 0xB2, 0xB2)]
        [TestCase(1, 4, 0x68, 0x68)]
        [TestCase(1, 5, 0x39, 0x19)]
        [TestCase(1, 6, 0xC9, 0x89)]
        [TestCase(1, 7, 0x6D, 0x6D)]
        [TestCase(2, 0, 0x9F, 0x9E)]
        [TestCase(2, 1, 0xA3, 0xA1)]
        [TestCase(2, 2, 0x8B, 0x8B)]
        [TestCase(2, 3, 0xB8, 0xB0)]
        [TestCase(2, 4, 0x70, 0x60)]
        [TestCase(2, 5, 0xAA, 0x8A)]
        [TestCase(2, 6, 0xBC, 0xBC)]
        [TestCase(2, 7, 0x50, 0x50)]
        [TestCase(3, 0, 0x96, 0x96)]
        [TestCase(3, 1, 0x5F, 0x5D)]
        [TestCase(3, 2, 0x23, 0x23)]
        [TestCase(3, 3, 0x3C, 0x34)]
        [TestCase(3, 4, 0x2E, 0x2E)]
        [TestCase(3, 5, 0xA9, 0x89)]
        [TestCase(3, 6, 0xD0, 0x90)]
        [TestCase(3, 7, 0x2D, 0x2D)]
        [TestCase(4, 0, 0xBD, 0xBC)]
        [TestCase(4, 1, 0xAC, 0xAC)]
        [TestCase(4, 2, 0x30, 0x30)]
        [TestCase(4, 3, 0x00, 0x00)]
        [TestCase(4, 4, 0x67, 0x67)]
        [TestCase(4, 5, 0xF4, 0xD4)]
        [TestCase(4, 6, 0xE2, 0xA2)]
        [TestCase(4, 7, 0x7D, 0x7D)]
        [TestCase(5, 0, 0xF0, 0xF0)]
        [TestCase(5, 1, 0xE5, 0xE5)]
        [TestCase(5, 2, 0xE7, 0xE3)]
        [TestCase(5, 3, 0x55, 0x55)]
        [TestCase(5, 4, 0xEA, 0xEA)]
        [TestCase(5, 5, 0x53, 0x53)]
        [TestCase(5, 6, 0x01, 0x01)]
        [TestCase(5, 7, 0x0E, 0x0E)]
        [TestCase(7, 0, 0xE2, 0xE2)]
        [TestCase(7, 1, 0xEB, 0xE9)]
        [TestCase(7, 2, 0x93, 0x93)]
        [TestCase(7, 3, 0xF5, 0xF5)]
        [TestCase(7, 4, 0x58, 0x48)]
        [TestCase(7, 5, 0xD0, 0xD0)]
        [TestCase(7, 6, 0x5D, 0x1D)]
        [TestCase(7, 7, 0xA8, 0x28)]
        #endregion
        public void Test_RES_B_R(byte register, byte bit, byte set, byte res)
        {
            asm.LoadRegVal(register, set);
            asm.ResNReg(bit, register);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, en.Reg8(register), $"Expected 0x{res:X2} but was 0x{en.Reg8(register):X2}");

        }

        [Test]
        #region testcases
        [TestCase(0, 0x7E, 0x7E)]
        [TestCase(1, 0x64, 0x64)]
        [TestCase(2, 0x81, 0x81)]
        [TestCase(3, 0x08, 0x00)]
        [TestCase(4, 0x8E, 0x8E)]
        [TestCase(5, 0x91, 0x91)]
        [TestCase(6, 0xB5, 0xB5)]
        [TestCase(7, 0x55, 0x55)]
        #endregion
        public void Test_RES_B_HL(byte bit, byte set, byte res)
        {
            asm.LoadReg16Val(2, 0x0040);
            asm.LoadAtHLVal(set);
            asm.ResNAtHl(bit);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[en.HL], $"Expected 0x{res:X2} but was 0x{_ram[en.HL]:X2}");
        }

        [Test]
        #region testcases
        [TestCase(0, -90, 0x3C, 0x3C)]
        [TestCase(1, -122, 0x3E, 0x3C)]
        [TestCase(2, -127, 0xE7, 0xE3)]
        [TestCase(3, 26, 0x26, 0x26)]
        [TestCase(4, -26, 0x90, 0x80)]
        [TestCase(5, -93, 0x4C, 0x4C)]
        [TestCase(6, -102, 0x7E, 0x3E)]
        [TestCase(7, 68, 0x31, 0x31)]
        #endregion
        public void Test_RES_B_IX_d(byte bit, sbyte d, byte set, byte res)
        {
            asm.LoadReg16Val(2, (ushort)(0x0140 + d));
            asm.LoadAtHLVal(set);
            asm.LoadIxVal(0x0140);
            asm.ResNAtIxd(bit, d);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[en.IX + d], $"Expected 0x{res:X2} but was 0x{_ram[en.IX + d]:X2}");
        }

        [Test]
        #region testcases
        [TestCase(0, 58, 0x52, 0x52)]
        [TestCase(1, -101, 0x45, 0x45)]
        [TestCase(2, 57, 0x43, 0x43)]
        [TestCase(3, 125, 0x5A, 0x52)]
        [TestCase(4, -123, 0x65, 0x65)]
        [TestCase(5, 42, 0x09, 0x09)]
        [TestCase(6, -30, 0x4E, 0x0E)]
        [TestCase(7, -80, 0x83, 0x03)]
        #endregion
        public void Test_RES_B_IY_d(byte bit, sbyte d, byte set, byte res)
        {
            asm.LoadReg16Val(2, (ushort)(0x0140 + d));
            asm.LoadAtHLVal(set);
            asm.LoadIyVal(0x0140);
            asm.ResNAtIyd(bit, d);
            asm.Halt();

            en.Run();

            Assert.AreEqual(asm.Position, en.PC);
            Assert.AreEqual(res, _ram[en.IY + d], $"Expected 0x{res:X2} but was 0x{_ram[en.IY + d]:X2}");
        }


        private void TestCaseGenerator()
        {
            Random r = new Random();
            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i & 7) == 6) continue;
                    var val = r.Next(256);
                    var disp = (sbyte)r.Next(256);
                    Console.WriteLine($"[TestCase({j}, {disp}, 0x{val:X2}, 0x{val & ~(1 << j):X2})]");
                }
            }
        }

        private void TestCaseTester(byte bit, byte set, byte res)
        {
            Console.WriteLine("Bit {0}, res 0x{1:X2}", bit, res);
            var padLeft = Convert.ToString(set, 2).PadLeft(8, '0');
            var padLeft2 = Convert.ToString(res, 2).PadLeft(8, '0');
            Console.WriteLine(padLeft);
            Console.WriteLine(padLeft2);
            Console.WriteLine("76543210");
            Console.WriteLine(new string('-', 7 - bit) + "^");
            Console.WriteLine(new string(' ', 7 - bit) + padLeft2[7 - bit]);

            if (padLeft2[7 - bit] != '1')
                Assert.Fail("TestCase Fails");
        }
    }
    }
