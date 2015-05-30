using System;

namespace z80.Tests
{
    public class TestSystem
    {
        private const byte _B = 0;
        private const byte _C = 1;
        private const byte _D = 2;
        private const byte _E = 3;
        private const byte _H = 4;
        private const byte _L = 5;
        private const byte _F = 6;
        private const byte _A = 7;
        private const byte _Bp = 8;
        private const byte _Cp = 9;
        private const byte _Dp = 10;
        private const byte _Ep = 11;
        private const byte _Hp = 12;
        private const byte _Lp = 13;
        private const byte _Fp = 14;
        private const byte _Ap = 15;
        private const byte _I = 16;
        private const byte _R = 17;
        private const byte _IX = 18;
        private const byte _IY = 20;
        private const byte _SP = 22;
        private const byte _PC = 24;
        private const byte _IFF1 = 26;
        private const byte _IFF2 = 27;

        private readonly byte[] _ram;
        private byte[] _dumpedState;
        private bool _hasDump;
        private readonly Z80 _myZ80;

        public ushort AF => Reg16(_F);
        public ushort BC => Reg16(_B);
        public ushort DE => Reg16(_D);
        public ushort HL => Reg16(_H);
        public ushort IX => Reg16(_IX);
        public ushort IY => Reg16(_IY);
        public ushort SP => Reg16(_SP);
        public ushort PC => Reg16(_PC);
        public ushort AFp => Reg16(_Fp);
        public ushort BCp => Reg16(_Bp);
        public ushort DEp => Reg16(_Dp);
        public ushort HLp => Reg16(_Hp);

        public byte A => Reg8(_A);
        public byte B => Reg8(_B);
        public byte C => Reg8(_C);
        public byte D => Reg8(_D);
        public byte E => Reg8(_E);
        public byte F => Reg8(_F);
        public byte H => Reg8(_H);
        public byte L => Reg8(_L);
        public byte I => Reg8(_I);
        public byte R => Reg8(_R);

        public byte Ap => Reg8(_Ap);
        public byte Bp => Reg8(_Bp);
        public byte Cp => Reg8(_Cp);
        public byte Dp => Reg8(_Dp);
        public byte Ep => Reg8(_Ep);
        public byte Fp => Reg8(_Fp);
        public byte Hp => Reg8(_Hp);
        public byte Lp => Reg8(_Lp);



        // SZ-H-PNC
        public bool FlagS => (Reg8(_F) & 0x80) > 0;
        public bool FlagZ => (Reg8(_F) & 0x40) > 0;
        public bool FlagH => (Reg8(_F) & 0x10) > 0;
        public bool FlagP => (Reg8(_F) & 0x04) > 0;
        public bool FlagN => (Reg8(_F) & 0x02) > 0;
        public bool FlagC => (Reg8(_F) & 0x01) > 0;

        public bool Iff1 => Reg8(_IFF1) > 0;
        public bool Iff2 => Reg8(_IFF2) > 0;


        public byte Reg8(int reg)
        {
            if (!_hasDump) throw new InvalidOperationException("Don't have a state!");
            return _dumpedState[reg];
        }
        private ushort Reg16(byte reg)
        {
            if (!_hasDump) throw new InvalidOperationException("Don't have a state!");
            var ret = _dumpedState[reg + 1] + _dumpedState[reg] * 256;
            return (ushort)ret;
        }

        public TestSystem(byte[] ram)
        {
            _ram = ram;
            _myZ80 = new Z80(new Memory(ram, 0));
        }

        public void Run()
        {
            int bailout = 1000;

            while (!_myZ80.Halted && bailout > 0)
            {
                _myZ80.Parse();
                bailout--;
                //Console.WriteLine(_myZ80.DumpState());
                //DumpRam();
            }
            _dumpedState = _myZ80.GetState();
            _hasDump = true;
            if (!_myZ80.Halted) Console.WriteLine("BAILOUT!");
        }

        public void Reset()
        {
            _dumpedState = null;
            _hasDump = false;
            _myZ80.Reset();
        }

        public void DumpCpu()
        {
            Console.WriteLine(_myZ80.DumpState());
        }

        public void DumpRam()
        {

            for (var i = 0; i < 0x80; i++)
            {
                if (i % 16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", _ram[i]);
                if (i % 8 == 7) Console.Write("  ");
                if (i % 16 == 15) Console.WriteLine();
            }
            Console.WriteLine();
            for (var i = 0x8080; i < 0x80A0; i++)
            {
                if (i % 16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", _ram[i]);
                if (i % 8 == 7) Console.Write("  ");
                if (i % 16 == 15) Console.WriteLine();
            }

        }
    }
}