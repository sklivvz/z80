using z80;

namespace ula
{
    public static class ScreenRenderer
    {
        private static readonly uint[] NormalColours =
        {
            0xFF000000, // 0: black
            0xFF0000D7, // 1: blue
            0xFFD70000, // 2: red
            0xFFD700D7, // 3: magenta
            0xFF00D700, // 4: green
            0xFF00D7D7, // 5: cyan
            0xFFD7D700, // 6: yellow
            0xFFD7D7D7, // 7: white
        };

        private static readonly uint[] BrightColours =
        {
            0xFF000000, // 0: black
            0xFF0000FF, // 1: blue
            0xFFFF0000, // 2: red
            0xFFFF00FF, // 3: magenta
            0xFF00FF00, // 4: green
            0xFF00FFFF, // 5: cyan
            0xFFFFFF00, // 6: yellow
            0xFFFFFFFF, // 7: white
        };

        public static uint ColourToArgb(int colour, bool bright)
        {
            return bright ? BrightColours[colour] : NormalColours[colour];
        }

        public static void DecodeAttribute(byte attr, out int ink, out int paper, out bool bright, out bool flash)
        {
            ink = attr & 0x07;
            paper = (attr >> 3) & 0x07;
            bright = (attr & 0x40) != 0;
            flash = (attr & 0x80) != 0;
        }

        public static ushort PixelAddress(int x, int y, ushort screenBase = 0x4000)
        {
            int high = (screenBase >> 8) | ((y & 0xC0) >> 3) | (y & 0x07);
            int low = ((y & 0x38) << 2) | (x >> 3);
            return (ushort)((high << 8) | low);
        }

        public static void RenderPixelByte(byte pixels, byte attr, bool flashActive, uint[] output)
        {
            RenderPixelByte(pixels, attr, flashActive, output, 0);
        }

        public static void RenderPixelByte(byte pixels, byte attr, bool flashActive, uint[] output, int offset)
        {
            DecodeAttribute(attr, out var ink, out var paper, out var bright, out var flash);

            var foreground = ColourToArgb(ink, bright);
            var background = ColourToArgb(paper, bright);

            if (flash && flashActive)
            {
                var temp = foreground;
                foreground = background;
                background = temp;
            }

            for (int i = 0; i < 8; i++)
            {
                bool isSet = (pixels & (0x80 >> i)) != 0;
                output[offset + i] = isSet ? foreground : background;
            }
        }

        public static ushort AttributeAddress(int column, int row, ushort screenBase = 0x4000)
        {
            return (ushort)(screenBase + 0x1800 + row * 32 + column);
        }

        public static void RenderBorderScanline(int borderColour, uint[] output)
        {
            uint colour = ColourToArgb(borderColour, false);

            for (int i = 0; i < output.Length; i++)
            {
                output[i] = colour;
            }
        }

        public static void RenderDisplayScanline(IMemory memory, int y, bool flashActive, uint[] output, ushort screenBase = 0x4000)
        {
            RenderDisplayScanlineAt(memory, y, flashActive, output, 0, screenBase);
        }

        public static void RenderDisplayScanlineWithBorder(IMemory memory, int y, bool flashActive, int borderColour, uint[] output, ushort screenBase = 0x4000)
        {
            RenderBorderScanline(borderColour, output);
            RenderDisplayScanlineAt(memory, y, flashActive, output, 48, screenBase);
        }

        private static void RenderDisplayScanlineAt(IMemory memory, int y, bool flashActive, uint[] output, int outputOffset, ushort screenBase)
        {
            int row = y / 8;

            for (int col = 0; col < 32; col++)
            {
                byte pixels = memory[PixelAddress(col * 8, y, screenBase)];
                byte attr = memory[AttributeAddress(col, row, screenBase)];
                RenderPixelByte(pixels, attr, flashActive, output, outputOffset + col * 8);
            }
        }
    }
}
