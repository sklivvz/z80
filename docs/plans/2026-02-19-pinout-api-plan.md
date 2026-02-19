# Z80 Pinout-Based API Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Redesign the Z80 emulator's public API to mirror the real Z80 chip's 40-pin pinout, with a unified IBus interface replacing Memory and IPorts.

**Architecture:** Single `IBus` interface with semantic methods for bus operations and interrupt signals. `SimpleBus` helper class replaces both `Memory` and `TestPorts`. Z80 gets public register properties and `Parse()` returns T-states. See `docs/plans/2026-02-19-pinout-api-design.md` for the full design.

**Tech Stack:** C# / .NET 10, NUnit 4

---

### Task 1: Create IBus Interface

**Files:**
- Create: `z80/IBus.cs`

**Step 1: Create the IBus interface file**

```csharp
namespace z80
{
    public interface IBus
    {
        byte MemoryRead(ushort address);
        void MemoryWrite(ushort address, byte data);
        byte IoRead(ushort address);
        void IoWrite(ushort address, byte data);

        bool INT { get; }
        bool NMI { get; }
        byte Data { get; }

        bool WAIT { get; }
        bool BUSRQ { get; }
        bool RESET { get; }
    }
}
```

**Step 2: Verify it compiles**

Run: `dotnet build z80/z80.csproj --verbosity quiet`
Expected: Build succeeded.

**Step 3: Commit**

```bash
git add z80/IBus.cs
git commit -m "feat: add IBus interface for pinout-based bus abstraction"
```

---

### Task 2: Create SimpleBus

**Files:**
- Create: `z80/SimpleBus.cs`
- Test: `z80.Tests/SimpleBusTests.cs`

**Step 1: Write failing tests for SimpleBus**

```csharp
using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    public class SimpleBusTests
    {
        [Test]
        public void MemoryRead_ReturnsValue()
        {
            var mem = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var bus = new SimpleBus(mem, 0);

            for (ushort i = 0; i < mem.Length; i++)
                Assert.AreEqual(i, bus.MemoryRead(i));
        }

        [Test]
        public void MemoryWrite_InRam_WritesValue()
        {
            var mem = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var bus = new SimpleBus(mem, 0);

            for (ushort i = 0; i < mem.Length; i++)
            {
                bus.MemoryWrite(i, (byte)(0xFF ^ i));
                Assert.AreEqual((byte)(0xFF ^ i), bus.MemoryRead(i));
            }
        }

        [Test]
        public void MemoryWrite_InRom_IsIgnored()
        {
            var mem = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var bus = new SimpleBus(mem, 10);

            for (ushort i = 0; i < mem.Length; i++)
            {
                bus.MemoryWrite(i, (byte)(0xFF ^ i));
                Assert.AreEqual(i, bus.MemoryRead(i));
            }
        }

        [Test]
        public void IoRead_ReturnsSetInputValue()
        {
            var bus = new SimpleBus(new byte[10], 0);
            bus.SetInput(0x42, 0xAB);
            Assert.AreEqual(0xAB, bus.IoRead(0x42));
        }

        [Test]
        public void IoWrite_IsReadableViaGetOutput()
        {
            var bus = new SimpleBus(new byte[10], 0);
            bus.IoWrite(0x42, 0xCD);
            Assert.AreEqual(0xCD, bus.GetOutput(0x42));
        }

        [Test]
        public void NMI_AutoClearsOnRead()
        {
            var bus = new SimpleBus(new byte[10], 0);
            bus.NMI = true;
            Assert.IsTrue(bus.NMI);
            Assert.IsFalse(bus.NMI);
        }

        [Test]
        public void INT_DoesNotAutoClear()
        {
            var bus = new SimpleBus(new byte[10], 0);
            bus.INT = true;
            Assert.IsTrue(bus.INT);
            Assert.IsTrue(bus.INT);
        }

        [Test]
        public void StubbedSignals_ReturnFalse()
        {
            var bus = new SimpleBus(new byte[10], 0);
            Assert.IsFalse(bus.WAIT);
            Assert.IsFalse(bus.BUSRQ);
            Assert.IsFalse(bus.RESET);
        }
    }
}
```

**Step 2: Run tests to verify they fail**

