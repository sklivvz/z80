using System;
using System.Threading;

namespace z80
{
    public static class Z80
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
        private static readonly byte[] registers = new byte[26];
        private static byte[] _ram;
        private static DateTime _clock = DateTime.UtcNow;
        public static bool Halted { get; private set; }

        private static void Wait(int t)
        {
            const int realTicksPerTick = 250; // 4MHz
            var ticks = t*realTicksPerTick;
            var sleep = _clock + new TimeSpan(ticks) - DateTime.UtcNow;
            if (sleep.Ticks > 0)
            {
                Thread.Sleep(sleep);
                _clock = _clock + new TimeSpan(ticks);
            }
            else
            {
                Console.WriteLine("*");
                _clock = DateTime.UtcNow;
            }
        }

        public static void Parse()
        {
            if (Halted) return;
            var mc = Fetch();
            if (mc == 0xDD) // IX mode
            {
                ParseDD();
                return;
            }
            if (mc == 0xFD) // IY mode
            {
                ParseFD();
                return;
            }
            var hi = (byte) (mc >> 6);
            var lo = (byte) (mc & 0x07);
            var r = (byte) ((mc >> 3) & 0x07);

            switch (mc)
            {
                case 0x00:
                    // NOP
                    Log("NOP");
                    return;
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
                    Log("LD {0}, {1}", RName(r), RName(lo));
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
                    Log("LD {0}, {1}", RName(r), n, mc);
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
                    Log("LD {0}, (HL)", RName(r));
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
                    Log("LD (HL), {0}", RName(r));
                    var addr = (registers[H] << 8) + registers[L];
                    _ram[addr] = registers[r];
                    Wait(7);
                    return;
                }
                case 0x36:
                {
                    // LD (HL), n
                    var n = Fetch();
                    Log("LD (HL), {0}", n);
                    var addr = (registers[H] << 8) + registers[L];
                    _ram[addr] = n;
                    Wait(10);
                    return;
                }
                case 0x76:
                    //HALT
                    Log("HALT");
                    Halted = true;
                    return;
                case 0x0A:
                {
                    // LD A, (BC)
                    var addr = (registers[B] << 8) + registers[C];
                    registers[A] = _ram[addr];
                    Log("LD A, (BC)");
                    Wait(7);
                    return;
                }
                case 0x1A:
                {
                    // LD A, (DE)
                    var addr = (registers[D] << 8) + registers[E];
                    registers[A] = _ram[addr];
                    Log("LD A, (DE)");
                    Wait(7);
                    return;
                }
                case 0x3A:
                {
                    // LD A, (nn)
                    var addr = (Fetch() << 8) + Fetch();
                    registers[A] = _ram[addr];
                    Log("LD A, ({0:x4})", addr);
                    Wait(13);
                    return;
                }
                case 0x02:
                {
                    // LD (BC), A
                    var addr = (registers[B] << 8) + registers[C];
                    _ram[addr] = registers[A] ;
                    Log("LD (BC), A");
                    Wait(7);
                    return;
                }
                case 0x12:
                {
                    // LD (DE), A
                    var addr = (registers[D] << 8) + registers[E];
                    _ram[addr] = registers[A];
                    Log("LD (DE), A");
                    Wait(7);
                    return;
                }
                case 0x32:
                {
                    // LD (nn), A 
                    var addr = (Fetch() << 8) + Fetch();
                    _ram[addr] = registers[A];
                    Log("LD ({0:x4}),A", addr);
                    Wait(13);
                    return;
                }
            }

