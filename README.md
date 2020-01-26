# z80
A csharp emulator for the Zilog Z80 CPU.

## The project

z80 a Z80 emulator that works in real time written in C#.

These are contained:

* Z80 Emulator (`Z80`)
* Z80 Assembler backend (`Z80Asm`)
* Zilog-based Z80 tests (`z80.Tests`)

The tests are a translation of the documentation, the assembler backend is needed to write tests and stay sane and the emulator is the whole point. 

There's a very basic step debugger in the tests (`TestSystem`) which basically came for free with them.

## Usage example

```csharp
var ram = new byte[65536];

// Load a ROM image
Array.Clear(ram, 0, ram.Length);
Array.Copy(File.ReadAllBytes("48.rom"), ram, 16384);

// Ports is something you supply to emulate I/O ports
var ports = new SamplePorts();

// Set up memory layout
var myZ80 = new Z80(new Memory(ram, 16384), ports);

// Run
while (!myZ80.Halt)
{
    myZ80.Parse();
}

// Show the registers
Console.WriteLine(myZ80.DumpState());
```

## Status

Test Coverage: **98.29%**  
Spectrum ROM: **_Apparently_ it works**, but needs a ULA to work.

### Opcodes


The following opcodes are supported

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

These other features are supported

* Address and Data bus
* R register counts machine cycles (approximately)
* Interrupts
* Other pins


## The future

The following opcodes are not done

* Undocumented opcodes (`DD`, `FD`)
* Undocumented effects (`BIT`, Memory Block Instructions, I/O Block Instructions, 16 Bit I/O ports, Block Instructions, Bit Additions, DAA Instruction)

These new features are highly desirable

* An assembler frontend thus having a full z80 assembler
* A disassembler based on the current CPU emulator code

Also, the project should have NuGet packages at some point.

## Bibliography

The following resources have been useful documentation:

* [Z80 CPU User Manual](http://www.zilog.com/manage_directlink.php?filepath=docs/z80/um0080) by Zilog
* [ZEMU - Z80 Emulator](http://www.z80.info/zip/zemu.zip) by Joe Moore
* [The Undocumented Z80 Documented](http://www.myquest.nl/z80undocumented/z80-documented-v0.91.pdf) by Sean Young
* [comp.sys.sinclair FAQ](http://www.worldofspectrum.org/faq/reference/z80reference.htm)
* [jsspeccy](https://github.com/gasman/jsspeccy) by Matt Westcott
* [The Complete Spectrum ROM Disassembly](http://dac.escet.urjc.es/~csanchez/pfcs/zxspectrum/CompleteSpectrumROMDisassemblyThe.pdf) by Dr Ian Logan & Dr Frank O&apos;Hara

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
