using System;
using z80;

namespace ula
{
    public class Ula
    {
        public const int ScreenWidth = 352;
        public const int ScreenHeight = 288;
        public const int BorderTop = 48;
        public const int BorderBottom = 48;
        public const int BorderLeft = 48;
        public const int BorderRight = 48;
        public const int DisplayWidth = 256;
        public const int DisplayHeight = 192;

        private readonly IMemory _memory;
        private readonly IUlaBus _bus;
        private readonly ushort _screenBaseAddress;
        private int _borderColour;
        private int _frameCounter;
        private bool _flashActive;
        private readonly uint[] _scanlineBuffer = new uint[ScreenWidth];

        public uint[] FrameBuffer { get; }
        public bool InterruptRequested { get; private set; }
        public bool MicOutput { get; private set; }
        public bool SpeakerOutput { get; private set; }

        public Ula(IMemory memory, IUlaBus bus, uint[] frameBuffer, ushort screenBaseAddress = 0x4000)
        {
            if (frameBuffer.Length != ScreenWidth * ScreenHeight)
                throw new ArgumentException($"Frame buffer must be exactly {ScreenWidth * ScreenHeight} elements.", nameof(frameBuffer));

            _memory = memory;
            _bus = bus;
            _screenBaseAddress = screenBaseAddress;
            FrameBuffer = frameBuffer;
        }

        public void WritePort(byte data)
        {
            _borderColour = data & 0x07;
            MicOutput = (data & 0x08) != 0;
            SpeakerOutput = (data & 0x10) != 0;
        }

        public byte ReadPort(byte highByte)
        {
            byte keyboard = (byte)(_bus.ReadKeyboard(highByte) & 0x1F);
            byte ear = (byte)(_bus.EarInput ? 0x40 : 0x00);
            return (byte)(keyboard | ear);
        }

        public void RenderFrame()
        {
            _frameCounter++;
            if (_frameCounter >= 16)
            {
                _frameCounter = 0;
                _flashActive = !_flashActive;
            }

            uint borderArgb = ScreenRenderer.ColourToArgb(_borderColour, false);

            for (int line = 0; line < ScreenHeight; line++)
            {
                int offset = line * ScreenWidth;

                if (line < BorderTop || line >= BorderTop + DisplayHeight)
                {
                    for (int x = 0; x < ScreenWidth; x++)
                        FrameBuffer[offset + x] = borderArgb;
                }
                else
                {
                    int displayY = line - BorderTop;
                    ScreenRenderer.RenderDisplayScanlineWithBorder(_memory, displayY, _flashActive, _borderColour, _scanlineBuffer, _screenBaseAddress);
                    Array.Copy(_scanlineBuffer, 0, FrameBuffer, offset, ScreenWidth);
                }
            }

            InterruptRequested = true;
        }

        public void AcknowledgeInterrupt()
        {
            InterruptRequested = false;
        }
    }
}
