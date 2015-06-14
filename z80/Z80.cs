using System;
using System.Collections.Generic;
using System.Threading;

// ReSharper disable InconsistentNaming

namespace z80
{
    public class Z80
    {
        private static string log = "";
        private const byte B = 0;
        private const byte C = 1;
        private const byte D = 2;
        private const byte E = 3;
        private const byte H = 4;
        private const byte L = 5;
        private const byte F = 6;
        private const byte A = 7;
        private const byte Bp = 8;
        private const byte Cp = 9;
        private const byte Dp = 10;
        private const byte Ep = 11;
        private const byte Hp = 12;
        private const byte Lp = 13;
        private const byte Fp = 14;
        private const byte Ap = 15;
        private const byte I = 16;
        private const byte R = 17;
        private const byte IX = 18;
        private const byte IY = 20;
        private const byte SP = 22;
        private const byte PC = 24;
        private readonly Memory mem;
        private readonly byte[] registers = new byte[26];
        private DateTime _clock = DateTime.UtcNow;
        private bool IFF1;
        private bool IFF2;
        private int interruptMode;

        private Func<Z80, ushort, byte> inPorts;
        private Action<Z80, ushort, byte> outPorts;

        public Z80(Memory memory, Func<Z80, ushort, byte> inputs = null, Action<Z80, ushort, byte> outputs = null)
        {
            mem = memory;
            inPorts = inputs ?? ((_, __) => 0);
            outPorts = outputs ?? ((_, __, ___) => { });
            Reset();
        }

        private ushort Hl => (ushort)(registers[L] + (registers[H] << 8));
        private ushort Sp => (ushort)(registers[SP + 1] + (registers[SP] << 8));
        private ushort Ix => (ushort)(registers[IX + 1] + (registers[IX] << 8));
        private ushort Iy => (ushort)(registers[IY + 1] + (registers[IY] << 8));
        private ushort Bc => (ushort)((registers[B] << 8) + registers[C]);
        private ushort De => (ushort)((registers[D] << 8) + registers[E]);
        private ushort Pc => (ushort)(registers[PC + 1] + (registers[PC] << 8));
        public bool Halt { get; private set; }

