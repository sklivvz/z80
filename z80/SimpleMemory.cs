namespace z80
{
    public sealed class SimpleMemory : IMemory
    {
        private readonly byte[] _memory;

        public SimpleMemory(byte[] memory)
        {
            _memory = memory;
        }

        public byte this[ushort address]
        {
            get => _memory[address];
            set => _memory[address] = value;
        }
    }
}
