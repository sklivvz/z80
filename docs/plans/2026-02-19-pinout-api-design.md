# Z80 Pinout-Based API Redesign

## Goal

Redesign the Z80 emulator's public API to closely mirror the real Z80 chip's 40-pin DIP pinout, making the emulator feel authentic and predictable to anyone familiar with the real hardware.

## Approach

Unified IBus interface with semantic methods. A single `IBus` replaces both `Memory` and `IPorts`. The Z80 constructor takes just `IBus`. A `SimpleBus` helper class ships with the library to keep simple use cases trivial.

## IBus Interface

```csharp
public interface IBus
{
    // Data transfer (active bus operations)
    byte MemoryRead(ushort address);              // /MREQ + /RD
    void MemoryWrite(ushort address, byte data);   // /MREQ + /WR
    byte IoRead(ushort address);                   // /IORQ + /RD
    void IoWrite(ushort address, byte data);        // /IORQ + /WR

    // Interrupt signals (Z80 input pins)
    bool INT { get; }       // /INT — level-sensitive
    bool NMI { get; }       // /NMI — edge-triggered; impl clears after read
    byte Data { get; }      // Data bus during interrupt acknowledge (IM0/IM2)

    // Stubbed for future (not read by CPU yet)
    bool WAIT { get; }      // /WAIT
    bool BUSRQ { get; }     // /BUSRQ
    bool RESET { get; }     // /RESET
}
```

Positive logic throughout: `true` = asserted, even though real pins are active-low.

## Z80 Public API

```csharp
public class Z80
{
    // Constructor
    public Z80(IBus bus);

    // Execution
    public int Parse();       // Execute one instruction, return T-states consumed
    public void Reset();      // Reset CPU to power-on state

    // Output pins
    public bool HALT { get; }     // /HALT
    public bool M1 { get; }       // /M1 — true during opcode fetch
    public bool RFSH { get; }     // /RFSH — true during refresh
    public bool BUSACK { get; }   // /BUSACK — stubbed, always false

    // Bus state (last values driven)
    public ushort AddressBus { get; }  // A0-A15
    public byte DataBus { get; }       // D0-D7

    // Registers (read-only properties)
    public byte A, F, B, C, D, E, H, L { get; }
    public ushort AF, BC, DE, HL, IX, IY, SP, PC { get; }
    public byte I, R { get; }
    // Shadow registers
    public byte Ap, Fp, Bp, Cp, Dp, Ep, Hp, Lp { get; }
    public ushort AFp, BCp, DEp, HLp { get; }
    // Interrupt state
    public bool IFF1, IFF2 { get; }
    public int InterruptMode { get; }

    // Convenience (kept from original)
    public byte[] GetState();
    public string DumpState();
}
```

`Parse()` returns T-state count — the key addition for debugging, testing, and caller-driven pacing.

## SimpleBus Helper

```csharp
public class SimpleBus : IBus
{
    public SimpleBus(byte[] memory, ushort ramStart);

    // IBus implementation
    // Memory: ROM below ramStart (writes ignored), RAM above
    // I/O: 64K port space backed by arrays

    // Interrupt signals — settable by caller
    public bool INT { get; set; }
    public bool NMI { get; set; }    // auto-clears on read
    public byte Data { get; set; }

    // Stubbed
    public bool WAIT => false;
    public bool BUSRQ => false;
    public bool RESET => false;

    // Helpers
    public void SetInput(ushort address, byte value);
    public byte GetOutput(ushort address);
}
```

## Pin-to-API Mapping

| Real Z80 Pin | Direction | API Mapping |
|---|---|---|
| A0-A15 | Output | `Z80.AddressBus` |
| D0-D7 | Bidir | `Z80.DataBus` / `IBus` method args |
| /MREQ + /RD | Output | `IBus.MemoryRead()` |
| /MREQ + /WR | Output | `IBus.MemoryWrite()` |
| /IORQ + /RD | Output | `IBus.IoRead()` |
| /IORQ + /WR | Output | `IBus.IoWrite()` |
| /M1 | Output | `Z80.M1` |
| /RFSH | Output | `Z80.RFSH` |
| /HALT | Output | `Z80.HALT` |
| /BUSACK | Output | `Z80.BUSACK` (stubbed) |
| /INT | Input | `IBus.INT` |
| /NMI | Input | `IBus.NMI` |
| /WAIT | Input | `IBus.WAIT` (stubbed) |
| /BUSRQ | Input | `IBus.BUSRQ` (stubbed) |
| /RESET | Input | `IBus.RESET` (stubbed) |
| CLK | Input | `Z80.Parse()` return value / future `Tick()` |

## What Gets Removed

- `Memory` class — replaced by `SimpleBus`
- `IPorts` interface — absorbed into `IBus`
- `TestPorts` class — replaced by `SimpleBus` in tests
- `Thread.Sleep` pacing in `Wait()` — removed entirely
- `DateTime _clock` field — removed

## What Stays the Same

- All instruction implementations (switch/case, ParseCB/DD/ED/FD)
- `GetState()` byte layout (28 bytes, same format)
- `DumpState()` output format
- `Z80Asm` helper
- Internal `byte[26]` register storage
- Interrupt handling logic flow

## Sample App (after redesign)

```csharp
var rom = File.ReadAllBytes("48.rom");
var memory = new byte[0x10000];
Array.Copy(rom, memory, rom.Length);

var bus = new SimpleBus(memory, (ushort)rom.Length);
var cpu = new Z80(bus);

while (!cpu.HALT)
{
    cpu.Parse();
}
```
