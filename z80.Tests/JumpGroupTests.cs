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
    }
}

