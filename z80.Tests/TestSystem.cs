using System;

namespace z80.Tests
{
    public class TestSystem
    {
        private readonly byte[] _ram;
        private readonly Z80 _myZ80;
        private readonly SimpleBus _bus;

        public ushort AF => _myZ80.AF;
        public ushort BC => _myZ80.BC;
        public ushort DE => _myZ80.DE;
        public ushort HL => _myZ80.HL;
        public ushort IX => _myZ80.IX;
        public ushort IY => _myZ80.IY;
        public ushort SP => _myZ80.SP;
        public ushort PC => _myZ80.PC;
        public ushort AFp => _myZ80.AFp;
        public ushort BCp => _myZ80.BCp;
        public ushort DEp => _myZ80.DEp;
        public ushort HLp => _myZ80.HLp;

        public byte A => _myZ80.A;
        public byte B => _myZ80.B;
        public byte C => _myZ80.C;
        public byte D => _myZ80.D;
        public byte E => _myZ80.E;
        public byte F => _myZ80.F;
        public byte H => _myZ80.H;
        public byte L => _myZ80.L;
        public byte I => _myZ80.I;
        public byte R => _myZ80.R;

        public byte Ap => _myZ80.Ap;
        public byte Bp => _myZ80.Bp;
        public byte Cp => _myZ80.Cp;
        public byte Dp => _myZ80.Dp;
        public byte Ep => _myZ80.Ep;
        public byte Fp => _myZ80.Fp;
        public byte Hp => _myZ80.Hp;
        public byte Lp => _myZ80.Lp;

        public bool FlagS => (F & 0x80) > 0;
        public bool FlagZ => (F & 0x40) > 0;
        public bool FlagH => (F & 0x10) > 0;
        public bool FlagP => (F & 0x04) > 0;
        public bool FlagN => (F & 0x02) > 0;
        public bool FlagC => (F & 0x01) > 0;

        public bool Iff1 => _myZ80.IFF1;
        public bool Iff2 => _myZ80.IFF2;

        public SimpleBus Bus => _bus;

        public byte Reg8(int reg) => reg switch
        {
            0 => B, 1 => C, 2 => D, 3 => E,
            4 => H, 5 => L, 6 => F, 7 => A,
            _ => throw new ArgumentOutOfRangeException(nameof(reg))
        };

        public TestSystem(byte[] ram)
        {
            _ram = ram;
            _bus = new SimpleBus();
            _myZ80 = new Z80(new SimpleMemory(ram), _bus);
        }

        public void Run()
        {
            int bailout = 1000;

            while (!_myZ80.HALT && bailout > 0)
            {
                _myZ80.Parse();
                bailout--;
            }
            if (!_myZ80.HALT) Console.WriteLine("BAILOUT!");
        }

        public bool Step()
        {
            _myZ80.Parse();
            return _myZ80.HALT;
        }

        public void Reset()
        {
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

        public void RaiseInterrupt(bool maskable, byte data = 0x00)
        {
            if (maskable)
            {
                _bus.INT = true;
                _bus.NMI = false;
                _bus.Data = data;
            }
            else
            {
                _bus.INT = false;
                _bus.NMI = true;
                _bus.Data = data;
            }
        }
    }
}
