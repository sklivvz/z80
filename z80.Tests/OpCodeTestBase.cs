using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace z80.Tests
{
    [TestFixture]
    public abstract class OpCodeTestBase
    {
        protected TestSystem en;
        protected Z80Asm asm;
        protected byte[] _ram;


        //////////////////////////
        [SetUp]
        public void TestSetup()
        {
            _ram = new byte[0x10000];
            en = new TestSystem(_ram);
            asm = new Z80Asm(_ram);
            en.Reset();
            asm.Reset();
        }
    }
}