Run: `dotnet test z80.Tests/z80.Tests.csproj --filter SimpleBusTests --verbosity quiet`
Expected: Build error — `SimpleBus` does not exist yet.

**Step 3: Implement SimpleBus**

```csharp
namespace z80
{
    public class SimpleBus : IBus
    {
        private readonly byte[] _memory;
        private readonly ushort _ramStart;
        private readonly byte[] _inputs = new byte[0x10000];
        private readonly byte[] _outputs = new byte[0x10000];
        private bool _nmi;
        private byte _data;

        public SimpleBus(byte[] memory, ushort ramStart)
        {
            _memory = memory;
            _ramStart = ramStart;
        }

        public byte MemoryRead(ushort address) => _memory[address];

        public void MemoryWrite(ushort address, byte data)
        {
            if (address >= _ramStart)
                _memory[address] = data;
        }

        public byte IoRead(ushort address) => _inputs[address];

        public void IoWrite(ushort address, byte data) => _outputs[address] = data;

        public bool INT { get; set; }

        public bool NMI
        {
            get { var ret = _nmi; _nmi = false; return ret; }
            set { _nmi = value; }
        }

        public byte Data
        {
            get { var ret = _data; _data = 0x00; return ret; }
            set { _data = value; }
        }

        public bool WAIT => false;
        public bool BUSRQ => false;
        public bool RESET => false;

        public void SetInput(ushort address, byte value) => _inputs[address] = value;
        public byte GetOutput(ushort address) => _outputs[address];
    }
}
```

**Step 4: Run tests to verify they pass**

Run: `dotnet test z80.Tests/z80.Tests.csproj --filter SimpleBusTests --verbosity quiet`
Expected: All 8 tests pass.

**Step 5: Commit**

```bash
git add z80/SimpleBus.cs z80.Tests/SimpleBusTests.cs
git commit -m "feat: add SimpleBus implementing IBus with ROM/RAM and I/O"
```

---

### Task 3: Rewire Z80 to Use IBus

This is the core change. Replace `Memory mem` + `IPorts ports` with `IBus bus` inside Z80.cs. This is a mechanical substitution throughout the 3600+ line file.

**Files:**
- Modify: `z80/Z80.cs`

**Step 1: Change constructor and fields**

In `z80/Z80.cs`, replace:
- `private readonly Memory mem;` → `private readonly IBus bus;`
- `private readonly IPorts ports;` → remove
- `private DateTime _clock = DateTime.UtcNow;` → remove
- Constructor: `public Z80(Memory memory, IPorts ports)` → `public Z80(IBus bus)`
- Constructor body: replace null checks and assignments

The constructor becomes:
```csharp
public Z80(IBus bus)
{
    if (bus == null) throw new ArgumentNullException(nameof(bus));
    this.bus = bus;
    Reset();
}
```

**Step 2: Replace all `mem[address]` reads with `bus.MemoryRead(address)`**

There are ~158 occurrences of `mem[` in Z80.cs. Each `mem[addr]` on the right side of an assignment or in an expression becomes `bus.MemoryRead(addr)`. Each `mem[addr] = value` becomes `bus.MemoryWrite(addr, value)`.

Specific patterns:
- `mem[addr]` (read) → `bus.MemoryRead(addr)`
- `mem[addr] = value` → `bus.MemoryWrite(addr, value)`
- `mem[Pc]` in `Fetch()` → `bus.MemoryRead(Pc)`

**Step 3: Replace all `ports.ReadPort`/`ports.WritePort` calls**

There are ~12 occurrences:
- `ports.ReadPort(addr)` → `bus.IoRead(addr)`
- `ports.WritePort(addr, value)` → `bus.IoWrite(addr, value)`

**Step 4: Replace interrupt signal reads**

There are ~4 occurrences:
- `ports.NMI` → `bus.NMI`
- `ports.MI` → `bus.INT`  (rename MI to INT)
- `ports.Data` → `bus.Data`

**Step 5: Change `Parse()` to return int**

Change signature from `public void Parse()` to `public int Parse()`.

Add a private field `private int _tStates;` to accumulate T-states within a single `Parse()` call.

At the start of `Parse()`, set `_tStates = 0;`.
At the end of `Parse()`, `return _tStates;`.
Each `return;` statement inside Parse that exits early (interrupts, HALT) must also return `_tStates`.

