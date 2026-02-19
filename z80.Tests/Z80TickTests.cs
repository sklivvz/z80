using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class Z80TickTests : OpCodeTestBase
    {
        [Test]
        public void Tick_NOP_ReturnsTrueOnFourthTick()
        {
            asm.Noop();
            asm.Halt();

            Assert.IsFalse(en.Tick());
            Assert.IsFalse(en.Tick());
            Assert.IsFalse(en.Tick());
            Assert.IsTrue(en.Tick());
        }

        [Test]
        public void TStates_IncrementsByOnePerTick()
        {
            asm.Noop();
            asm.Halt();

            en.Tick();
            en.Tick();
            en.Tick();
            en.Tick();
            en.Tick();

            Assert.That(en.TStates, Is.EqualTo(5));
        }

        [Test]
        public void TickBudget_ExecutesMultipleInstructions()
        {
            // Two NOPs (4+4=8 T-states) + HALT
            asm.Noop();
            asm.Noop();
            asm.Halt();

            var completed = en.Tick(8);

            Assert.That(completed, Is.EqualTo(2));
            Assert.That(en.TStates, Is.EqualTo(8));
        }

        [Test]
        public void Tick_HaltedCpu_CyclesEveryFourTStates()
        {
            // HALT at address 0 — Parse() returns 0 when halted
            asm.Halt();

            // First halted NOP cycle: 3 false + 1 true
            Assert.IsFalse(en.Tick());
            Assert.IsFalse(en.Tick());
            Assert.IsFalse(en.Tick());
            Assert.IsTrue(en.Tick());

            // Second halted NOP cycle: same pattern
            Assert.IsFalse(en.Tick());
            Assert.IsFalse(en.Tick());
            Assert.IsFalse(en.Tick());
            Assert.IsTrue(en.Tick());

            Assert.That(en.TStates, Is.EqualTo(8));
        }

        [Test]
        public void Tick_HaltedCpu_InterruptBreaksHalt()
        {
            // EI + IM 1 + HALT
            asm.Ei();
            asm.Im1();
            asm.Halt();

            // Tick through EI (4 T-states) + IM 1 (8 T-states) + HALT (4 T-states)
            for (var i = 0; i < 16; i++) en.Tick();

            // Raise maskable interrupt
            en.RaiseInterrupt(maskable: true);

            // Tick through interrupt handling (17 T-states)
            for (var i = 0; i < 17; i++) en.Tick();

            // PC should be at 0x0038 (IM 1 handler)
            Assert.That(en.PC, Is.EqualTo(0x0038));
        }

        [Test]
        public void Tick_HaltedCpu_NMIBreaksHalt()
        {
            // HALT at address 0
            asm.Halt();

            // Tick through HALT (4 T-states)
            for (var i = 0; i < 4; i++) en.Tick();

            // Raise NMI
            en.RaiseInterrupt(maskable: false);

            // Tick through NMI handling (17 T-states)
            for (var i = 0; i < 17; i++) en.Tick();

            // PC should be at 0x0066 (NMI handler)
            Assert.That(en.PC, Is.EqualTo(0x0066));
        }

        [Test]
        public void Reset_ClearsTickState()
        {
            asm.Noop();
            asm.Halt();

            // Tick a few times to accumulate state
            en.Tick();
            en.Tick();

            en.Reset();

            Assert.That(en.TStates, Is.EqualTo(0));
        }

        [Test]
        public void Parse_HALT_CostsFourTStates()
        {
            asm.Halt();

            var cost = en.Parse();

            Assert.That(cost, Is.EqualTo(4));
        }

        [TestCase(2, 13)] // branch taken
        [TestCase(1, 8)]  // branch not taken
        public void Tick_DJNZ_CorrectTStates(int b, int expectedTStates)
        {
            asm.LoadRegVal(0, (byte)b); // LD B, b (7 T-states)
            asm.Djnz(0);               // DJNZ +0
            asm.Halt();

            // Tick through LD B,n (7 T-states)
            for (var i = 0; i < 7; i++) en.Tick();
            var before = en.TStates;

            // Tick through DJNZ
            while (!en.Tick()) { }

            Assert.That(en.TStates - before, Is.EqualTo(expectedTStates));
        }

        [TestCase(true, 10)]  // Z set (default F=0xFF), CALL NZ not taken
        [TestCase(false, 17)] // Z clear (after OR 1), CALL NZ taken
        public void Tick_ConditionalCall_CorrectTStates(bool zSet, int expectedTStates)
        {
            long before;

            if (!zSet)
            {
                // LD A, 1 (7 T) + OR A (4 T) clears Z flag
                asm.LoadRegVal(7, 1);
                asm.OrReg(7);
            }

            asm.CallNz(0x0100);
            asm.Halt();

            // Tick through setup instructions if any
            if (!zSet)
            {
                for (var i = 0; i < 11; i++) en.Tick(); // 7 + 4 T-states
            }
            before = en.TStates;

            // Tick through CALL NZ
            while (!en.Tick()) { }

            Assert.That(en.TStates - before, Is.EqualTo(expectedTStates));
        }

        [TestCase(2, 21)] // BC=2: repeating iteration = 21 T-states
        [TestCase(1, 16)] // BC=1: last iteration = 16 T-states
        public void Tick_LDIR_CorrectTStates(int count, int expectedTStates)
        {
            // HL=source, DE=dest, BC=count
            asm.LoadReg16Val(2, 0x8000); // LD HL, 0x8000 (10 T)
            asm.LoadReg16Val(1, 0x9000); // LD DE, 0x9000 (10 T)
            asm.LoadReg16Val(0, (ushort)count); // LD BC, count (10 T)
            asm.Ldir();
            asm.Halt();

            // Tick through 3x LD r16,nn = 30 T-states
            for (var i = 0; i < 30; i++) en.Tick();
            var before = en.TStates;

            // Tick through first LDIR iteration
            while (!en.Tick()) { }

            Assert.That(en.TStates - before, Is.EqualTo(expectedTStates));
        }
    }
}
