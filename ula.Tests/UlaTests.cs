using System;
using NUnit.Framework;

namespace ula.Tests
{
    [TestFixture]
    public class UlaTests
    {
        private class TestMemory : z80.IMemory
        {
            private readonly byte[] _ram = new byte[0x10000];
            public byte this[ushort address]
            {
                get => _ram[address];
                set => _ram[address] = value;
            }
        }

        private class TestUlaBus : IUlaBus
        {
            public byte KeyboardState { get; set; } = 0xFF; // all keys released
            public bool EarInput { get; set; }
            public byte LastHighByte { get; private set; }
            public byte ReadKeyboard(byte highByte)
            {
                LastHighByte = highByte;
                return KeyboardState;
            }
        }

        [Test]
        public void Ula_Constructor_StoresFrameBuffer()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            Assert.That(ula.FrameBuffer, Is.SameAs(fb));
        }

        [Test]
        public void Ula_Constructor_RejectsWrongSizeFrameBuffer()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[100]; // wrong size

            Assert.Throws<ArgumentException>(() => new Ula(mem, bus, fb));
        }

        [Test]
        public void Ula_RenderFrame_BorderOnly()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            ula.WritePort(0x01); // border colour = blue
            ula.RenderFrame();

            var blue = ScreenRenderer.ColourToArgb(1, false);
            var black = ScreenRenderer.ColourToArgb(0, false);

            // Top border line (line 0): all blue
            Assert.That(fb[0], Is.EqualTo(blue), "top-left corner");
            Assert.That(fb[351], Is.EqualTo(blue), "top-right corner");

            // Display area, left border (line 48, pixel 0): blue
            Assert.That(fb[48 * 352], Is.EqualTo(blue), "display row left border");

            // Display area, display pixel (line 48, pixel 48): black (memory is 0, paper=0)
            Assert.That(fb[48 * 352 + 48], Is.EqualTo(black), "first display pixel");

            // Bottom border (last line, pixel 0): blue
            Assert.That(fb[287 * 352], Is.EqualTo(blue), "bottom-left corner");
        }

        [Test]
        public void Ula_RenderFrame_DisplayAreaShowsPixels()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            // Write all-set pixels at 0x4000, white ink / black paper attr at 0x5800
            mem[0x4000] = 0xFF;
            mem[0x5800] = 0x07; // ink=7(white), paper=0(black)

            ula.RenderFrame();

            var white = ScreenRenderer.ColourToArgb(7, false);

            // First display pixel is at frame buffer position: line 48, pixel 48
            int offset = 48 * 352 + 48;
            for (int i = 0; i < 8; i++)
                Assert.That(fb[offset + i], Is.EqualTo(white), $"pixel {i}");
        }

        [Test]
        public void Ula_WritePort_SetsBorderColour()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            ula.WritePort(0x02); // border = red (colour 2)
            ula.RenderFrame();

            var red = ScreenRenderer.ColourToArgb(2, false);
            Assert.That(fb[0], Is.EqualTo(red));
        }

        [Test]
        public void Ula_ReadPort_ReturnsKeyboardState()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            bus.KeyboardState = 0b11110111; // bit 3 pressed
            var result = ula.ReadPort(0xFE); // high byte 0xFE selects half-row

            // Bits 0-4 come from keyboard, bit 6 from EAR
            Assert.That(result & 0x1F, Is.EqualTo(0x17)); // 0b10111 = bits 0-4 of 0b11110111
        }

        [Test]
        public void Ula_ReadPort_IncludesEarBit()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            bus.EarInput = true;
            var result = ula.ReadPort(0xFE);

            Assert.That(result & 0x40, Is.EqualTo(0x40)); // bit 6 set
        }

        [Test]
        public void Ula_FlashToggle_Every16Frames()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            // Set up a pixel with flash attribute
            mem[0x4000] = 0xFF;
            mem[0x5800] = 0x87; // flash=true, ink=7(white), paper=0(black)

            // Render 1 frame -- flash inactive, should show ink (white)
            ula.RenderFrame();
            var white = ScreenRenderer.ColourToArgb(7, false);
            Assert.That(fb[48 * 352 + 48], Is.EqualTo(white), "frame 1: flash inactive");

            // Render 15 more frames (total 16) -- flash toggles
            for (int i = 0; i < 15; i++) ula.RenderFrame();

            var black = ScreenRenderer.ColourToArgb(0, false);
            Assert.That(fb[48 * 352 + 48], Is.EqualTo(black), "frame 16: flash active, swapped to paper");

            // Render 16 more frames -- flash toggles back
            for (int i = 0; i < 16; i++) ula.RenderFrame();
            Assert.That(fb[48 * 352 + 48], Is.EqualTo(white), "frame 32: flash inactive again");
        }

        [Test]
        public void Ula_InterruptRequested_TrueAfterRenderFrame()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            Assert.That(ula.InterruptRequested, Is.False, "before render");

            ula.RenderFrame();

            Assert.That(ula.InterruptRequested, Is.True, "after render");
        }

        [Test]
        public void Ula_AcknowledgeInterrupt_ClearsRequest()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            ula.RenderFrame();
            Assert.That(ula.InterruptRequested, Is.True);

            ula.AcknowledgeInterrupt();
            Assert.That(ula.InterruptRequested, Is.False);
        }

        [Test]
        public void Ula_ReadPort_PassesHighByteToKeyboard()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            ula.ReadPort(0xFE); // high byte = 0xFE selects row 0 (Caps Shift, Z, X, C, V)
            Assert.That(bus.LastHighByte, Is.EqualTo(0xFE));

            ula.ReadPort(0x7F); // high byte = 0x7F selects row 7 (Space, Sym Shift, M, N, B)
            Assert.That(bus.LastHighByte, Is.EqualTo(0x7F));
        }

        [Test]
        public void Ula_WritePort_StoresMicOutput()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            Assert.That(ula.MicOutput, Is.False);

            ula.WritePort(0x08); // bit 3 set
            Assert.That(ula.MicOutput, Is.True);

            ula.WritePort(0x00); // bit 3 clear
            Assert.That(ula.MicOutput, Is.False);
        }

        [Test]
        public void Ula_WritePort_StoresSpeakerOutput()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            Assert.That(ula.SpeakerOutput, Is.False);

            ula.WritePort(0x10); // bit 4 set
            Assert.That(ula.SpeakerOutput, Is.True);

            ula.WritePort(0x00); // bit 4 clear
            Assert.That(ula.SpeakerOutput, Is.False);
        }

        [Test]
        public void Ula_WritePort_PreservesAllBits()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            var ula = new Ula(mem, bus, fb);

            // Write 0x1B = border=3(magenta), MIC=1, speaker=1
            ula.WritePort(0x1B);

            Assert.That(ula.MicOutput, Is.True, "MIC bit 3");
            Assert.That(ula.SpeakerOutput, Is.True, "Speaker bit 4");

            // Verify border colour too
            ula.RenderFrame();
            var magenta = ScreenRenderer.ColourToArgb(3, false);
            Assert.That(fb[0], Is.EqualTo(magenta), "border colour 3");
        }

        [Test]
        public void Ula_CustomScreenBase_ReadsFromCorrectAddress()
        {
            var mem = new TestMemory();
            var bus = new TestUlaBus();
            var fb = new uint[352 * 288];
            // Use screen base 0xC000 (like ZX Spectrum 128K shadow screen)
            var ula = new Ula(mem, bus, fb, screenBaseAddress: 0xC000);

            // Pixel at 0xC000, attribute at 0xD800 (0xC000 + 0x1800)
            mem[0xC000] = 0xFF;
            mem[0xD800] = 0x07; // ink=7(white), paper=0(black)

            ula.RenderFrame();

            var white = ScreenRenderer.ColourToArgb(7, false);
            int offset = 48 * 352 + 48;
            Assert.That(fb[offset], Is.EqualTo(white));
        }
    }
}
