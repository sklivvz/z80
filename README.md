# z80
A csharp z80 emulator

## The project

My goal is to write a Z80 emulator that works in real time.

At the moment, I'm working in parallel on:

* Z80 Emulator & step debugger
* Z80 Assembler backend
* Zilog-based Z80 tests

The tests are my documentation, the assembler backend is needed to write tests and stay sane and the emulator is the whole point. The step debugger comes for free with it.

## Status

Progress: **6/12 (50%)**  
Coverage: **96.8%**  
Spectrum ROM: **Does not work**, runs up to address `0x0005` (`JP nn`)

The following opcodes are supported

* 8-bit load group (e.g. `LD A, 0x42`)
* 16-bit load group (e.g. `POP HL`)
* Exchange, Block Transfer, and Search group (e.g. `EX AF, AF'`)
* 8-Bit Arithmetic Group (e.g. `ADD 0x23`)
* General-Purpose Arithmetic and CPU Control Groups (e.g. `NOP`, `HALT`, ...)
* 16-Bit Arithmetic Group (e.g. `ADD HL, 0x2D5F`, ...)

The following opcodes are not done

* Rotate and Shift Group (`LCA`, `LA`, `RCA`, `RA`, `LC r`, `LC (HL)`, `LC (IX+d)`, `LC (IY+d)`, `L m`, `RC m`, `R m`, `LA m`, `RA m`, `RL m`, `LD`, `RRD`)
* Bit Set, Reset, and Test Group
* Jump Group
* Call and Return Group
* Input and Output Group
* Undocumented opcodes

## The future

After these are done it should be "easy" to add:

* An assembler frontend thus having a full z80 assembler
* A disassembler based on the current CPU emulator code