namespace z80
{
    public interface IPorts
    {
        byte Read(ushort address);
        void Write(ushort address, byte value);
    }
}