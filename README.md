# z80
A csharp z80 emulator

Still work-in-progress, but so far I'm working in parallel on:

* Z80 Emulator (which could also double as a disassembler with a little tweaking)
* Z80 Assembler backend
* Zilog-based Z80 tests

The following opcodes are supported

* 8-bit load group (e.g. `LD A, 0x42`)
* 16-bit load group (e.g. `POP HL`)
* Exchange, Block Transfer, and Search group (e.g. `EX AF, AF'`)

