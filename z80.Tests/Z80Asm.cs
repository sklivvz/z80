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

        public void LdR(byte register, byte value)
        {
            WriteByte(register * 8 + 6);
            WriteByte(value);
        }

        public void LdRR(byte register, byte register2)
        {
            WriteByte(register * 8 + register2 + 64);
        }

        public void LdR16(byte register16, ushort value)
        {
            WriteByte(1+register16*16);
            WriteByte(value & 0xFF);
            WriteByte(value >> 8);
        }

        public void LdRHl(byte register)
        {
            WriteByte(70+register*8);
        }

        public void Data(byte value)
        {
            WriteByte(value);
        }

        public void LdRIxD(byte register, sbyte displacement)
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

        public void LdRIyD(byte register, sbyte displacement)
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

        public void LdIxDR(byte register, sbyte displacement)
        {
            WriteByte(0xDD);
            WriteByte(0x70 + register);
            WriteByte(displacement);
        }

        public void LdIyDR(byte register, sbyte displacement)
        {
            WriteByte(0xFD);
            WriteByte(0x70 + register);
            WriteByte(displacement);
        }

        public void LdHLN(byte value)
        {
            WriteByte(0x36);
            WriteByte(value);
        }

        public void LdIxDN(sbyte displacement, byte value)
        {
            WriteByte(0xDD);
            WriteByte(0x36);
            WriteByte(displacement);
            WriteByte(value);
        }

        public void LdIyDN(sbyte displacement, byte value)
        {
            WriteByte(0xFD);
            WriteByte(0x36);
            WriteByte(displacement);
            WriteByte(value);
        }

        public void LdABc()
        {
            WriteByte(0x0A);
        }

        public void LdADe()
        {
            WriteByte(0x1A);
        }

        public void LdANn(ushort address)
        {
            WriteByte(0x3A);
            WriteByte(address & 0xFF);
            WriteByte(address >> 8);
        }

        public void LdBcA()
        {
            WriteByte(0x02);
        }

        public void LdDeA()
        {
            WriteByte(0x12);
        }

        public void LdNnA(ushort address)
        {
            WriteByte(0x32);
            WriteByte(address & 0xFF);
            WriteByte(address >> 8);
        }

    }
}