**Step 6: Change `Wait(int t)` to accumulate instead of sleeping**

Replace:
```csharp
private void Wait(int t)
{
    registers[R] += (byte)((t + 3) / 4);
    const int realTicksPerTick = 250;
    // ... Thread.Sleep logic ...
}
```

With:
```csharp
private void Wait(int t)
{
    registers[R] += (byte)((t + 3) / 4);
    _tStates += t;
}
```

Remove `using System.Threading;` from the top of the file.

**Step 7: Rename `Halt` property to `HALT`**

Change `public bool Halt { get; private set; }` to `public bool HALT { get; private set; }`.
Update all internal references from `Halt` to `HALT`.

**Step 8: Add output pin properties and bus state**

Add after the HALT property:
```csharp
public bool M1 { get; private set; }
public bool RFSH { get; private set; }
public bool BUSACK => false;
public ushort AddressBus { get; private set; }
public byte DataBus { get; private set; }
```

Set `M1 = true` at the start of the opcode fetch in `Parse()`, and `M1 = false` after fetching.
Set `AddressBus` and `DataBus` whenever bus operations occur (in `Fetch()` and around `bus.MemoryRead`/`bus.MemoryWrite` calls — at minimum in the `Fetch()` method).
Set `RFSH = true` / `RFSH = false` around the R-register increment in Wait() (optional — can be deferred).

Note: Full signal tracking for every bus access throughout 3600 lines would be invasive. For this task, set them in `Fetch()` (the opcode fetch path). Further granularity can be added later.

**Step 9: Add public register properties**

Add after the output pin properties:
```csharp
public new byte A => registers[Z80.A];
public new byte F => registers[Z80.F];
public new byte B => registers[Z80.B];
// ... etc for all registers
```

Note: The internal constants `A`, `B`, `C`, etc. are `private const byte` fields that conflict with public property names. The simplest resolution is to rename the internal constants to have a prefix (e.g., `rA`, `rB`, `rC`, etc.) or use a different naming scheme. This requires a find-and-replace across Z80.cs:

- `const byte B = 0;` → `const byte rB = 0;` etc.
- All `registers[B]` → `registers[rB]` etc.

Then the public properties become:
```csharp
public byte A => registers[rA];
public byte F => registers[rF];
public byte B => registers[rB];
public byte C => registers[rC];
public byte D => registers[rD];
public byte E => registers[rE];
public byte H => registers[rH];
public byte L => registers[rL];
public ushort AF => (ushort)((registers[rA] << 8) | registers[rF]);
public ushort BC => (ushort)((registers[rB] << 8) | registers[rC]);
public ushort DE => (ushort)((registers[rD] << 8) | registers[rE]);
public ushort HL => (ushort)((registers[rH] << 8) | registers[rL]);
public ushort IX => (ushort)((registers[rIX] << 8) | registers[rIX + 1]);
public ushort IY => (ushort)((registers[rIY] << 8) | registers[rIY + 1]);
public ushort SP => (ushort)((registers[rSP] << 8) | registers[rSP + 1]);
public ushort PC => (ushort)((registers[rPC] << 8) | registers[rPC + 1]);
public byte I => registers[rI];
public byte R => registers[rR];

public byte Ap => registers[rAp];
public byte Fp => registers[rFp];
public byte Bp => registers[rBp];
public byte Cp => registers[rCp];
public byte Dp => registers[rDp];
public byte Ep => registers[rEp];
public byte Hp => registers[rHp];
public byte Lp => registers[rLp];
public ushort AFp => (ushort)((registers[rAp] << 8) | registers[rFp]);
public ushort BCp => (ushort)((registers[rBp] << 8) | registers[rCp]);
public ushort DEp => (ushort)((registers[rDp] << 8) | registers[rEp]);
public ushort HLp => (ushort)((registers[rHp] << 8) | registers[rLp]);

public bool IFF1 => _iff1;
public bool IFF2 => _iff2;
public int InterruptMode => _interruptMode;
```

The private fields `IFF1`/`IFF2`/`interruptMode` need renaming to avoid conflict with the new public properties:
- `private bool IFF1;` → `private bool _iff1;`
- `private bool IFF2;` → `private bool _iff2;`
- `private int interruptMode;` → `private int _interruptMode;`

