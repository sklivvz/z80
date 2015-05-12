using System;

namespace z80.Tests
{
    public class System
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

        private readonly byte[] _ram;
        private byte[] dumpedState;
        private bool hasDump;
        private readonly Z80 _myZ80;

        public ushort PC { get { return Reg16(_PC); } }
        public ushort SP { get { return Reg16(_SP); } }
        public byte A { get { return Reg8(_A); } }
        public byte B { get { return Reg8(_B); } }
        public byte C { get { return Reg8(_C); } }
        public byte D { get { return Reg8(_D); } }
        public byte E { get { return Reg8(_E); } }
        public byte H { get { return Reg8(_H); } }
        public byte L { get { return Reg8(_L); } }
        public byte I { get { return Reg8(_I); } }
        public byte R { get { return Reg8(_R); } }

        // SZ-H-PNC
        public bool FlagS { get { return (Reg8(_F) & 0x80) > 0; } }
        public bool FlagZ { get { return (Reg8(_F) & 0x40) > 0; } }
        public bool FlagH { get { return (Reg8(_F) & 0x10) > 0; } }
        public bool FlagP { get { return (Reg8(_F) & 0x04) > 0; } }
        public bool FlagN { get { return (Reg8(_F) & 0x02) > 0; } }
        public bool FlagC { get { return (Reg8(_F) & 0x01) > 0; } }

        public byte Reg8(int reg)
        {
            if (!hasDump) throw new InvalidOperationException("Don't have a state!");
            return dumpedState[reg];
        }
        public ushort Reg16(byte reg)
        {
            if (!hasDump) throw new InvalidOperationException("Don't have a state!");
            var ret = dumpedState[reg + 1] + dumpedState[reg] * 256;
            return (ushort)ret;
        }

        public System(byte[] ram)
        {
            _ram = ram;
            _myZ80 = new Z80(_ram);
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
            dumpedState = _myZ80.GetState();
            hasDump = true;
            if (!_myZ80.Halted) Console.WriteLine("BAILOUT!");
        }

        public void Reset()
        {
            dumpedState = null;
            hasDump = false;
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