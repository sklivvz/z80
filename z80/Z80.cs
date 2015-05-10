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
        private readonly byte[] _ram;
        private readonly byte[] registers = new byte[26];
        private DateTime _clock = DateTime.UtcNow;
        private bool IFF1;
        private bool IFF2;

        public Z80(byte[] ram)
        {
            _ram = ram;
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
                        Log("LD {0}{1}, {2}{3}", RName(r), RName((byte)(r + 1)), registers[r], registers[r + 1]);
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
                        Log("LD SP, {0}{1}", registers[r], registers[r + 1]);
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
                        Log("LD {0}, {1}", RName(r), n, mc);
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
#if(DEBUG)
                        Log("LD {0}, (HL)", RName(r));
#endif
                        var addr = (registers[H] << 8) + registers[L];
                        registers[r] = _ram[addr];
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
#if(DEBUG)
                        Log("LD (HL), {0}", RName(lo));
#endif
                        var addr = (registers[H] << 8) + registers[L];
                        _ram[addr] = registers[lo];
                        Wait(7);
                        return;
                    }
                case 0x36:
                    {
                        // LD (HL), n
                        var n = Fetch();
#if(DEBUG)
                        Log("LD (HL), {0}", n);
#endif
                        var addr = (registers[H] << 8) + registers[L];
                        _ram[addr] = n;
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
                        var addr = (registers[B] << 8) + registers[C];
                        registers[A] = _ram[addr];
#if(DEBUG)
                        Log("LD A, (BC)");
#endif
                        Wait(7);
                        return;
                    }
                case 0x1A:
                    {
                        // LD A, (DE)
                        var addr = (registers[D] << 8) + registers[E];
                        registers[A] = _ram[addr];
#if(DEBUG)
                        Log("LD A, (DE)");
#endif
                        Wait(7);
                        return;
                    }
                case 0x3A:
                    {
                        // LD A, (nn)
                        var addr = (Fetch() << 8) + Fetch();
                        registers[A] = _ram[addr];
#if(DEBUG)
                        Log("LD A, ({0:x4})", addr);
#endif
                        Wait(13);
                        return;
                    }
                case 0x02:
                    {
                        // LD (BC), A
                        var addr = (registers[B] << 8) + registers[C];
                        _ram[addr] = registers[A];
#if(DEBUG)
                        Log("LD (BC), A");
#endif
                        Wait(7);
                        return;
                    }
                case 0x12:
                    {
                        // LD (DE), A
                        var addr = (registers[D] << 8) + registers[E];
                        _ram[addr] = registers[A];
#if(DEBUG)
                        Log("LD (DE), A");
#endif
                        Wait(7);
                        return;
                    }
                case 0x32:
                    {
                        // LD (nn), A 
                        var addr = Fetch() + (Fetch() << 8);
                        _ram[addr] = registers[A];
#if(DEBUG)
                        Log("LD ({0:x4}),A", addr);
#endif
                        Wait(13);
                        return;
                    }
                case 0x2A:
                    {
                        // LD HL, (nn) 
                        var addr = Fetch() + (Fetch() << 8);
                        registers[H] = _ram[addr + 1];
                        registers[L] = _ram[addr];
#if(DEBUG)
                        Log("LD HL, ({0:x4})", addr);
#endif
                        Wait(16);
                        return;
                    }
                case 0x22:
                    {
                        // LD (nn), HL
                        var addr = Fetch() + (Fetch() << 8);
                        _ram[addr] = registers[L];
                        _ram[addr + 1] = registers[H];
#if(DEBUG)
                        Log("LD ({0:x4}), HL", addr);
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        addr--;
                        _ram[addr] = registers[B];
                        addr--;
                        _ram[addr] = registers[C];
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        addr--;
                        _ram[addr] = registers[D];
                        addr--;
                        _ram[addr] = registers[E];
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        addr--;
                        _ram[addr] = registers[H];
                        addr--;
                        _ram[addr] = registers[L];
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        addr--;
                        _ram[addr] = registers[A];
                        addr--;
                        _ram[addr] = registers[F];
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        registers[C] = _ram[addr];
                        addr++;
                        registers[B] = _ram[addr];
                        addr++;
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        registers[E] = _ram[addr];
                        addr++;
                        registers[D] = _ram[addr];
                        addr++;
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        registers[L] = _ram[addr];
                        addr++;
                        registers[H] = _ram[addr];
                        addr++;
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        registers[F] = _ram[addr];
                        addr++;
                        registers[A] = _ram[addr];
                        addr++;
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
                        byte reg = registers[D];
                        registers[D] = registers[H];
                        registers[H] = reg;
                        reg = registers[E];
                        registers[E] = registers[L];
                        registers[L] = reg;
#if(DEBUG)
                        Log("EX DE, HL");
#endif
                        Wait(4);
                        return;
                    }
                case 0x08:
                    {
                        // EX AF, AF'
                        byte reg = registers[Ap];
                        registers[Ap] = registers[A];
                        registers[A] = reg;
                        reg = registers[F];
                        registers[F] = registers[Fp];
                        registers[Fp] = reg;
#if(DEBUG)
                        Log("EX AF, AF'");
#endif
                        Wait(4);
                        return;
                    }
            }

