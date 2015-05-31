# z80
A csharp z80 emulator.

## The project

The project's goal is to write a Z80 emulator that works in real time.

At the moment, I'm working in parallel on:

* Z80 Emulator & step debugger
* Z80 Assembler backend
* Zilog-based Z80 tests

The tests are a translation of the documentation, the assembler backend is needed to write tests and stay sane and the emulator is the whole point. The step debugger comes for free with it. Currently developed but still work in progress, see the "Status" section to see what's currently implemented.

## Usage example

```csharp
var ram = new byte[65536];

// Load a ROM image
Array.Clear(ram, 0, ram.Length);
Array.Copy(File.ReadAllBytes("48.rom"), ram, 16384);

// Set up memory layout
var myZ80 = new Z80(new Memory(ram, 16384));

// Run
while (!myZ80.Halted)
{
    myZ80.Parse();
}

// Show the registers
Console.WriteLine(myZ80.DumpState());
```

## Status

### Opcodes

Progress: **6.3/12 (52%)**  
Coverage: **96.9%**  
Spectrum ROM: **Does not work**, runs up to address `0x0005` (`JP nn`)

The following opcodes are supported

* 8-bit load group (e.g. `LD A, 0x42`)
* 16-bit load group (e.g. `POP HL`)
* Exchange, Block Transfer, and Search group (e.g. `EX AF, AF'`)
* 8-Bit Arithmetic Group (e.g. `ADD 0x23`)
* General-Purpose Arithmetic and CPU Control Groups (e.g. `NOP`, `HALT`, ...)
* 16-Bit Arithmetic Group (e.g. `ADD HL, 0x2D5F`, ...)
* Rotate and Shift Group (`RLCA`, `RLA`, `RRCA`, `RRA`)

The following opcodes are not done

* Rotate and Shift Group (`RLC r`, `RLC (HL)`, `RLC (IX+d)`, `RLC (IY+d)`, `RL m`, `RRC m`, `RR m`, `RLA m`, `RRA m`, `RRL m`, `RLD`, `RRRD`)
* Bit Set, Reset, and Test Group
* Jump Group
* Call and Return Group
* Input and Output Group
* Undocumented opcodes & flags

### Other features

Other features that need implementation

* R register auto-incrementing
* Interrupts
* Data bus
* External Clock

## The future

After these are done it should be "easy" to add:

* An assembler frontend thus having a full z80 assembler
* A disassembler based on the current CPU emulator code

Also, the project should have NuGet packages at some point.

## Bibliography

The following resources have bee useful documentation:

* [Z80 CPU User Manual](http://www.zilog.com/manage_directlink.php?filepath=docs/z80/um0080) by Zilog
* [ZEMU - Z80 Emulator](http://www.z80.info/zip/zemu.zip) by Joe Moore
* [The Undocumented Z80 Documented](http://www.myquest.nl/z80undocumented/z80-documented-v0.91.pdf) by Sean Young
* [comp.sys.sinclair FAQ](http://www.worldofspectrum.org/faq/reference/z80reference.htm)
* [jsspeccy](https://github.com/gasman/jsspeccy) by Matt Westcott

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