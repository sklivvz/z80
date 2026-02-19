namespace z80
{
    public sealed class SimpleBus : IBus
    {
        private readonly byte[] _inputs = new byte[0x10000];
        private readonly byte[] _outputs = new byte[0x10000];
        private bool _nmi;
        private byte _data;

        public byte IoRead(ushort address) => _inputs[address];

        public void IoWrite(ushort address, byte data) => _outputs[address] = data;

        private bool _int;

        public bool INT
        {
            get { var ret = _int; _int = false; return ret; }
            set { _int = value; }
        }

        public bool NMI
        {
            get { var ret = _nmi; _nmi = false; return ret; }
            set { _nmi = value; }
        }

        public byte Data
        {
            get { var ret = _data; _data = 0x00; return ret; }
            set { _data = value; }
        }

        public bool WAIT => false;
        public bool BUSRQ => false;
        public bool RESET => false;

        public void SetInput(ushort address, byte value) => _inputs[address] = value;
        public byte GetOutput(ushort address) => _outputs[address];
    }
}
