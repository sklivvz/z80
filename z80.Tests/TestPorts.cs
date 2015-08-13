namespace z80.Tests
{
    public class TestPorts: IPorts
    {
        private readonly byte[] inputs = new byte[0x10000];
        private readonly byte[] outputs = new byte[0x10000];
        private byte _data;
        private bool _mi;
        private bool _nmi;

        public byte ReadPort(ushort address)
        {
            return inputs[address];
        }

        public void SetInput(ushort address, byte value)
        {
            inputs[address] = value;
        }

        public byte GetOutput(ushort address)
        {
            return outputs[address];
        }

        public void WritePort(ushort address, byte value)
        {
            outputs[address] = value;
        }

        public bool NMI
        {
            get
            {
                var ret = _nmi;
                _nmi = false;
                return ret;
            }
            set { _nmi = value; }
        }

        public bool MI
        {
            get
            {
                var ret = _mi;
                _mi = false;
                return ret;
            }
            set { _mi = value; }
        }

        public byte Data
        {
            get
            {
                var ret = _data;
                _data = 0x00;
                return ret;
            }
            set { _data = value; }
        }

    }
}