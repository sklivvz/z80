using System;
using System.Threading;

// ReSharper disable InconsistentNaming

namespace z80
{
    public class Z80
    {
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

        private ushort Hl => (ushort)(registers[L] + (registers[H] << 8));
        private ushort Sp => (ushort)(registers[SP + 1] + (registers[SP] << 8));
        private ushort Ix => (ushort)(registers[IX + 1] + (registers[IX] << 8));
        private ushort Iy => (ushort)(registers[IY + 1] + (registers[IY] << 8));
        private ushort Bc => (ushort)((registers[B] << 8) + registers[C]);
        private ushort De => (ushort)((registers[D] << 8) + registers[E]);


        public Z80(Memory memory)
        {
            mem = memory;
            Reset();
        }

        public bool Halted { get; private set; }

        public void Parse()
        {
            if (Halted) return;
            var mc = Fetch();
            var hi = (byte)(mc >> 6);
            var lo = (byte)(mc & 0x07);
            var r = (byte)((mc >> 3) & 0x07);
            switch (mc)
            {
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
                    return;
                case 0x01:
                case 0x11:
                case 0x21:
                    {
                        // LD dd, nn
                        registers[r + 1] = Fetch();
                        registers[r] = Fetch();
#if(DEBUG)
                        Log("LD {0}{1}, {2:X2}{3:X2}", RName(r), RName((byte)(r + 1)), registers[r], registers[r + 1]);
#endif
                        Wait(10);
                        return;
                    }
                case 0x31:
                    {
                        // LD SP, nn
                        registers[SP + 1] = Fetch();
                        registers[SP] = Fetch();
#if(DEBUG)
                        Log("LD SP, 0x{0:X2}{1:X2}", registers[SP], registers[SP + 1]);
#endif
                        Wait(10);
                        return;
                    }
                case 0x40:
                case 0x41:
                case 0x42:
                case 0x43:
                case 0x44:
                case 0x45:
                case 0x47:
                case 0x48:
                case 0x49:
                case 0x4a:
                case 0x4b:
                case 0x4c:
                case 0x4d:
                case 0x4f:
                case 0x50:
                case 0x51:
                case 0x52:
                case 0x53:
                case 0x54:
                case 0x55:
                case 0x57:
                case 0x58:
                case 0x59:
                case 0x5a:
                case 0x5b:
                case 0x5c:
                case 0x5d:
                case 0x5f:
                case 0x60:
                case 0x61:
                case 0x62:
                case 0x63:
                case 0x64:
                case 0x65:
                case 0x67:
                case 0x68:
                case 0x69:
                case 0x6a:
                case 0x6b:
                case 0x6c:
                case 0x6d:
                case 0x6f:
                case 0x78:
                case 0x79:
                case 0x7a:
                case 0x7b:
                case 0x7c:
                case 0x7d:
                case 0x7f:
                    {
                        // LD r, r'
#if(DEBUG)
                        Log("LD {0}, {1}", RName(r), RName(lo));
#endif
                        registers[r] = registers[lo];
                        Wait(4);
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
#if(DEBUG)
                        Log("LD {0}, 0x{1:X2}", RName(r), n);
#endif
                        Wait(7);
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
                        // LD r, (HL)
                        registers[r] = mem[Hl];
#if(DEBUG)
                        Log("LD {0}, (HL)", RName(r));
#endif
                        Wait(7);
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
                        // LD (HL), r
                        mem[Hl] = registers[lo];
#if(DEBUG)
                        Log("LD (HL), {0}", RName(lo));
#endif
                        Wait(7);
                        return;
                    }
                case 0x36:
                    {
                        // LD (HL), n
                        var n = Fetch();
                        mem[Hl] = n;
#if(DEBUG)
                        Log("LD (HL), {0}", n);
#endif
                        Wait(10);
                        return;
                    }
                case 0x76:
                    //HALT
#if(DEBUG)
                    Log("HALT");
#endif
                    Halted = true;
                    return;
                case 0x0A:
                    {
                        // LD A, (BC)
                        registers[A] = mem[Bc];
#if(DEBUG)
                        Log("LD A, (BC)");
#endif
                        Wait(7);
                        return;
                    }
                case 0x1A:
                    {
                        // LD A, (DE)
                        registers[A] = mem[De];
#if(DEBUG)
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
#if(DEBUG)
                        Log("LD A, (0x{0:X4})", addr);
#endif
                        Wait(13);
                        return;
                    }
                case 0x02:
                    {
                        // LD (BC), A
                        mem[Bc] = registers[A];
#if(DEBUG)
                        Log("LD (BC), A");
#endif
                        Wait(7);
                        return;
                    }
                case 0x12:
                    {
                        // LD (DE), A
                        mem[De] = registers[A];
#if(DEBUG)
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
#if(DEBUG)
                        Log("LD (0x{0:X4}), A", addr);
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
#if(DEBUG)
                        Log("LD HL, (0x{0:X4})", --addr);
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
#if(DEBUG)
                        Log("LD (0x{0:X4}), HL", --addr);
#endif
                        Wait(16);
                        return;
                    }
                case 0xF9:
                    {
                        // LD SP, HL
                        registers[SP + 1] = registers[L];
                        registers[SP] = registers[H];
#if(DEBUG)
                        Log("LD SP, HL");
#endif
                        Wait(6);
                        return;
                    }

                case 0xC5:
                    {
                        // PUSH BC
                        ushort addr = Sp;
                        mem[--addr] = registers[B];
                        mem[--addr] = registers[C];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if(DEBUG)
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
#if(DEBUG)
                        Log("PUSH DE");
#endif
                        Wait(11);
                        return;
                    }
                case 0xE5:
                    {
                        // PUSH HL
                        ushort addr = Sp;
                        mem[--addr] = registers[H];
                        mem[--addr] = registers[L];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if(DEBUG)
                        Log("PUSH HL");
#endif
                        Wait(11);
                        return;
                    }
                case 0xF5:
                    {
                        // PUSH AF
                        ushort addr = Sp;
                        mem[--addr] = registers[A];
                        mem[--addr] = registers[F];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if(DEBUG)
                        Log("PUSH AF");
#endif
                        Wait(11);
                        return;
                    }
                case 0xC1:
                    {
                        // POP BC
                        ushort addr = Sp;
                        registers[C] = mem[addr++];
                        registers[B] = mem[addr++];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if(DEBUG)
                        Log("POP BC");
#endif
                        Wait(10);
                        return;
                    }
                case 0xD1:
                    {
                        // POP DE
                        ushort addr = Sp;
                        registers[E] = mem[addr++];
                        registers[D] = mem[addr++];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if(DEBUG)
                        Log("POP DE");
#endif
                        Wait(10);
                        return;
                    }
                case 0xE1:
                    {
                        // POP HL
                        ushort addr = Sp;
                        registers[L] = mem[addr++];
                        registers[H] = mem[addr++];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if(DEBUG)
                        Log("POP HL");
#endif
                        Wait(10);
                        return;
                    }
                case 0xF1:
                    {
                        // POP AF
                        ushort addr = Sp;
                        registers[F] = mem[addr++];
                        registers[A] = mem[addr++];
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
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

#if(DEBUG)
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
#if(DEBUG)
                        Log("ADD A, {0}", RName(lo));
#endif
                        Wait(4);
                        return;
                    }
                case 0xC6:
                    {
                        // ADD A, n
                        var b = Fetch();
                        Add(b);
#if(DEBUG)
                        Log("ADD A, 0x{0:X2}", b);
#endif
                        Wait(4);
                        Wait(4);
                        return;
                    }
                case 0x86:
                    {
                        // ADD A, (HL)
                        Add(mem[Hl]);
#if(DEBUG)
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
#if(DEBUG)
                        Log("ADC A, {0}", RName(lo));
#endif
                        Wait(4);
                        return;
                    }
                case 0xCE:
                    {
                        // ADC A, n
                        var b = Fetch();
                        Adc(b);
#if(DEBUG)
                        Log("ADC A, 0x{0:X2}", b);
#endif
                        Wait(4);
                        return;
                    }
                case 0x8E:
                    {
                        // ADC A, (HL)
                        Adc(mem[Hl]);
#if(DEBUG)
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
#if(DEBUG)
                        Log("SUB A, {0}", RName(lo));
#endif
                        Wait(4);
                        return;
                    }
                case 0xD6:
                    {
                        // SUB A, n
                        var b = Fetch();
                        Sub(b);
#if(DEBUG)
                        Log("SUB A, 0x{0:X2}", b);
#endif
                        Wait(4);
                        return;
                    }
                case 0x96:
                    {
                        // SUB A, (HL)
                        Sub(mem[Hl]);
#if(DEBUG)
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
#if(DEBUG)
                        Log("SBC A, {0}", RName(lo));
#endif
                        Wait(4);
                        return;
                    }
                case 0xDE:
                    {
                        // SBC A, n
                        var b = Fetch();
                        Sbc(b);
#if(DEBUG)
                        Log("SBC A, 0x{0:X2}", b);
#endif
                        Wait(4);
                        return;
                    }
                case 0x9E:
                    {
                        // SBC A, (HL)
                        Sbc(mem[Hl]);
#if(DEBUG)
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
#if(DEBUG)
                        Log("AND A, {0}", RName(lo));
#endif
                        Wait(4);
                        return;
                    }
                case 0xE6:
                    {
                        // AND A, n
                        var b = Fetch();

                        And(b);
#if(DEBUG)
                        Log("AND A, 0x{0:X2}", b);
#endif
                        Wait(4);
                        return;
                    }
                case 0xA6:
                    {
                        // AND A, (HL)
                        And(mem[Hl]);
#if(DEBUG)
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
#if(DEBUG)
                        Log("OR A, {0}", RName(lo));
#endif
                        Wait(4);
                        return;
                    }
                case 0xF6:
                    {
                        // OR A, n
                        var b = Fetch();
                        Or(b);
#if(DEBUG)
                        Log("OR A, 0x{0:X2}", b);
#endif
                        Wait(4);
                        return;
                    }
                case 0xB6:
                    {
                        // OR A, (HL)
                        Or(mem[Hl]);
#if(DEBUG)
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
#if(DEBUG)
                        Log("XOR A, {0}", RName(lo));
#endif
                        Wait(4);
                        return;
                    }
                case 0xEE:
                    {
                        // XOR A, n
                        var b = Fetch();
                        Xor(b);
#if(DEBUG)
                        Log("XOR A, 0x{0:X2}", b);
#endif
                        Wait(4);
                        return;
                    }
                case 0xAE:
                    {
                        // XOR A, (HL)
                        Xor(mem[Hl]);
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
                        Log("CP A, {0}", RName(lo));
#endif
                        Wait(4);
                        return;
                    }
                case 0xFE:
                    {
                        // CP A, n
                        var b = Fetch();
                        Cmp(b);
#if(DEBUG)
                        Log("CP A, 0x{0:X2}", b);
#endif
                        Wait(4);
                        return;
                    }
                case 0xBE:
                    {
                        // CP A, (HL)
                        Cmp(mem[Hl]);
#if(DEBUG)
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
#if(DEBUG)
                        Log("INC {0}", RName(r));
#endif
                        Wait(4);
                        return;
                    }
                case 0x34:
                    {
                        // INC (HL)
                        mem[Hl] = Inc(mem[Hl]);
#if(DEBUG)
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
#if(DEBUG)
                        Log("DEC {0}", RName(r));
#endif
                        Wait(7);
                        return;
                    }
                case 0x35:
                    {
                        // DEC (HL)
                        mem[Hl] = Dec(mem[Hl]);
#if(DEBUG)
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
            }

#if(DEBUG)
            Log("{3:X2}: {0:X2} {1:X2} {2:X2}", hi, lo, r, mc);
            //throw new InvalidOperationException("Invalid Opcode: "+mc.ToString("X2"));
#endif
            Halted = true;
        }

        private void ParseED()
        {
            if (Halted) return;
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
#if(DEBUG)
                        Log("LD I, A");
#endif
                        Wait(9);
                        return;
                    }
                case 0x4F:
                    {
                        // LD R, A
                        registers[R] = registers[A];
#if(DEBUG)
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
                            f = (byte)(f | (byte)Fl.S);
                        }
                        else if (i == 0x00)
                        {
                            f = (byte)(f | (byte)Fl.Z);
                        }
                        if (IFF2)
                        {
                            f = (byte)(f | (byte)Fl.PV);
                        }
                        registers[F] = f;
#if(DEBUG)
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
                            f = (byte)(f | (byte)Fl.S);
                        }
                        else if (reg == 0x00)
                        {
                            f = (byte)(f | (byte)Fl.Z);
                        }
                        if (IFF2)
                        {
                            f = (byte)(f | (byte)Fl.PV);
                        }
                        registers[F] = f;
#if(DEBUG)
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
#if(DEBUG)
                        Log("LD BC, (0x{0:X4})", --addr);
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
#if(DEBUG)
                        Log("LD DE, (0x{0:X4})", --addr);
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
#if(DEBUG)
                        Log("LD HL, (0x{0:X4})*", --addr);
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
#if(DEBUG)
                        Log("LD SP, (0x{0:X4})", --addr);
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
#if(DEBUG)
                        Log("LD (0x{0:X4}), BC", --addr);
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
#if(DEBUG)
                        Log("LD (0x{0:X4}), DE", --addr);
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
#if(DEBUG)
                        Log("LD (0x{0:X4}), HL", --addr);
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
#if(DEBUG)
                        Log("LD (0x{0:X4}), SP", --addr);
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
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
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
                    {
                        // CPL
                        var a = -registers[A];
                        registers[A] = (byte)a;
                        registers[F] |= (byte)(Fl.H | Fl.N);

#if (DEBUG)
                        Log("CPL");
#endif
                        Wait(4);
                        return;

                    }
            }
#if(DEBUG)
            Log("ED {3:X2}: {0:X2} {1:X2} {2:X2}", hi, lo, r, mc);
#endif
            Halted = true;
        }

        private void ParseDD()
        {
            if (Halted) return;
            var mc = Fetch();
            var hi = (byte)(mc >> 6);
            var lo = (byte)(mc & 0x07);
            var r = (byte)((mc >> 3) & 0x07);

            switch (mc)
            {
                case 0x21:
                    {
                        // LD IX, nn
                        registers[IX + 1] = Fetch();
                        registers[IX] = Fetch();
#if(DEBUG)
                        Log("LD IX, 0x{0:X4}", Ix);
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
                        registers[r] = mem[(ushort)(Ix + d)];
#if(DEBUG)
                        Log("LD {0}, (IX{1:+0;-#})", RName(r), d);
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
#if(DEBUG)
                        Log("LD (IX{1:+0;-#}), {0}", RName(lo), d);
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
#if(DEBUG)
                        Log("LD (IX{1:+0;-#}), {0}", n, d);
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
#if(DEBUG)
                        Log("LD IX, (0x{0:X4})*", addr);
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
#if(DEBUG)
                        Log("LD (0x{0:X4}), IX", addr);
#endif
                        Wait(20);
                        return;
                    }

                case 0xF9:
                    {
                        // LD SP, IX
                        registers[SP] = registers[IX];
                        registers[SP + 1] = registers[IX + 1];
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
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
                        ushort addr = Sp;
                        registers[IX + 1] = mem[addr++];
                        registers[IX] = mem[addr];
                        mem[addr--] = h;
                        mem[addr] = l;

#if(DEBUG)
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
#if(DEBUG)
                        Log("ADD A, (IX{0:+0;-#})", d);
#endif
                        Wait(19);
                        return;
                    }
                case 0x8E:
                    {
                        // ADC A, (IX+d)
                        var d = (sbyte)Fetch();
                        byte a = registers[A];
                        Adc(mem[(ushort)(Ix + d)]);
#if(DEBUG)
                        Log("ADC A, (IX{0:+0;-#})", d);
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
#if(DEBUG)
                        Log("SUB A, (IX{0:+0;-#})", d);
#endif
                        Wait(19);
                        return;
                    }
                case 0x9E:
                    {
                        // SBC A, (IX+d)
                        var d = (sbyte)Fetch();

                        Sbc(mem[(ushort)(Ix + d)]);
#if(DEBUG)
                        Log("SBC A, (IX{0:+0;-#})", d);
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
#if(DEBUG)
                        Log("AND A, (IX{0:+0;-#})", d);
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
#if(DEBUG)
                        Log("OR A, (IX{0:+0;-#})", d);
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
#if(DEBUG)
                        Log("XOR A, (IX{0:+0;-#})", d);
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
#if(DEBUG)
                        Log("CP A, (IX{0:+0;-#})", d);
#endif
                        Wait(19);
                        return;
                    }
                case 0x34:
                    {
                        // INC (IX+d)
                        var d = (sbyte)Fetch();
                        mem[(ushort)(Ix + d)] = Inc(mem[(ushort)(Ix + d)]);
#if(DEBUG)
                        Log("INC (IX{0:+0;-#})", d);
#endif
                        Wait(7);
                        return;
                    }
                case 0x35:
                    {
                        // DEC (IX+d)
                        var d = (sbyte)Fetch();
                        mem[(ushort)(Ix + d)] = Dec(mem[(ushort)(Ix + d)]);
#if(DEBUG)
                        Log("DEC (IX{0:+0;-#})", d);
#endif
                        Wait(7);
                        return;
                    }

            }
#if(DEBUG)
            Log("DD {3:X2}: {0:X2} {1:X2} {2:X2}", hi, lo, r, mc);
#endif
            Halted = true;
        }

        private void ParseFD()
        {
            if (Halted) return;
            var mc = Fetch();
            var hi = (byte)(mc >> 6);
            var lo = (byte)(mc & 0x07);
            var r = (byte)((mc >> 3) & 0x07);

            switch (mc)
            {
                case 0x21:
                    {
                        // LD IY, nn
                        registers[IY + 1] = Fetch();
                        registers[IY] = Fetch();
#if(DEBUG)
                        Log("LD IY, 0x{0:X4}", Iy);
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
#if(DEBUG)
                        Log("LD {0}, (IY{1:+0;-#})", RName(r), d);
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
#if(DEBUG)
                        Log("LD (IY{1:+0;-#}), {0}", RName(lo), d);
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
#if(DEBUG)
                        Log("LD (IY{1:+0;-#}), {0}", n, d);
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
#if(DEBUG)
                        Log("LD IY, (0x{0:X4})*", --addr);
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
#if(DEBUG)
                        Log("LD (0x{0:X4}), IY", --addr);
#endif
                        Wait(20);
                        return;
                    }
                case 0xF9:
                    {
                        // LD SP, IY
                        registers[SP] = registers[IY];
                        registers[SP + 1] = registers[IY + 1];
#if(DEBUG)
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
#if(DEBUG)
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
#if(DEBUG)
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
                        ushort addr = Sp;
                        registers[IY + 1] = mem[addr];
                        mem[addr++] = l;
                        registers[IY] = mem[addr];
                        mem[addr] = h;

#if(DEBUG)
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
#if(DEBUG)
                        Log("ADD A, (IY{0:+0;-#})", d);
#endif
                        Wait(19);
                        return;
                    }
                case 0x8E:
                    {
                        // ADC A, (IY+d)
                        var d = (sbyte)Fetch();
                        byte a = registers[A];
                        Adc(mem[(ushort)(Iy + d)]);

#if(DEBUG)
                        Log("ADC A, (IY{0:+0;-#})", d);
#endif
                        Wait(19);
                        return;
                    }
                case 0x96:
                    {
                        // SUB A, (IY+d)
                        var d = (sbyte)Fetch();

                        Sub(mem[(ushort)(Iy + d)]);
#if(DEBUG)
                        Log("SUB A, (IY{0:+0;-#})", d);
#endif
                        Wait(19);
                        return;
                    }
                case 0x9E:
                    {
                        // SBC A, (IY+d)
                        var d = (sbyte)Fetch();

                        Sbc(mem[(ushort)(Iy + d)]);
#if(DEBUG)
                        Log("SBC A, (IY{0:+0;-#})", d);
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
#if(DEBUG)
                        Log("AND A, (IY{0:+0;-#})", d);
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
#if(DEBUG)
                        Log("OR A, (IY{0:+0;-#})", d);
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
#if(DEBUG)
                        Log("XOR A, (IY{0:+0;-#})", d);
#endif
                        Wait(19);
                        return;
                    }
                case 0xBE:
                    {
                        // CP A, (IY+d)
                        var d = (sbyte)Fetch();

                        Cmp(mem[(ushort)(Iy + d)]);
#if(DEBUG)
                        Log("CP A, (IY{0:+0;-#})", d);
#endif
                        Wait(19);
                        return;
                    }
                case 0x34:
                    {
                        // INC (IY+d)
                        var d = (sbyte)Fetch();
                        mem[(ushort)(Iy + d)] = Inc(mem[(ushort)(Iy + d)]);
#if(DEBUG)
                        Log("INC (IY{0:+0;-#})", d);
#endif
                        Wait(7);
                        return;
                    }
                case 0x35:
                    {
                        // DEC (IY+d)
                        var d = (sbyte)Fetch();
                        mem[(ushort)(Iy + d)] = Dec(mem[(ushort)(Iy + d)]);
#if(DEBUG)
                        Log("DEC (IY{0:+0;-#})", d);
#endif
                        Wait(7);
                        return;
                    }
            }
#if(DEBUG)
            Log("FD {3:X2}: 0x{0:X2} {1:X2} {2:X2}", hi, lo, r, mc);
#endif
            Halted = true;
        }

        private void Add(byte b)
        {
            var a = registers[A];
            var sum = a + b;
            registers[A] = (byte)sum;
            var f = (byte)(registers[F] & 0x28);
            if ((sum & 0x80) > 0) f = (byte)(f | (byte)Fl.S);
            if ((byte)sum == 0) f = (byte)(f | (byte)Fl.Z);
            if ((a & 0xF + b & 0xF) > 0xF) f = (byte)(f | (byte)Fl.H);
            if ((a > 0x80 && b > 0x80 && (sbyte)sum > 0) || (a < 0x80 && b < 0x80 && (sbyte)sum < 0)) f = (byte)(f | (byte)Fl.PV);
            if (sum > 0xFF) f = (byte)(f | (byte)Fl.C);
            registers[F] = f;
        }

        private void Adc(byte b)
        {
            var a = registers[A];
            var c = (byte)(registers[F] & (byte)Fl.C);
            var sum = a + b + c;
            registers[A] = (byte)sum;
            var f = (byte)(registers[F] & 0x28);
            if ((sum & 0x80) > 0) f = (byte)(f | (byte)Fl.S);
            if ((byte)sum == 0) f = (byte)(f | (byte)Fl.Z);
            if ((a & 0xF + b & 0xF) > 0xF) f = (byte)(f | (byte)Fl.H);
            if ((a > 0x80 && b > 0x80 && (sbyte)sum > 0) || (a < 0x80 && b < 0x80 && (sbyte)sum < 0)) f = (byte)(f | (byte)Fl.PV);
            f = (byte)(f & ~(byte)Fl.N);
            if (sum > 0xFF) f = (byte)(f | (byte)Fl.C);
            registers[F] = f;
        }

        private void Sub(byte b)
        {
            var a = registers[A];
            var diff = a - b;
            registers[A] = (byte)diff;
            var f = (byte)(registers[F] & 0x28);
            if ((diff & 0x80) > 0) f = (byte)(f | 0x80);
            if (diff == 0) f = (byte)(f | 0x40);
            if ((a & 0xF) < (b & 0xF)) f = (byte)(f | 0x10);
            if ((a > 0x80 && b > 0x80 && (sbyte)diff > 0) || (a < 0x80 && b < 0x80 && (sbyte)diff < 0)) f = (byte)(f | 0x04);
            f = (byte)(f | 0x02);
            if (diff > 0xFF) f = (byte)(f | 0x01);
            registers[F] = f;
        }

        private void Sbc(byte b)
        {
            var a = registers[A];
            var c = (byte)(registers[F] & 0x01);
            var diff = a - b - c;
            registers[A] = (byte)diff;
            var f = (byte)(registers[F] & 0x28);
            if ((diff & 0x80) > 0) f = (byte)(f | 0x80);
            if (diff == 0) f = (byte)(f | 0x40);
            if ((a & 0xF) < (b & 0xF) + c) f = (byte)(f | 0x10);
            if ((a > 0x80 && b > 0x80 && (sbyte)diff > 0) || (a < 0x80 && b < 0x80 && (sbyte)diff < 0)) f = (byte)(f | 0x04);
            f = (byte)(f | 0x02);
            if (diff > 0xFF) f = (byte)(f | 0x01);
            registers[F] = f;
        }

        private void And(byte b)
        {
            var a = registers[A];
            var res = (byte)(a & b);
            registers[A] = res;
            var f = (byte)(registers[F] & 0x28);
            if ((res & 0x80) > 0) f = (byte)(f | 0x80);
            if (res == 0) f = (byte)(f | 0x40);
            f = (byte)(f | 0x10);
            if (Parity(res)) f = (byte)(f | 0x04);
            registers[F] = f;
        }

        private void Or(byte b)
        {
            var a = registers[A];
            var res = (byte)(a | b);
            registers[A] = res;
            var f = (byte)(registers[F] & 0x28);
            if ((res & 0x80) > 0) f = (byte)(f | 0x80);
            if (res == 0) f = (byte)(f | 0x40);
            if (Parity(res)) f = (byte)(f | 0x04);
            registers[F] = f;
        }

        private void Xor(byte b)
        {
            var a = registers[A];
            var res = (byte)(a ^ b);
            registers[A] = res;
            var f = (byte)(registers[F] & 0x28);
            if ((res & 0x80) > 0) f = (byte)(f | 0x80);
            if (res == 0) f = (byte)(f | 0x40);
            if (Parity(res)) f = (byte)(f | 0x04);
            registers[F] = f;
        }

        private void Cmp(byte b)
        {
            var a = registers[A];
            var diff = a - b;
            var f = (byte)(registers[F] & 0x28);
            if ((diff & 0x80) > 0) f = (byte)(f | 0x80);
            if (diff == 0) f = (byte)(f | 0x40);
            if ((a & 0xF) < (b & 0xF)) f = (byte)(f | 0x10);
            if ((a > 0x80 && b > 0x80 && (sbyte)diff > 0) || (a < 0x80 && b < 0x80 && (sbyte)diff < 0)) f = (byte)(f | 0x04);
            f = (byte)(f | 0x02);
            if (diff > 0xFF) f = (byte)(f | 0x01);
            registers[F] = f;
        }

        private byte Inc(byte b)
        {
            var sum = b + 1;
            var f = (byte)(registers[F] & 0x28);
            if ((sum & 0x80) > 0) f = (byte)(f | 0x80);
            if (sum == 0) f = (byte)(f | 0x40);
            if ((b & 0xF) == 0xF) f = (byte)(f | 0x10);
            if ((b < 0x80 && (sbyte)sum < 0)) f = (byte)(f | 0x04);
            f = (byte)(f | 0x02);
            if (sum > 0xFF) f = (byte)(f | 0x01);
            registers[F] = f;

            return (byte)sum;
        }

        private byte Dec(byte b)
        {
            var sum = b - 1;
            var f = (byte)(registers[F] & 0x28);
            if ((sum & 0x80) > 0) f = (byte)(f | 0x80);
            if (sum == 0) f = (byte)(f | 0x40);
            if ((b & 0x0F) == 0) f = (byte)(f | 0x10);
            if (b == 0x80) f = (byte)(f | 0x04);
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


        /// <summary>
        ///     Fetches from [PC] and increments PC
        /// </summary>
        /// <returns></returns>
        private byte Fetch()
        {
            var pc = (ushort)((registers[PC] << 8) + registers[PC + 1]);
            var ret = mem[pc];
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
            ret += string.Format("{0:X2}{1:X2} {2:X2}{3:X2} {4:X2}{5:X2} {6}{7}{8}{9}{10}{11}{12}{13} {14:X2}",
                registers[B],
                registers[C],
                registers[D],
                registers[E],
                registers[H],
                registers[L],
                (registers[F] & 0x80) >> 7,
                (registers[F] & 0x40) >> 6,
                (registers[F] & 0x20) >> 5,
                (registers[F] & 0x10) >> 4,
                (registers[F] & 0x08) >> 3,
                (registers[F] & 0x04) >> 2,
                (registers[F] & 0x02) >> 1,
                (registers[F] & 0x01),
                registers[A]
            );
            ret += string.Format("\n{0:X2}{1:X2} {2:X2}{3:X2} {4:X2}{5:X2} {6}{7}{8}{9}{10}{11}{12}{13} {14:X2}",
                registers[Bp],
                registers[Cp],
                registers[Dp],
                registers[Ep],
                registers[Hp],
                registers[Lp],
                (registers[Fp] & 0x80) >> 7,
                (registers[Fp] & 0x40) >> 6,
                (registers[Fp] & 0x20) >> 5,
                (registers[Fp] & 0x10) >> 4,
                (registers[Fp] & 0x08) >> 3,
                (registers[Fp] & 0x04) >> 2,
                (registers[Fp] & 0x02) >> 1,
                registers[Fp] & 0x01,
                registers[Ap]
            );
            ret += Environment.NewLine + Environment.NewLine + "I  R   IX   IY   SP   PC" + Environment.NewLine;
            ret += string.Format("{0:X2} {1:X2} {2:X2}{3:X2} {4:X2}{5:X2} {6:X2}{7:X2} {8:X2}{9:X2} ",
                registers[I],
                registers[R],
                registers[IX],
                registers[IX + 1],
                registers[IY],
                registers[IY + 1],
                registers[SP],
                registers[SP + 1],
                registers[PC],
                registers[PC + 1]
            );

            ret += Environment.NewLine;
            return ret;
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
#if(DEBUG)
                Log("Clock expected {0:0.00} but was {1:0.00}", ((double)ticks) / realTicksPerTick,
                    ((double)elapsed) / realTicksPerTick);
#endif
                _clock = DateTime.UtcNow;
            }
        }

#if(DEBUG)
        private static void Log(string text)
        {
            Console.WriteLine(text);
        }
        private static void Log(string format, object o1)
        {
            Console.WriteLine(format, o1);
        }
        private static void Log(string format, object o1, object o2)
        {
            Console.WriteLine(format, o1, o2);
        }
        private static void Log(string format, object o1, object o2, object o3)
        {
            Console.WriteLine(format, o1, o2, o3);
        }
        private static void Log(string format, params object[] vals)
        {
            Console.WriteLine(format, vals);
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
                case 6:
                    return "F";
                case 7:
                    return "A";
            }
            return "";
        }
#endif

        private void SwapReg8(byte r1, byte r2)
        {
            var t = registers[r1];
            registers[r1] = registers[r2];
            registers[r2] = t;
        }
    }
}