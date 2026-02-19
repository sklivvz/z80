using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace z80
{
    public class Z80
    {
        private const byte rB = 0;
        private const byte rC = 1;
        private const byte rD = 2;
        private const byte rE = 3;
        private const byte rH = 4;
        private const byte rL = 5;
        private const byte rF = 6;
        private const byte rA = 7;
        private const byte rBp = 8;
        private const byte rCp = 9;
        private const byte rDp = 10;
        private const byte rEp = 11;
        private const byte rHp = 12;
        private const byte rLp = 13;
        private const byte rFp = 14;
        private const byte rAp = 15;
        private const byte rI = 16;
        private const byte rR = 17;
        private const byte rIX = 18;
        private const byte rIY = 20;
        private const byte rSP = 22;
        private const byte rPC = 24;
        private readonly IMemory mem;
        private readonly byte[] registers = new byte[26];
        private bool _iff1;
        private bool _iff2;
        private int _interruptMode;
        private int _tStates;
        private int _remainingTStates;
        private long _totalTStates;

        private readonly IBus bus;

        public Z80(IMemory memory, IBus bus)
        {
            if (memory == null) throw new ArgumentNullException(nameof(memory));
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            mem = memory;
            this.bus = bus;
            Reset();
        }

        private ushort _hl => (ushort)(registers[rL] + (registers[rH] << 8));
        private ushort _sp => (ushort)(registers[rSP + 1] + (registers[rSP] << 8));
        private ushort _ix => (ushort)(registers[rIX + 1] + (registers[rIX] << 8));
        private ushort _iy => (ushort)(registers[rIY + 1] + (registers[rIY] << 8));
        private ushort _bc => (ushort)((registers[rB] << 8) + registers[rC]);
        private ushort _de => (ushort)((registers[rD] << 8) + registers[rE]);
        private ushort _pc => (ushort)(registers[rPC + 1] + (registers[rPC] << 8));
        public bool HALT { get; private set; }
        public bool M1 { get; private set; }
        public bool RFSH { get; private set; }
        public bool BUSACK => false;
        public ushort AddressBus { get; private set; }
        public byte DataBus { get; private set; }

        public byte A => registers[rA];
        public byte F => registers[rF];
        public byte B => registers[rB];
        public byte C => registers[rC];
        public byte D => registers[rD];
        public byte E => registers[rE];
        public byte H => registers[rH];
        public byte L => registers[rL];
        public ushort AF => (ushort)((registers[rA] << 8) | registers[rF]);
        public ushort BC => (ushort)((registers[rB] << 8) | registers[rC]);
        public ushort DE => (ushort)((registers[rD] << 8) | registers[rE]);
        public ushort HL => (ushort)((registers[rH] << 8) | registers[rL]);
        public ushort IX => (ushort)((registers[rIX] << 8) | registers[rIX + 1]);
        public ushort IY => (ushort)((registers[rIY] << 8) | registers[rIY + 1]);
        public ushort SP => (ushort)((registers[rSP] << 8) | registers[rSP + 1]);
        public ushort PC => (ushort)((registers[rPC] << 8) | registers[rPC + 1]);
        public byte I => registers[rI];
        public byte R => registers[rR];

        public byte Ap => registers[rAp];
        public byte Fp => registers[rFp];
        public byte Bp => registers[rBp];
        public byte Cp => registers[rCp];
        public byte Dp => registers[rDp];
        public byte Ep => registers[rEp];
        public byte Hp => registers[rHp];
        public byte Lp => registers[rLp];
        public ushort AFp => (ushort)((registers[rAp] << 8) | registers[rFp]);
        public ushort BCp => (ushort)((registers[rBp] << 8) | registers[rCp]);
        public ushort DEp => (ushort)((registers[rDp] << 8) | registers[rEp]);
        public ushort HLp => (ushort)((registers[rHp] << 8) | registers[rLp]);

        public bool IFF1 => _iff1;
        public bool IFF2 => _iff2;
        public int InterruptMode => _interruptMode;

        public long TStates => _totalTStates;

        public bool Tick()
        {
            _totalTStates++;

            if (_remainingTStates > 0)
            {
                _remainingTStates--;
                return _remainingTStates == 0;
            }

            var cost = Parse();
            if (cost == 0)
            {
                _remainingTStates = 3;
                return false;
            }
            _remainingTStates = cost - 1;
            return _remainingTStates == 0;
        }

        public int Tick(int budget)
        {
            var completed = 0;
            for (var i = 0; i < budget; i++)
            {
                if (Tick()) completed++;
            }
            return completed;
        }

        public int Parse()
        {
            _tStates = 0;
            if (bus.NMI)
            {
                var stack = _sp;
                mem[--stack] = (byte)(_pc >> 8);
                mem[--stack] = (byte)(_pc);
                registers[rSP] = (byte)(stack >> 8);
                registers[rSP + 1] = (byte)(stack);
                registers[rPC] = 0x00;
                registers[rPC + 1] = 0x66;
                _iff1 = _iff2;
                _iff1 = false;
#if (DEBUG)
                Log("NMI");
#endif
                Wait(17);
                HALT = false;
                return _tStates;
            }
            if (_iff1 && bus.INT)
            {
                _iff1 = false;
                _iff2 = false;
                switch (_interruptMode)
                {
                    case 0:
                        {
                            // This is not quite correct, as it only runs a RST xx
                            // Instead, it should also support any other instruction
                            var instruction = bus.Data;
                            var stack = _sp;
                            mem[--stack] = (byte)(_pc >> 8);
                            mem[--stack] = (byte)(_pc);
                            registers[rSP] = (byte)(stack >> 8);
                            registers[rSP + 1] = (byte)(stack);
                            registers[rPC] = 0x00;
                            registers[rPC + 1] = (byte)(instruction & 0x38);
                            Wait(17);

#if (DEBUG)
                            Log("MI 0");
#endif
                            HALT = false;
                            return _tStates;
                        }
                    case 1:
                        {
                            var stack = _sp;
                            mem[--stack] = (byte)(_pc >> 8);
                            mem[--stack] = (byte)(_pc);
                            registers[rSP] = (byte)(stack >> 8);
                            registers[rSP + 1] = (byte)(stack);
                            registers[rPC] = 0x00;
                            registers[rPC + 1] = 0x38;
#if (DEBUG)
                            Log("MI 1");
#endif
                            Wait(17);
                            HALT = false;
                            return _tStates;
                        }
                    case 2:
                        {
                            var vector = bus.Data;
                            var stack = _sp;
                            mem[--stack] = (byte)(_pc >> 8);
                            mem[--stack] = (byte)(_pc);
                            registers[rSP] = (byte)(stack >> 8);
                            registers[rSP + 1] = (byte)(stack);
                            var address = (ushort)((registers[rI] << 8) + vector);
                            registers[rPC] = mem[address++];
                            registers[rPC + 1] = mem[address];
#if (DEBUG)
                            Log("MI 2");
#endif
                            Wait(17);
                            HALT = false;
                            return _tStates;
                        }
                }
                return _tStates;
            }
            if (HALT) return _tStates;
            M1 = true;
            var mc = Fetch();
            M1 = false;
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
                    Wait(4);
                    HALT = true;
                    return _tStates;
                }
                var reg = useHL2 ? mem[_hl] : registers[lo];

                if (useHL1)
                    mem[_hl] = reg;
                else
                    registers[r] = reg;
                Wait(useHL1 || useHL2 ? 7 : 4);
#if (DEBUG)
                Log($"LD {(useHL1 ? "(HL)" : RName(r))}, {(useHL2 ? "(HL)" : RName(lo))}");
#endif
                return _tStates;
            }
            switch (mc)
            {
                case 0xCB:
                    ParseCB();
                    return _tStates;
                case 0xDD:
                    ParseDD();
                    return _tStates;
                case 0xED:
                    ParseED();
                    return _tStates;
                case 0xFD:
                    ParseFD();
                    return _tStates;
                case 0x00:
                    // NOP
#if(DEBUG)
                    Log("NOP");
#endif
                    Wait(4);
                    return _tStates;
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
                        return _tStates;
                    }
                case 0x31:
                    {
                        // LD SP, nn
                        registers[rSP + 1] = Fetch();
                        registers[rSP] = Fetch();
#if (DEBUG)
                        Log($"LD SP, 0x{registers[rSP]:X2}{registers[rSP + 1]:X2}");
#endif
                        Wait(10);
                        return _tStates;
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
                        return _tStates;
                    }
                case 0x36:
                    {
                        // LD (HL), n
                        var n = Fetch();
                        mem[_hl] = n;
#if (DEBUG)
                        Log($"LD (HL), {n}");
#endif
                        Wait(10);
                        return _tStates;
                    }
                case 0x0A:
                    {
                        // LD A, (BC)
                        registers[rA] = mem[_bc];
#if (DEBUG)
                        Log("LD A, (BC)");
#endif
                        Wait(7);
                        return _tStates;
                    }
                case 0x1A:
                    {
                        // LD A, (DE)
                        registers[rA] = mem[_de];
#if (DEBUG)
                        Log("LD A, (DE)");
#endif
                        Wait(7);
                        return _tStates;
                    }
                case 0x3A:
                    {
                        // LD A, (nn)
                        var addr = Fetch16();
                        registers[rA] = mem[addr];
#if (DEBUG)
                        Log($"LD A, (0x{addr:X4})");
#endif
                        Wait(13);
                        return _tStates;
                    }
                case 0x02:
                    {
                        // LD (BC), A
                        mem[_bc] = registers[rA];
#if (DEBUG)
                        Log("LD (BC), A");
#endif
                        Wait(7);
                        return _tStates;
                    }
                case 0x12:
                    {
                        // LD (DE), A
                        mem[_de] = registers[rA];
#if (DEBUG)
                        Log("LD (DE), A");
#endif
                        Wait(7);
                        return _tStates;
                    }
                case 0x32:
                    {
                        // LD (nn), A 
                        var addr = Fetch16();
                        mem[addr] = registers[rA];
#if (DEBUG)
                        Log($"LD (0x{addr:X4}), A");
#endif
                        Wait(13);
                        return _tStates;
                    }
                case 0x2A:
                    {
                        // LD HL, (nn) 
                        var addr = Fetch16();
                        registers[rL] = mem[addr++];
                        registers[rH] = mem[addr];
#if (DEBUG)
                        Log($"LD HL, (0x{--addr:X4})");
#endif
                        Wait(16);
                        return _tStates;
                    }
                case 0x22:
                    {
                        // LD (nn), HL
                        var addr = Fetch16();
                        mem[addr++] = registers[rL];
                        mem[addr] = registers[rH];
#if (DEBUG)
                        Log($"LD (0x{--addr:X4}), HL");
#endif
                        Wait(16);
                        return _tStates;
                    }
                case 0xF9:
                    {
                        // LD SP, HL
                        registers[rSP + 1] = registers[rL];
                        registers[rSP] = registers[rH];
#if (DEBUG)
                        Log("LD SP, HL");
#endif
                        Wait(6);
                        return _tStates;
                    }

                case 0xC5:
                    {
                        // PUSH BC
                        var addr = _sp;
                        mem[--addr] = registers[rB];
                        mem[--addr] = registers[rC];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH BC");
#endif
                        Wait(11);
                        return _tStates;
                    }
                case 0xD5:
                    {
                        // PUSH DE
                        var addr = _sp;
                        mem[--addr] = registers[rD];
                        mem[--addr] = registers[rE];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH DE");
#endif
                        Wait(11);
                        return _tStates;
                    }
                case 0xE5:
                    {
                        // PUSH HL
                        var addr = _sp;
                        mem[--addr] = registers[rH];
                        mem[--addr] = registers[rL];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH HL");
#endif
                        Wait(11);
                        return _tStates;
                    }
                case 0xF5:
                    {
                        // PUSH AF
                        var addr = _sp;
                        mem[--addr] = registers[rA];
                        mem[--addr] = registers[rF];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH AF");
#endif
                        Wait(11);
                        return _tStates;
                    }
                case 0xC1:
                    {
                        // POP BC
                        var addr = _sp;
                        registers[rC] = mem[addr++];
                        registers[rB] = mem[addr++];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP BC");
#endif
                        Wait(10);
                        return _tStates;
                    }
                case 0xD1:
                    {
                        // POP DE
                        var addr = _sp;
                        registers[rE] = mem[addr++];
                        registers[rD] = mem[addr++];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP DE");
#endif
                        Wait(10);
                        return _tStates;
                    }
                case 0xE1:
                    {
                        // POP HL
                        var addr = _sp;
                        registers[rL] = mem[addr++];
                        registers[rH] = mem[addr++];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP HL");
#endif
                        Wait(10);
                        return _tStates;
                    }
                case 0xF1:
                    {
                        // POP AF
                        var addr = _sp;
                        registers[rF] = mem[addr++];
                        registers[rA] = mem[addr++];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP AF");
#endif
                        Wait(10);
                        return _tStates;
                    }
                case 0xEB:
                    {
                        // EX DE, HL
                        SwapReg8(rD, rH);
                        SwapReg8(rE, rL);
#if (DEBUG)
                        Log("EX DE, HL");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x08:
                    {
                        // EX AF, AF'
                        SwapReg8(rAp, rA);
                        SwapReg8(rFp, rF);
#if (DEBUG)
                        Log("EX AF, AF'");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0xD9:
                    {
                        // EXX
                        SwapReg8(rB, rBp);
                        SwapReg8(rC, rCp);
                        SwapReg8(rD, rDp);
                        SwapReg8(rE, rEp);
                        SwapReg8(rH, rHp);
                        SwapReg8(rL, rLp);
#if (DEBUG)
                        Log("EXX");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0xE3:
                    {
                        // EX (SP), HL
                        var addr = _sp;

                        var tmp = registers[rL];
                        registers[rL] = mem[addr];
                        mem[addr++] = tmp;

                        tmp = registers[rH];
                        registers[rH] = mem[addr];
                        mem[addr] = tmp;

#if (DEBUG)
                        Log("EX (SP), HL");
#endif
                        Wait(19);
                        return _tStates;
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
                        return _tStates;
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
                        return _tStates;
                    }
                case 0x86:
                    {
                        // ADD A, (HL)
                        Add(mem[_hl]);
#if (DEBUG)
                        Log("ADD A, (HL)");
#endif
                        Wait(7);
                        return _tStates;
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
                        return _tStates;
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
                        return _tStates;
                    }
                case 0x8E:
                    {
                        // ADC A, (HL)
                        Adc(mem[_hl]);
#if (DEBUG)
                        Log("ADC A, (HL)");
#endif
                        Wait(7);
                        return _tStates;
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
                        return _tStates;
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
                        return _tStates;
                    }
                case 0x96:
                    {
                        // SUB A, (HL)
                        Sub(mem[_hl]);
#if (DEBUG)
                        Log("SUB A, (HL)");
#endif
                        Wait(7);
                        return _tStates;
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
                        return _tStates;
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
                        return _tStates;
                    }
                case 0x9E:
                    {
                        // SBC A, (HL)
                        Sbc(mem[_hl]);
#if (DEBUG)
                        Log("SBC A, (HL)");
#endif
                        Wait(7);
                        return _tStates;
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
                        return _tStates;
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
                        return _tStates;
                    }
                case 0xA6:
                    {
                        // AND A, (HL)
                        And(mem[_hl]);
#if (DEBUG)
                        Log("AND A, (HL)");
#endif
                        Wait(7);
                        return _tStates;
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
                        return _tStates;
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
                        return _tStates;
                    }
                case 0xB6:
                    {
                        // OR A, (HL)
                        Or(mem[_hl]);
#if (DEBUG)
                        Log("OR A, (HL)");
#endif
                        Wait(7);
                        return _tStates;
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
                        return _tStates;
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
                        return _tStates;
                    }
                case 0xAE:
                    {
                        // XOR A, (HL)
                        Xor(mem[_hl]);
#if (DEBUG)
                        Log("XOR A, (HL)");
#endif
                        Wait(7);
                        return _tStates;
                    }

                case 0xF3:
                    {
                        // DI
                        _iff1 = false;
                        _iff2 = false;
#if (DEBUG)
                        Log("DI");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0xFB:
                    {
                        // EI
                        _iff1 = true;
                        _iff2 = true;
#if (DEBUG)
                        Log("EI");
#endif
                        Wait(4);
                        return _tStates;
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
                        return _tStates;
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
                        return _tStates;
                    }
                case 0xBE:
                    {
                        // CP A, (HL)
                        Cmp(mem[_hl]);
#if (DEBUG)
                        Log("CP A, (HL)");
#endif
                        Wait(7);
                        return _tStates;
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
                        return _tStates;
                    }
                case 0x34:
                    {
                        // INC (HL)
                        mem[_hl] = Inc(mem[_hl]);
#if (DEBUG)
                        Log("INC (HL)");
#endif
                        Wait(7);
                        return _tStates;
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
                        return _tStates;
                    }
                case 0x35:
                    {
                        // DEC (HL)
                        mem[_hl] = Dec(mem[_hl]);
#if (DEBUG)
                        Log("DEC (HL)");
#endif
                        Wait(7);
                        return _tStates;
                    }
                case 0x27:
                    {
                        // DAA
                        var origA = registers[rA];
                        int a = origA;
                        bool n = (registers[rF] & (byte)Fl.N) != 0;
                        bool c = (registers[rF] & (byte)Fl.C) != 0;
                        bool h = (registers[rF] & (byte)Fl.H) != 0;

                        int correction = 0;
                        bool newCarry = c;

                        if (h || (!n && (a & 0x0F) > 0x09))
                            correction = 0x06;

                        if (c || (!n && a > 0x99))
                        {
                            correction |= 0x60;
                            newCarry = true;
                        }

                        if (n)
                            a -= correction;
                        else
                            a += correction;

                        registers[rA] = (byte)a;

                        var f = (byte)0;
                        if (((byte)a & 0x80) != 0) f |= (byte)Fl.S;
                        if ((byte)a == 0) f |= (byte)Fl.Z;
                        // H flag per Z80 spec: for add, H if low nibble >= 10; for sub, H if prev H and low nibble <= 5
                        int lowNibble = origA & 0x0F;
                        if (n ? (h && lowNibble <= 5) : (lowNibble >= 10)) f |= (byte)Fl.H;
                        if (Parity((byte)a)) f |= (byte)Fl.PV;
                        if (n) f |= (byte)Fl.N;
                        if (newCarry) f |= (byte)Fl.C;
                        registers[rF] = f;
#if (DEBUG)
                        Log("DAA");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x2F:
                    {
                        // CPL
                        registers[rA] ^= 0xFF;
                        registers[rF] |= (byte)(Fl.H | Fl.N);
#if (DEBUG)
                        Log("CPL");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x3F:
                    {
                        // CCF - H is set to previous C value, then C is complemented
                        var prevC = (registers[rF] & (byte)Fl.C) != 0;
                        registers[rF] &= (byte)~(Fl.N | Fl.H);
                        if (prevC)
                            registers[rF] |= (byte)Fl.H;
                        registers[rF] ^= (byte)(Fl.C);
#if (DEBUG)
                        Log("CCF");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x37:
                    {
                        // SCF
                        registers[rF] &= (byte)~(Fl.N);
                        registers[rF] |= (byte)(Fl.C);
#if (DEBUG)
                        Log("SCF");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x09:
                    {
                        AddHl(_bc);

#if (DEBUG)
                        Log("ADD HL, BC");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x19:
                    {
                        AddHl(_de);
#if (DEBUG)
                        Log("ADD HL, DE");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x29:
                    {
                        AddHl(_hl);
#if (DEBUG)
                        Log("ADD HL, HL");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x39:
                    {
                        AddHl(_sp);
#if (DEBUG)
                        Log("ADD HL, SP");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x03:
                    {
                        var val = _bc + 1;
                        registers[rB] = (byte)(val >> 8);
                        registers[rC] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC BC");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x13:
                    {
                        var val = _de + 1;
                        registers[rD] = (byte)(val >> 8);
                        registers[rE] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC DE");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x23:
                    {
                        var val = _hl + 1;
                        registers[rH] = (byte)(val >> 8);
                        registers[rL] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC HL");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x33:
                    {
                        var val = _sp + 1;
                        registers[rSP] = (byte)(val >> 8);
                        registers[rSP + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC SP");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x0B:
                    {
                        var val = _bc - 1;
                        registers[rB] = (byte)(val >> 8);
                        registers[rC] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC BC");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x1B:
                    {
                        var val = _de - 1;
                        registers[rD] = (byte)(val >> 8);
                        registers[rE] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC DE");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x2B:
                    {
                        var val = _hl - 1;
                        registers[rH] = (byte)(val >> 8);
                        registers[rL] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC HL");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x3B:
                    {
                        var val = _sp - 1;
                        registers[rSP] = (byte)(val >> 8);
                        registers[rSP + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC SP");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x07:
                    {
                        var a = registers[rA];
                        var c = (byte)((a & 0x80) >> 7);
                        a <<= 1;
                        a |= c; // bit 7 rotates to bit 0
                        registers[rA] = a;
                        registers[rF] &= (byte)~(Fl.H | Fl.N | Fl.C);
                        registers[rF] |= c;
#if (DEBUG)
                        Log("RLCA");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x17:
                    {
                        var a = registers[rA];
                        var c = (byte)((a & 0x80) >> 7);
                        a <<= 1;
                        var f = registers[rF];
                        a |= (byte)(f & (byte)Fl.C);
                        registers[rA] = a;
                        f &= (byte)~(Fl.H | Fl.N | Fl.C);
                        f |= c;
                        registers[rF] = f;
#if (DEBUG)
                        Log("RLA");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x0F:
                    {
                        var a = registers[rA];
                        var c = (byte)(a & 0x01);
                        a >>= 1;
                        a |= (byte)(c << 7); // bit 0 rotates to bit 7
                        registers[rA] = a;
                        registers[rF] &= (byte)~(Fl.H | Fl.N | Fl.C);
                        registers[rF] |= c;
#if (DEBUG)
                        Log("RRCA");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x1F:
                    {
                        var a = registers[rA];
                        var c = (byte)(a & 0x01);
                        a >>= 1;
                        var f = registers[rF];
                        a |= (byte)((f & (byte)Fl.C) << 7);
                        registers[rA] = a;
                        f &= (byte)~(Fl.H | Fl.N | Fl.C);
                        f |= c;
                        registers[rF] = f;
#if (DEBUG)
                        Log("RRA");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0xC3:
                    {
                        var addr = Fetch16();
                        registers[rPC] = (byte)(addr >> 8);
                        registers[rPC + 1] = (byte)(addr);
#if (DEBUG)
                        Log($"JP 0x{addr:X4}");
#endif
                        Wait(10);
                        return _tStates;
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
                            registers[rPC] = (byte)(addr >> 8);
                            registers[rPC + 1] = (byte)(addr);
                        }
#if (DEBUG)
                        Log($"JP {JCName(r)}, 0x{addr:X4}");
#endif
                        Wait(10);
                        return _tStates;

                    }
                case 0x18:
                    {
                        // order is important here
                        var d = (sbyte)Fetch();
                        var addr = _pc + d;
                        registers[rPC] = (byte)(addr >> 8);
                        registers[rPC + 1] = (byte)(addr);
#if (DEBUG)
                        Log($"JR 0x{addr:X4}");
#endif
                        Wait(12);
                        return _tStates;
                    }
                case 0x20:
                case 0x28:
                case 0x30:
                case 0x38:
                    {
                        // order is important here
                        var d = (sbyte)Fetch();
                        var addr = _pc + d;
                        if (JumpCondition((byte)(r & 3)))
                        {
                            registers[rPC] = (byte)(addr >> 8);
                            registers[rPC + 1] = (byte)(addr);
                            Wait(12);
                        }
                        else
                        {
                            Wait(7);
                        }
#if (DEBUG)
                        Log($"JR {JCName((byte)(r & 3))}, 0x{addr:X4}");
#endif
                        return _tStates;

                    }
                case 0xE9:
                    {
                        var addr = _hl;
                        registers[rPC] = (byte)(addr >> 8);
                        registers[rPC + 1] = (byte)(addr);
#if (DEBUG)
                        Log("JP HL");
#endif
                        Wait(4);
                        return _tStates;
                    }
                case 0x10:
                    {
                        // order is important here
                        var d = (sbyte)Fetch();
                        var addr = _pc + d;
                        var b = registers[rB];
                        registers[rB] = --b;
                        if (b != 0)
                        {
                            registers[rPC] = (byte)(addr >> 8);
                            registers[rPC + 1] = (byte)(addr);
                            Wait(13);
                        }
                        else
                        {
                            Wait(8);
                        }
#if (DEBUG)
                        Log($"DJNZ 0x{addr:X4}");
#endif
                        return _tStates;
                    }
                case 0xCD:
                    {
                        var addr = Fetch16();
                        var stack = _sp;
                        mem[--stack] = (byte)(_pc >> 8);
                        mem[--stack] = (byte)(_pc);
                        registers[rSP] = (byte)(stack >> 8);
                        registers[rSP + 1] = (byte)(stack);
                        registers[rPC] = (byte)(addr >> 8);
                        registers[rPC + 1] = (byte)(addr);
#if (DEBUG)
                        Log($"CALL 0x{addr:X4}");
#endif
                        Wait(17);
                        return _tStates;
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
                            var stack = _sp;
                            mem[--stack] = (byte)(_pc >> 8);
                            mem[--stack] = (byte)(_pc);
                            registers[rSP] = (byte)(stack >> 8);
                            registers[rSP + 1] = (byte)(stack);
                            registers[rPC] = (byte)(addr >> 8);
                            registers[rPC + 1] = (byte)(addr);
                            Wait(17);
                        }
                        else
                        {
                            Wait(10);
                        }
#if (DEBUG)
                        Log($"CALL {JCName(r)}, 0x{addr:X4}");
#endif
                        return _tStates;

                    }
                case 0xC9:
                    {
                        var stack = _sp;
                        registers[rPC + 1] = mem[stack++];
                        registers[rPC] = mem[stack++];
                        registers[rSP] = (byte)(stack >> 8);
                        registers[rSP + 1] = (byte)(stack);
#if (DEBUG)
                        Log("RET");
#endif
                        Wait(10);
                        return _tStates;
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
                            var stack = _sp;
                            registers[rPC + 1] = mem[stack++];
                            registers[rPC] = mem[stack++];
                            registers[rSP] = (byte)(stack >> 8);
                            registers[rSP + 1] = (byte)(stack);
                            Wait(11);
                        }
                        else
                        {
                            Wait(5);
                        }
#if (DEBUG)
                        Log($"RET {JCName(r)}");
#endif
                        return _tStates;

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
                        var stack = _sp;
                        mem[--stack] = (byte)(_pc >> 8);
                        mem[--stack] = (byte)(_pc);
                        registers[rSP] = (byte)(stack >> 8);
                        registers[rSP + 1] = (byte)(stack);
                        registers[rPC] = 0;
                        registers[rPC + 1] = (byte)(mc & 0x38);
#if (DEBUG)
                        Log($"RST 0x{mc & 0x38:X4}");
#endif
                        Wait(17);
                        return _tStates;
                    }
                case 0xDB:
                    {
                        var port = Fetch() + (registers[rA] << 8);
                        registers[rA] = bus.IoRead((ushort)port);
#if (DEBUG)
                        Log($"IN A, (0x{port:X2})");
#endif
                        Wait(11);
                        return _tStates;
                    }
                case 0xD3:
                    {
                        var port = Fetch() + (registers[rA] << 8);
                        bus.IoWrite((ushort)port, registers[rA]);
#if (DEBUG)
                        Log($"OUT (0x{port:X2}), A");
#endif
                        Wait(11);
                        return _tStates;
                    }
            }

#if(DEBUG)
            Log($"{mc:X2}: {hi:X} {r:X} {lo:X}");
            //throw new InvalidOperationException("Invalid Opcode: "+mc.ToString("X2"));
#endif
            Wait(4);
            HALT = true;
            return _tStates;
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
            if (HALT) return;
            var mc = Fetch();
            var hi = (byte)(mc >> 6);
            var lo = (byte)(mc & 0x07);
            var r = (byte)((mc >> 3) & 0x07);
            var useHL = lo == 6;
            var useIX = mode == 0xDD;
            var useIY = mode == 0XFD;
            var reg = useHL ? useIX ? mem[(ushort)(_ix + d)] : useIY ? mem[(ushort)(_iy + d)] : mem[_hl] : registers[lo];
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
                    var f = registers[rF];
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
                    registers[rF] = f;

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
                    mem[(ushort)(_ix + d)] = reg;
                    Wait(23);
                }
                else if (useIY)
                {
                    mem[(ushort)(_iy + d)] = reg;
                    Wait(23);
                }
                else
                {
                    mem[_hl] = reg;
                    Wait(15);
                }
            }
            else
            {
                if (useIX)
                {
                    mem[(ushort)(_ix + d)] = reg;
                    Wait(23);
                }
                else if (useIY)
                {
                    mem[(ushort)(_iy + d)] = reg;
                    Wait(23);
                }
                registers[lo] = reg;
                Wait(8);
            }
        }

        private void Bit(byte bit, byte value)
        {
            var f = (byte)(registers[rF] & (byte)~(Fl.S | Fl.Z | Fl.H | Fl.PV | Fl.N));
            var bitSet = (value & (0x01 << bit)) != 0;
            if (!bitSet)
            {
                f |= (byte)Fl.Z;
                f |= (byte)Fl.PV;  // P/V = Z
            }
            // S is set only when testing bit 7 and it's set
            if (bit == 7 && bitSet)
                f |= (byte)Fl.S;
            f |= (byte)Fl.H;
            registers[rF] = f;
        }

        private void AddHl(ushort value)
        {
            var sum = Add(_hl, value);
            registers[rH] = (byte)(sum >> 8);
            registers[rL] = (byte)(sum & 0xFF);
        }

        private void AddIx(ushort value)
        {
            var sum = Add(_ix, value);
            registers[rIX] = (byte)(sum >> 8);
            registers[rIX + 1] = (byte)(sum & 0xFF);
        }

        private void AddIy(ushort value)
        {
            var sum = Add(_iy, value);
            registers[rIY] = (byte)(sum >> 8);
            registers[rIY + 1] = (byte)(sum & 0xFF);
        }

        private ushort Add(ushort value1, ushort value2)
        {
            var sum = value1 + value2;
            var f = (byte)(registers[rF] & (byte)~(Fl.H | Fl.N | Fl.C));
            if ((value1 & 0x0FFF) + (value2 & 0x0FFF) > 0x0FFF)
                f |= (byte)Fl.H;
            if (sum > 0xFFFF)
                f |= (byte)Fl.C;
            registers[rF] = f;
            return (ushort)sum;
        }

        private void AdcHl(ushort value)
        {
            var sum = Adc(_hl, value);
            registers[rH] = (byte)(sum >> 8);
            registers[rL] = (byte)(sum & 0xFF);
        }

        private ushort Adc(ushort value1, ushort value2)
        {
            var c = registers[rF] & (byte)Fl.C;
            var sum = value1 + value2 + c;
            var f = (byte)(registers[rF] & (byte)~(Fl.S | Fl.Z | Fl.H | Fl.PV | Fl.N | Fl.C));
            if ((short)(ushort)sum < 0)
                f |= (byte)Fl.S;
            if ((ushort)sum == 0)
                f |= (byte)Fl.Z;
            if ((value1 & 0x0FFF) + (value2 & 0x0FFF) + c > 0x0FFF)
                f |= (byte)Fl.H;
            // Overflow: both operands same sign and result has different sign
            var v1Sign = value1 & 0x8000;
            var v2Sign = value2 & 0x8000;
            var resSign = (ushort)sum & 0x8000;
            if (v1Sign == v2Sign && v1Sign != resSign)
                f |= (byte)Fl.PV;
            if (sum > 0xFFFF)
                f |= (byte)Fl.C;
            registers[rF] = f;
            return (ushort)sum;
        }

        private void SbcHl(ushort value)
        {
            var sum = Sbc(_hl, value);
            registers[rH] = (byte)(sum >> 8);
            registers[rL] = (byte)(sum & 0xFF);
        }


        private ushort Sbc(ushort value1, ushort value2)
        {
            var diff = value1 - value2 - (registers[rF] & (byte)Fl.C);
            var f = (byte)(registers[rF] & (byte)~(Fl.S | Fl.Z | Fl.H | Fl.PV | Fl.N | Fl.C));
            if ((short)diff < 0)
                f |= (byte)Fl.S;
            if (diff == 0)
                f |= (byte)Fl.Z;
            if ((value1 & 0xFFF) < (value2 & 0xFFF) + (registers[rF] & (byte)Fl.C))
                f |= (byte)Fl.H;
            if (diff > short.MaxValue || diff < short.MinValue)
                f |= (byte)Fl.PV;
            if ((ushort)diff > value1)
                f |= (byte)Fl.C;
            registers[rF] = f;
            return (ushort)diff;
        }

        private void ParseED()
        {
            if (HALT) return;
            var mc = Fetch();
            var r = (byte)((mc >> 3) & 0x07);

            switch (mc)
            {
                case 0x47:
                    {
                        // LD I, A
                        registers[rI] = registers[rA];
#if (DEBUG)
                        Log("LD I, A");
#endif
                        Wait(9);
                        return;
                    }
                case 0x4F:
                    {
                        // LD R, A
                        registers[rR] = registers[rA];
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
                        var i = registers[rI];
                        registers[rA] = i;
                        var f = (byte)(registers[rF] & (~(byte)(Fl.H | Fl.PV | Fl.N | Fl.S | Fl.Z | Fl.PV)));
                        if (i >= 0x80)
                        {
                            f |= (byte)Fl.S;
                        }
                        else if (i == 0x00)
                        {
                            f |= (byte)Fl.Z;
                        }
                        if (_iff2)
                        {
                            f |= (byte)Fl.PV;
                        }
                        registers[rF] = f;
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
                        var reg = registers[rR];
                        registers[rA] = reg;
                        var f = (byte)(registers[rF] & (~(byte)(Fl.H | Fl.PV | Fl.N | Fl.S | Fl.Z | Fl.PV)));
                        if (reg >= 0x80)
                        {
                            f |= (byte)Fl.S;
                        }
                        else if (reg == 0x00)
                        {
                            f |= (byte)Fl.Z;
                        }
                        if (_iff2)
                        {
                            f |= (byte)Fl.PV;
                        }
                        registers[rF] = f;
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
                        registers[rC] = mem[addr++];
                        registers[rB] = mem[addr];
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
                        registers[rE] = mem[addr++];
                        registers[rD] = mem[addr];
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
                        registers[rL] = mem[addr++];
                        registers[rH] = mem[addr];
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
                        registers[rSP + 1] = mem[addr++];
                        registers[rSP] = mem[addr];
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
                        mem[addr++] = registers[rC];
                        mem[addr] = registers[rB];
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
                        mem[addr++] = registers[rE];
                        mem[addr] = registers[rD];
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
                        mem[addr++] = registers[rL];
                        mem[addr] = registers[rH];
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
                        mem[addr++] = registers[rSP + 1];
                        mem[addr] = registers[rSP];
#if (DEBUG)
                        Log($"LD (0x{--addr:X4}), SP");
#endif
                        Wait(20);
                        return;
                    }
                case 0xA0:
                    {
                        // LDI
                        var bc = _bc;
                        var de = _de;
                        var hl = _hl;

                        mem[de] = mem[hl];
                        de++;
                        hl++;
                        bc--;

                        registers[rB] = (byte)(bc >> 8);
                        registers[rC] = (byte)(bc & 0xFF);
                        registers[rD] = (byte)(de >> 8);
                        registers[rE] = (byte)(de & 0xFF);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)(hl & 0xFF);

                        var f = (byte)(registers[rF] & 0xE9);
                        if (bc != 0) f = (byte)(f | 0x04);
                        registers[rF] = f;
#if (DEBUG)
                        Log("LDI");
#endif
                        Wait(16);
                        return;
                    }
                case 0xB0:
                    {
                        // LDIR
                        var bc = _bc;
                        var de = _de;
                        var hl = _hl;

                        mem[de] = mem[hl];
                        de++;
                        hl++;
                        bc--;

                        registers[rB] = (byte)(bc >> 8);
                        registers[rC] = (byte)(bc & 0xFF);
                        registers[rD] = (byte)(de >> 8);
                        registers[rE] = (byte)(de & 0xFF);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)(hl & 0xFF);

                        registers[rF] = (byte)(registers[rF] & 0xE9);
                        if (bc != 0)
                        {
                            var pc = (ushort)((registers[rPC] << 8) + registers[rPC + 1]);
                            // jumps back to itself
                            pc -= 2;
                            registers[rPC] = (byte)(pc >> 8);
                            registers[rPC + 1] = (byte)(pc & 0xFF);
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
                        var bc = _bc;
                        var de = _de;
                        var hl = _hl;

                        mem[de] = mem[hl];
                        de--;
                        hl--;
                        bc--;

                        registers[rB] = (byte)(bc >> 8);
                        registers[rC] = (byte)(bc & 0xFF);
                        registers[rD] = (byte)(de >> 8);
                        registers[rE] = (byte)(de & 0xFF);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)(hl & 0xFF);

                        var f = (byte)(registers[rF] & 0xE9);
                        if (bc != 0) f = (byte)(f | 0x04);
                        registers[rF] = f;
#if (DEBUG)
                        Log("LDD");
#endif
                        Wait(16);
                        return;
                    }
                case 0xB8:
                    {
                        // LDDR
                        var bc = _bc;
                        var de = _de;
                        var hl = _hl;

                        mem[de] = mem[hl];
                        de--;
                        hl--;
                        bc--;

                        registers[rB] = (byte)(bc >> 8);
                        registers[rC] = (byte)(bc & 0xFF);
                        registers[rD] = (byte)(de >> 8);
                        registers[rE] = (byte)(de & 0xFF);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)(hl & 0xFF);

                        registers[rF] = (byte)(registers[rF] & 0xE9);
                        if (bc != 0)
                        {
                            var pc = (ushort)((registers[rPC] << 8) + registers[rPC + 1]);
                            // jumps back to itself
                            pc -= 2;
                            registers[rPC] = (byte)(pc >> 8);
                            registers[rPC + 1] = (byte)(pc & 0xFF);
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
                        var bc = _bc;
                        var hl = _hl;

                        var a = registers[rA];
                        var b = mem[hl];
                        hl++;
                        bc--;

                        registers[rB] = (byte)(bc >> 8);
                        registers[rC] = (byte)(bc & 0xFF);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)(hl & 0xFF);

                        var result = (byte)(a - b);
                        var f = (byte)(registers[rF] & 0x29);  // preserve C flag
                        if ((result & 0x80) != 0) f = (byte)(f | 0x80);  // S from result MSB
                        if (a == b) f = (byte)(f | 0x40);
                        if ((a & 0x0F) < (b & 0x0F)) f = (byte)(f | 0x10);  // H = half-borrow
                        if (bc != 0) f = (byte)(f | 0x04);
                        registers[rF] = (byte)(f | 0x02);  // N always set
#if (DEBUG)
                        Log("CPI");
#endif
                        Wait(16);
                        return;
                    }

                case 0xB1:
                    {
                        // CPIR
                        var bc = _bc;
                        var hl = _hl;

                        var a = registers[rA];
                        var b = mem[hl];
                        hl++;
                        bc--;

                        registers[rB] = (byte)(bc >> 8);
                        registers[rC] = (byte)(bc & 0xFF);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)(hl & 0xFF);

                        if (a == b || bc == 0)
                        {
                            var result = (byte)(a - b);
                            var f = (byte)(registers[rF] & 0x29);
                            if ((result & 0x80) != 0) f = (byte)(f | 0x80);
                            if (a == b) f = (byte)(f | 0x40);
                            if ((a & 0x0F) < (b & 0x0F)) f = (byte)(f | 0x10);
                            if (bc != 0) f = (byte)(f | 0x04);
                            registers[rF] = (byte)(f | 0x02);
#if (DEBUG)
                            Log("CPIR");
#endif
                            Wait(16);
                            return;
                        }

                        var pc = (ushort)((registers[rPC] << 8) + registers[rPC + 1]);
                        // jumps back to itself
                        pc -= 2;
                        registers[rPC] = (byte)(pc >> 8);
                        registers[rPC + 1] = (byte)(pc & 0xFF);
                        Wait(21);
                        return;
                    }

                case 0xA9:
                    {
                        // CPD
                        var bc = _bc;
                        var hl = _hl;

                        var a = registers[rA];
                        var b = mem[hl];
                        hl--;
                        bc--;

                        registers[rB] = (byte)(bc >> 8);
                        registers[rC] = (byte)(bc & 0xFF);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)(hl & 0xFF);

                        var result = (byte)(a - b);
                        var f = (byte)(registers[rF] & 0x29);
                        if ((result & 0x80) != 0) f = (byte)(f | 0x80);
                        if (a == b) f = (byte)(f | 0x40);
                        if ((a & 0x0F) < (b & 0x0F)) f = (byte)(f | 0x10);
                        if (bc != 0) f = (byte)(f | 0x04);
                        registers[rF] = (byte)(f | 0x02);
#if (DEBUG)
                        Log("CPD");
#endif
                        Wait(16);
                        return;
                    }

                case 0xB9:
                    {
                        // CPDR
                        var bc = _bc;
                        var hl = _hl;

                        var a = registers[rA];
                        var b = mem[hl];
                        hl--;
                        bc--;

                        registers[rB] = (byte)(bc >> 8);
                        registers[rC] = (byte)(bc & 0xFF);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)(hl & 0xFF);

                        if (a == b || bc == 0)
                        {
                            var result = (byte)(a - b);
                            var f = (byte)(registers[rF] & 0x29);
                            if ((result & 0x80) != 0) f = (byte)(f | 0x80);
                            if (a == b) f = (byte)(f | 0x40);
                            if ((a & 0x0F) < (b & 0x0F)) f = (byte)(f | 0x10);
                            if (bc != 0) f = (byte)(f | 0x04);
                            registers[rF] = (byte)(f | 0x02);
#if (DEBUG)
                            Log("CPDR");
#endif
                            Wait(21);
                            return;
                        }

                        var pc = (ushort)((registers[rPC] << 8) + registers[rPC + 1]);
                        // jumps back to itself
                        pc -= 2;
                        registers[rPC] = (byte)(pc >> 8);
                        registers[rPC + 1] = (byte)(pc & 0xFF);
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
                        var a = registers[rA];
                        var diff = -a;
                        registers[rA] = (byte)diff;

                        var f = (byte)(registers[rF] & 0x28);
                        if ((diff & 0x80) > 0) f |= (byte)Fl.S;
                        if (diff == 0) f |= (byte)Fl.Z;
                        if ((a & 0xF) != 0) f |= (byte)Fl.H;
                        if (a == 0x80) f |= (byte)Fl.PV;
                        f |= (byte)Fl.N;
                        if (diff != 0) f |= (byte)Fl.C;
                        registers[rF] = f;


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
                        _interruptMode = 0;
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
                        _interruptMode = 1;
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
                        _interruptMode = 2;
#if (DEBUG)
                        Log("IM 2");
#endif
                        Wait(8);
                        return;
                    }
                case 0x4A:
                    {
                        AdcHl(_bc);

#if (DEBUG)
                        Log("ADC HL, BC");
#endif
                        Wait(15);
                        return;
                    }
                case 0x5A:
                    {
                        AdcHl(_de);
#if (DEBUG)
                        Log("ADC HL, DE");
#endif
                        Wait(15);
                        return;
                    }
                case 0x6A:
                    {
                        AdcHl(_hl);
#if (DEBUG)
                        Log("ADC HL, HL");
#endif
                        Wait(15);
                        return;
                    }
                case 0x7A:
                    {
                        AdcHl(_sp);
#if (DEBUG)
                        Log("ADC HL, SP");
#endif
                        Wait(15);
                        return;
                    }
                case 0x42:
                    {
                        SbcHl(_bc);

#if (DEBUG)
                        Log("SBC HL, BC");
#endif
                        Wait(15);
                        return;
                    }
                case 0x52:
                    {
                        SbcHl(_de);
#if (DEBUG)
                        Log("SBC HL, DE");
#endif
                        Wait(15);
                        return;
                    }
                case 0x62:
                    {
                        SbcHl(_hl);
#if (DEBUG)
                        Log("SBC HL, HL");
#endif
                        Wait(15);
                        return;
                    }
                case 0x72:
                    {
                        SbcHl(_sp);
#if (DEBUG)
                        Log("SBC HL, SP");
#endif
                        Wait(15);
                        return;
                    }

                case 0x6F:
                    {
                        var a = registers[rA];
                        var b = mem[_hl];
                        mem[_hl] = (byte)((b << 4) | (a & 0x0F));
                        a = (byte)((a & 0xF0) | (b >> 4));
                        registers[rA] = a;
                        var f = (byte)(registers[rF] & 0x29);
                        if ((a & 0x80) > 0) f |= (byte)Fl.S;
                        if (a == 0) f |= (byte)Fl.Z;
                        if (Parity(a)) f |= (byte)Fl.PV;
                        registers[rF] = f;
#if (DEBUG)
                        Log("RLD");
#endif
                        Wait(18);
                        return;
                    }
                case 0x67:
                    {
                        var a = registers[rA];
                        var b = mem[_hl];
                        mem[_hl] = (byte)((b >> 4) | (a << 4));
                        a = (byte)((a & 0xF0) | (b & 0x0F));
                        registers[rA] = a;
                        var f = (byte)(registers[rF] & 0x29);
                        if ((a & 0x80) > 0) f |= (byte)Fl.S;
                        if (a == 0) f |= (byte)Fl.Z;
                        if (Parity(a)) f |= (byte)Fl.PV;
                        registers[rF] = f;
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
                        var stack = _sp;
                        registers[rPC + 1] = mem[stack++];
                        registers[rPC] = mem[stack++];
                        registers[rSP] = (byte)(stack >> 8);
                        registers[rSP + 1] = (byte)(stack);
                        _iff1 = _iff2;
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
                        var a = (byte)bus.IoRead(_bc);
                        registers[r] = a;
                        var f = (byte)(registers[rF] & 0x29);
                        if ((a & 0x80) > 0) f |= (byte)Fl.S;
                        if (a == 0) f |= (byte)Fl.Z;
                        if (Parity(a)) f |= (byte)Fl.PV;
                        registers[rF] = f;
#if (DEBUG)
                        Log($"IN {RName(r)}, (BC)");
#endif
                        Wait(8);
                        return;
                    }
                case 0xA2:
                    {
                        var a = (byte)bus.IoRead(_bc);
                        var hl = _hl;
                        mem[hl++] = a;
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)hl;
                        var b = (byte)(registers[rB] - 1);
                        registers[rB] = b;
                        var f = (byte)(registers[rF] & (byte)~(Fl.N | Fl.Z));
                        if (b == 0) f |= (byte)Fl.Z;
                        f |= (byte)Fl.N;
                        registers[rF] = f;

#if (DEBUG)
                        Log("INI");
#endif
                        Wait(16);
                        return;
                    }
                case 0xB2:
                    {
                        var a = (byte)bus.IoRead(_bc);
                        var hl = _hl;
                        mem[hl++] = a;
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)hl;
                        var b = (byte)(registers[rB] - 1);
                        registers[rB] = b;
                        if (b != 0)
                        {
                            var pc = _pc - 2;
                            registers[rPC] = (byte)(pc >> 8);
                            registers[rPC + 1] = (byte)pc;
#if (DEBUG)
                            Log("(INIR)");
#endif
                            Wait(21);
                        }
                        else
                        {
                            registers[rF] = (byte)(registers[rF] | (byte)(Fl.N | Fl.Z));
#if (DEBUG)
                            Log("INIR");
#endif
                            Wait(16);
                        }
                        return;
                    }
                case 0xAA:
                    {
                        var a = (byte)bus.IoRead(_bc);
                        var hl = _hl;
                        mem[hl--] = a;
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)hl;
                        var b = (byte)(registers[rB] - 1);
                        registers[rB] = b;
                        var f = (byte)(registers[rF] & (byte)~(Fl.N | Fl.Z));
                        if (b == 0) f |= (byte)Fl.Z;
                        f |= (byte)Fl.N;
                        registers[rF] = f;
#if (DEBUG)
                        Log("IND");
#endif
                        Wait(16);
                        return;
                    }
                case 0xBA:
                    {
                        var a = (byte)bus.IoRead(_bc);
                        var hl = _hl;
                        mem[hl--] = a;
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)hl;
                        var b = (byte)(registers[rB] - 1);
                        registers[rB] = b;
                        if (b != 0)
                        {
                            var pc = _pc - 2;
                            registers[rPC] = (byte)(pc >> 8);
                            registers[rPC + 1] = (byte)pc;
#if (DEBUG)
                            Log("(INDR)");
#endif
                            Wait(21);
                        }
                        else
                        {
                            registers[rF] = (byte)(registers[rF] | (byte)(Fl.N | Fl.Z));
#if (DEBUG)
                            Log("INDR");
#endif
                            Wait(16);
                        }
                        return;
                    }
                case 0x41:
                case 0x49:
                case 0x51:
                case 0x59:
                case 0x61:
                case 0x69:
                case 0x79:
                    {
                        var a = registers[r];
                        bus.IoWrite(_bc, a);
                        var f = (byte)(registers[rF] & 0x29);
                        if ((a & 0x80) > 0) f |= (byte)Fl.S;
                        if (a == 0) f |= (byte)Fl.Z;
                        if (Parity(a)) f |= (byte)Fl.PV;
                        registers[rF] = f;
#if (DEBUG)
                        Log($"OUT (BC), {RName(r)}");
#endif
                        Wait(8);
                        return;
                    }
                case 0xA3:
                    {
                        var hl = _hl;
                        var a = mem[hl++];
                        bus.IoWrite(_bc, a);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)hl;
                        var b = (byte)(registers[rB] - 1);
                        registers[rB] = b;
                        var f = (byte)(registers[rF] & (byte)~(Fl.N | Fl.Z));
                        if (b == 0) f |= (byte)Fl.Z;
                        f |= (byte)Fl.N;
                        registers[rF] = f;

#if (DEBUG)
                        Log("OUTI");
#endif
                        Wait(16);
                        return;
                    }
                case 0xB3:
                    {
                        var hl = _hl;
                        var a = mem[hl++];
                        bus.IoWrite(_bc, a);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)hl;
                        var b = (byte)(registers[rB] - 1);
                        registers[rB] = b;
                        if (b != 0)
                        {
                            var pc = _pc - 2;
                            registers[rPC] = (byte)(pc >> 8);
                            registers[rPC + 1] = (byte)pc;
#if (DEBUG)
                            Log("(OUTIR)");
#endif
                            Wait(21);
                        }
                        else
                        {
                            registers[rF] = (byte)(registers[rF] | (byte)(Fl.N | Fl.Z));
#if (DEBUG)
                            Log("OUTIR");
#endif
                            Wait(16);
                        }
                        return;
                    }
                case 0xAB:
                    {
                        var hl = _hl;
                        var a = mem[hl--];
                        bus.IoWrite(_bc, a);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)hl;
                        var b = (byte)(registers[rB] - 1);
                        registers[rB] = b;
                        var f = (byte)(registers[rF] & (byte)~(Fl.N | Fl.Z));
                        if (b == 0) f |= (byte)Fl.Z;
                        f |= (byte)Fl.N;
                        registers[rF] = f;
#if (DEBUG)
                        Log("OUTD");
#endif
                        Wait(16);
                        return;
                    }
                case 0xBB:
                    {
                        var hl = _hl;
                        var a = mem[hl--];
                        bus.IoWrite(_bc, a);
                        registers[rH] = (byte)(hl >> 8);
                        registers[rL] = (byte)hl;
                        var b = (byte)(registers[rB] - 1);
                        registers[rB] = b;
                        if (b != 0)
                        {
                            var pc = _pc - 2;
                            registers[rPC] = (byte)(pc >> 8);
                            registers[rPC + 1] = (byte)pc;
#if (DEBUG)
                            Log("(OUTDR)");
#endif
                            Wait(21);
                        }
                        else
                        {
                            registers[rF] = (byte)(registers[rF] | (byte)(Fl.N | Fl.Z));
#if (DEBUG)
                            Log("OUTDR");
#endif
                            Wait(16);
                        }
                        return;
                    }
            }
#if (DEBUG)
            Log($"ED {mc:X2}: {r:X2}");
#endif
            Wait(4);
            HALT = true;
        }

        private void ParseDD()
        {
            if (HALT) return;
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
                        registers[rIX + 1] = Fetch();
                        registers[rIX] = Fetch();
#if (DEBUG)
                        Log($"LD IX, 0x{_ix:X4}");
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
                        registers[mid] = mem[(ushort)(_ix + d)];
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
                        mem[(ushort)(_ix + d)] = registers[lo];
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
                        mem[(ushort)(_ix + d)] = n;
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
                        registers[rIX + 1] = mem[addr++];
                        registers[rIX] = mem[addr];
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
                        mem[addr++] = registers[rIX + 1];
                        mem[addr] = registers[rIX];
#if (DEBUG)
                        Log($"LD (0x{addr:X4}), IX");
#endif
                        Wait(20);
                        return;
                    }

                case 0xF9:
                    {
                        // LD SP, IX
                        registers[rSP] = registers[rIX];
                        registers[rSP + 1] = registers[rIX + 1];
#if (DEBUG)
                        Log("LD SP, IX");
#endif
                        Wait(10);
                        return;
                    }
                case 0xE5:
                    {
                        // PUSH IX
                        var addr = _sp;
                        addr--;
                        mem[addr] = registers[rIX];
                        addr--;
                        mem[addr] = registers[rIX + 1];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH IX");
#endif
                        Wait(15);
                        return;
                    }
                case 0xE1:
                    {
                        // POP IX
                        var addr = _sp;
                        registers[rIX + 1] = mem[addr++];
                        registers[rIX] = mem[addr++];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP IX");
#endif
                        Wait(14);
                        return;
                    }
                case 0xE3:
                    {
                        // EX (SP), IX
                        var h = registers[rIX];
                        var l = registers[rIX + 1];
                        var addr = _sp;
                        registers[rIX + 1] = mem[addr++];
                        registers[rIX] = mem[addr];
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

                        Add(mem[(ushort)(_ix + d)]);
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
                        var a = registers[rA];
                        Adc(mem[(ushort)(_ix + d)]);
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
                        var b = mem[(ushort)(_ix + d)];

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

                        Sbc(mem[(ushort)(_ix + d)]);
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
                        var b = mem[(ushort)(_ix + d)];

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
                        var b = mem[(ushort)(_ix + d)];

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
                        var b = mem[(ushort)(_ix + d)];

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
                        var b = mem[(ushort)(_ix + d)];

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
                        mem[(ushort)(_ix + d)] = Inc(mem[(ushort)(_ix + d)]);
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
                        mem[(ushort)(_ix + d)] = Dec(mem[(ushort)(_ix + d)]);
#if (DEBUG)
                        Log($"DEC (IX{d:+0;-#})");
#endif
                        Wait(7);
                        return;
                    }
                case 0x09:
                    {
                        AddIx(_bc);
#if (DEBUG)
                        Log("ADD IX, BC");
#endif
                        Wait(4);
                        return;
                    }
                case 0x19:
                    {
                        AddIx(_de);
#if (DEBUG)
                        Log("ADD IX, DE");
#endif
                        Wait(4);
                        return;
                    }
                case 0x29:
                    {
                        AddIx(_ix);
#if (DEBUG)
                        Log("ADD IX, IX");
#endif
                        Wait(4);
                        return;
                    }
                case 0x39:
                    {
                        AddIx(_sp);
#if (DEBUG)
                        Log("ADD IX, SP");
#endif
                        Wait(4);
                        return;
                    }
                case 0x23:
                    {
                        var val = _ix + 1;
                        registers[rIX] = (byte)(val >> 8);
                        registers[rIX + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC IX");
#endif
                        Wait(4);
                        return;
                    }
                case 0x2B:
                    {
                        var val = _ix - 1;
                        registers[rIX] = (byte)(val >> 8);
                        registers[rIX + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC IX");
#endif
                        Wait(4);
                        return;
                    }
                case 0xE9:
                    {
                        var addr = _ix;
                        registers[rPC] = (byte)(addr >> 8);
                        registers[rPC + 1] = (byte)(addr);
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
            Wait(4);
            HALT = true;
        }

        private void ParseFD()
        {
            if (HALT) return;
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
                        registers[rIY + 1] = Fetch();
                        registers[rIY] = Fetch();
#if (DEBUG)
                        Log($"LD IY, 0x{_iy:X4}");
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
                        registers[r] = mem[(ushort)(_iy + d)];
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
                        mem[(ushort)(_iy + d)] = registers[lo];
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
                        mem[(ushort)(_iy + d)] = n;
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
                        registers[rIY + 1] = mem[addr++];
                        registers[rIY] = mem[addr];
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
                        mem[addr++] = registers[rIY + 1];
                        mem[addr] = registers[rIY];
#if (DEBUG)
                        Log($"LD (0x{--addr:X4}), IY");
#endif
                        Wait(20);
                        return;
                    }
                case 0xF9:
                    {
                        // LD SP, IY
                        registers[rSP] = registers[rIY];
                        registers[rSP + 1] = registers[rIY + 1];
#if (DEBUG)
                        Log("LD SP, IY");
#endif
                        Wait(10);
                        return;
                    }
                case 0xE5:
                    {
                        // PUSH IY
                        var addr = _sp;
                        mem[--addr] = registers[rIY];
                        mem[--addr] = registers[rIY + 1];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("PUSH IY");
#endif
                        Wait(15);
                        return;
                    }
                case 0xE1:
                    {
                        // POP IY
                        var addr = _sp;
                        registers[rIY + 1] = mem[addr++];
                        registers[rIY] = mem[addr++];
                        registers[rSP + 1] = (byte)(addr & 0xFF);
                        registers[rSP] = (byte)(addr >> 8);
#if (DEBUG)
                        Log("POP IY");
#endif
                        Wait(14);
                        return;
                    }
                case 0xE3:
                    {
                        // EX (SP), IY
                        var h = registers[rIY];
                        var l = registers[rIY + 1];
                        var addr = _sp;
                        registers[rIY + 1] = mem[addr];
                        mem[addr++] = l;
                        registers[rIY] = mem[addr];
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

                        Add(mem[(ushort)(_iy + d)]);
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
                        var a = registers[rA];
                        Adc(mem[(ushort)(_iy + d)]);

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

                        Sub(mem[(ushort)(_iy + d)]);
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

                        Sbc(mem[(ushort)(_iy + d)]);
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
                        var b = mem[(ushort)(_iy + d)];

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
                        var b = mem[(ushort)(_iy + d)];

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
                        var b = mem[(ushort)(_iy + d)];

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

                        Cmp(mem[(ushort)(_iy + d)]);
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
                        mem[(ushort)(_iy + d)] = Inc(mem[(ushort)(_iy + d)]);
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
                        mem[(ushort)(_iy + d)] = Dec(mem[(ushort)(_iy + d)]);
#if (DEBUG)
                        Log($"DEC (IY{d:+0;-#})");
#endif
                        Wait(7);
                        return;
                    }
                case 0x09:
                    {
                        AddIy(_bc);
#if (DEBUG)
                        Log("ADD IY, BC");
#endif
                        Wait(4);
                        return;
                    }
                case 0x19:
                    {
                        AddIy(_de);
#if (DEBUG)
                        Log("ADD IY, DE");
#endif
                        Wait(4);
                        return;
                    }
                case 0x29:
                    {
                        AddIy(_iy);
#if (DEBUG)
                        Log("ADD IY, IY");
#endif
                        Wait(4);
                        return;
                    }
                case 0x39:
                    {
                        AddIy(_sp);
#if (DEBUG)
                        Log("ADD IY, SP");
#endif
                        Wait(4);
                        return;
                    }
                case 0x23:
                    {
                        var val = _iy + 1;
                        registers[rIY] = (byte)(val >> 8);
                        registers[rIY + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("INC IY");
#endif
                        Wait(4);
                        return;
                    }
                case 0x2B:
                    {
                        var val = _iy - 1;
                        registers[rIY] = (byte)(val >> 8);
                        registers[rIY + 1] = (byte)(val & 0xFF);
#if (DEBUG)
                        Log("DEC IY");
#endif
                        Wait(4);
                        return;
                    }
                case 0xE9:
                    {
                        var addr = _iy;
                        registers[rPC] = (byte)(addr >> 8);
                        registers[rPC + 1] = (byte)(addr);
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
            Wait(4);
            HALT = true;
        }

        private void Add(byte b)
        {
            var a = registers[rA];
            var sum = a + b;
            registers[rA] = (byte)sum;
            var f = (byte)(registers[rF] & 0x28);
            if ((sum & 0x80) > 0)
                f |= (byte)Fl.S;
            if ((byte)sum == 0)
                f |= (byte)Fl.Z;
            if (((a & 0xF) + (b & 0xF)) > 0xF)
                f |= (byte)Fl.H;
            if ((a >= 0x80 && b >= 0x80 && (sbyte)sum > 0) || (a < 0x80 && b < 0x80 && (sbyte)sum < 0))
                f |= (byte)Fl.PV;
            if (sum > 0xFF)
                f |= (byte)Fl.C;
            registers[rF] = f;
        }

        private void Adc(byte b)
        {
            var a = registers[rA];
            var c = (byte)(registers[rF] & (byte)Fl.C);
            var sum = a + b + c;
            registers[rA] = (byte)sum;
            var f = (byte)(registers[rF] & 0x28);
            if ((sum & 0x80) > 0)
                f |= (byte)Fl.S;
            if ((byte)sum == 0)
                f |= (byte)Fl.Z;
            if (((a & 0xF) + (b & 0xF) + c) > 0xF)
                f |= (byte)Fl.H;
            if ((a >= 0x80 && b >= 0x80 && (sbyte)sum > 0) || (a < 0x80 && b < 0x80 && (sbyte)sum < 0))
                f |= (byte)Fl.PV;
            f = (byte)(f & ~(byte)Fl.N);
            if (sum > 0xFF) f |= (byte)Fl.C;
            registers[rF] = f;
        }

        private void Sub(byte b)
        {
            var a = registers[rA];
            var diff = a - b;
            registers[rA] = (byte)diff;
            var f = (byte)(registers[rF] & 0x28);
            if ((diff & 0x80) > 0)
                f |= (byte)Fl.S;
            if (diff == 0)
                f |= (byte)Fl.Z;
            if ((a & 0xF) < (b & 0xF))
                f |= (byte)Fl.H;
            if ((a >= 0x80 && b < 0x80 && (sbyte)diff >= 0) || (a < 0x80 && b >= 0x80 && (sbyte)diff < 0))
                f |= (byte)Fl.PV;
            f |= (byte)Fl.N;
            if (diff < 0)
                f |= (byte)Fl.C;
            registers[rF] = f;
        }

        private void Sbc(byte b)
        {
            var a = registers[rA];
            var c = (byte)(registers[rF] & 0x01);
            var diff = a - b - c;
            registers[rA] = (byte)diff;
            var f = (byte)(registers[rF] & 0x28);
            if ((diff & 0x80) > 0) f |= (byte)Fl.S;
            if (diff == 0) f |= (byte)Fl.Z;
            if ((a & 0xF) < (b & 0xF) + c) f |= (byte)Fl.H;
            if ((a >= 0x80 && b < 0x80 && (sbyte)diff >= 0) || (a < 0x80 && b >= 0x80 && (sbyte)diff < 0))
                f |= (byte)Fl.PV;
            f |= (byte)Fl.N;
            if (diff < 0) f |= (byte)Fl.C;
            registers[rF] = f;
        }

        private void And(byte b)
        {
            var a = registers[rA];
            var res = (byte)(a & b);
            registers[rA] = res;
            var f = (byte)(registers[rF] & 0x28);
            if ((res & 0x80) > 0) f |= (byte)Fl.S;
            if (res == 0) f |= (byte)Fl.Z;
            f |= (byte)Fl.H;
            if (Parity(res)) f |= (byte)Fl.PV;
            registers[rF] = f;
        }

        private void Or(byte b)
        {
            var a = registers[rA];
            var res = (byte)(a | b);
            registers[rA] = res;
            var f = (byte)(registers[rF] & 0x28);
            if ((res & 0x80) > 0)
                f |= (byte)Fl.S;
            if (res == 0)
                f |= (byte)Fl.Z;
            if (Parity(res))
                f |= (byte)Fl.PV;
            registers[rF] = f;
        }

        private void Xor(byte b)
        {
            var a = registers[rA];
            var res = (byte)(a ^ b);
            registers[rA] = res;
            var f = (byte)(registers[rF] & 0x28);
            if ((res & 0x80) > 0)
                f |= (byte)Fl.S;
            if (res == 0)
                f |= (byte)Fl.Z;
            if (Parity(res))
                f |= (byte)Fl.PV;
            registers[rF] = f;
        }

        private void Cmp(byte b)
        {
            var a = registers[rA];
            var diff = a - b;
            var f = (byte)(registers[rF] & 0x28);
            if ((diff & 0x80) > 0)
                f = (byte)(f | 0x80);
            if (diff == 0)
                f = (byte)(f | 0x40);
            if ((a & 0xF) < (b & 0xF))
                f = (byte)(f | 0x10);
            if ((a >= 0x80 && b < 0x80 && (sbyte)diff >= 0) || (a < 0x80 && b >= 0x80 && (sbyte)diff < 0))
                f = (byte)(f | 0x04);
            f = (byte)(f | 0x02);
            if ((diff & 0x100) != 0)
                f = (byte)(f | 0x01);
            registers[rF] = f;
        }

        private byte Inc(byte b)
        {
            var sum = b + 1;
            var f = (byte)(registers[rF] & 0x29); // preserve carry flag
            if ((sum & 0x80) > 0)
                f = (byte)(f | 0x80);
            if ((byte)sum == 0)
                f = (byte)(f | 0x40);
            if ((b & 0xF) == 0xF)
                f = (byte)(f | 0x10);
            if (b == 0x7F)
                f = (byte)(f | 0x04);
            // N is reset (not set) - INC is addition
            registers[rF] = f;

            return (byte)sum;
        }

        private byte Dec(byte b)
        {
            var sum = b - 1;
            var f = (byte)(registers[rF] & 0x29); // preserve carry flag
            if ((sum & 0x80) > 0)
                f = (byte)(f | 0x80);
            if (sum == 0)
                f = (byte)(f | 0x40);
            if ((b & 0x0F) == 0)
                f = (byte)(f | 0x10);
            if (b == 0x80)
                f = (byte)(f | 0x04);
            f = (byte)(f | 0x02);
            registers[rF] = f;

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
            return ((registers[rF] & (byte)mask) > 0) == ((condition & 1) == 1);

        }

        /// <summary>
        ///     Fetches from [PC] and increments PC
        /// </summary>
        /// <returns></returns>
        private byte Fetch()
        {
            var pc = _pc;
            var ret = mem[pc];
            AddressBus = pc;
            DataBus = ret;
#if (DEBUG)
            LogMemRead(pc, ret);
#endif
            pc++;
            registers[rPC] = (byte)(pc >> 8);
            registers[rPC + 1] = (byte)(pc & 0xFF);
            return ret;
        }

        private ushort Fetch16()
        {
            return (ushort)(Fetch() + (Fetch() << 8));
        }

        public void Reset()
        {
            Array.Clear(registers, 0, registers.Length);

            registers[rA] = 0xFF;
            registers[rF] = 0xFF;
            registers[rSP] = 0xFF;
            registers[rSP + 1] = 0xFF;

            //A CPU reset forces both the IFF1 and IFF2 to the reset state, which disables interrupts
            _iff1 = false;
            _iff2 = false;

            _totalTStates = 0;
            _remainingTStates = 0;
        }

        public byte[] GetState()
        {
            var length = registers.Length;
            var ret = new byte[length + 2];
            Array.Copy(registers, ret, length);
            ret[length] = (byte)(_iff1 ? 1 : 0);
            ret[length + 1] = (byte)(_iff2 ? 1 : 0);
            return ret;
        }

        public string DumpState()
        {
            var ret = " BC   DE   HL  SZ-H-PNC A" + Environment.NewLine;
            ret +=
                $"{registers[rB]:X2}{registers[rC]:X2} {registers[rD]:X2}{registers[rE]:X2} {registers[rH]:X2}{registers[rL]:X2} {(registers[rF] & 0x80) >> 7}{(registers[rF] & 0x40) >> 6}{(registers[rF] & 0x20) >> 5}{(registers[rF] & 0x10) >> 4}{(registers[rF] & 0x08) >> 3}{(registers[rF] & 0x04) >> 2}{(registers[rF] & 0x02) >> 1}{(registers[rF] & 0x01)} {registers[rA]:X2}";
            ret +=
                $"\n{registers[rBp]:X2}{registers[rCp]:X2} {registers[rDp]:X2}{registers[rEp]:X2} {registers[rHp]:X2}{registers[rLp]:X2} {(registers[rFp] & 0x80) >> 7}{(registers[rFp] & 0x40) >> 6}{(registers[rFp] & 0x20) >> 5}{(registers[rFp] & 0x10) >> 4}{(registers[rFp] & 0x08) >> 3}{(registers[rFp] & 0x04) >> 2}{(registers[rFp] & 0x02) >> 1}{registers[rFp] & 0x01} {registers[rAp]:X2}";
            ret += Environment.NewLine + Environment.NewLine + "I  R   IX   IY   SP   PC" + Environment.NewLine;
            ret +=
                $"{registers[rI]:X2} {registers[rR]:X2} {registers[rIX]:X2}{registers[rIX + 1]:X2} {registers[rIY]:X2}{registers[rIY + 1]:X2} {registers[rSP]:X2}{registers[rSP + 1]:X2} {registers[rPC]:X2}{registers[rPC + 1]:X2} ";

            ret += Environment.NewLine;
            return ret;
        }

        private void Wait(int t)
        {
            registers[rR] += (byte)((t + 3) / 4);
            _tStates += t;
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