#if(DEBUG)
            Log("{3:X2}: {0:X2} {1:X2} {2:X2}", hi, lo, r, mc);
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
                        var f = (byte)(registers[F] & (~(byte)(Fl.H | Fl.PV | Fl.N)));
                        if (i >= 0x80)
                        {
                            f = (byte)(f & (byte)Fl.S);
                        }
                        else if (i == 0x00)
                        {
                            f = (byte)(f & (byte)Fl.Z);
                        }
                        if (IFF2)
                        {
                            f = (byte)(f & (byte)Fl.PV);
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
                        var f = (byte)(registers[F] & (~(byte)(Fl.H | Fl.PV | Fl.N)));
                        if (reg >= 0x80)
                        {
                            f = (byte)(f & (byte)Fl.S);
                        }
                        else if (reg == 0x00)
                        {
                            f = (byte)(f & (byte)Fl.Z);
                        }
                        if (IFF2)
                        {
                            f = (byte)(f & (byte)Fl.PV);
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
                        var addr = Fetch() + (Fetch() << 8);
                        registers[B] = _ram[addr + 1];
                        registers[C] = _ram[addr];
#if(DEBUG)
                        Log("LD BC, ({0:x4})", addr);
#endif
                        Wait(20);
                        return;

                    }
                case 0x5B:
                    {
                        // LD DE, (nn)
                        var addr = Fetch() + (Fetch() << 8);
                        registers[D] = _ram[addr + 1];
                        registers[E] = _ram[addr];
#if(DEBUG)
                        Log("LD DE, ({0:x4})", addr);
#endif
                        Wait(20);
                        return;

                    }
                case 0x6B:
                    {
                        // LD HL, (nn)
                        var addr = Fetch() + (Fetch() << 8);
                        registers[H] = _ram[addr + 1];
                        registers[L] = _ram[addr];
#if(DEBUG)
                        Log("LD HL, ({0:x4})*", addr);
#endif
                        Wait(20);
                        return;

                    }
                case 0x7B:
                    {
                        // LD SP, (nn)
                        var addr = Fetch() + (Fetch() << 8);
                        registers[SP] = _ram[addr + 1];
                        registers[SP + 1] = _ram[addr];
#if(DEBUG)
                        Log("LD SP, ({0:x4})", addr);
#endif
                        Wait(20);
                        return;

                    }
                case 0x43:
                    {
                        // LD (nn), BC
                        var addr = Fetch() + (Fetch() << 8);
                        _ram[addr + 1] = registers[B];
                        _ram[addr] = registers[C];
#if(DEBUG)
                        Log("LD ({0:x4}), BC", addr);
#endif
                        Wait(20);
                        return;

                    }
                case 0x53:
                    {
                        // LD (nn), DE
                        var addr = Fetch() + (Fetch() << 8);
                        _ram[addr + 1] = registers[D];
                        _ram[addr] = registers[E];
#if(DEBUG)
                        Log("LD ({0:x4}), DE", addr);
#endif
                        Wait(20);
                        return;

                    }
                case 0x63:
                    {
                        // LD (nn), HL
                        var addr = Fetch() + (Fetch() << 8);
                        _ram[addr + 1] = registers[H];
                        _ram[addr] = registers[L];
#if(DEBUG)
                        Log("LD ({0:x4})*, HL", addr);
#endif
                        Wait(20);
                        return;

                    }
                case 0x73:
                    {
                        // LD (nn), SP
                        var addr = Fetch() + (Fetch() << 8);
                        _ram[addr + 1] = registers[SP];
                        _ram[addr] = registers[SP + 1];
#if(DEBUG)
                        Log("LD ({0:x4}), SP", addr);
#endif
                        Wait(20);
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
                        Log("LD IX, {0:X4}", registers[IX] * 8 + registers[IX + 1]);
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
#if(DEBUG)
                        Log("LD {0}, (IX{1:+0;-#})", RName(r), d);
#endif
                        var addr = (registers[IX] << 8) + registers[IX + 1] + d;
                        registers[r] = _ram[addr];
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
#if(DEBUG)
                        Log("LD (IX{1:+0;-#}), {0}", RName(r), d);
#endif
                        var addr = (registers[IX] << 8) + registers[IX + 1] + d;
                        _ram[addr] = registers[r];
                        Wait(19);
                        return;
                    }
                case 0x36:
                    {
                        // LD (IX+d), n
                        var d = (sbyte)Fetch();
                        var n = Fetch();
#if(DEBUG)
                        Log("LD (IX{1:+0;-#}), {0}", n, d);
#endif
                        var addr = (registers[IX] << 8) + registers[IX + 1] + d;
                        _ram[addr] = n;
                        Wait(19);
                        return;
                    }
                case 0x2A:
                    {
                        // LD IX, (nn)
                        var addr = Fetch() + (Fetch() << 8);
                        registers[IX] = _ram[addr + 1];
                        registers[IX + 1] = _ram[addr];
#if(DEBUG)
                        Log("LD IX, ({0:x4})*", addr);
#endif
                        Wait(20);
                        return;

                    }
                case 0x22:
                    {
                        // LD (nn), IX
                        var addr = Fetch() + (Fetch() << 8);
                        _ram[addr] = registers[IX + 1];
                        _ram[addr + 1] = registers[IX];
#if(DEBUG)
                        Log("LD ({0:x4}), IX", addr);
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        addr--;
                        _ram[addr] = registers[IX];
                        addr--;
                        _ram[addr] = registers[IX + 1];
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        registers[IX + 1] = _ram[addr];
                        addr++;
                        registers[IX] = _ram[addr];
                        addr++;
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if(DEBUG)
                        Log("POP IX");
#endif
                        Wait(14);
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
                        Log("LD IY, {0:X4}", registers[IY] * 8 + registers[IY + 1]);
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
#if(DEBUG)
                        Log("LD {0}, (IY{1:+0;-#})", RName(r), d);
#endif
                        var addr = (registers[IY] << 8) + registers[IY + 1] + d;
                        registers[r] = _ram[addr];
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
#if(DEBUG)
                        Log("LD (IY{1:+0;-#}), {0}", RName(r), d);
#endif
                        var addr = (registers[IY] << 8) + registers[IY + 1] + d;
                        _ram[addr] = registers[r];
                        Wait(19);
                        return;
                    }
                case 0x36:
                    {
                        // LD (IY+d), n
                        var d = (sbyte)Fetch();
                        var n = Fetch();
#if(DEBUG)
                        Log("LD (IY{1:+0;-#}), {0}", n, d);
#endif
                        var addr = (registers[IY] << 8) + registers[IY + 1] + d;
                        _ram[addr] = n;
                        Wait(19);
                        return;
                    }
                case 0x2A:
                    {
                        // LD IY, (nn)
                        var addr = Fetch() + (Fetch() << 8);
                        registers[IY] = _ram[addr + 1];
                        registers[IY + 1] = _ram[addr];
#if(DEBUG)
                        Log("LD IY, ({0:x4})*", addr);
#endif
                        Wait(20);
                        return;

                    }

                case 0x22:
                    {
                        // LD (nn), IY
                        var addr = Fetch() + (Fetch() << 8);
                        _ram[addr] = registers[IY + 1];
                        _ram[addr + 1] = registers[IY];
#if(DEBUG)
                        Log("LD ({0:x4}), IY", addr);
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        addr--;
                        _ram[addr] = registers[IY];
                        addr--;
                        _ram[addr] = registers[IY + 1];
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
                        var addr = registers[SP + 1] + (registers[SP] << 8);
                        registers[IY + 1] = _ram[addr];
                        addr++;
                        registers[IY] = _ram[addr];
                        addr++;
                        registers[SP + 1] = (byte)(addr & 0xFF);
                        registers[SP] = (byte)(addr >> 8);
#if(DEBUG)
                        Log("POP IY");
#endif
                        Wait(14);
                        return;
                    }
            }
