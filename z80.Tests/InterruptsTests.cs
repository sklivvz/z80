using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Security.Policy;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace z80.Tests
{
    [TestFixture]
    internal class InterruptsTests : OpCodeTestBase
    {
        [Test]
        public void Test_NMI_Run()
        {
            asm.Ei();
            asm.Noop();

            asm.Position = 0x66;
            asm.Noop();


            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: false);
            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x66, en.PC);
            Assert.IsFalse(en.Iff1);
            Assert.IsTrue(en.Iff2);
        }

        [Test]
        public void Test_NMI_Halt()
        {
            asm.Ei();
            asm.Halt();

            asm.Position = 0x66;
            asm.Noop();

            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: false);
            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x66, en.PC);
            Assert.IsFalse(en.Iff1);
            Assert.IsTrue(en.Iff2);
        }

        [Test]
        public void Test_NMI_Halt_DisabledInterrupts()
        {
            asm.Di();
            asm.Halt();

            asm.Position = 0x66;
            asm.Noop();

            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: false);
            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x66, en.PC);
            Assert.IsFalse(en.Iff1);
            Assert.IsFalse(en.Iff2);
        }

        [Test]
        public void Test_NMI_RetN()
        {
            asm.Ei();
            asm.Halt();

            asm.Position = 0x66;
            asm.RetN();

            en.Step();
            en.Step();
            en.RaiseInterrupt(maskable: false);
            en.Step();

            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x02, en.PC);
            Assert.IsTrue(en.Iff1);
            Assert.IsTrue(en.Iff2);
        }

        [Test]
        public void Test_MI_IM0_Run()
        {
            asm.Ei();
            asm.Im0();
            asm.Noop();

            en.Step();
            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: true, data: 0xC7 /*RST 0*/);
            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x00, en.PC);
            Assert.IsFalse(en.Iff1);
            Assert.IsFalse(en.Iff2);
        }

        [Test]
        public void Test_MI_IM0_Halt()
        {
            asm.Ei();
            asm.Im0();
            asm.Halt();


            en.Step();
            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: true, data: 0xC7 /*RST 0*/);
            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x00, en.PC);
            Assert.IsFalse(en.Iff1);
            Assert.IsFalse(en.Iff2);
        }

        [Test]
        public void Test_MI_IM0_Halt_DisabledInterrupts()
        {
            asm.Di();
            asm.Im0();
            asm.Halt();


            en.Step();
            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: true, data: 0xC7 /*RST 0*/);
            var halted = en.Step();

            Assert.IsTrue(halted);
            Assert.AreEqual(0x04, en.PC);
            Assert.IsFalse(en.Iff1);
            Assert.IsFalse(en.Iff2);
        }

        [Test]
        public void Test_MI_IM0_RetI()
        {
            asm.Ei();
            asm.Im0();
            asm.Halt();

            asm.Position = 0x38;
            asm.Ei();
            asm.RetN();

            en.Step();
            en.Step();
            en.RaiseInterrupt(maskable: true, data: 0xFF /*RST 7*/);
            en.Step();
            en.Step();

            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x03, en.PC);
            Assert.IsTrue(en.Iff1);
            Assert.IsTrue(en.Iff2);
        }

        [Test]
        public void Test_MI_IM1_Run()
        {
            asm.Ei();
            asm.Im1();
            asm.Noop();

            en.Step();
            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: true);
            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x38, en.PC);
            Assert.IsFalse(en.Iff1);
            Assert.IsFalse(en.Iff2);
        }

        [Test]
        public void Test_MI_IM1_Halt()
        {
            asm.Ei();
            asm.Im1();
            asm.Halt();


            en.Step();
            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: true);
            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x38, en.PC);
            Assert.IsFalse(en.Iff1);
            Assert.IsFalse(en.Iff2);
        }

        [Test]
        public void Test_MI_IM1_Halt_DisabledInterrupts()
        {
            asm.Di();
            asm.Im1();
            asm.Halt();


            en.Step();
            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: true);
            var halted = en.Step();

            Assert.IsTrue(halted);
            Assert.AreEqual(0x04, en.PC);
            Assert.IsFalse(en.Iff1);
            Assert.IsFalse(en.Iff2);
        }

        [Test]
        public void Test_MI_IM1_RetI()
        {
            asm.Ei();
            asm.Im1();
            asm.Halt();

            asm.Position = 0x38;
            asm.Ei();
            asm.RetN();

            en.Step();
            en.Step();
            en.RaiseInterrupt(maskable: true);
            en.Step();
            en.Step();

            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x03, en.PC);
            Assert.IsTrue(en.Iff1);
            Assert.IsTrue(en.Iff2);
        }

        [Test]
        public void Test_MI_IM2_Run()
        {
            asm.Ei();
            asm.LoadRegVal(7,0x12);
            asm.LoadIA();
            asm.Im2();
            asm.Noop();

            asm.Position = 0x1234;
            asm.Data(0x56);
            asm.Data(0x78);

            en.Step();
            en.Step();
            en.Step();
            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: true, data: 0x34);
            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x5678, en.PC, en.PC.ToString("X"));
            Assert.IsFalse(en.Iff1);
            Assert.IsFalse(en.Iff2);
        }

        [Test]
        public void Test_MI_IM2_Halt()
        {
            asm.Ei();
            asm.LoadRegVal(7, 0x12);
            asm.LoadIA();
            asm.Im2();
            asm.Halt();

            asm.Position = 0x1234;
            asm.Data(0x56);
            asm.Data(0x78);

            en.Step();
            en.Step();
            en.Step();
            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: true, data: 0x34);
            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x5678, en.PC, en.PC.ToString("X"));
            Assert.IsFalse(en.Iff1);
            Assert.IsFalse(en.Iff2);
        }

        [Test]
        public void Test_MI_IM2_Halt_DisabledInterrupts()
        {
            asm.Di();
            asm.LoadRegVal(7, 0x12);
            asm.LoadIA();
            asm.Im2();
            asm.Halt();

            asm.Position = 0x1234;
            asm.Data(0x56);
            asm.Data(0x78);

            en.Step();
            en.Step();
            en.Step();
            en.Step();
            en.Step();

            en.RaiseInterrupt(maskable: true, data: 0xC7 /*RST 0*/);
            var halted = en.Step();

            Assert.IsTrue(halted);
            Assert.AreEqual(0x08, en.PC);
            Assert.IsFalse(en.Iff1);
            Assert.IsFalse(en.Iff2);
        }

        [Test]
        public void Test_MI_IM2_RetI()
        {
            asm.Ei();
            asm.Im2();
            asm.Halt();

            asm.Position = 0x38;
            asm.Ei();
            asm.RetN();

            en.Step();
            en.Step();
            en.RaiseInterrupt(maskable: true, data: 0xFF /*RST 7*/);
            en.Step();
            en.Step();

            var halted = en.Step();

            Assert.IsFalse(halted);
            Assert.AreEqual(0x03, en.PC);
            Assert.IsTrue(en.Iff1);
            Assert.IsTrue(en.Iff2);
        }

        [Test]
        public void Test_EI_DelaysInterruptByOneInstruction()
        {
            // EI + NOP + HALT
            // IM 1 handler at 0x38
            asm.Im1();
            asm.Ei();
            asm.Noop();
            asm.Halt();

            asm.Position = 0x38;
            asm.Halt();

            en.Step(); // IM 1
            en.Step(); // EI — interrupts should NOT be recognized until after next instruction

            // Raise interrupt immediately after EI
            en.RaiseInterrupt(maskable: true);

            en.Step(); // NOP — this must execute before interrupt is taken

            // After the NOP, PC should be 0x04 (past NOP), not 0x38 (handler)
            // The interrupt should be serviced on the NEXT Step()
            Assert.AreEqual(0x04, en.PC, "NOP after EI must execute before interrupt is recognized");
        }
    }
}

