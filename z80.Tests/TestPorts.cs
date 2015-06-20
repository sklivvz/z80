namespace z80.Tests
{
    public class TestPorts: IPorts
    {
        private readonly byte[] inputs = new byte[0x10000];
        private readonly byte[] outputs = new byte[0x10000];

        public byte Read(ushort address)
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

        public void Write(ushort address, byte value)
        {
            outputs[address] = value;
        }
    }
}