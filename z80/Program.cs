using System;
using System.IO;

namespace z80
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

            var myZ80 = new Z80(new Memory(ram, 16384));
            while (!myZ80.Halted)
            {
                myZ80.Parse();
                //Console.Write(Z80.DumpState());
            }


            Console.WriteLine(Environment.NewLine + myZ80.DumpState());
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