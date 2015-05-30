namespace z80
{
    public class Memory
    {
        private readonly byte[] _memory;
        private readonly ushort _ramStart;

        public Memory(byte[] memory, ushort ramStart)
        {
            _memory = memory;
            _ramStart = ramStart;
        }

        public byte this[ushort address]
        {
            get
            {
                return _memory[address];
            }
            set
            {
                if (address >= _ramStart)
                    _memory[address] = value;
            }
        }
    }
}