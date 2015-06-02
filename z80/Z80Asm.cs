using System;

// ReSharper disable InconsistentNaming

namespace z80
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

        public void LoadHlAddr(ushort address)
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

        public void ExAddrSpHl()
        {
            Write(0xE3);
        }

        public void ExAddrSpIx()
        {
            Write(0xDD);
            Write(0xE3);
        }

        public void ExAddrSpIy()
        {
            Write(0xFD);
            Write(0xE3);
        }

        public void Ldi()
        {
            Write(0xED);
            Write(0xA0);
        }

        public void Ldir()
        {
            Write(0xED);
            Write(0xB0);
        }

        public void Ldd()
        {
            Write(0xED);
            Write(0xA8);
        }

        public void Lddr()
        {
            Write(0xED);
            Write(0xB8);
        }

        public void Cpi()
        {
            Write(0xED);
            Write(0xA1);
        }

        public void Cpir()
        {
            Write(0xED);
            Write(0xB1);
        }

        public void Cpd()
        {
            Write(0xED);
            Write(0xA9);
        }

        public void Cpdr()
        {
            Write(0xED);
            Write(0xB9);
        }

        public void AddAReg(byte register)
        {
            Write(0x80 + register);
        }

        public void AddAVal(byte value)
        {
            Write(0xC6);
            Write(value);
        }

        public void AddAAddrHl()
        {
            Write(0x86);
        }

        public void AddAAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0x86);
            Write(displacement);
        }

        public void AddAAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0x86);
            Write(displacement);
        }

        public void AdcAReg(byte register)
        {
            Write(0x88 + register);
        }

        public void AdcAVal(byte value)
        {
            Write(0xCE);
            Write(value);
        }

        public void AdcAAddrHl()
        {
            Write(0x8E);
        }

        public void AdcAAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0x8E);
            Write(displacement);
        }

        public void AdcAAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0x8E);
            Write(displacement);
        }

        public void SubReg(byte register)
        {
            Write(0x90 + register);
        }

        public void SubVal(byte value)
        {
            Write(0xD6);
            Write(value);
        }

        public void SubAddrHl()
        {
            Write(0x96);
        }

        public void SubAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0x96);
            Write(displacement);
        }

        public void SubAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0x96);
            Write(displacement);
        }

        public void SbcAReg(byte register)
        {
            Write(0x98 + register);
        }

        public void SbcAVal(byte value)
        {
            Write(0xDE);
            Write(value);
        }

        public void SbcAAddrHl()
        {
            Write(0x9E);
        }

        public void SbcAAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0x9E);
            Write(displacement);
        }

        public void SbcAAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0x9E);
            Write(displacement);
        }

        public void AndReg(byte reg)
        {
            Write(0xA0 + reg);
        }

        public void AndVal(byte value)
        {
            Write(0xE6);
            Write(value);
        }

        public void AndAddrHl()
        {
            Write(0xA6);
        }

        public void AndAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0xA6);
            Write(displacement);
        }

        public void AndAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0xA6);
            Write(displacement);
        }


        public void OrReg(byte reg)
        {
            Write(0xB0 + reg);
        }

        public void OrVal(byte value)
        {
            Write(0xF6);
            Write(value);
        }

        public void OrAddrHl()
        {
            Write(0xB6);
        }

        public void OrAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0xB6);
            Write(displacement);
        }

        public void OrAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0xB6);
            Write(displacement);
        }

        public void XorReg(byte reg)
        {
            Write(0xA8 + reg);
        }

        public void XorVal(byte value)
        {
            Write(0xEE);
            Write(value);
        }

        public void XorAddrHl()
        {
            Write(0xAE);
        }

        public void XorAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0xAE);
            Write(displacement);
        }

        public void XorAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0xAE);
            Write(displacement);
        }

        public void CpReg(byte register)
        {
            Write(0xB8 + register);
        }

        public void CpVal(byte value)
        {
            Write(0xFE);
            Write(value);
        }

        public void CpAddrHl()
        {
            Write(0xBE);
        }

        public void CpAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0xBE);
            Write(displacement);
        }

        public void CpAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0xBE);
            Write(displacement);
        }

        public void IncReg(byte register)
        {
            Write(0x04 + register * 8);
        }

        public void IncAddrHl()
        {
            Write(0x34);
        }

        public void IncAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0x34);
            Write(displacement);
        }

        public void IncAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0x34);
            Write(displacement);
        }

        public void DecReg(byte register)
        {
            Write(0x05 + register * 8);
        }

        public void DecAddrHl()
        {
            Write(0x35);
        }

        public void DecAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0x35);
            Write(displacement);
        }

        public void DecAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0x35);
            Write(displacement);
        }

        public void Daa()
        {
            Write(0x27);
        }
        public void Cpl()
        {
            Write(0x2F);
        }

        public void Neg()
        {
            Write(0xED);
            Write(0x44);
        }

        public void Ccf()
        {
            Write(0x3F);
        }

        public void Scf()
        {
            Write(0x37);
        }

        public void Im0()
        {
            Write(0xED);
            Write(0x46);
        }

        public void Im1()
        {
            Write(0xED);
            Write(0x56);
        }

        public void Im2()
        {
            Write(0xED);
            Write(0x5E);
        }

        public void AddHlReg16(byte register16)
        {
            Write(0x09 + register16 * 16);
        }

        public void AdcHlReg16(byte register16)
        {
            Write(0xED);
            Write(0x4A + register16 * 16);
        }

        public void SbcHlReg16(byte register16)
        {
            Write(0xED);
            Write(0x42 + register16 * 16);
        }

        public void AddIxReg16(byte register16)
        {
            Write(0xDD);
            Write(0x09 + register16 * 16);
        }

        public void AddIyReg16(byte register16)
        {
            Write(0xFD);
            Write(0x09 + register16 * 16);
        }

        public void IncReg16(byte register16)
        {
            Write(0x03 + register16 * 16);
        }

        public void IncIx()
        {
            Write(0xDD);
            Write(0x23);
        }

        public void IncIy()
        {
            Write(0xFD);
            Write(0x23);
        }

        public void DecReg16(byte register16)
        {
            Write(0x0B + register16 * 16);
        }

        public void DecIx()
        {
            Write(0xDD);
            Write(0x2B);
        }

        public void DecIy()
        {
            Write(0xFD);
            Write(0x2B);
        }

        public void Rlca()
        {
            Write(0x07);
        }

        public void Rla()
        {
            Write(0x17);
        }

        public void Rrca()
        {
            Write(0x0F);
        }

        public void Rra()
        {
            Write(0x1F);
        }

        public void RlcReg(byte register)
        {
            Write(0xCB);
            Write(register);
        }
        public void RlcAddrHl()
        {
            Write(0xCB);
            Write(0x06);
        }
        public void RlcAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0xCB);
            Write(0x06);
            Write(displacement);
        }
        public void RlcAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0xCB);
            Write(0x06);
            Write(displacement);
        }
        public void RlReg(byte register)
        {
            Write(0xCB);
            Write(0x10 + register);
        }
        public void RlAddrHl()
        {
            Write(0xCB);
            Write(0x16);
        }
        public void RlAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0xCB);
            Write(0x16);
            Write(displacement);
        }
        public void RlAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0xCB);
            Write(0x16);
            Write(displacement);
        }
        public void RrcReg(byte register)
        {
            Write(0xCB);
            Write(0x08 + register);
        }
        public void RrcAddrHl()
        {
            Write(0xCB);
            Write(0x0E);
        }
        public void RrcAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0xCB);
            Write(0x0E);
            Write(displacement);
        }
        public void RrcAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0xCB);
            Write(0x0E);
            Write(displacement);
        }
        public void RrReg(byte register)
        {
            Write(0xCB);
            Write(0x18 + register);
        }
        public void RrAddrHl()
        {
            Write(0xCB);
            Write(0x1E);
        }
        public void RrAddrIx(sbyte displacement)
        {
            Write(0xDD);
            Write(0xCB);
            Write(0x1E);
            Write(displacement);
        }
        public void RrAddrIy(sbyte displacement)
        {
            Write(0xFD);
            Write(0xCB);
            Write(0x1E);
            Write(displacement);
        }
    }
}