Similarly, internal helper properties `Hl`, `Sp`, `Ix`, `Iy`, `Bc`, `De`, `Pc` conflict with the new public ones. Rename them:
- `private ushort Hl` → `private ushort _hl`
- `private ushort Sp` → `private ushort _sp`
- `private ushort Ix` → `private ushort _ix`
- `private ushort Iy` → `private ushort _iy`
- `private ushort Bc` → `private ushort _bc`
- `private ushort De` → `private ushort _de`
- `private ushort Pc` → `private ushort _pc`

**Step 10: Verify it compiles**

Run: `dotnet build z80/z80.csproj --verbosity quiet`
Expected: Build succeeded. The test project will NOT build yet (next task fixes that).

**Step 11: Commit**

```bash
git add z80/Z80.cs
git commit -m "feat: rewire Z80 to use IBus, return T-states from Parse(), add register properties"
```

---

### Task 4: Update Test Infrastructure

**Files:**
- Modify: `z80.Tests/TestSystem.cs`
- Delete: `z80.Tests/TestPorts.cs`
- Modify: `z80.Tests/OpCodeTestBase.cs`
- Modify: `z80.Tests/MemoryTests.cs`
- Modify: `z80.Tests/InterruptsTests.cs`

**Step 1: Rewrite TestSystem to use SimpleBus**

TestSystem currently creates `new Z80(new Memory(ram, 0), TestPorts)`. Replace with `new Z80(new SimpleBus(ram, 0))`.

The register access properties (`A`, `B`, `C`, `AF`, `BC`, etc.) currently go through `GetState()` byte indexing. Replace them to read directly from the Z80 instance's new public properties. This eliminates all the `_B`, `_C`, `_D`, etc. constants and the `Reg8`/`Reg16` methods.

The `RaiseInterrupt` method currently sets `TestPorts.MI`/`TestPorts.NMI`/`TestPorts.Data`. It should now access the `SimpleBus` instance to set `INT`/`NMI`/`Data`.

New TestSystem:
```csharp
using System;

namespace z80.Tests
{
    public class TestSystem
    {
        private readonly byte[] _ram;
        private readonly Z80 _myZ80;
        private readonly SimpleBus _bus;
        private bool _hasDump;

        public ushort AF => _myZ80.AF;
        public ushort BC => _myZ80.BC;
        public ushort DE => _myZ80.DE;
        public ushort HL => _myZ80.HL;
        public ushort IX => _myZ80.IX;
        public ushort IY => _myZ80.IY;
        public ushort SP => _myZ80.SP;
        public ushort PC => _myZ80.PC;
        public ushort AFp => _myZ80.AFp;
        public ushort BCp => _myZ80.BCp;
        public ushort DEp => _myZ80.DEp;
        public ushort HLp => _myZ80.HLp;

        public byte A => _myZ80.A;
        public byte B => _myZ80.B;
        public byte C => _myZ80.C;
        public byte D => _myZ80.D;
        public byte E => _myZ80.E;
        public byte F => _myZ80.F;
        public byte H => _myZ80.H;
        public byte L => _myZ80.L;
        public byte I => _myZ80.I;
        public byte R => _myZ80.R;

        public byte Ap => _myZ80.Ap;
        public byte Bp => _myZ80.Bp;
        public byte Cp => _myZ80.Cp;
        public byte Dp => _myZ80.Dp;
        public byte Ep => _myZ80.Ep;
        public byte Fp => _myZ80.Fp;
        public byte Hp => _myZ80.Hp;
        public byte Lp => _myZ80.Lp;

        public bool FlagS => (F & 0x80) > 0;
        public bool FlagZ => (F & 0x40) > 0;
        public bool FlagH => (F & 0x10) > 0;
        public bool FlagP => (F & 0x04) > 0;
        public bool FlagN => (F & 0x02) > 0;
        public bool FlagC => (F & 0x01) > 0;

        public bool Iff1 => _myZ80.IFF1;
        public bool Iff2 => _myZ80.IFF2;

        public SimpleBus Bus => _bus;

        public TestSystem(byte[] ram)
        {
            _ram = ram;
            _bus = new SimpleBus(ram, 0);
            _myZ80 = new Z80(_bus);
        }

        public void Run()
        {
            int bailout = 1000;

            while (!_myZ80.HALT && bailout > 0)
            {
                _myZ80.Parse();
                bailout--;
            }
            _hasDump = true;
            if (!_myZ80.HALT) Console.WriteLine("BAILOUT!");
        }

        public bool Step()
        {
            _myZ80.Parse();
            _hasDump = true;
            return _myZ80.HALT;
        }

        public void Reset()
        {
            _hasDump = false;
            _myZ80.Reset();
        }

        public void DumpCpu()
        {
            Console.WriteLine(_myZ80.DumpState());
        }

        public void DumpRam()
        {
            for (var i = 0; i < 0x80; i++)
            {
                if (i % 16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", _ram[i]);
                if (i % 8 == 7) Console.Write("  ");
                if (i % 16 == 15) Console.WriteLine();
            }
            Console.WriteLine();
            for (var i = 0x8080; i < 0x80A0; i++)
            {
                if (i % 16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", _ram[i]);
                if (i % 8 == 7) Console.Write("  ");
                if (i % 16 == 15) Console.WriteLine();
            }
        }

        public void RaiseInterrupt(bool maskable, byte data = 0x00)
        {
            if (maskable)
            {
                _bus.INT = true;
                _bus.NMI = false;
                _bus.Data = data;
            }
            else
            {
                _bus.INT = false;
                _bus.NMI = true;
                _bus.Data = data;
            }
        }
    }
}
```

