using NUnit.Framework;

namespace ula.Tests
{
    [TestFixture]
    public class ScreenRendererTests
    {
        [TestCase(0, 0xFF000000u)]
        [TestCase(1, 0xFF0000D7u)]
        [TestCase(2, 0xFFD70000u)]
        [TestCase(3, 0xFFD700D7u)]
        [TestCase(4, 0xFF00D700u)]
        [TestCase(5, 0xFF00D7D7u)]
        [TestCase(6, 0xFFD7D700u)]
        [TestCase(7, 0xFFD7D7D7u)]
        public void ColourToArgb_AllNormalColours(int colour, uint expected)
        {
            Assert.That(ScreenRenderer.ColourToArgb(colour, bright: false), Is.EqualTo(expected));
        }

        [TestCase(0, 0xFF000000u)]
        [TestCase(1, 0xFF0000FFu)]
        [TestCase(2, 0xFFFF0000u)]
        [TestCase(3, 0xFFFF00FFu)]
        [TestCase(4, 0xFF00FF00u)]
        [TestCase(5, 0xFF00FFFFu)]
        [TestCase(6, 0xFFFFFF00u)]
        [TestCase(7, 0xFFFFFFFFu)]
        public void ColourToArgb_AllBrightColours(int colour, uint expected)
        {
            Assert.That(ScreenRenderer.ColourToArgb(colour, bright: true), Is.EqualTo(expected));
        }

        [Test]
        public void DecodeAttribute_InkPaperBrightFlash()
        {
            // 0b_1_1_010_001 = flash, bright, paper=2, ink=1
            ScreenRenderer.DecodeAttribute(0xD1, out var ink, out var paper, out var bright, out var flash);
            Assert.That(ink, Is.EqualTo(1));
            Assert.That(paper, Is.EqualTo(2));
            Assert.That(bright, Is.True);
            Assert.That(flash, Is.True);
        }

        [Test]
        public void RenderPixelByte_AllSet_ReturnsInkColours()
        {
            // attr: ink=7(white), paper=0(black), no bright, no flash -> 0b_00_000_111 = 0x07
            var result = new uint[8];
            ScreenRenderer.RenderPixelByte(0xFF, 0x07, flashActive: false, result);
            var white = ScreenRenderer.ColourToArgb(7, false); // 0xFFD7D7D7
            Assert.That(result, Is.All.EqualTo(white));
        }

        [Test]
        public void RenderPixelByte_AllClear_ReturnsPaperColours()
        {
            var result = new uint[8];
            ScreenRenderer.RenderPixelByte(0x00, 0x07, flashActive: false, result);
            var black = ScreenRenderer.ColourToArgb(0, false); // 0xFF000000
            Assert.That(result, Is.All.EqualTo(black));
        }

        [Test]
        public void RenderPixelByte_Alternating_ReturnsCorrectPattern()
        {
            // 0xAA = 10101010, attr: ink=1(blue), paper=2(red), no bright, no flash -> 0b_00_010_001 = 0x11
            var result = new uint[8];
            ScreenRenderer.RenderPixelByte(0xAA, 0x11, flashActive: false, result);
            var blue = ScreenRenderer.ColourToArgb(1, false);
            var red = ScreenRenderer.ColourToArgb(2, false);
            Assert.That(result, Is.EqualTo(new[] { blue, red, blue, red, blue, red, blue, red }));
        }

        [Test]
        public void RenderPixelByte_FlashActive_SwapsInkAndPaper()
        {
            // attr: ink=7, paper=0, flash=true -> 0b_1_0_000_111 = 0x87
            var result = new uint[8];
            ScreenRenderer.RenderPixelByte(0xFF, 0x87, flashActive: true, result);
            var black = ScreenRenderer.ColourToArgb(0, false); // paper colour (swapped to foreground)
            Assert.That(result, Is.All.EqualTo(black));
        }

        [Test]
        public void RenderPixelByte_FlashInactive_NoSwap()
        {
            // Same flash attr, but flashActive=false -> no swap
            var result = new uint[8];
            ScreenRenderer.RenderPixelByte(0xFF, 0x87, flashActive: false, result);
            var white = ScreenRenderer.ColourToArgb(7, false);
            Assert.That(result, Is.All.EqualTo(white));
        }

        [Test]
        public void RenderPixelByte_BrightAttribute_UsesBrightPalette()
        {
            // attr: ink=1, paper=0, bright=true -> 0b_0_1_000_001 = 0x41
            var result = new uint[8];
            ScreenRenderer.RenderPixelByte(0xFF, 0x41, flashActive: false, result);
            var brightBlue = ScreenRenderer.ColourToArgb(1, true); // 0xFF0000FF
            Assert.That(result, Is.All.EqualTo(brightBlue));
        }

        [Test]
        public void PixelAddress_TopLeft_Returns0x4000()
        {
            Assert.That(ScreenRenderer.PixelAddress(0, 0), Is.EqualTo(0x4000));
        }

        [Test]
        public void PixelAddress_Row1_Returns0x4100()
        {
            Assert.That(ScreenRenderer.PixelAddress(0, 1), Is.EqualTo(0x4100));
        }

        [Test]
        public void PixelAddress_Row8_Returns0x4020()
        {
            Assert.That(ScreenRenderer.PixelAddress(0, 8), Is.EqualTo(0x4020));
        }

        [Test]
        public void PixelAddress_SecondThird_Returns0x4800()
        {
            Assert.That(ScreenRenderer.PixelAddress(0, 64), Is.EqualTo(0x4800));
        }

