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

Progress: **3.8/11 (35%)**  
Coverage: **98.7%**  
Spectrum ROM: **Does not work**, runs up to address `0x0005` (`JP nn`)
al
The following opcodes are supported

* 8-bit load group (e.g. `LD A, 0x42`)
* 16-bit load group (e.g. `POP HL`)
* Exchange, Block Transfer, and Search group (e.g. `EX AF, AF'`)
* 8-Bit Arithmetic Group (`ADD`, `ADC`, `SUB`, `SBC`, `AND`, `OR`, `XOR`, `CP`)

The following opcodes are not done

* 8-Bit Arithmetic Group (`INC`, `DEC`)
* General-Purpose Arithmetic and CPU Control Groups
* 16-Bit Arithmetic Group
* Rotate and Shift Group
* Bit Set, Reset, and Test Group
* Jump Group
* Call and Return Group
* Input and Output Group

## The future

After these are done it should be "easy" to add:

* An assembler frontend thus having a full z80 assembler
* A disassembler based on the current CPU emulator code