**Step 2: Delete TestPorts.cs**

```bash
git rm z80.Tests/TestPorts.cs
```

**Step 3: Update MemoryTests.cs to test SimpleBus**

Replace `new Memory(ram, ...)` with `new SimpleBus(ram, ...)` and use `bus.MemoryRead(i)` / `bus.MemoryWrite(i, value)` instead of `sut[i]`.

```csharp
using NUnit.Framework;

namespace z80.Tests
{
    [TestFixture]
    class MemoryTests
    {
        [Test]
        public void ReadInRam()
        {
            var ram = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var sut = new SimpleBus(ram, 0);

            for (ushort i = 0; i < ram.Length; i++)
                Assert.AreEqual(i, sut.MemoryRead(i));
        }

        [Test]
        public void ReadInRom()
        {
            var ram = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var sut = new SimpleBus(ram, 10);

            for (ushort i = 0; i < ram.Length; i++)
                Assert.AreEqual(i, sut.MemoryRead(i));
        }

        [Test]
        public void WriteInRam()
        {
            var ram = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var sut = new SimpleBus(ram, 0);

            for (ushort i = 0; i < ram.Length; i++)
            {
                sut.MemoryWrite(i, (byte)(0xFF ^ i));
                Assert.AreEqual((byte)(0xFF ^ i), sut.MemoryRead(i));
            }
        }

        [Test]
        public void WriteInRom()
        {
            var ram = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var sut = new SimpleBus(ram, 10);

            for (ushort i = 0; i < ram.Length; i++)
            {
                sut.MemoryWrite(i, (byte)(0xFF ^ i));
                Assert.AreEqual(i, sut.MemoryRead(i));
            }
        }
    }
}
```

**Step 4: Update InterruptsTests.cs**

No changes needed to test logic — `RaiseInterrupt` on TestSystem handles the mapping. Just verify it compiles.

**Step 5: Run all tests**

Run: `dotnet test --verbosity quiet`
Expected: All 2306+ tests pass (plus 8 new SimpleBus tests).

**Step 6: Commit**

```bash
git add z80.Tests/TestSystem.cs z80.Tests/MemoryTests.cs z80.Tests/OpCodeTestBase.cs
git rm z80.Tests/TestPorts.cs
git commit -m "refactor: update test infrastructure to use SimpleBus and IBus"
```

---

### Task 5: Delete Memory.cs and IPorts.cs

**Files:**
- Delete: `z80/Memory.cs`
- Delete: `z80/IPorts.cs`

**Step 1: Delete the files**

```bash
git rm z80/Memory.cs z80/IPorts.cs
```

**Step 2: Verify build and tests**

Run: `dotnet test --verbosity quiet`
Expected: All tests pass. No references to `Memory` or `IPorts` remain.

**Step 3: Commit**

```bash
git commit -m "refactor: remove Memory and IPorts, replaced by IBus and SimpleBus"
```

