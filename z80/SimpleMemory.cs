namespace z80
{
    public sealed class SimpleMemory : IMemory
    {
        private readonly byte[] _memory;
        private readonly ushort _romSize;

        public SimpleMemory(byte[] memory, ushort romSize = 0)
        {
            _memory = memory;
            _romSize = romSize;
        }

        public byte this[ushort address]
        {
            get => _memory[address];
            set
            {
                if (address >= _romSize)
                    _memory[address] = value;
            }
        }
    }
}
