# z80

A C# emulator for the Zilog Z80 CPU and ZX Spectrum ULA.

## The project

This project contains a Z80 CPU emulator and a ZX Spectrum ULA (Uncommitted Logic Array), both written in C#. Together they can run a ZX Spectrum 48K.

```
                  +-----------+
                  |   Z80     |
                  |   CPU     |
                  +-----+-----+
                        |
                   IMemory  IBus
                        |
          +-------------+-------------+
          |                           |
    +-----+-----+             +------+------+
    |  Memory   |             | SpectrumBus |
    |  (64 KB)  |             |  (IBus)     |
    +-----+-----+             +------+------+
          |                           |
          |                    +------+------+
          +--------------------+    ULA      |
                  IMemory      |  (IUlaBus)  |
                               +------+------+
                                      |
                               +------+------+
                               |  Keyboard   |
                               |  (IUlaBus)  |
                               +-------------+
```

The projects are:

* **z80** -- Z80 CPU emulator (`Z80`)
* **z80asm** -- Z80 assembler backend (`Z80Asm`)
* **ula** -- ZX Spectrum ULA video/IO chip (`Ula`, `ScreenRenderer`)
* **z80.Tests** -- Zilog-documentation-based Z80 tests (NUnit)
* **ula.Tests** -- ULA and screen renderer tests (NUnit)
* **z80sample** -- ZX Spectrum 48K emulator wiring Z80 + ULA + MonoGame together

The Z80 tests are a translation of the official Zilog documentation. The assembler backend exists to write tests readably. The ULA generates video output from the Z80's memory and handles keyboard/speaker I/O through port 0xFE.

## Usage example

```csharp
// 64 KB RAM with 16 KB ROM write-protection
var ram = new byte[65536];
Array.Copy(File.ReadAllBytes("48.rom"), ram, 16384);

// Shared memory -- both CPU and ULA read/write the same RAM
var memory = new SimpleMemory(ram, romSize: 16384);

// ULA setup: frame buffer + keyboard bus
var frameBuffer = new uint[Ula.ScreenWidth * Ula.ScreenHeight];
var keyboardBus = new SpectrumKeyboard();
var spectrumUla = new Ula(memory, keyboardBus, frameBuffer);

// CPU bus routes I/O through the ULA
var cpuBus = new SpectrumBus(spectrumUla);

// Z80 CPU
var cpu = new Z80(memory, cpuBus);

// Run one frame (69888 T-states at 3.5 MHz)
cpu.Tick(69888);

// Render the frame from current video RAM state
spectrumUla.RenderFrame();
```

## Z80 CPU

### Opcodes

* 8-bit load group (e.g. `LD A, 0x42`)
* 16-bit load group (e.g. `POP HL`)
* Exchange, Block Transfer, and Search group (e.g. `EX AF, AF'`)
* 8-Bit Arithmetic Group (e.g. `ADD 0x23`)
* General-Purpose Arithmetic and CPU Control Groups (e.g. `NOP`, `HALT`, ...)
* 16-Bit Arithmetic Group (e.g. `ADD HL, 0x2D5F`, ...)
* Rotate and Shift Group (e.g. `RLCA`, `RLA`, ...)
* Bit Set, Reset, and Test Group (`BIT`, `SET`, `RES`)
* Jump Group (`JP nn`, `JR e`, `DJNZ e`, ...)
* Call and Return Group (`CALL`, `RET`, `RST`)
* Undocumented opcodes (`CB`, `DDCB`, `FDCB`, `ED`)
* Input and Output Group (`IN`, `OUT`, ...)

### Other features

* Address and Data bus
* R register counts machine cycles (approximately)
* Interrupt modes 0, 1, 2
* EI delayed enable (one-instruction delay per Zilog spec)
* LD A,I / LD A,R set P/V from IFF2
* T-state counting via `Tick(int tStates)`
* All bus pins (INT, NMI, WAIT, BUSRQ, RESET)

## ULA

The ULA (Uncommitted Logic Array) generates the ZX Spectrum's video signal and handles I/O.

### Video

* 352 x 288 pixel frame buffer (48px border + 256x192 display + 48px border)
* Accurate ZX Spectrum screen memory address decoding (bit-shuffled layout)
* 15-colour palette (8 normal + 7 bright, black has no bright variant)
* Attribute-based colour: 8x8 character cells with ink, paper, bright, flash
* FLASH toggle every 16 frames
* Configurable screen base address (0x4000 for 48K, 0xC000 for 128K shadow screen)

### I/O (Port 0xFE)

* Write: border colour (bits 0-2), MIC output (bit 3), speaker output (bit 4)
* Read: keyboard state (bits 0-4, active low), EAR input (bit 6)
* Keyboard half-row selection via high address byte
* Interrupt request generation (set after each frame render)

## z80sample

A complete ZX Spectrum 48K emulator using MonoGame for display, keyboard, and sound.

* 50fps video output (352x288 scaled 2x) with accurate border colours
* Full 40-key keyboard matrix mapped from PC keyboard (including arrow keys, backspace, escape)
* 1-bit speaker audio at 48 kHz, sampled every ~73 T-states for accurate beeper sound
* ROM write-protection (writes to 0x0000-0x3FFF silently ignored)

## Build and test

```bash
dotnet build
dotnet test
```

Run the sample emulator (requires `48.rom` in the z80sample directory):

```bash
dotnet run --project z80sample/
```

## Bibliography

* [Z80 CPU User Manual](http://www.zilog.com/manage_directlink.php?filepath=docs/z80/um0080) by Zilog
* [ZEMU - Z80 Emulator](http://www.z80.info/zip/zemu.zip) by Joe Moore
* [The Undocumented Z80 Documented](http://www.myquest.nl/z80undocumented/z80-documented-v0.91.pdf) by Sean Young
* [comp.sys.sinclair FAQ](http://www.worldofspectrum.org/faq/reference/z80reference.htm)
* [jsspeccy](https://github.com/gasman/jsspeccy) by Matt Westcott
* [The Complete Spectrum ROM Disassembly](http://dac.escet.urjc.es/~csanchez/pfcs/zxspectrum/CompleteSpectrumROMDisassemblyThe.pdf) by Dr Ian Logan & Dr Frank O'Hara
* [World of Spectrum 48K Reference](https://worldofspectrum.org/faq/reference/48kreference.htm)
* [ZX Spectrum ULA - Sinclair Wiki](https://sinclair.wiki.zxnet.co.uk/wiki/ZX_Spectrum_ULA)
* [ZX Spectrum Screen Memory Layout](https://espamatica.com/zx-spectrum-screen/)

## License

Copyright &copy; 2015, Marco Cecconi
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

### ZX Spectrum 48k ROM image

The ZX Spectrum 48k ROM is &copy; copyright Amstrad Ltd. Amstrad have kindly given their permission for the redistribution of their copyrighted material but retain that copyright.
