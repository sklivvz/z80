namespace z80
{
    public interface IMemory
    {
        byte this[ushort address] { get; set; }
    }
}