---

### Task 6: Update Sample App

**Files:**
- Modify: `z80sample/Program.cs`

**Step 1: Rewrite Program.cs**

```csharp
using System;
using System.IO;
using z80;

namespace z80Sample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ram = new byte[65536];
            Array.Clear(ram, 0, ram.Length);
            var inp = File.ReadAllBytes("48.rom");
            if (inp.Length != 16384) throw new InvalidOperationException("Invalid 48.rom file");

            Array.Copy(inp, ram, 16384);

            var bus = new SampleBus(ram, 16384);
            var cpu = new Z80(bus);

            while (!cpu.HALT)
            {
                cpu.Parse();
            }

            Console.WriteLine(Environment.NewLine + cpu.DumpState());
            for (var i = 0; i < 0x80; i++)
            {
                if (i % 16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", ram[i]);
                if (i % 8 == 7) Console.Write("  ");
                if (i % 16 == 15) Console.WriteLine();
            }
            Console.WriteLine();
            for (var i = 0x4000; i < 0x4100; i++)
            {
                if (i % 16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", ram[i]);
                if (i % 8 == 7) Console.Write("  ");
                if (i % 16 == 15) Console.WriteLine();
            }
        }
    }

    class SampleBus : SimpleBus
    {
        public SampleBus(byte[] memory, ushort ramStart) : base(memory, ramStart) { }

        public new byte IoRead(ushort address)
        {
            Console.WriteLine($"IN 0x{address:X4}");
            return 0;
        }

        public new void IoWrite(ushort address, byte data)
        {
            Console.WriteLine($"OUT 0x{address:X4}, 0x{data:X2}");
        }
    }
}
```

Note: The `new` keyword hides the base SimpleBus methods, but since IBus calls go through the interface, the SampleBus overrides won't be called via the Z80. To make this work properly, the IoRead/IoWrite methods in SimpleBus should be `virtual`, or SampleBus should implement IBus directly. The simplest fix: make `IoRead` and `IoWrite` virtual in SimpleBus. Alternatively, SampleBus can just implement IBus directly inheriting from SimpleBus for the memory part. The cleanest approach is to make the four bus operation methods in SimpleBus `virtual`.

Update SimpleBus to mark bus operations as virtual:
```csharp
public virtual byte MemoryRead(ushort address) => _memory[address];
public virtual void MemoryWrite(ushort address, byte data) { ... }
public virtual byte IoRead(ushort address) => _inputs[address];
public virtual void IoWrite(ushort address, byte data) => _outputs[address] = data;
```

Then SampleBus uses `override` instead of `new`.

**Step 2: Verify it compiles**

Run: `dotnet build --verbosity quiet`
Expected: Build succeeded.

**Step 3: Run all tests (to make sure SimpleBus virtual didn't break anything)**

Run: `dotnet test --verbosity quiet`
Expected: All tests pass.

**Step 4: Commit**

```bash
git add z80sample/Program.cs z80/SimpleBus.cs
git commit -m "refactor: update sample app to use SimpleBus and IBus"
```

---

### Task 7: Final Verification

**Step 1: Run all tests**

Run: `dotnet test --verbosity quiet`
Expected: All tests pass.

**Step 2: Verify the public API surface**

Manually check that the Z80 class exposes:
- `Z80(IBus bus)` constructor
- `int Parse()` returns T-states
- `void Reset()`
- `bool HALT`, `bool M1`, `bool RFSH`, `bool BUSACK`
- `ushort AddressBus`, `byte DataBus`
- All register properties (A, F, B, C, D, E, H, L, AF, BC, DE, HL, IX, IY, SP, PC, I, R)
- Shadow register properties (Ap, Fp, Bp, Cp, Dp, Ep, Hp, Lp, AFp, BCp, DEp, HLp)
- `bool IFF1`, `bool IFF2`, `int InterruptMode`
- `byte[] GetState()`, `string DumpState()`

**Step 3: Verify no references to old types remain**

Run: `grep -r "IPorts\|new Memory(" z80/ z80.Tests/ z80sample/ --include="*.cs"`
Expected: No matches.

**Step 4: Commit any final cleanup**

```bash
git add -A
git commit -m "chore: final cleanup after pinout API migration"
```
