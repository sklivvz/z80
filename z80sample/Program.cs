using System;
using System.IO;
using z80;

namespace z80Sample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ram = new byte[65536];
            Array.Clear(ram, 0, ram.Length);
            var inp = File.ReadAllBytes("48.rom");
            if (inp.Length != 16384) throw new InvalidOperationException("Invalid 48.rom file");

            Array.Copy(inp, ram, 16384);

            var myZ80 = new Z80(new SimpleMemory(ram), new SampleBus());
            Console.Clear();
            while (!myZ80.HALT)
            {
                myZ80.Parse();
            }

            Console.WriteLine(Environment.NewLine + myZ80.DumpState());
            for (var i = 0; i < 0x80; i++)
            {
                if (i % 16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", ram[i]);
                if (i % 8 == 7) Console.Write("  ");
                if (i % 16 == 15) Console.WriteLine();
            }
            Console.WriteLine();
            for (var i = 0x4000; i < 0x4100; i++)
            {
                if (i % 16 == 0) Console.Write("{0:X4} | ", i);
                Console.Write("{0:x2} ", ram[i]);
                if (i % 8 == 7) Console.Write("  ");
                if (i % 16 == 15) Console.WriteLine();
            }
        }
    }

    class SampleBus : IBus
    {
        public byte IoRead(ushort address)
        {
            Console.WriteLine($"IN 0x{address:X4}");
            return 0;
        }
        public void IoWrite(ushort address, byte data)
        {
            Console.WriteLine($"OUT 0x{address:X4}, 0x{data:X2}");
        }
        public bool INT => false;
        public bool NMI => false;
        public byte Data => 0x00;
        public bool WAIT => false;
        public bool BUSRQ => false;
        public bool RESET => false;
    }
}
