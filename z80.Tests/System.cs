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

        public ushort PC { get { return Reg16(_PC); } }

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
        }

        public void Run()
        {
            int bailout = 10000;

            var myZ80 = new Z80(_ram);
            while (!myZ80.Halted & --bailout>0)
            {
                myZ80.Parse();
            }
            dumpedState = myZ80.GetState();
            hasDump = true;
        }

        public void Reset()
        {
            dumpedState = null;
            hasDump = false;
        }
    }
}