        [Test]
        public void PixelAddress_BottomRight_Returns0x57FF()
        {
            // x=248 means byte column 31 (248/8=31), y=191
            Assert.That(ScreenRenderer.PixelAddress(248, 191), Is.EqualTo(0x57FF));
        }

        [TestCase(8, 0, 0x4001)]    // second byte column, row 0
        [TestCase(0, 7, 0x4700)]    // last pixel row of first char row
        [TestCase(0, 63, 0x47E0)]   // last pixel row of first third
        [TestCase(0, 128, 0x5000)]  // third third
        [TestCase(0, 191, 0x57E0)]  // last row, first column
        [TestCase(255, 0, 0x401F)]  // first row, last byte (pixel 255 = byte 31)
        public void PixelAddress_VariousCoordinates(int x, int y, int expected)
        {
            Assert.That(ScreenRenderer.PixelAddress(x, y), Is.EqualTo(expected));
        }

        [Test]
        public void AttributeAddress_TopLeft_Returns0x5800()
        {
            Assert.That(ScreenRenderer.AttributeAddress(0, 0), Is.EqualTo(0x5800));
        }

        [Test]
        public void AttributeAddress_BottomRight_Returns0x5AFF()
        {
            Assert.That(ScreenRenderer.AttributeAddress(31, 23), Is.EqualTo(0x5AFF));
        }

        [TestCase(1, 0, 0x5801)]   // second column, first row
        [TestCase(0, 1, 0x5820)]   // first column, second row
        [TestCase(31, 0, 0x581F)]  // last column, first row
        [TestCase(0, 23, 0x5AE0)]  // first column, last row
        [TestCase(15, 12, 0x598F)] // middle of screen
        public void AttributeAddress_VariousPositions(int column, int row, int expected)
        {
            Assert.That(ScreenRenderer.AttributeAddress(column, row), Is.EqualTo(expected));
        }

        private class TestMemory : z80.IMemory
        {
            private readonly byte[] _ram = new byte[0x10000];
            public byte this[ushort address]
            {
                get => _ram[address];
                set => _ram[address] = value;
            }
        }

        [Test]
        public void RenderDisplayScanline_Row0_ReadsCorrectBytes()
        {
            var mem = new TestMemory();
            // Set first pixel byte at 0x4000 to 0xFF (all pixels set)
            mem[0x4000] = 0xFF;
            // Set attribute for first cell (0x5800) to ink=7, paper=0 = 0x07
            mem[0x5800] = 0x07;

            var output = new uint[256];
            ScreenRenderer.RenderDisplayScanline(mem, 0, false, output);

            var white = ScreenRenderer.ColourToArgb(7, false);
            var black = ScreenRenderer.ColourToArgb(0, false);

            // First 8 pixels should be white (0xFF = all ink)
            for (int i = 0; i < 8; i++)
                Assert.That(output[i], Is.EqualTo(white), $"pixel {i}");

            // Next 8 pixels should be black (0x00 = all paper, default memory is 0)
            for (int i = 8; i < 16; i++)
                Assert.That(output[i], Is.EqualTo(black), $"pixel {i}");
        }

        [Test]
        public void RenderDisplayScanline_Row191_ReadsBottomOfScreen()
        {
            var mem = new TestMemory();
            // Row 191: pixel address for (0,191) = 0x57E0, attribute for (0, 23) = 0x5AE0
            mem[0x57E0] = 0xAA; // alternating pattern
            mem[0x5AE0] = 0x11; // ink=1(blue), paper=2(red)

            var output = new uint[256];
            ScreenRenderer.RenderDisplayScanline(mem, 191, false, output);

            var blue = ScreenRenderer.ColourToArgb(1, false);
            var red = ScreenRenderer.ColourToArgb(2, false);

            Assert.That(output[0], Is.EqualTo(blue));  // bit 7 set
            Assert.That(output[1], Is.EqualTo(red));   // bit 6 clear
            Assert.That(output[2], Is.EqualTo(blue));  // bit 5 set
            Assert.That(output[3], Is.EqualTo(red));   // bit 4 clear
        }

        [Test]
        public void RenderBorderScanline_FillsWithBorderColour()
        {
            var output = new uint[352];
            ScreenRenderer.RenderBorderScanline(2, output); // border colour 2 = red

            var red = ScreenRenderer.ColourToArgb(2, false);
            Assert.That(output, Is.All.EqualTo(red));
        }

        [Test]
        public void RenderDisplayScanlineWithBorder_IncludesBorderAndDisplay()
        {
            var mem = new TestMemory();
            mem[0x4000] = 0xFF;  // first pixel byte all set
            mem[0x5800] = 0x07;  // ink=7(white), paper=0(black)

            var output = new uint[352]; // 48 + 256 + 48
            ScreenRenderer.RenderDisplayScanlineWithBorder(mem, 0, false, 1, output);

            var blue = ScreenRenderer.ColourToArgb(1, false);   // border
            var white = ScreenRenderer.ColourToArgb(7, false);  // ink
            var black = ScreenRenderer.ColourToArgb(0, false);  // paper

            // First 48 pixels = border (blue)
            Assert.That(output[0], Is.EqualTo(blue));
            Assert.That(output[47], Is.EqualTo(blue));

            // Display starts at pixel 48: first 8 pixels are white
            Assert.That(output[48], Is.EqualTo(white));
            Assert.That(output[55], Is.EqualTo(white));

            // Next 8 display pixels are black (memory is 0)
            Assert.That(output[56], Is.EqualTo(black));

            // Last 48 pixels = border (blue)
            Assert.That(output[304], Is.EqualTo(blue));
            Assert.That(output[351], Is.EqualTo(blue));
        }
    }
}
