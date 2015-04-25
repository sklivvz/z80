using System;
namespace z80
{
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
            ram[0x0007] = 0xDD;
            ram[0x0008] = 0x46;
            ram[0x0009] = 0x01; // LD B, (IX+1)
            ram[0x000A] = 0xDD;
            ram[0x000B] = 0x46;
            ram[0x000C] = 0x01; // LD B, (IX+1)
            ram[0x000D] = 0x76; // HALT
            ram[0x8080] = 0x42; // Data

            var myZ80 = new Z80(ram);
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