            Log("{3:X2}: {0:X2} {1:X2} {2:X2}", hi, lo, r, mc);
            Halted = true;
        }

        private static void ParseDD()
        {
            if (Halted) return;
            var mc = Fetch();
            var hi = (byte) (mc >> 6);
            var lo = (byte) (mc & 0x07);
            var r = (byte) ((mc >> 3) & 0x07);

            switch (mc)
            {
                case 0x46:
                case 0x4e:
                case 0x56:
                case 0x5e:
                case 0x66:
                case 0x6e:
                case 0x7e:
                {
                    // LD r, (IX+d)
                    var d = (sbyte) Fetch();
                    Log("LD {0}, (IX{1:+#;-#})", RName(r), d);
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
                    var d = (sbyte) Fetch();
                    Log("LD (IX{1:+#;-#}), {0}", RName(r), d);
                    var addr = (registers[IX] << 8) + registers[IX + 1] + d;
                    _ram[addr] = registers[r];
                    Wait(19);
                    return;
                }
                case 0x36:
                {
                    // LD (IX+d), n
                    var d = (sbyte) Fetch();
                    var n = Fetch();
                    Log("LD (IX{1:+#;-#}), {0}", n, d);
                    var addr = (registers[IX] << 8) + registers[IX + 1] + d;
                    _ram[addr] = n;
                    Wait(19);
                    return;
                }
            }
            Log("{3:X2}: {0:X2} {1:X2} {2:X2}", hi, lo, r, mc);
            Halted = true;
        }

        private static void ParseFD()
        {
            if (Halted) return;
            var mc = Fetch();
            var hi = (byte) (mc >> 6);
            var lo = (byte) (mc & 0x07);
            var r = (byte) ((mc >> 3) & 0x07);

            switch (mc)
            {
                case 0x46:
                case 0x4e:
                case 0x56:
                case 0x5e:
                case 0x66:
                case 0x6e:
                case 0x7e:
                {
                    // LD r, (IY+d)
                    var d = (sbyte) Fetch();
                    Log("LD {0}, (IY{1:+#;-#})", RName(r), d);
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
                    var d = (sbyte) Fetch();
                    Log("LD (IY{1:+#;-#}), {0}", RName(r), d);
                    var addr = (registers[IY] << 8) + registers[IY + 1] + d;
                    _ram[addr] = registers[r];
                    Wait(19);
                    return;
                }
                case 0x36:
                {
                    // LD (IY+d), n
                    var d = (sbyte) Fetch();
                    var n = Fetch();
                    Log("LD (IY{1:+#;-#}), {0}", n, d);
                    var addr = (registers[IY] << 8) + registers[IY + 1] + d;
                    _ram[addr] = n;
                    Wait(19);
                    return;
                }
            }
            Log("{3:X2}: {0:X2} {1:X2} {2:X2}", hi, lo, r, mc);
            Halted = true;
        }

        private static void Log(string format, params object[] vals)
        {
#if(DEBUG)
            //Console.WriteLine(format, vals);
#endif
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

        /// <summary>
        ///     Fetches from [PC] and increments PC
        /// </summary>
        /// <returns></returns>
        private static byte Fetch()
        {
            var pc = (ushort) ((registers[PC] << 8) + registers[PC + 1]);
            var ret = _ram[pc];
            pc++;
            registers[PC] = (byte) (pc >> 8);
            registers[PC + 1] = (byte) (pc & 0xFF);
            return ret;
        }

        public static void Reset(byte[] ram)
        {
            Array.Clear(registers, 0, registers.Length);
            _ram = ram;
            _clock = DateTime.UtcNow;
        }

        public static string DumpState()
        {
            var ret = "BC    DE    HL    F  A" + Environment.NewLine;
            var i = 0;
            for (; i < 8; i++) ret += string.Format("{0:X2} ", registers[i]);
            ret += Environment.NewLine + "BC'   DE'   HL'   F' A'" + Environment.NewLine;
            for (; i < 16; i++) ret += string.Format("{0:X2} ", registers[i]);
            ret += Environment.NewLine + "I  R  IX    IY    SP    PC    " + Environment.NewLine;
            for (; i < registers.Length; i++) ret += string.Format("{0:X2} ", registers[i]);
            ret += Environment.NewLine;
            return ret;
        }
    }


    internal class Program
    {
        private static void Main(string[] args)
        {
            var ram = new byte[65536];
            Array.Clear(ram, 0, ram.Length);
            ram[0x0000] = 0x26;
            ram[0x0001] = 0x80; // LD H, 128
            ram[0x0002] = 0x6C; // LD L, H
            ram[0x0003] = 0x7E; // LD A, (HL)
            ram[0x0004] = 0xDD;
            ram[0x0005] = 0x46;
            ram[0x0006] = 0x01; // LD B, (IX+1)
            ram[0x0007] = 0x76; // HALT
            ram[0x8080] = 0x42; // Data
            Z80.Reset(ram);
            while (!Z80.Halted)
            {
                Z80.Parse();
                //Console.Write(Z80.DumpState());
            }
            Console.WriteLine(Environment.NewLine + Z80.DumpState());
            for (var i = 0; i < 0x80; i++)
            {
                if (i%16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", ram[i]);
                if (i%8 == 7) Console.Write("  ");
                if (i%16 == 15) Console.WriteLine();
            }
            Console.WriteLine();
            for (var i = 0x8080; i < 0x80A0; i++)
            {
                if (i%16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", ram[i]);
                if (i%8 == 7) Console.Write("  ");
                if (i%16 == 15) Console.WriteLine();
            }
            Console.ReadLine();
        }
    }
}