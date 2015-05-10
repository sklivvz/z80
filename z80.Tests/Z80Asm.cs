using System;
// ReSharper disable InconsistentNaming

namespace z80.Tests
{
    public class Z80Asm
    {
        private readonly byte[] _ram;
        private ushort addr = 0;

        public ushort WritePointer
        {
            get { return addr; }
        }

        public Z80Asm(byte[] ram)
        {
            _ram = ram;
        }

        public void Reset()
        {
            Array.Clear(_ram, 0, _ram.Length);
            addr = 0;
        }

        public void Halt()
        {
            WriteByte(0x76);
        }

        private void WriteByte(int value)
        {
            WriteByte((byte)value);
        }

        private void WriteByte(byte value)
        {
            _ram[addr] = value;
            addr++;
        }

        public void Noop()
        {
            WriteByte(0x00);
        }

        public void LdR(int register, byte value)
        {
            WriteByte(register * 8 + 6);
            WriteByte(value);
        }

        public void LdRR(int register, int register2)
        {
            WriteByte(register * 8 + register2 + 64);
        }

        public void LdR16(int register16, int value)
        {
            WriteByte(1+register16*16);
            WriteByte(value & 0xFF);
            WriteByte(value >> 8);
        }

        public void LdRHl(int register)
        {
            WriteByte(70+register*8);
        }

        public void Data(byte value)
        {
            WriteByte(value);
        }

        public void LdRIxD(int register, byte displacement)
        {
            WriteByte(0xDD);
            WriteByte(70+register*8);
            WriteByte(displacement);
        }

        public void LdIx(int value)
        {
            WriteByte(0xDD);
            WriteByte(33);
            WriteByte(value & 0xFF);
            WriteByte(value >> 8);
        }

        public void LdRIyD(int register, byte displacement)
        {
            WriteByte(0xFD);
            WriteByte(70 + register * 8);
            WriteByte(displacement);
        }

        public void LdIy(int value)
        {
            WriteByte(0xFD);
            WriteByte(33);
            WriteByte(value & 0xFF);
            WriteByte(value >> 8);
        }

        public void LdHLR(byte register)
        {
            WriteByte(0x70+register);
        }
    }
}