#if(DEBUG)
            Log("FD {3:X2}: {0:X2} {1:X2} {2:X2}", hi, lo, r, mc);
#endif
            Halted = true;
        }

        /// <summary>
        ///     Fetches from [PC] and increments PC
        /// </summary>
        /// <returns></returns>
        private byte Fetch()
        {
            var pc = (ushort)((registers[PC] << 8) + registers[PC + 1]);
            var ret = _ram[pc];
            pc++;
            registers[PC] = (byte)(pc >> 8);
            registers[PC + 1] = (byte)(pc & 0xFF);
            return ret;
        }

        public void Reset()
        {
            Array.Clear(registers, 0, registers.Length);

            //A CPU reset forces both the IFF1 and IFF2 to the reset state, which disables interrupts
            IFF1 = false;
            IFF2 = false;

            _clock = DateTime.UtcNow;
        }

        public byte[] GetState()
        {
            var length = registers.Length;
            var ret = new byte[length];
            Array.Copy(registers, ret, length);
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
                registers[F] & 0x80 >> 7,
                registers[F] & 0x40 >> 6,
                registers[F] & 0x20 >> 5,
                registers[F] & 0x10 >> 4,
                registers[F] & 0x08 >> 3,
                registers[F] & 0x03 >> 2,
                registers[F] & 0x02 >> 1,
                registers[F] & 0x01,
                registers[A]
            );
            ret += string.Format("\n{0:X2}{1:X2} {2:X2}{3:X2} {4:X2}{5:X2} {6}{7}{8}{9}{10}{11}{12}{13} {14:X2}",
                registers[Bp],
                registers[Cp],
                registers[Dp],
                registers[Ep],
                registers[Hp],
                registers[Lp],
                registers[Fp] & 0x80 >> 7,
                registers[Fp] & 0x40 >> 6,
                registers[Fp] & 0x20 >> 5,
                registers[Fp] & 0x10 >> 4,
                registers[Fp] & 0x08 >> 3,
                registers[Fp] & 0x03 >> 2,
                registers[Fp] & 0x02 >> 1,
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
    }
}