        public void Parse()
        {
            if (Halt) return;
            var mc = Fetch();
            var hi = (byte)(mc >> 6);
            var lo = (byte)(mc & 0x07);
            var r = (byte)((mc >> 3) & 0x07);
            if (hi == 1)
            {
                var useHL1 = r == 6;
                var useHL2 = lo == 6;
                if (useHL2 && useHL1)
                {
#if(DEBUG)
                    Log("HALT");
#endif
                    Halt = true;
                    return;
                }
                var reg = useHL2 ? mem[Hl] : registers[lo];

                if (useHL1)
                    mem[Hl] = reg;
                else
                    registers[r] = reg;
                Wait(useHL1 || useHL2 ? 7 : 4);
#if (DEBUG)
                Log($"LD {(useHL1 ? "(HL)" : RName(r))}, {(useHL2 ? "(HL)" : RName(lo))}");
#endif
                return;
            }
            switch (mc)
            {
                case 0xCB:
                    ParseCB();
                    return;
                case 0xDD:
                    ParseDD();
                    return;
                case 0xED:
                    ParseED();
                    return;
                case 0xFD:
                    ParseFD();
                    return;
                case 0x00:
                    // NOP
#if(DEBUG)
                    Log("NOP");
#endif
                    Wait(4);
                    return;
                case 0x01:
                case 0x11:
                case 0x21:
                    {
                        // LD dd, nn
                        registers[r + 1] = Fetch();
                        registers[r] = Fetch();
#if (DEBUG)
                        Log($"LD {RName(r)}{RName((byte)(r + 1))}, 0x{registers[r]:X2}{registers[r + 1]:X2}");
#endif
                        Wait(10);
                        return;
                    }
                case 0x31:
                    {
                        // LD SP, nn
                        registers[SP + 1] = Fetch();
                        registers[SP] = Fetch();
#if (DEBUG)
                        Log($"LD SP, 0x{registers[SP]:X2}{registers[SP + 1]:X2}");
#endif
                        Wait(10);
                        return;
                    }
                case 0x06:
                case 0x0e:
                case 0x16:
                case 0x1e:
                case 0x26:
                case 0x2e:
                case 0x3e:
                    {
                        // LD r,n
                        var n = Fetch();
                        registers[r] = n;
#if (DEBUG)
                        Log($"LD {RName(r)}, 0x{n:X2}");
#endif
                        Wait(7);
                        return;
                    }
                case 0x36:
                    {
                        // LD (HL), n
                        var n = Fetch();
                        mem[Hl] = n;
#if (DEBUG)
                        Log($"LD (HL), {n}");
#endif
                        Wait(10);
                        return;
                    }
                case 0x0A:
                    {
                        // LD A, (BC)
                        registers[A] = mem[Bc];
#if (DEBUG)
                        Log("LD A, (BC)");
#endif
                        Wait(7);
                        return;
                    }
                case 0x1A:
                    {
                        // LD A, (DE)
                        registers[A] = mem[De];
#if (DEBUG)
                        Log("LD A, (DE)");
#endif
                        Wait(7);
                        return;
                    }
                case 0x3A:
                    {
                        // LD A, (nn)
                        var addr = Fetch16();
                        registers[A] = mem[addr];
#if (DEBUG)
                        Log($"LD A, (0x{addr:X4})");
#endif
                        Wait(13);
                        return;
                    }
                case 0x02:
                    {
                        // LD (BC), A
                        mem[Bc] = registers[A];
#if (DEBUG)
                        Log("LD (BC), A");
#endif
                        Wait(7);
                        return;
                    }
                case 0x12:
                    {
                        // LD (DE), A
                        mem[De] = registers[A];
#if (DEBUG)
                        Log("LD (DE), A");
#endif
                        Wait(7);
                        return;
                    }
                case 0x32:
                    {
                        // LD (nn), A 
                        var addr = Fetch16();
                        mem[addr] = registers[A];
#if (DEBUG)
                        Log($"LD (0x{addr:X4}), A");
#endif
                        Wait(13);
                        return;
                    }
                case 0x2A:
                    {
                        // LD HL, (nn) 
                        var addr = Fetch16();
                        registers[L] = mem[addr++];
                        registers[H] = mem[addr];
#if (DEBUG)
                        Log($"LD HL, (0x{--addr:X4})");
#endif
                        Wait(16);
                        return;
                    }
                case 0x22:
                    {
                        // LD (nn), HL
                        var addr = Fetch16();
                        mem[addr++] = registers[L];
                        mem[addr] = registers[H];
#if (DEBUG)
                        Log($"LD (0x{--addr:X4}), HL");
#endif
                        Wait(16);
                        return;
                    }
                case 0xF9:
                    {
                        // LD SP, HL
                        registers[SP + 1] = registers[L];
                        registers[SP] = registers[H];
#if (DEBUG)
                        Log("LD SP, HL");
#endif
                        Wait(6);
                        return;
                    }

                case 0xC5:
                    {
                        // PUSH BC
                        var addr = Sp;
                        mem[--addr] = registers[B];
                        mem[--addr] = registers[C];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH BC");
#endif
                        Wait(11);
                        return;
                    }
                case 0xD5:
                    {
                        // PUSH DE
                        var addr = Sp;
                        mem[--addr] = registers[D];
                        mem[--addr] = registers[E];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH DE");
#endif
                        Wait(11);
                        return;
                    }
                case 0xE5:
                    {
                        // PUSH HL
                        var addr = Sp;
                        mem[--addr] = registers[H];
                        mem[--addr] = registers[L];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH HL");
#endif
                        Wait(11);
                        return;
                    }
                case 0xF5:
                    {
                        // PUSH AF
                        var addr = Sp;
                        mem[--addr] = registers[A];
                        mem[--addr] = registers[F];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH AF");
#endif
                        Wait(11);
                        return;
                    }
                case 0xC1:
                    {
                        // POP BC
                        var addr = Sp;
                        registers[C] = mem[addr++];
                        registers[B] = mem[addr++];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP BC");
#endif
                        Wait(10);
                        return;
                    }
                case 0xD1:
                    {
                        // POP DE
                        var addr = Sp;
                        registers[E] = mem[addr++];
                        registers[D] = mem[addr++];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP DE");
#endif
                        Wait(10);
                        return;
                    }
                case 0xE1:
                    {
                        // POP HL
                        var addr = Sp;
                        registers[L] = mem[addr++];
                        registers[H] = mem[addr++];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP HL");
#endif
                        Wait(10);
                        return;
                    }
                case 0xF1:
                    {
                        // POP AF
                        var addr = Sp;
                        registers[F] = mem[addr++];
                        registers[A] = mem[addr++];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP AF");
#endif
                        Wait(10);
                        return;
                    }
                case 0xEB:
                    {
                        // EX DE, HL
                        SwapReg8(D, H);
                        SwapReg8(E, L);
#if (DEBUG)
                        Log("EX DE, HL");
#endif
                        Wait(4);
                        return;
                    }
                case 0x08:
                    {
                        // EX AF, AF'
                        SwapReg8(Ap, A);
                        SwapReg8(Fp, F);
#if (DEBUG)
                        Log("EX AF, AF'");
#endif
                        Wait(4);
                        return;
                    }
                case 0xD9:
                    {
                        // EXX
                        SwapReg8(B, Bp);
                        SwapReg8(C, Cp);
                        SwapReg8(D, Dp);
                        SwapReg8(E, Ep);
                        SwapReg8(H, Hp);
                        SwapReg8(L, Lp);
#if (DEBUG)
                        Log("EXX");
#endif
                        Wait(4);
                        return;
                    }
                case 0xE3:
                    {
                        // EX (SP), HL
                        var addr = Sp;

                        var tmp = registers[L];
                        registers[L] = mem[addr];
                        mem[addr++] = tmp;

                        tmp = registers[H];
                        registers[H] = mem[addr];
                        mem[addr] = tmp;

#if (DEBUG)
                        Log("EX (SP), HL");
#endif
                        Wait(19);
                        return;
                    }
                case 0x80:
                case 0x81:
                case 0x82:
                case 0x83:
                case 0x84:
                case 0x85:
                case 0x87:
                    {
                        // ADD A, r
                        Add(registers[lo]);
#if (DEBUG)
                        Log($"ADD A, {RName(lo)}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xC6:
                    {
                        // ADD A, n
                        var b = Fetch();
                        Add(b);
#if (DEBUG)
                        Log($"ADD A, 0x{b:X2}");
#endif
                        Wait(4);
                        Wait(4);
                        return;
                    }
                case 0x86:
                    {
                        // ADD A, (HL)
                        Add(mem[Hl]);
#if (DEBUG)
                        Log("ADD A, (HL)");
#endif
                        Wait(7);
                        return;
                    }
                case 0x88:
                case 0x89:
                case 0x8A:
                case 0x8B:
                case 0x8C:
                case 0x8D:
                case 0x8F:
                    {
                        // ADC A, r
                        Adc(registers[lo]);
#if (DEBUG)
                        Log($"ADC A, {RName(lo)}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xCE:
                    {
                        // ADC A, n
                        var b = Fetch();
                        Adc(b);
#if (DEBUG)
                        Log($"ADC A, 0x{b:X2}");
#endif
                        Wait(4);
                        return;
                    }
                case 0x8E:
                    {
                        // ADC A, (HL)
                        Adc(mem[Hl]);
#if (DEBUG)
                        Log("ADC A, (HL)");
#endif
                        Wait(7);
                        return;
                    }
                case 0x90:
                case 0x91:
                case 0x92:
                case 0x93:
                case 0x94:
                case 0x95:
                case 0x97:
                    {
                        // SUB A, r
                        Sub(registers[lo]);
#if (DEBUG)
                        Log($"SUB A, {RName(lo)}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xD6:
                    {
                        // SUB A, n
                        var b = Fetch();
                        Sub(b);
#if (DEBUG)
                        Log($"SUB A, 0x{b:X2}");
#endif
                        Wait(4);
                        return;
                    }
                case 0x96:
                    {
                        // SUB A, (HL)
                        Sub(mem[Hl]);
#if (DEBUG)
                        Log("SUB A, (HL)");
#endif
                        Wait(7);
                        return;
                    }
                case 0x98:
                case 0x99:
                case 0x9A:
                case 0x9B:
                case 0x9C:
                case 0x9D:
                case 0x9F:
                    {
                        // SBC A, r
                        Sbc(registers[lo]);
#if (DEBUG)
                        Log($"SBC A, {RName(lo)}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xDE:
                    {
                        // SBC A, n
                        var b = Fetch();
                        Sbc(b);
#if (DEBUG)
                        Log($"SBC A, 0x{b:X2}");
#endif
                        Wait(4);
                        return;
                    }
                case 0x9E:
                    {
                        // SBC A, (HL)
                        Sbc(mem[Hl]);
#if (DEBUG)
                        Log("SBC A, (HL)");
#endif
                        Wait(7);
                        return;
                    }

                case 0xA0:
                case 0xA1:
                case 0xA2:
                case 0xA3:
                case 0xA4:
                case 0xA5:
                case 0xA7:
                    {
                        // AND A, r
                        And(registers[lo]);
#if (DEBUG)
                        Log($"AND A, {RName(lo)}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xE6:
                    {
                        // AND A, n
                        var b = Fetch();

                        And(b);
#if (DEBUG)
                        Log($"AND A, 0x{b:X2}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xA6:
                    {
                        // AND A, (HL)
                        And(mem[Hl]);
#if (DEBUG)
                        Log("AND A, (HL)");
#endif
                        Wait(7);
                        return;
                    }
                case 0xB0:
                case 0xB1:
                case 0xB2:
                case 0xB3:
                case 0xB4:
                case 0xB5:
                case 0xB7:
                    {
                        // OR A, r
                        Or(registers[lo]);
#if (DEBUG)
                        Log($"OR A, {RName(lo)}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xF6:
                    {
                        // OR A, n
                        var b = Fetch();
                        Or(b);
#if (DEBUG)
                        Log($"OR A, 0x{b:X2}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xB6:
                    {
                        // OR A, (HL)
                        Or(mem[Hl]);
#if (DEBUG)
                        Log("OR A, (HL)");
#endif
                        Wait(7);
                        return;
                    }
                case 0xA8:
                case 0xA9:
                case 0xAA:
                case 0xAB:
                case 0xAC:
                case 0xAD:
                case 0xAF:
                    {
                        // XOR A, r
                        Xor(registers[lo]);
#if (DEBUG)
                        Log($"XOR A, {RName(lo)}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xEE:
                    {
                        // XOR A, n
                        var b = Fetch();
                        Xor(b);
#if (DEBUG)
                        Log($"XOR A, 0x{b:X2}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xAE:
                    {
                        // XOR A, (HL)
                        Xor(mem[Hl]);
#if (DEBUG)
                        Log("XOR A, (HL)");
#endif
                        Wait(7);
                        return;
                    }

                case 0xF3:
                    {
                        // DI
                        IFF1 = false;
                        IFF2 = false;
#if (DEBUG)
                        Log("DI");
#endif
                        Wait(4);
                        return;
                    }
                case 0xFB:
                    {
                        // EI
                        IFF1 = true;
                        IFF2 = true;
#if (DEBUG)
                        Log("EI");
#endif
                        Wait(4);
                        return;
                    }
                case 0xB8:
                case 0xB9:
                case 0xBA:
                case 0xBB:
                case 0xBC:
                case 0xBD:
                case 0xBF:
                    {
                        // CP A, r
                        Cmp(registers[lo]);
#if (DEBUG)
                        Log($"CP A, {RName(lo)}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xFE:
                    {
                        // CP A, n
                        var b = Fetch();
                        Cmp(b);
#if (DEBUG)
                        Log($"CP A, 0x{b:X2}");
#endif
                        Wait(4);
                        return;
                    }
                case 0xBE:
                    {
                        // CP A, (HL)
                        Cmp(mem[Hl]);
#if (DEBUG)
                        Log("CP A, (HL)");
#endif
                        Wait(7);
                        return;
                    }
                case 0x04:
                case 0x0C:
                case 0x14:
                case 0x1C:
                case 0x24:
                case 0x2C:
                case 0x3C:
                    {
                        // INC r
                        registers[r] = Inc(registers[r]);
#if (DEBUG)
                        Log($"INC {RName(r)}");
#endif
                        Wait(4);
                        return;
                    }
                case 0x34:
                    {
                        // INC (HL)
                        mem[Hl] = Inc(mem[Hl]);
#if (DEBUG)
                        Log("INC (HL)");
#endif
                        Wait(7);
                        return;
                    }

                case 0x05:
                case 0x0D:
                case 0x15:
                case 0x1D:
                case 0x25:
                case 0x2D:
                case 0x3D:
                    {
                        // DEC r
                        registers[r] = Dec(registers[r]);
#if (DEBUG)
                        Log($"DEC {RName(r)}");
#endif
                        Wait(7);
                        return;
                    }
                case 0x35:
                    {
                        // DEC (HL)
                        mem[Hl] = Dec(mem[Hl]);
#if (DEBUG)
                        Log("DEC (HL)");
#endif
                        Wait(7);
                        return;
                    }
                case 0x27:
                    {
                        // DAA
                        var a = registers[A];
                        var f = registers[F];
                        if ((a & 0x0F) > 0x09 || (f & (byte)Fl.H) > 0)
                        {
                            Add(0x06);
                            a = registers[A];
                        }
                        if ((a & 0xF0) > 0x90 || (f & (byte)Fl.C) > 0)
                        {
                            Add(0x60);
                        }
#if (DEBUG)
                        Log("DAA");
#endif
                        Wait(4);
                        return;
                    }
                case 0x2F:
                    {
                        // CPL
                        registers[A] ^= 0xFF;
                        registers[F] |= (byte)(Fl.H | Fl.N);
#if (DEBUG)
                        Log("CPL");
#endif
                        Wait(4);
                        return;
                    }
                case 0x3F:
                    {
                        // CCF
                        registers[F] &= (byte)~(Fl.N);
                        registers[F] ^= (byte)(Fl.C);
#if (DEBUG)
                        Log("CCF");
#endif
                        Wait(4);
                        return;
                    }
                case 0x37:
                    {
                        // SCF
                        registers[F] &= (byte)~(Fl.N);
                        registers[F] |= (byte)(Fl.C);
#if (DEBUG)
                        Log("SCF");
#endif
                        Wait(4);
                        return;
                    }
                case 0x09:
                    {
                        AddHl(Bc);

#if (DEBUG)
                        Log("ADD HL, BC");
#endif
                        Wait(4);
                        return;
                    }
                case 0x19:
                    {
                        AddHl(De);
#if (DEBUG)
                        Log("ADD HL, DE");
#endif
                        Wait(4);
                        return;
                    }
                case 0x29:
                    {
                        AddHl(Hl);
#if (DEBUG)
                        Log("ADD HL, HL");
#endif
                        Wait(4);
                        return;
                    }
                case 0x39:
                    {
                        AddHl(Sp);
#if (DEBUG)
                        Log("ADD HL, SP");
#endif
                        Wait(4);
                        return;
                    }
                case 0x03:
                    {
                        var val = Bc + 1;
                        registers[B] = (byte)(val >> 8);
                        registers[C] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC BC");
#endif
                        Wait(4);
                        return;
                    }
                case 0x13:
                    {
                        var val = De + 1;
                        registers[D] = (byte)(val >> 8);
                        registers[E] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC DE");
#endif
                        Wait(4);
                        return;
                    }
                case 0x23:
                    {
                        var val = Hl + 1;
                        registers[H] = (byte)(val >> 8);
                        registers[L] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC HL");
#endif
                        Wait(4);
                        return;
                    }
                case 0x33:
                    {
                        var val = Sp + 1;
                        registers[SP] = (byte)(val >> 8);
                        registers[SP + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC SP");
#endif
                        Wait(4);
                        return;
                    }
                case 0x0B:
                    {
                        var val = Bc - 1;
                        registers[B] = (byte)(val >> 8);
                        registers[C] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC BC");
#endif
                        Wait(4);
                        return;
                    }
                case 0x1B:
                    {
                        var val = De - 1;
                        registers[D] = (byte)(val >> 8);
                        registers[E] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC DE");
#endif
                        Wait(4);
                        return;
                    }
                case 0x2B:
                    {
                        var val = Hl - 1;
                        registers[H] = (byte)(val >> 8);
                        registers[L] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC HL");
#endif
                        Wait(4);
                        return;
                    }
                case 0x3B:
                    {
                        var val = Sp - 1;
                        registers[SP] = (byte)(val >> 8);
                        registers[SP + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC SP");
#endif
                        Wait(4);
                        return;
                    }
                case 0x07:
                    {
                        var a = registers[A];
                        var c = (byte)((a & 0x80) >> 7);
                        a <<= 1;
                        registers[A] = a;
                        registers[F] &= (byte)~(Fl.H | Fl.N | Fl.C);
                        registers[F] |= c;
#if (DEBUG)
                        Log("RLCA");
#endif
                        Wait(4);
                        return;
                    }
                case 0x17:
                    {
                        var a = registers[A];
                        var c = (byte)((a & 0x80) >> 7);
                        a <<= 1;
                        var f = registers[F];
                        a |= (byte)(f & (byte)Fl.C);
                        registers[A] = a;
                        f &= (byte)~(Fl.H | Fl.N | Fl.C);
                        f |= c;
                        registers[F] = f;
#if (DEBUG)
                        Log("RLA");
#endif
                        Wait(4);
                        return;
                    }
                case 0x0F:
                    {
                        var a = registers[A];
                        var c = (byte)(a & 0x01);
                        a >>= 1;
                        registers[A] = a;
                        registers[F] &= (byte)~(Fl.H | Fl.N | Fl.C);
                        registers[F] |= c;
#if (DEBUG)
                        Log("RRCA");
#endif
                        Wait(4);
                        return;
                    }
                case 0x1F:
                    {
                        var a = registers[A];
                        var c = (byte)(a & 0x01);
                        a >>= 1;
                        var f = registers[F];
                        a |= (byte)((f & (byte)Fl.C) << 7);
                        registers[A] = a;
                        f &= (byte)~(Fl.H | Fl.N | Fl.C);
                        f |= c;
                        registers[F] = f;
#if (DEBUG)
                        Log("RRA");
#endif
                        Wait(4);
                        return;
                    }
                case 0xC3:
                    {
                        var addr = Fetch16();
                        registers[PC] = (byte)(addr >> 8);
                        registers[PC + 1] = (byte)(addr);
#if (DEBUG)
                        Log($"JP 0x{addr:X4}");
#endif
                        Wait(10);
                        return;
                    }
                case 0xC2:
                case 0xCA:
                case 0xD2:
                case 0xDA:
                case 0xE2:
                case 0xEA:
                case 0xF2:
                case 0xFA:
                    {
                        var addr = Fetch16();
                        if (JumpCondition(r))
                        {
                            registers[PC] = (byte)(addr >> 8);
                            registers[PC + 1] = (byte)(addr);
                        }
#if (DEBUG)
                        Log($"JP {JCName(r)}, 0x{addr:X4}");
#endif
                        Wait(10);
                        return;

                    }
                case 0x18:
                    {
                        // order is important here
                        var d = (sbyte)Fetch();
                        var addr = Pc + d;
                        registers[PC] = (byte)(addr >> 8);
                        registers[PC + 1] = (byte)(addr);
#if (DEBUG)
                        Log($"JR 0x{addr:X4}");
#endif
                        Wait(12);
                        return;
                    }
                case 0x20:
                case 0x28:
                case 0x30:
                case 0x38:
                    {
                        // order is important here
                        var d = (sbyte)Fetch();
                        var addr = Pc + d;
                        if (JumpCondition((byte)(r & 3)))
                        {
                            registers[PC] = (byte)(addr >> 8);
                            registers[PC + 1] = (byte)(addr);
                            Wait(12);
                        }
                        else
                        {
                            Wait(7);
                        }
#if (DEBUG)
                        Log($"JR {JCName((byte)(r & 3))}, 0x{addr:X4}");
#endif
                        return;

                    }
                case 0xE9:
                    {
                        var addr = Hl;
                        registers[PC] = (byte)(addr >> 8);
                        registers[PC + 1] = (byte)(addr);
#if (DEBUG)
                        Log("JP HL");
#endif
                        Wait(4);
                        return;
                    }
                case 0x10:
                    {
                        // order is important here
                        var d = (sbyte)Fetch();
                        var addr = Pc + d;
                        var b = registers[B];
                        registers[B] = --b;
                        if (b != 0)
                        {
                            registers[PC] = (byte)(addr >> 8);
                            registers[PC + 1] = (byte)(addr);
                            Wait(13);
                        }
                        else
                        {
                            Wait(8);
                        }
#if (DEBUG)
                        Log($"DJNZ 0x{addr:X4}");
#endif
                        return;
                    }
                case 0xCD:
                    {
                        var addr = Fetch16();
                        var stack = Sp;
                        mem[--stack] = (byte)(Pc >> 8);
                        mem[--stack] = (byte)(Pc);
                        registers[SP] = (byte)(stack >> 8);
                        registers[SP + 1] = (byte)(stack);
                        registers[PC] = (byte)(addr >> 8);
                        registers[PC + 1] = (byte)(addr);
#if (DEBUG)
                        Log($"CALL 0x{addr:X4}");
#endif
                        Wait(17);
                        return;
                    }
                case 0xC4:
                case 0xCC:
                case 0xD4:
                case 0xDC:
                case 0xE4:
                case 0xEC:
                case 0xF4:
                case 0xFC:
                    {
                        var addr = Fetch16();
                        if (JumpCondition(r))
                        {
                            var stack = Sp;
                            mem[--stack] = (byte)(Pc >> 8);
                            mem[--stack] = (byte)(Pc);
                            registers[SP] = (byte)(stack >> 8);
                            registers[SP + 1] = (byte)(stack);
                            registers[PC] = (byte)(addr >> 8);
                            registers[PC + 1] = (byte)(addr);
                            Wait(17);
                        }
                        else
                        {
                            Wait(10);
                        }
#if (DEBUG)
                        Log($"CALL {JCName(r)}, 0x{addr:X4}");
#endif
                        return;

                    }
                case 0xC9:
                    {
                        var stack = Sp;
                        registers[PC + 1] = mem[stack++];
                        registers[PC] = mem[stack++];
                        registers[SP] = (byte)(stack >> 8);
                        registers[SP + 1] = (byte)(stack);
#if (DEBUG)
                        Log("RET");
#endif
                        Wait(10);
                        return;
                    }
                case 0xC0:
                case 0xC8:
                case 0xD0:
                case 0xD8:
                case 0xE0:
                case 0xE8:
                case 0xF0:
                case 0xF8:
                    {
                        if (JumpCondition(r))
                        {
                            var stack = Sp;
                            registers[PC + 1] = mem[stack++];
                            registers[PC] = mem[stack++];
                            registers[SP] = (byte)(stack >> 8);
                            registers[SP + 1] = (byte)(stack);
                            Wait(11);
                        }
                        else
                        {
                            Wait(5);
                        }
#if (DEBUG)
                        Log($"RET {JCName(r)}");
#endif
                        return;

                    }
                case 0xC7:
                case 0xCF:
                case 0xD7:
                case 0xDF:
                case 0xE7:
                case 0xEF:
                case 0xF7:
                case 0xFF:
                    {
                        var stack = Sp;
                        mem[--stack] = (byte)(Pc >> 8);
                        mem[--stack] = (byte)(Pc);
                        registers[SP] = (byte)(stack >> 8);
                        registers[SP + 1] = (byte)(stack);
                        registers[PC] = 0;
                        registers[PC + 1] = (byte)(mc & 0x38);
#if (DEBUG)
                        Log($"RST 0x{mc & 0x38:X4}");
#endif
                        Wait(17);
                        return;
                    }
                case 0xDB:
                    {
                        var port = Fetch() + (registers[A] << 8);
                        registers[A] = inPorts(this, (ushort)port);
#if (DEBUG)
                        Log($"IN A, (0x{port:X2})");
#endif
                        Wait(11);
                        return;
                    }
            }

#if(DEBUG)
            Log($"{mc:X2}: {hi:X} {r:X} {lo:X}");
            //throw new InvalidOperationException("Invalid Opcode: "+mc.ToString("X2"));
#endif
            Halt = true;
        }

        private string JCName(byte condition)
        {
            switch (condition)
            {
                case 0:
                    return "NZ";
                case 1:
                    return "Z";
                case 2:
                    return "NC";
                case 3:
                    return "C";
                case 4:
                    return "PO";
                case 5:
                    return "PE";
                case 6:
                    return "P";
                case 7:
                    return "M";
            }
            return "";
        }

        private void ParseCB(byte mode = 0)
        {
            sbyte d = 0;
            if (mode != 0)
            {
                d = (sbyte)Fetch();
            }
            if (Halt) return;
            var mc = Fetch();
            var hi = (byte)(mc >> 6);
            var lo = (byte)(mc & 0x07);
            var r = (byte)((mc >> 3) & 0x07);
            var useHL = lo == 6;
            var useIX = mode == 0xDD;
            var useIY = mode == 0XFD;
            var reg = useHL ? useIX ? mem[(ushort)(Ix + d)] : useIY ? mem[(ushort)(Iy + d)] : mem[Hl] : registers[lo];
#if (DEBUG)
            string debug_target;
            if (useHL)
                if (useIX) debug_target = $"(IX{d:+0;-#})";
                else debug_target = useIY ? $"(IY{d:+0;-#})" : "(HL)";
            else
                debug_target = useIX ? $"(IX{d:+0;-#}), {RName(lo)}" : useIY ? $"(IY{d:+0;-#}), {RName(lo)}" : RName(lo);
#endif
            switch (hi)
            {
                case 0:
                    byte c;
                    if ((r & 1) == 1)
                    {
                        c = (byte)(reg & 0x01);
                        reg >>= 1;
                    }
                    else
                    {
                        c = (byte)((reg & 0x80) >> 7);
                        reg <<= 1;
                    }
                    var f = registers[F];
                    switch (r)
                    {
                        case 0:
                            {
                                reg |= c;
#if (DEBUG)
                                Log($"RLC {debug_target}");
#endif
                                break;
                            }
                        case 1:
                            {
                                reg |= (byte)(c << 7);
#if (DEBUG)
                                Log($"RRC {debug_target}");
#endif
                                break;
                            }
                        case 2:
                            {
                                reg |= (byte)(f & (byte)Fl.C);
#if (DEBUG)
                                Log($"RL {debug_target}");
#endif
                                break;
                            }
                        case 3:
                            {
                                reg |= (byte)((f & (byte)Fl.C) << 7);
#if (DEBUG)
                                Log($"RR {debug_target}");
#endif
                                break;
                            }
                        case 4:
                            {
#if (DEBUG)
                                Log($"SLA {debug_target}");
#endif
                                break;
                            }
                        case 5:
                            {
                                reg |= (byte)((reg & 0x40) << 1);
#if (DEBUG)
                                Log($"SRA {debug_target}");

#endif
                                break;
                            }
                        case 6:
                            {
                                reg |= 1;
#if (DEBUG)
                                Log($"SLL {debug_target}");
#endif
                                break;
                            }
                        case 7:
                            {
#if (DEBUG)
                                Log($"SRL {debug_target}");
#endif
                                break;
                            }
                    }
                    f &= (byte)~(Fl.H | Fl.N | Fl.C | Fl.PV | Fl.S | Fl.Z);
                    f |= (byte)(reg & (byte)Fl.S);
                    if (reg == 0) f |= (byte)Fl.Z;
                    if (Parity(reg)) f |= (byte)Fl.PV;
                    f |= c;
                    registers[F] = f;

                    break;
                case 1:
                    {
                        Bit(r, reg);
#if (DEBUG)
                        Log($"BIT {r}, {debug_target}");
#endif
                        Wait(useHL ? 12 : 8);
                        return;
                    }
                case 2:
                    reg &= (byte)~(0x01 << r);
#if (DEBUG)
                    Log($"RES {r}, {debug_target}");
#endif
                    Wait(useHL ? 12 : 8);
                    break;
                case 3:
                    reg |= (byte)(0x01 << r);
#if (DEBUG)
                    Log($"SET {r}, {debug_target}");
#endif
                    Wait(useHL ? 12 : 8);
                    break;
            }
            if (useHL)
            {
                if (useIX)
                {
                    mem[(ushort)(Ix + d)] = reg;
                    Wait(23);
                }
                else if (useIY)
                {
                    mem[(ushort)(Iy + d)] = reg;
                    Wait(23);
                }
                else
                {
                    mem[Hl] = reg;
                    Wait(15);
                }
            }
            else
            {
                if (useIX)
                {
                    mem[(ushort)(Ix + d)] = reg;
                    Wait(23);
                }
                else if (useIY)
                {
                    mem[(ushort)(Iy + d)] = reg;
                    Wait(23);
                }
                registers[lo] = reg;
                Wait(8);
            }
        }

        private void Bit(byte bit, byte value)
        {
            var f = (byte)(registers[F] & (byte)~(Fl.Z | Fl.H | Fl.N));
            if ((value & (0x01 << bit)) == 0) f |= (byte)Fl.Z;
            f |= (byte)Fl.H;
            registers[F] = f;
        }

        private void AddHl(ushort value)
        {
            var sum = Add(Hl, value);
            registers[H] = (byte)(sum >> 8);
            registers[L] = (byte)(sum & 0xFF);
        }

        private void AddIx(ushort value)
        {
            var sum = Add(Ix, value);
            registers[IX] = (byte)(sum >> 8);
            registers[IX + 1] = (byte)(sum & 0xFF);
        }

        private void AddIy(ushort value)
        {
            var sum = Add(Iy, value);
            registers[IY] = (byte)(sum >> 8);
            registers[IY + 1] = (byte)(sum & 0xFF);
        }

        private ushort Add(ushort value1, ushort value2)
        {
            var sum = value1 + value2;
            var f = (byte)(registers[F] & (byte)~(Fl.H | Fl.N | Fl.C));
            if ((value1 & 0x0FFF) + (value2 & 0x0FFF) > 0x0FFF)
                f |= (byte)Fl.H;
            if (sum > 0xFFFF)
                f |= (byte)Fl.C;
            registers[F] = f;
            return (ushort)sum;
        }

        private void AdcHl(ushort value)
        {
            var sum = Adc(Hl, value);
            registers[H] = (byte)(sum >> 8);
            registers[L] = (byte)(sum & 0xFF);
        }

        private ushort Adc(ushort value1, ushort value2)
        {
            var sum = value1 + value2 + (registers[F] & (byte)Fl.C);
            var f = (byte)(registers[F] & (byte)~(Fl.S | Fl.Z | Fl.H | Fl.PV | Fl.N | Fl.C));
            if ((short)sum < 0)
                f |= (byte)Fl.S;
            if (sum == 0)
                f |= (byte)Fl.Z;
            if ((value1 & 0x0FFF) + (value2 & 0x0FFF) + (byte)Fl.C > 0x0FFF)
                f |= (byte)Fl.H;
            if (sum > 0x7FFF)
                f |= (byte)Fl.PV;
            if (sum > 0xFFFF)
                f |= (byte)Fl.C;
            registers[F] = f;
            return (ushort)sum;
        }

        private void SbcHl(ushort value)
        {
            var sum = Sbc(Hl, value);
            registers[H] = (byte)(sum >> 8);
            registers[L] = (byte)(sum & 0xFF);
        }


        private ushort Sbc(ushort value1, ushort value2)
        {
            var diff = value1 - value2 - (registers[F] & (byte)Fl.C);
            var f = (byte)(registers[F] & (byte)~(Fl.S | Fl.Z | Fl.H | Fl.PV | Fl.N | Fl.C));
            if ((short)diff < 0)
                f |= (byte)Fl.S;
            if (diff == 0)
                f |= (byte)Fl.Z;
            if ((value1 & 0xFFF) < (value2 & 0xFFF) + (registers[F] & (byte)Fl.C))
                f |= (byte)Fl.H;
            if (diff > short.MaxValue || diff < short.MinValue)
                f |= (byte)Fl.PV;
            if ((ushort)diff > value1)
                f |= (byte)Fl.C;
            registers[F] = f;
            return (ushort)diff;
        }

        private void ParseED()
        {
            if (Halt) return;
            var mc = Fetch();
            var hi = (byte)(mc >> 6);
            var lo = (byte)(mc & 0x07);
            var r = (byte)((mc >> 3) & 0x07);

            switch (mc)
            {
                case 0x47:
                    {
                        // LD I, A
                        registers[I] = registers[A];
#if (DEBUG)
                        Log("LD I, A");
#endif
                        Wait(9);
                        return;
                    }
                case 0x4F:
                    {
                        // LD R, A
                        registers[R] = registers[A];
#if (DEBUG)
                        Log("LD R, A");
#endif
                        Wait(9);
                        return;
                    }
                case 0x57:
                    {
                        // LD A, I

                        /*
                                     * Condition Bits Affected
                                     * S is set if the I Register is negative; otherwise, it is reset.
                                     * Z is set if the I Register is 0; otherwise, it is reset.
                                     * H is reset.
                                     * P/V contains contents of IFF2.
                                     * N is reset.
                                     * C is not affected.
                                     * If an interrupt occurs during execution of this instruction, the Parity flag contains a 0.
                                     */
                        var i = registers[I];
                        registers[A] = i;
                        var f = (byte)(registers[F] & (~(byte)(Fl.H | Fl.PV | Fl.N | Fl.S | Fl.Z | Fl.PV)));
                        if (i >= 0x80)
                        {
                            f |= (byte)Fl.S;
                        }
                        else if (i == 0x00)
                        {
                            f |= (byte)Fl.Z;
                        }
                        if (IFF2)
                        {
                            f |= (byte)Fl.PV;
                        }
                        registers[F] = f;
#if (DEBUG)
                        Log("LD A, I");
#endif
                        Wait(9);
                        return;
                    }
                case 0x5F:
                    {
                        // LD A, R

                        /*
                                     * Condition Bits Affected
                                     * S is set if, R-Register is negative; otherwise, it is reset.
                                     * Z is set if the R Register is 0; otherwise, it is reset.
                                     * H is reset.
                                     * P/V contains contents of IFF2.
                                     * N is reset.
                                     * C is not affected.
                                     * If an interrupt occurs during execution of this instruction, the parity flag contains a 0. 
                                     */
                        var reg = registers[R];
                        registers[A] = reg;
                        var f = (byte)(registers[F] & (~(byte)(Fl.H | Fl.PV | Fl.N | Fl.S | Fl.Z | Fl.PV)));
                        if (reg >= 0x80)
                        {
                            f |= (byte)Fl.S;
                        }
                        else if (reg == 0x00)
                        {
                            f |= (byte)Fl.Z;
                        }
                        if (IFF2)
                        {
                            f |= (byte)Fl.PV;
                        }
                        registers[F] = f;
#if (DEBUG)
                        Log("LD A, R");
#endif
                        Wait(9);
                        return;
                    }
                case 0x4B:
                    {
                        // LD BC, (nn)
                        var addr = Fetch16();
                        registers[C] = mem[addr++];
                        registers[B] = mem[addr];
#if (DEBUG)
                        Log($"LD BC, (0x{--addr:X4})");
#endif
                        Wait(20);
                        return;
                    }
                case 0x5B:
                    {
                        // LD DE, (nn)
                        var addr = Fetch16();
                        registers[E] = mem[addr++];
                        registers[D] = mem[addr];
#if (DEBUG)
                        Log($"LD DE, (0x{--addr:X4})");
#endif
                        Wait(20);
                        return;
                    }
                case 0x6B:
                    {
                        // LD HL, (nn)
                        var addr = Fetch16();
                        registers[L] = mem[addr++];
                        registers[H] = mem[addr];
#if (DEBUG)
                        Log($"LD HL, (0x{--addr:X4})*");
#endif
                        Wait(20);
                        return;
                    }
                case 0x7B:
                    {
                        // LD SP, (nn)
                        var addr = Fetch16();
                        registers[SP + 1] = mem[addr++];
                        registers[SP] = mem[addr];
#if (DEBUG)
                        Log($"LD SP, (0x{--addr:X4})");
#endif
                        Wait(20);
                        return;
                    }
                case 0x43:
                    {
                        // LD (nn), BC
                        var addr = Fetch16();
                        mem[addr++] = registers[C];
                        mem[addr] = registers[B];
#if (DEBUG)
                        Log($"LD (0x{--addr:X4}), BC");
#endif
                        Wait(20);
                        return;
                    }
                case 0x53:
                    {
                        // LD (nn), DE
                        var addr = Fetch16();
                        mem[addr++] = registers[E];
                        mem[addr] = registers[D];
#if (DEBUG)
                        Log($"LD (0x{--addr:X4}), DE");
#endif
                        Wait(20);
                        return;
                    }
                case 0x63:
                    {
                        // LD (nn), HL
                        var addr = Fetch16();
                        mem[addr++] = registers[L];
                        mem[addr] = registers[H];
#if (DEBUG)
                        Log($"LD (0x{--addr:X4}), HL");
#endif
                        Wait(20);
                        return;
                    }
                case 0x73:
                    {
                        // LD (nn), SP
                        var addr = Fetch16();
                        mem[addr++] = registers[SP + 1];
                        mem[addr] = registers[SP];
#if (DEBUG)
                        Log($"LD (0x{--addr:X4}), SP");
#endif
                        Wait(20);
                        return;
                    }
                case 0xA0:
                    {
                        // LDI
                        var bc = Bc;
                        var de = De;
                        var hl = Hl;

                        mem[de] = mem[hl];
                        de++;
                        hl++;
                        bc--;

                        registers[B] = (byte)(bc >> 8);
                        registers[C] = (byte)(bc & 0xFF);
                        registers[D] = (byte)(de >> 8);
                        registers[E] = (byte)(de & 0xFF);
                        registers[H] = (byte)(hl >> 8);
                        registers[L] = (byte)(hl & 0xFF);

                        var f = (byte)(registers[F] & 0xE9);
                        if (bc != 0) f = (byte)(f | 0x04);
                        registers[F] = f;
#if (DEBUG)
                        Log("LDI");
#endif
                        Wait(16);
                        return;
                    }
                case 0xB0:
                    {
                        // LDIR
                        var bc = Bc;
                        var de = De;
                        var hl = Hl;

                        mem[de] = mem[hl];
                        de++;
                        hl++;
                        bc--;

                        registers[B] = (byte)(bc >> 8);
                        registers[C] = (byte)(bc & 0xFF);
                        registers[D] = (byte)(de >> 8);
                        registers[E] = (byte)(de & 0xFF);
                        registers[H] = (byte)(hl >> 8);
                        registers[L] = (byte)(hl & 0xFF);

                        registers[F] = (byte)(registers[F] & 0xE9);
                        if (bc != 0)
                        {
                            var pc = (ushort)((registers[PC] << 8) + registers[PC + 1]);
                            // jumps back to itself
                            pc -= 2;
                            registers[PC] = (byte)(pc >> 8);
                            registers[PC + 1] = (byte)(pc & 0xFF);
                            Wait(21);
                            return;
                        }
#if (DEBUG)
                        Log("LDIR");
#endif
                        Wait(16);
                        return;
                    }
                case 0xA8:
                    {
                        // LDD
                        var bc = Bc;
                        var de = De;
                        var hl = Hl;

                        mem[de] = mem[hl];
                        de--;
                        hl--;
                        bc--;

                        registers[B] = (byte)(bc >> 8);
                        registers[C] = (byte)(bc & 0xFF);
                        registers[D] = (byte)(de >> 8);
                        registers[E] = (byte)(de & 0xFF);
                        registers[H] = (byte)(hl >> 8);
                        registers[L] = (byte)(hl & 0xFF);

                        var f = (byte)(registers[F] & 0xE9);
                        if (bc != 0) f = (byte)(f | 0x04);
                        registers[F] = f;
#if (DEBUG)
                        Log("LDD");
#endif
                        Wait(16);
                        return;
                    }
                case 0xB8:
                    {
                        // LDDR
                        var bc = Bc;
                        var de = De;
                        var hl = Hl;

                        mem[de] = mem[hl];
                        de--;
                        hl--;
                        bc--;

                        registers[B] = (byte)(bc >> 8);
                        registers[C] = (byte)(bc & 0xFF);
                        registers[D] = (byte)(de >> 8);
                        registers[E] = (byte)(de & 0xFF);
                        registers[H] = (byte)(hl >> 8);
                        registers[L] = (byte)(hl & 0xFF);

                        registers[F] = (byte)(registers[F] & 0xE9);
                        if (bc != 0)
                        {
                            var pc = (ushort)((registers[PC] << 8) + registers[PC + 1]);
                            // jumps back to itself
                            pc -= 2;
                            registers[PC] = (byte)(pc >> 8);
                            registers[PC + 1] = (byte)(pc & 0xFF);
                            Wait(21);
                            return;
                        }
#if (DEBUG)
                        Log("LDDR");
#endif
                        Wait(16);
                        return;
                    }

                case 0xA1:
                    {
                        // CPI
                        var bc = Bc;
                        var hl = Hl;

                        var a = registers[A];
                        var b = mem[hl];
                        hl++;
                        bc--;

                        registers[B] = (byte)(bc >> 8);
                        registers[C] = (byte)(bc & 0xFF);
                        registers[H] = (byte)(hl >> 8);
                        registers[L] = (byte)(hl & 0xFF);

                        var f = (byte)(registers[F] & 0x2A);
                        if (a < b) f = (byte)(f | 0x80);
                        if (a == b) f = (byte)(f | 0x40);
                        if ((a & 8) < (b & 8)) f = (byte)(f | 0x10);
                        if (bc != 0) f = (byte)(f | 0x04);
                        registers[F] = (byte)(f | 0x02);
#if (DEBUG)
                        Log("CPI");
#endif
                        Wait(16);
                        return;
                    }

                case 0xB1:
                    {
                        // CPIR
                        var bc = Bc;
                        var hl = Hl;

                        var a = registers[A];
                        var b = mem[hl];
                        hl++;
                        bc--;

                        registers[B] = (byte)(bc >> 8);
                        registers[C] = (byte)(bc & 0xFF);
                        registers[H] = (byte)(hl >> 8);
                        registers[L] = (byte)(hl & 0xFF);

                        if (a == b || bc == 0)
                        {
                            var f = (byte)(registers[F] & 0x2A);
                            if (a < b) f = (byte)(f | 0x80);
                            if (a == b) f = (byte)(f | 0x40);
                            if ((a & 8) < (b & 8)) f = (byte)(f | 0x10);
                            if (bc != 0) f = (byte)(f | 0x04);
                            registers[F] = (byte)(f | 0x02);
#if (DEBUG)
                            Log("CPIR");
#endif
                            Wait(16);
                            return;
                        }

                        var pc = (ushort)((registers[PC] << 8) + registers[PC + 1]);
                        // jumps back to itself
                        pc -= 2;
                        registers[PC] = (byte)(pc >> 8);
                        registers[PC + 1] = (byte)(pc & 0xFF);
                        Wait(21);
                        return;
                    }

                case 0xA9:
                    {
                        // CPD
                        var bc = Bc;
                        var hl = Hl;

                        var a = registers[A];
                        var b = mem[hl];
                        hl--;
                        bc--;

                        registers[B] = (byte)(bc >> 8);
                        registers[C] = (byte)(bc & 0xFF);
                        registers[H] = (byte)(hl >> 8);
                        registers[L] = (byte)(hl & 0xFF);

                        var f = (byte)(registers[F] & 0x2A);
                        if (a < b) f = (byte)(f | 0x80);
                        if (a == b) f = (byte)(f | 0x40);
                        if ((a & 8) < (b & 8)) f = (byte)(f | 0x10);
                        if (bc != 0) f = (byte)(f | 0x04);
                        registers[F] = (byte)(f | 0x02);
#if (DEBUG)
                        Log("CPD");
#endif
                        Wait(16);
                        return;
                    }

                case 0xB9:
                    {
                        // CPDR
                        var bc = Bc;
                        var hl = Hl;

                        var a = registers[A];
                        var b = mem[hl];
                        hl--;
                        bc--;

                        registers[B] = (byte)(bc >> 8);
                        registers[C] = (byte)(bc & 0xFF);
                        registers[H] = (byte)(hl >> 8);
                        registers[L] = (byte)(hl & 0xFF);

                        if (a == b || bc == 0)
                        {
                            var f = (byte)(registers[F] & 0x2A);
                            if (a < b) f = (byte)(f | 0x80);
                            if (a == b) f = (byte)(f | 0x40);
                            if ((a & 8) < (b & 8)) f = (byte)(f | 0x10);
                            if (bc != 0) f = (byte)(f | 0x04);
                            registers[F] = (byte)(f | 0x02);
#if (DEBUG)
                            Log("CPDR");
#endif
                            Wait(21);
                            return;
                        }

                        var pc = (ushort)((registers[PC] << 8) + registers[PC + 1]);
                        // jumps back to itself
                        pc -= 2;
                        registers[PC] = (byte)(pc >> 8);
                        registers[PC + 1] = (byte)(pc & 0xFF);
                        Wait(21);
                        return;
                    }
                case 0x44:
                case 0x54:
                case 0x64:
                case 0x74:
                case 0x4C:
                case 0x5C:
                case 0x6C:
                case 0x7C:
                    {
                        // NEG
                        var a = registers[A];
                        var diff = -a;
                        registers[A] = (byte)diff;

                        var f = (byte)(registers[F] & 0x28);
                        if ((diff & 0x80) > 0) f |= (byte)Fl.S;
                        if (diff == 0) f |= (byte)Fl.Z;
                        if ((a & 0xF) != 0) f |= (byte)Fl.H;
                        if (a == 0x80) f |= (byte)Fl.PV;
                        f |= (byte)Fl.N;
                        if (diff != 0) f |= (byte)Fl.C;
                        registers[F] = f;


#if (DEBUG)
                        Log("NEG");
#endif
                        Wait(8);
                        return;
                    }
                case 0x46:
                case 0x66:
                    {
                        // IM 0
                        interruptMode = 0;
#if (DEBUG)
                        Log("IM 0");
#endif
                        Wait(8);
                        return;
                    }
                case 0x56:
                case 0x76:
                    {
                        // IM 1
                        interruptMode = 1;
#if (DEBUG)
                        Log("IM 1");
#endif
                        Wait(8);
                        return;
                    }
                case 0x5E:
                case 0x7E:
                    {
                        // IM 2
                        interruptMode = 2;
#if (DEBUG)
                        Log("IM 2");
#endif
                        Wait(8);
                        return;
                    }
                case 0x4A:
                    {
                        AdcHl(Bc);

#if (DEBUG)
                        Log("ADC HL, BC");
#endif
                        Wait(15);
                        return;
                    }
                case 0x5A:
                    {
                        AdcHl(De);
#if (DEBUG)
                        Log("ADC HL, DE");
#endif
                        Wait(15);
                        return;
                    }
                case 0x6A:
                    {
                        AdcHl(Hl);
#if (DEBUG)
                        Log("ADC HL, HL");
#endif
                        Wait(15);
                        return;
                    }
                case 0x7A:
                    {
                        AdcHl(Sp);
#if (DEBUG)
                        Log("ADC HL, SP");
#endif
                        Wait(15);
                        return;
                    }
                case 0x42:
                    {
                        SbcHl(Bc);

#if (DEBUG)
                        Log("SBC HL, BC");
#endif
                        Wait(15);
                        return;
                    }
                case 0x52:
                    {
                        SbcHl(De);
#if (DEBUG)
                        Log("SBC HL, DE");
#endif
                        Wait(15);
                        return;
                    }
                case 0x62:
                    {
                        SbcHl(Hl);
#if (DEBUG)
                        Log("SBC HL, HL");
#endif
                        Wait(15);
                        return;
                    }
                case 0x72:
                    {
                        SbcHl(Sp);
#if (DEBUG)
                        Log("SBC HL, SP");
#endif
                        Wait(15);
                        return;
                    }

                case 0x6F:
                    {
                        var a = registers[A];
                        var b = mem[Hl];
                        mem[Hl] = (byte)((b << 4) | (a & 0x0F));
                        a = (byte)((a & 0xF0) | (b >> 4));
                        registers[A] = a;
                        var f = (byte)(registers[F] & 0x29);
                        if ((a & 0x80) > 0) f |= (byte)Fl.S;
                        if (a == 0) f |= (byte)Fl.Z;
                        if (Parity(a)) f |= (byte)Fl.PV;
                        registers[F] = f;
#if (DEBUG)
                        Log("RLD");
#endif
                        Wait(18);
                        return;
                    }
                case 0x67:
                    {
                        var a = registers[A];
                        var b = mem[Hl];
                        mem[Hl] = (byte)((b >> 4) | (a << 4));
                        a = (byte)((a & 0xF0) | (b & 0x0F));
                        registers[A] = a;
                        var f = (byte)(registers[F] & 0x29);
                        if ((a & 0x80) > 0) f |= (byte)Fl.S;
                        if (a == 0) f |= (byte)Fl.Z;
                        if (Parity(a)) f |= (byte)Fl.PV;
                        registers[F] = f;
#if (DEBUG)
                        Log("RRD");
#endif
                        Wait(18);
                        return;
                    }
                case 0x45:
                case 0x4D:
                case 0x55:
                case 0x5D:
                case 0x65:
                case 0x6D:
                case 0x75:
                case 0x7D:
                    {
                        var stack = Sp;
                        registers[PC + 1] = mem[stack++];
                        registers[PC] = mem[stack++];
                        registers[SP] = (byte)(stack >> 8);
                        registers[SP + 1] = (byte)(stack);
                        IFF1 = IFF2;
#if (DEBUG)
                        if (mc == 0x4D)
                            Log("RETN");
                        else
                            Log("RETI");
#endif
                        Wait(10);
                        return;
                    }

                case 0x77:
                case 0x7F:
                    {
#if (DEBUG)
                        Log("NOP");
#endif
                        Wait(8);
                        return;
                    }
                case 0x40:
                case 0x48:
                case 0x50:
                case 0x58:
                case 0x60:
                case 0x68:
                case 0x78:
                    {
                        var a = inPorts(this, Bc);
                        registers[r] = a;
                        var f = (byte)(registers[F] & 0x29);
                        if ((a & 0x80) > 0) f |= (byte)Fl.S;
                        if (a == 0) f |= (byte)Fl.Z;
                        if (Parity(a)) f |= (byte)Fl.PV;
                        registers[F] = f;
#if (DEBUG)
                        Log($"IN {RName(r)}, (BC)");
#endif
                        Wait(8);
                        return;
                    }
            }
#if (DEBUG)
            Log($"ED {mc:X2}: {hi:X2} {lo:X2} {r:X2}");
#endif
            Halt = true;
        }

        private void ParseDD()
        {
            if (Halt) return;
            var mc = Fetch();
            var hi = (byte)(mc >> 6);
            var lo = (byte)(mc & 0x07);
            var mid = (byte)((mc >> 3) & 0x07);

            switch (mc)
            {
                case 0xCB:
                    {
                        ParseCB(0xDD);
                        return;
                    }
                case 0x21:
                    {
                        // LD IX, nn
                        registers[IX + 1] = Fetch();
                        registers[IX] = Fetch();
#if (DEBUG)
                        Log($"LD IX, 0x{Ix:X4}");
#endif
                        Wait(14);
                        return;
                    }
                case 0x46:
                case 0x4e:
                case 0x56:
                case 0x5e:
                case 0x66:
                case 0x6e:
                case 0x7e:
                    {
                        // LD r, (IX+d)
                        var d = (sbyte)Fetch();
                        registers[mid] = mem[(ushort)(Ix + d)];
#if (DEBUG)
                        Log($"LD {RName(mid)}, (IX{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x77:
                    {
                        // LD (IX+d), r
                        var d = (sbyte)Fetch();
                        mem[(ushort)(Ix + d)] = registers[lo];
#if (DEBUG)
                        Log($"LD (IX{d:+0;-#}), {RName(lo)}");
#endif
                        Wait(19);
                        return;
                    }
                case 0x36:
                    {
                        // LD (IX+d), n
                        var d = (sbyte)Fetch();
                        var n = Fetch();
                        mem[(ushort)(Ix + d)] = n;
#if (DEBUG)
                        Log($"LD (IX{d:+0;-#}), {n}");
#endif
                        Wait(19);
                        return;
                    }
                case 0x2A:
                    {
                        // LD IX, (nn)
                        var addr = Fetch16();
                        registers[IX + 1] = mem[addr++];
                        registers[IX] = mem[addr];
#if (DEBUG)
                        Log($"LD IX, (0x{addr:X4})*");
#endif
                        Wait(20);
                        return;
                    }
                case 0x22:
                    {
                        // LD (nn), IX
                        var addr = Fetch16();
                        mem[addr++] = registers[IX + 1];
                        mem[addr] = registers[IX];
#if (DEBUG)
                        Log($"LD (0x{addr:X4}), IX");
#endif
                        Wait(20);
                        return;
                    }

                case 0xF9:
                    {
                        // LD SP, IX
                        registers[SP] = registers[IX];
                        registers[SP + 1] = registers[IX + 1];
#if (DEBUG)
                        Log("LD SP, IX");
#endif
                        Wait(10);
                        return;
                    }
                case 0xE5:
                    {
                        // PUSH IX
                        var addr = Sp;
                        addr--;
                        mem[addr] = registers[IX];
                        addr--;
                        mem[addr] = registers[IX + 1];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH IX");
#endif
                        Wait(15);
                        return;
                    }
                case 0xE1:
                    {
                        // POP IX
                        var addr = Sp;
                        registers[IX + 1] = mem[addr++];
                        registers[IX] = mem[addr++];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP IX");
#endif
                        Wait(14);
                        return;
                    }
                case 0xE3:
                    {
                        // EX (SP), IX
                        var h = registers[IX];
                        var l = registers[IX + 1];
                        var addr = Sp;
                        registers[IX + 1] = mem[addr++];
                        registers[IX] = mem[addr];
                        mem[addr--] = h;
                        mem[addr] = l;

#if (DEBUG)
                        Log("EX (SP), IX");
#endif
                        Wait(24);
                        return;
                    }

                case 0x86:
                    {
                        // ADD A, (IX+d)
                        var d = (sbyte)Fetch();

                        Add(mem[(ushort)(Ix + d)]);
#if (DEBUG)
                        Log($"ADD A, (IX{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0x8E:
                    {
                        // ADC A, (IX+d)
                        var d = (sbyte)Fetch();
                        var a = registers[A];
                        Adc(mem[(ushort)(Ix + d)]);
#if (DEBUG)
                        Log($"ADC A, (IX{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0x96:
                    {
                        // SUB A, (IX+d)
                        var d = (sbyte)Fetch();
                        var b = mem[(ushort)(Ix + d)];

                        Sub(b);
#if (DEBUG)
                        Log($"SUB A, (IX{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0x9E:
                    {
                        // SBC A, (IX+d)
                        var d = (sbyte)Fetch();

                        Sbc(mem[(ushort)(Ix + d)]);
#if (DEBUG)
                        Log($"SBC A, (IX{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0xA6:
                    {
                        // AND A, (IX+d)
                        var d = (sbyte)Fetch();
                        var b = mem[(ushort)(Ix + d)];

                        And(b);
#if (DEBUG)
                        Log($"AND A, (IX{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0xB6:
                    {
                        // OR A, (IX+d)
                        var d = (sbyte)Fetch();
                        var b = mem[(ushort)(Ix + d)];

                        Or(b);
#if (DEBUG)
                        Log($"OR A, (IX{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0xAE:
                    {
                        // OR A, (IX+d)
                        var d = (sbyte)Fetch();
                        var b = mem[(ushort)(Ix + d)];

                        Xor(b);
#if (DEBUG)
                        Log($"XOR A, (IX{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0xBE:
                    {
                        // CP A, (IX+d)
                        var d = (sbyte)Fetch();
                        var b = mem[(ushort)(Ix + d)];

                        Cmp(b);
#if (DEBUG)
                        Log($"CP A, (IX{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0x34:
                    {
                        // INC (IX+d)
                        var d = (sbyte)Fetch();
                        mem[(ushort)(Ix + d)] = Inc(mem[(ushort)(Ix + d)]);
#if (DEBUG)
                        Log($"INC (IX{d:+0;-#})");
#endif
                        Wait(7);
                        return;
                    }
                case 0x35:
                    {
                        // DEC (IX+d)
                        var d = (sbyte)Fetch();
                        mem[(ushort)(Ix + d)] = Dec(mem[(ushort)(Ix + d)]);
#if (DEBUG)
                        Log($"DEC (IX{d:+0;-#})");
#endif
                        Wait(7);
                        return;
                    }
                case 0x09:
                    {
                        AddIx(Bc);
#if (DEBUG)
                        Log("ADD IX, BC");
#endif
                        Wait(4);
                        return;
                    }
                case 0x19:
                    {
                        AddIx(De);
#if (DEBUG)
                        Log("ADD IX, DE");
#endif
                        Wait(4);
                        return;
                    }
                case 0x29:
                    {
                        AddIx(Ix);
#if (DEBUG)
                        Log("ADD IX, IX");
#endif
                        Wait(4);
                        return;
                    }
                case 0x39:
                    {
                        AddIx(Sp);
#if (DEBUG)
                        Log("ADD IX, SP");
#endif
                        Wait(4);
                        return;
                    }
                case 0x23:
                    {
                        var val = Ix + 1;
                        registers[IX] = (byte)(val >> 8);
                        registers[IX + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC IX");
#endif
                        Wait(4);
                        return;
                    }
                case 0x2B:
                    {
                        var val = Ix - 1;
                        registers[IX] = (byte)(val >> 8);
                        registers[IX + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC IX");
#endif
                        Wait(4);
                        return;
                    }
                case 0xE9:
                    {
                        var addr = Ix;
                        registers[PC] = (byte)(addr >> 8);
                        registers[PC + 1] = (byte)(addr);
#if (DEBUG)
                        Log("JP IX");
#endif
                        Wait(8);
                        return;
                    }

            }
#if (DEBUG)
            Log($"DD {mc:X2}: {hi:X} {mid:X} {lo:X}");
#endif
            Halt = true;
        }

        private void ParseFD()
        {
            if (Halt) return;
            var mc = Fetch();
            var hi = (byte)(mc >> 6);
            var lo = (byte)(mc & 0x07);
            var r = (byte)((mc >> 3) & 0x07);

            switch (mc)
            {
                case 0xCB:
                    {
                        ParseCB(0xFD);
                        return;
                    }
                case 0x21:
                    {
                        // LD IY, nn
                        registers[IY + 1] = Fetch();
                        registers[IY] = Fetch();
#if (DEBUG)
                        Log($"LD IY, 0x{Iy:X4}");
#endif
                        Wait(14);
                        return;
                    }

                case 0x46:
                case 0x4e:
                case 0x56:
                case 0x5e:
                case 0x66:
                case 0x6e:
                case 0x7e:
                    {
                        // LD r, (IY+d)
                        var d = (sbyte)Fetch();
                        registers[r] = mem[(ushort)(Iy + d)];
#if (DEBUG)
                        Log($"LD {RName(r)}, (IY{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0x70:
                case 0x71:
                case 0x72:
                case 0x73:
                case 0x74:
                case 0x75:
                case 0x77:
                    {
                        // LD (IY+d), r
                        var d = (sbyte)Fetch();
                        mem[(ushort)(Iy + d)] = registers[lo];
#if (DEBUG)
                        Log($"LD (IY{d:+0;-#}), {RName(lo)}");
#endif
                        Wait(19);
                        return;
                    }
                case 0x36:
                    {
                        // LD (IY+d), n
                        var d = (sbyte)Fetch();
                        var n = Fetch();
                        mem[(ushort)(Iy + d)] = n;
#if (DEBUG)
                        Log($"LD (IY{d:+0;-#}), {n}");
#endif
                        Wait(19);
                        return;
                    }
                case 0x2A:
                    {
                        // LD IY, (nn)
                        var addr = Fetch16();
                        registers[IY + 1] = mem[addr++];
                        registers[IY] = mem[addr];
#if (DEBUG)
                        Log($"LD IY, (0x{--addr:X4})*");
#endif
                        Wait(20);
                        return;
                    }

                case 0x22:
                    {
                        // LD (nn), IY
                        var addr = Fetch16();
                        mem[addr++] = registers[IY + 1];
                        mem[addr] = registers[IY];
#if (DEBUG)
                        Log($"LD (0x{--addr:X4}), IY");
#endif
                        Wait(20);
                        return;
                    }
                case 0xF9:
                    {
                        // LD SP, IY
                        registers[SP] = registers[IY];
                        registers[SP + 1] = registers[IY + 1];
#if (DEBUG)
                        Log("LD SP, IY");
#endif
                        Wait(10);
                        return;
                    }
                case 0xE5:
                    {
                        // PUSH IY
                        var addr = Sp;
                        mem[--addr] = registers[IY];
                        mem[--addr] = registers[IY + 1];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH IY");
#endif
                        Wait(15);
                        return;
                    }
                case 0xE1:
                    {
                        // POP IY
                        var addr = Sp;
                        registers[IY + 1] = mem[addr++];
                        registers[IY] = mem[addr++];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP IY");
#endif
                        Wait(14);
                        return;
                    }
                case 0xE3:
                    {
                        // EX (SP), IY
                        var h = registers[IY];
                        var l = registers[IY + 1];
                        var addr = Sp;
                        registers[IY + 1] = mem[addr];
                        mem[addr++] = l;
                        registers[IY] = mem[addr];
                        mem[addr] = h;

#if (DEBUG)
                        Log("EX (SP), IY");
#endif
                        Wait(24);
                        return;
                    }
                case 0x86:
                    {
                        // ADD A, (IY+d)
                        var d = (sbyte)Fetch();

                        Add(mem[(ushort)(Iy + d)]);
#if (DEBUG)
                        Log($"ADD A, (IY{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0x8E:
                    {
                        // ADC A, (IY+d)
                        var d = (sbyte)Fetch();
                        var a = registers[A];
                        Adc(mem[(ushort)(Iy + d)]);

#if (DEBUG)
                        Log($"ADC A, (IY{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0x96:
                    {
                        // SUB A, (IY+d)
                        var d = (sbyte)Fetch();

                        Sub(mem[(ushort)(Iy + d)]);
#if (DEBUG)
                        Log($"SUB A, (IY{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0x9E:
                    {
                        // SBC A, (IY+d)
                        var d = (sbyte)Fetch();

                        Sbc(mem[(ushort)(Iy + d)]);
#if (DEBUG)
                        Log($"SBC A, (IY{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0xA6:
                    {
                        // AND A, (IY+d)
                        var d = (sbyte)Fetch();
                        var b = mem[(ushort)(Iy + d)];

                        And(b);
#if (DEBUG)
                        Log($"AND A, (IY{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0xB6:
                    {
                        // OR A, (IY+d)
                        var d = (sbyte)Fetch();
                        var b = mem[(ushort)(Iy + d)];

                        Or(b);
#if (DEBUG)
                        Log($"OR A, (IY{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0xAE:
                    {
                        // XOR A, (IY+d)
                        var d = (sbyte)Fetch();
                        var b = mem[(ushort)(Iy + d)];

                        Xor(b);
#if (DEBUG)
                        Log($"XOR A, (IY{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0xBE:
                    {
                        // CP A, (IY+d)
                        var d = (sbyte)Fetch();

                        Cmp(mem[(ushort)(Iy + d)]);
#if (DEBUG)
                        Log($"CP A, (IY{d:+0;-#})");
#endif
                        Wait(19);
                        return;
                    }
                case 0x34:
                    {
                        // INC (IY+d)
                        var d = (sbyte)Fetch();
                        mem[(ushort)(Iy + d)] = Inc(mem[(ushort)(Iy + d)]);
#if (DEBUG)
                        Log($"INC (IY{d:+0;-#})");
#endif
                        Wait(7);
                        return;
                    }
                case 0x35:
                    {
                        // DEC (IY+d)
                        var d = (sbyte)Fetch();
                        mem[(ushort)(Iy + d)] = Dec(mem[(ushort)(Iy + d)]);
#if (DEBUG)
                        Log($"DEC (IY{d:+0;-#})");
#endif
                        Wait(7);
                        return;
                    }
                case 0x09:
                    {
                        AddIy(Bc);
#if (DEBUG)
                        Log("ADD IY, BC");
#endif
                        Wait(4);
                        return;
                    }
                case 0x19:
                    {
                        AddIy(De);
#if (DEBUG)
                        Log("ADD IY, DE");
#endif
                        Wait(4);
                        return;
                    }
                case 0x29:
                    {
                        AddIy(Iy);
#if (DEBUG)
                        Log("ADD IY, IY");
#endif
                        Wait(4);
                        return;
                    }
                case 0x39:
                    {
                        AddIy(Sp);
#if (DEBUG)
                        Log("ADD IY, SP");
#endif
                        Wait(4);
                        return;
                    }
                case 0x23:
                    {
                        var val = Iy + 1;
                        registers[IY] = (byte)(val >> 8);
                        registers[IY + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC IY");
#endif
                        Wait(4);
                        return;
                    }
                case 0x2B:
                    {
                        var val = Iy - 1;
                        registers[IY] = (byte)(val >> 8);
                        registers[IY + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC IY");
#endif
                        Wait(4);
                        return;
                    }
                case 0xE9:
                    {
                        var addr = Iy;
                        registers[PC] = (byte)(addr >> 8);
                        registers[PC + 1] = (byte)(addr);
#if (DEBUG)
                        Log("JP IY");
#endif
                        Wait(8);
                        return;
                    }

            }
#if (DEBUG)
            Log($"FD {mc:X2}: {hi:X2} {lo:X2} {r:X2}");
#endif
            Halt = true;
        }

        private void Add(byte b)
        {
            var a = registers[A];
            var sum = a + b;
            registers[A] = (byte)sum;
            var f = (byte)(registers[F] & 0x28);
            if ((sum & 0x80) > 0)
                f |= (byte)Fl.S;
            if ((byte)sum == 0)
                f |= (byte)Fl.Z;
            if ((a & 0xF + b & 0xF) > 0xF)
                f |= (byte)Fl.H;
            if ((a >= 0x80 && b >= 0x80 && (sbyte)sum > 0) || (a < 0x80 && b < 0x80 && (sbyte)sum < 0))
                f |= (byte)Fl.PV;
            if (sum > 0xFF)
                f |= (byte)Fl.C;
            registers[F] = f;
        }

        private void Adc(byte b)
        {
            var a = registers[A];
            var c = (byte)(registers[F] & (byte)Fl.C);
            var sum = a + b + c;
            registers[A] = (byte)sum;
            var f = (byte)(registers[F] & 0x28);
            if ((sum & 0x80) > 0)
                f |= (byte)Fl.S;
            if ((byte)sum == 0)
                f |= (byte)Fl.Z;
            if ((a & 0xF + b & 0xF) > 0xF)
                f |= (byte)Fl.H;
            if ((a >= 0x80 && b >= 0x80 && (sbyte)sum > 0) || (a < 0x80 && b < 0x80 && (sbyte)sum < 0))
                f |= (byte)Fl.PV;
            f = (byte)(f & ~(byte)Fl.N);
            if (sum > 0xFF) f |= (byte)Fl.C;
            registers[F] = f;
        }

        private void Sub(byte b)
        {
            var a = registers[A];
            var diff = a - b;
            registers[A] = (byte)diff;
            var f = (byte)(registers[F] & 0x28);
            if ((diff & 0x80) > 0)
                f |= (byte)Fl.S;
            if (diff == 0)
                f |= (byte)Fl.Z;
            if ((a & 0xF) < (b & 0xF))
                f |= (byte)Fl.H;
            if ((a >= 0x80 && b >= 0x80 && (sbyte)diff > 0) || (a < 0x80 && b < 0x80 && (sbyte)diff < 0))
                f |= (byte)Fl.PV;
            f |= (byte)Fl.N;
            if (diff > 0xFF)
                f |= (byte)Fl.C;
            registers[F] = f;
        }

        private void Sbc(byte b)
        {
            var a = registers[A];
            var c = (byte)(registers[F] & 0x01);
            var diff = a - b - c;
            registers[A] = (byte)diff;
            var f = (byte)(registers[F] & 0x28);
            if ((diff & 0x80) > 0) f |= (byte)Fl.S;
            if (diff == 0) f |= (byte)Fl.Z;
            if ((a & 0xF) < (b & 0xF) + c) f |= (byte)Fl.H;
            if ((a >= 0x80 && b >= 0x80 && (sbyte)diff > 0) || (a < 0x80 && b < 0x80 && (sbyte)diff < 0))
                f |= (byte)Fl.PV;
            f |= (byte)Fl.N;
            if (diff > 0xFF) f |= (byte)Fl.C;
            registers[F] = f;
        }

        private void And(byte b)
        {
            var a = registers[A];
            var res = (byte)(a & b);
            registers[A] = res;
            var f = (byte)(registers[F] & 0x28);
            if ((res & 0x80) > 0) f |= (byte)Fl.S;
            if (res == 0) f |= (byte)Fl.Z;
            f |= (byte)Fl.H;
            if (Parity(res)) f |= (byte)Fl.PV;
            registers[F] = f;
        }

        private void Or(byte b)
        {
            var a = registers[A];
            var res = (byte)(a | b);
            registers[A] = res;
            var f = (byte)(registers[F] & 0x28);
            if ((res & 0x80) > 0)
                f |= (byte)Fl.S;
            if (res == 0)
                f |= (byte)Fl.Z;
            if (Parity(res))
                f |= (byte)Fl.PV;
            registers[F] = f;
        }

        private void Xor(byte b)
        {
            var a = registers[A];
            var res = (byte)(a ^ b);
            registers[A] = res;
            var f = (byte)(registers[F] & 0x28);
            if ((res & 0x80) > 0)
                f |= (byte)Fl.S;
            if (res == 0)
                f |= (byte)Fl.Z;
            if (Parity(res))
                f |= (byte)Fl.PV;
            registers[F] = f;
        }

        private void Cmp(byte b)
        {
            var a = registers[A];
            var diff = a - b;
            var f = (byte)(registers[F] & 0x28);
            if ((diff & 0x80) > 0)
                f = (byte)(f | 0x80);
            if (diff == 0)
                f = (byte)(f | 0x40);
            if ((a & 0xF) < (b & 0xF))
                f = (byte)(f | 0x10);
            if ((a > 0x80 && b > 0x80 && (sbyte)diff > 0) || (a < 0x80 && b < 0x80 && (sbyte)diff < 0))
                f = (byte)(f | 0x04);
            f = (byte)(f | 0x02);
            if (diff > 0xFF)
                f = (byte)(f | 0x01);
            registers[F] = f;
        }

        private byte Inc(byte b)
        {
            var sum = b + 1;
            var f = (byte)(registers[F] & 0x28);
            if ((sum & 0x80) > 0)
                f = (byte)(f | 0x80);
            if (sum == 0)
                f = (byte)(f | 0x40);
            if ((b & 0xF) == 0xF)
                f = (byte)(f | 0x10);
            if ((b < 0x80 && (sbyte)sum < 0))
                f = (byte)(f | 0x04);
            f = (byte)(f | 0x02);
            if (sum > 0xFF) f = (byte)(f | 0x01);
            registers[F] = f;

            return (byte)sum;
        }

        private byte Dec(byte b)
        {
            var sum = b - 1;
            var f = (byte)(registers[F] & 0x28);
            if ((sum & 0x80) > 0)
                f = (byte)(f | 0x80);
            if (sum == 0)
                f = (byte)(f | 0x40);
            if ((b & 0x0F) == 0)
                f = (byte)(f | 0x10);
            if (b == 0x80)
                f = (byte)(f | 0x04);
            f = (byte)(f | 0x02);
            registers[F] = f;

            return (byte)sum;
        }

        private static bool Parity(ushort value)
        {
            var parity = true;
            while (value > 0)
            {
                if ((value & 1) == 1) parity = !parity;
                value = (byte)(value >> 1);
            }
            return parity;
        }

        private bool JumpCondition(byte condition)
        {
            Fl mask;
            switch (condition & 0xFE)
            {
                case 0:
                    mask = Fl.Z;
                    break;
                case 2:
                    mask = Fl.C;
                    break;
                case 4:
                    mask = Fl.PV;
                    break;
                case 6:
                    mask = Fl.S;
                    break;
                default:
                    return false;
            }
            return ((registers[F] & (byte)mask) > 0) == ((condition & 1) == 1);

        }

        /// <summary>
        ///     Fetches from [PC] and increments PC
        /// </summary>
        /// <returns></returns>
        private byte Fetch()
        {
            var pc = Pc;
            var ret = mem[pc];
#if (DEBUG)
            LogMemRead(pc, ret);
#endif
            pc++;
            registers[PC] = (byte)(pc >> 8);
            registers[PC + 1] = (byte)(pc & 0xFF);
            return ret;
        }

        private ushort Fetch16()
        {
            return (ushort)(Fetch() + (Fetch() << 8));
        }

        public void Reset()
        {
            Array.Clear(registers, 0, registers.Length);

            registers[A] = 0xFF;
            registers[F] = 0xFF;
            registers[SP] = 0xFF;
            registers[SP + 1] = 0xFF;

            //A CPU reset forces both the IFF1 and IFF2 to the reset state, which disables interrupts
            IFF1 = false;
            IFF2 = false;

            _clock = DateTime.UtcNow;
        }

        public byte[] GetState()
        {
            var length = registers.Length;
            var ret = new byte[length + 2];
            Array.Copy(registers, ret, length);
            ret[length] = (byte)(IFF1 ? 1 : 0);
            ret[length + 1] = (byte)(IFF2 ? 1 : 0);
            return ret;
        }

        public string DumpState()
        {
            var ret = " BC   DE   HL  SZ-H-PNC A" + Environment.NewLine;
            ret +=
                $"{registers[B]:X2}{registers[C]:X2} {registers[D]:X2}{registers[E]:X2} {registers[H]:X2}{registers[L]:X2} {(registers[F] & 0x80) >> 7}{(registers[F] & 0x40) >> 6}{(registers[F] & 0x20) >> 5}{(registers[F] & 0x10) >> 4}{(registers[F] & 0x08) >> 3}{(registers[F] & 0x04) >> 2}{(registers[F] & 0x02) >> 1}{(registers[F] & 0x01)} {registers[A]:X2}";
            ret +=
                $"\n{registers[Bp]:X2}{registers[Cp]:X2} {registers[Dp]:X2}{registers[Ep]:X2} {registers[Hp]:X2}{registers[Lp]:X2} {(registers[Fp] & 0x80) >> 7}{(registers[Fp] & 0x40) >> 6}{(registers[Fp] & 0x20) >> 5}{(registers[Fp] & 0x10) >> 4}{(registers[Fp] & 0x08) >> 3}{(registers[Fp] & 0x04) >> 2}{(registers[Fp] & 0x02) >> 1}{registers[Fp] & 0x01} {registers[Ap]:X2}";
            ret += Environment.NewLine + Environment.NewLine + "I  R   IX   IY   SP   PC" + Environment.NewLine;
            ret +=
                $"{registers[I]:X2} {registers[R]:X2} {registers[IX]:X2}{registers[IX + 1]:X2} {registers[IY]:X2}{registers[IY + 1]:X2} {registers[SP]:X2}{registers[SP + 1]:X2} {registers[PC]:X2}{registers[PC + 1]:X2} ";

            ret += Environment.NewLine;
            return ret;
        }

        private void Wait(int t)
        {
            const int realTicksPerTick = 250; // 4MHz
            var ticks = t * realTicksPerTick;
            var elapsed = (DateTime.UtcNow - _clock).Ticks;
            var sleep = ticks - elapsed;
            if (sleep > 0)
            {
                Thread.Sleep((int)sleep / 1000000);
                _clock = _clock + new TimeSpan(ticks);
            }
            else
            {
#if (DEBUG)
                log +=
                    $"Clock expected {((double)ticks) / realTicksPerTick:0.00} but was {((double)elapsed) / realTicksPerTick:0.00}";
#endif
                _clock = DateTime.UtcNow;
            }
        }

        private void SwapReg8(byte r1, byte r2)
        {
            var t = registers[r1];
            registers[r1] = registers[r2];
            registers[r2] = t;
        }

        [Flags]
        private enum Fl : byte
        {
            C = 0x01,
            N = 0x02,
            PV = 0x04,
            H = 0x10,
            Z = 0x40,
            S = 0x80,

            None = 0x00,
            All = 0xD7
        }

#if (DEBUG)
        private static bool debug_atStart = true;

        private static void LogMemRead(ushort addr, byte val)
        {
            if (debug_atStart)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{addr:X4} ");
                debug_atStart = false;
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{val:X2} ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void Log(string text)
        {
            Console.CursorLeft = 20;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;
            debug_atStart = true;
        }

        private static string RName(byte n)
        {
            switch (n)
            {
                case 0:
                    return "B";
                case 1:
                    return "C";
                case 2:
                    return "D";
                case 3:
                    return "E";
                case 4:
                    return "H";
                case 5:
                    return "L";
                case 7:
                    return "A";
                default:
                    return "";
            }
        }

        private static string R16Name(byte n)
        {
            switch (n)
            {
                case 0x00:
                    return "BC";
                case 0x10:
                    return "DE";
                case 0x20:
                    return "HL";
                case 0x30:
                    return "SP";
            }
            return "";
        }
#endif
    }
}