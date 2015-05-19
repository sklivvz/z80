using System;
// ReSharper disable InconsistentNaming

namespace z80.Tests
{
    public class Z80Asm
    {
        private readonly byte[] _ram;
        private ushort _address;

        public ushort Position
        {
            get { return _address; }
        }

        public Z80Asm(byte[] ram)
        {
            _ram = ram;
        }

        public void Reset()
        {
            Array.Clear(_ram, 0, _ram.Length);
            _address = 0;
        }

        public void Halt()
        {
            Write(0x76);
        }

        private void Write(int value)
        {
            Write((byte)value);
        }

        private void Write(byte value)
        {
            _ram[_address] = value;
            _address++;
        }

        public void Noop()
        {
            Write(0x00);
        }

        public void LoadRegVal(byte register, byte value)
        {
            Write(register * 8 + 6);
            Write(value);
        }

        public void LoadRegReg(byte register, byte register2)
        {
            Write(register * 8 + register2 + 64);
        }

        public void LoadReg16Val(byte register16, ushort value)
        {
            Write(1 + register16 * 16);
            Write(value & 0xFF);
            Write(value >> 8);
        }

        public void LoadRegAtHl(byte register)
        {
            Write(70 + register * 8);
        }

        public void Data(byte value)
        {
            Write(value);
        }

        public void LoadRegAddrIx(byte register, sbyte displacement)
        {
            Write(0xDD);
            Write(70 + register * 8);
            Write(displacement);
        }

        public void LoadIxVal(ushort value)
        {
            Write(0xDD);
            Write(33);
            Write(value & 0xFF);
            Write(value >> 8);
        }

        public void LoadRegAddrIy(byte register, sbyte displacement)
        {
            Write(0xFD);
            Write(70 + register * 8);
            Write(displacement);
        }

        public void LoadIyVal(int value)
        {
            Write(0xFD);
            Write(33);
            Write(value & 0xFF);
            Write(value >> 8);
        }

        public void LoadAtHLReg(byte register)
        {
            Write(0x70 + register);
        }

        public void LoadIxR(byte register, sbyte displacement)
        {
            Write(0xDD);
            Write(0x70 + register);
            Write(displacement);
        }

        public void LoadIyReg(byte register, sbyte displacement)
        {
            Write(0xFD);
            Write(0x70 + register);
            Write(displacement);
        }

        public void LoadAtHLVal(byte value)
        {
            Write(0x36);
            Write(value);
        }

        public void LoadAtIxVal(sbyte displacement, byte value)
        {
            Write(0xDD);
            Write(0x36);
            Write(displacement);
            Write(value);
        }

        public void LoadIyN(sbyte displacement, byte value)
        {
            Write(0xFD);
            Write(0x36);
            Write(displacement);
            Write(value);
        }

        public void LoadABc()
        {
            Write(0x0A);
        }

        public void LoadADe()
        {
            Write(0x1A);
        }

        public void LoadAAddr(ushort address)
        {
            Write(0x3A);
            Write(address & 0xFF);
            Write(address >> 8);
        }

        public void LoadBcA()
        {
            Write(0x02);
        }

        public void LoadDeA()
        {
            Write(0x12);
        }

        public void LoadAddrA(ushort address)
        {
            Write(0x32);
            Write(address & 0xFF);
            Write(address >> 8);
        }

        public void LoadAI()
        {
            Write(0xED);
            Write(0x57);
        }

        public void LoadIA()
        {
            Write(0xED);
            Write(0x47);
        }

        public void LoadAR()
        {
            Write(0xED);
            Write(0x5F);
        }

        public void LoadRA()
        {
            Write(0xED);
            Write(0x4F);
        }

        public void Di()
        {
            Write(0xF3);
        }

        public void Ei()
        {
            Write(0xFB);
        }

        public void LoadHLAddr(ushort address)
        {
            Write(0x2A);
            Write(address & 0xFF);
            Write(address >> 8);
        }

        public void LoadReg16Addr(byte register16, ushort address)
        {
            Write(0xED);
            Write(0x4B + register16 * 16);
            Write(address & 0xFF);
            Write(address >> 8);
        }

        public void LoadIXAddr(ushort address)
        {
            Write(0xDD);
            Write(0x2A);
            Write(address & 0xFF);
            Write(address >> 8);
        }

        public void LoadIYAddr(ushort address)
        {
            Write(0xFD);
            Write(0x2A);
            Write(address & 0xFF);
            Write(address >> 8);
        }

        public void LoadAddrHl(ushort address)
        {
            Write(0x22);
            Write(address & 0xFF);
            Write(address >> 8);
        }

        public void LoadAddrReg16(byte register16, ushort address)
        {
            Write(0xED);
            Write(0x43 + register16 * 16);
            Write(address & 0xFF);
            Write(address >> 8);
        }

        public void LoadAddrIx(ushort address)
        {
            Write(0xDD);
            Write(0x22);
            Write(address & 0xFF);
            Write(address >> 8);
        }
        public void LoadAddrIy(ushort address)
        {
            Write(0xFD);
            Write(0x22);
            Write(address & 0xFF);
            Write(address >> 8);
        }

        public void LoadSpHl()
        {
            Write(0xF9);
        }

        public void LoadSpIx()
        {
            Write(0xDD);
            Write(0xF9);
        }

        public void LoadSpIy()
        {
            Write(0xFD);
            Write(0xF9);
        }

        public void PushReg16(byte register16)
        {
            Write(0xC5 + register16 * 16);
        }

        public void PushIx()
        {
            Write(0xDD);
            Write(0xE5);
        }

        public void PushIy()
        {
            Write(0xFD);
            Write(0xE5);
        }

        public void PopReg16(byte register16)
        {
            Write(0xC1 + register16 * 16);
        }

        public void PopIx()
        {
            Write(0xDD);
            Write(0xE1);
        }

        public void PopIy()
        {
            Write(0xFD);
            Write(0xE1);
        }

        public void ExDeHl()
        {
            Write(0xEB);
        }

        public void ExAfAfp()
        {
            Write(0x08);
        }

        public void Exx()
        {
            Write(0xD9);
        }
    }
}