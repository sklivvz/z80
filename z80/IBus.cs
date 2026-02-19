namespace z80
{
    public interface IBus
    {
        byte IoRead(ushort address);
        void IoWrite(ushort address, byte data);

        bool INT { get; }
        bool NMI { get; }
        byte Data { get; }

        bool WAIT { get; }
        bool BUSRQ { get; }
        bool RESET { get; }
    }
}
