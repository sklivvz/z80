using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using z80;
using ula;

namespace z80Sample
{
    internal class Program
    {
        private static void Main()
        {
            using var game = new SpectrumEmulator();
            game.Run();
        }
    }

    class SpectrumEmulator : Game
    {
        private const int Scale = 2;
        private const int TStatesPerFrame = 69888;

        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _screenTexture;

        // Spectrum hardware
        private readonly byte[] _ram = new byte[65536];
        private readonly uint[] _frameBuffer = new uint[Ula.ScreenWidth * Ula.ScreenHeight];
        private readonly uint[] _textureBuffer = new uint[Ula.ScreenWidth * Ula.ScreenHeight];
        private SimpleMemory _memory;
        private SpectrumKeyboard _keyboard;
        private Ula _ula;
        private Z80 _cpu;
        private int _frameCount;
        private double _fpsTimer;

        // Audio
        private const int SampleRate = 48000;
        private const int SamplesPerFrame = SampleRate / 50;
        private const int TStatesPerSample = TStatesPerFrame / SamplesPerFrame;
        private DynamicSoundEffectInstance _speaker;
        private readonly byte[] _audioBuffer = new byte[SamplesPerFrame * 2];

        public SpectrumEmulator()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Ula.ScreenWidth * Scale,
                PreferredBackBufferHeight = Ula.ScreenHeight * Scale,
                SynchronizeWithVerticalRetrace = false
            };

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 50.08);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Window.Title = "ZX Spectrum 48K";

            // Load ROM
            var rom = File.ReadAllBytes("48.rom");
            if (rom.Length != 16384) throw new InvalidOperationException("Invalid 48.rom file");
            Array.Copy(rom, _ram, 16384);

            // Wire up Spectrum hardware
            _memory = new SimpleMemory(_ram, 16384);
            _keyboard = new SpectrumKeyboard();
            _ula = new Ula(_memory, _keyboard, _frameBuffer);
            var cpuBus = new SpectrumBus(_ula);
            _cpu = new Z80(_memory, cpuBus);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _screenTexture = new Texture2D(GraphicsDevice,
                Ula.ScreenWidth, Ula.ScreenHeight);

            _speaker = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Mono);
            _speaker.Play();
        }

        protected override void Update(GameTime gameTime)
        {
            // Update keyboard state
            _keyboard.Update(Keyboard.GetState());

            // Execute CPU in small chunks, sampling speaker for audio
            var tStatesLeft = TStatesPerFrame;
            for (int s = 0; s < SamplesPerFrame; s++)
            {
                var chunk = Math.Min(TStatesPerSample, tStatesLeft);
                _cpu.Tick(chunk);
                tStatesLeft -= chunk;

                short sample = _ula.SpeakerOutput ? (short)8192 : (short)0;
                _audioBuffer[s * 2] = (byte)(sample & 0xFF);
                _audioBuffer[s * 2 + 1] = (byte)(sample >> 8);
            }
            if (tStatesLeft > 0) _cpu.Tick(tStatesLeft);

            _speaker.SubmitBuffer(_audioBuffer);
            _ula.RenderFrame();

            // FPS counter in title
            _frameCount++;
            _fpsTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (_fpsTimer >= 1.0)
            {
                Window.Title = $"ZX Spectrum 48K \u2014 {_frameCount} fps";
                _frameCount = 0;
                _fpsTimer -= 1.0;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Convert ARGB → ABGR for MonoGame's SurfaceFormat.Color
            for (int i = 0; i < _frameBuffer.Length; i++)
            {
                var argb = _frameBuffer[i];
                _textureBuffer[i] = (argb & 0xFF00FF00)
                    | ((argb & 0x00FF0000) >> 16)
                    | ((argb & 0x000000FF) << 16);
            }
            _screenTexture.SetData(_textureBuffer);

            // Draw scaled to window
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_screenTexture,
                new Rectangle(0, 0,
                    _graphics.PreferredBackBufferWidth,
                    _graphics.PreferredBackBufferHeight),
                Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    /// <summary>
    /// Routes Z80 I/O to the ULA. On real hardware, the ULA decodes port 0xFE
    /// (any even port — bit 0 of address is low).
    /// </summary>
    class SpectrumBus : IBus
    {
        private readonly Ula _ula;

        public SpectrumBus(Ula ula)
        {
            _ula = ula;
        }

        public byte IoRead(ushort address)
        {
            if ((address & 0x01) == 0)
                return _ula.ReadPort((byte)(address >> 8));

            return 0xFF; // floating bus
        }

        public void IoWrite(ushort address, byte data)
        {
            if ((address & 0x01) == 0)
                _ula.WritePort(data);
        }

        public bool INT
        {
            get
            {
                if (!_ula.InterruptRequested) return false;
                _ula.AcknowledgeInterrupt();
                return true;
            }
        }

        public bool NMI => false;
        public byte Data => 0xFF;
        public bool WAIT => false;
        public bool BUSRQ => false;
        public bool RESET => false;
    }

    /// <summary>
    /// ZX Spectrum keyboard: 8 half-rows × 5 keys, active-low.
    /// ULA selects rows via high byte of port address (bit low = row selected).
    /// </summary>
    class SpectrumKeyboard : IUlaBus
    {
        private readonly byte[] _rows = new byte[8];

        public SpectrumKeyboard()
        {
            for (int i = 0; i < 8; i++) _rows[i] = 0xFF;
        }

        public void Update(KeyboardState kb)
        {
            for (int i = 0; i < 8; i++) _rows[i] = 0xFF;

            // Row 0: CapsShift, Z, X, C, V
            if (kb.IsKeyDown(Keys.LeftShift))    _rows[0] &= unchecked((byte)~0x01);
            if (kb.IsKeyDown(Keys.Z))            _rows[0] &= unchecked((byte)~0x02);
            if (kb.IsKeyDown(Keys.X))            _rows[0] &= unchecked((byte)~0x04);
            if (kb.IsKeyDown(Keys.C))            _rows[0] &= unchecked((byte)~0x08);
            if (kb.IsKeyDown(Keys.V))            _rows[0] &= unchecked((byte)~0x10);

            // Row 1: A, S, D, F, G
            if (kb.IsKeyDown(Keys.A))            _rows[1] &= unchecked((byte)~0x01);
            if (kb.IsKeyDown(Keys.S))            _rows[1] &= unchecked((byte)~0x02);
            if (kb.IsKeyDown(Keys.D))            _rows[1] &= unchecked((byte)~0x04);
            if (kb.IsKeyDown(Keys.F))            _rows[1] &= unchecked((byte)~0x08);
            if (kb.IsKeyDown(Keys.G))            _rows[1] &= unchecked((byte)~0x10);

            // Row 2: Q, W, E, R, T
            if (kb.IsKeyDown(Keys.Q))            _rows[2] &= unchecked((byte)~0x01);
            if (kb.IsKeyDown(Keys.W))            _rows[2] &= unchecked((byte)~0x02);
            if (kb.IsKeyDown(Keys.E))            _rows[2] &= unchecked((byte)~0x04);
            if (kb.IsKeyDown(Keys.R))            _rows[2] &= unchecked((byte)~0x08);
            if (kb.IsKeyDown(Keys.T))            _rows[2] &= unchecked((byte)~0x10);

            // Row 3: 1, 2, 3, 4, 5
            if (kb.IsKeyDown(Keys.D1))           _rows[3] &= unchecked((byte)~0x01);
            if (kb.IsKeyDown(Keys.D2))           _rows[3] &= unchecked((byte)~0x02);
            if (kb.IsKeyDown(Keys.D3))           _rows[3] &= unchecked((byte)~0x04);
            if (kb.IsKeyDown(Keys.D4))           _rows[3] &= unchecked((byte)~0x08);
            if (kb.IsKeyDown(Keys.D5))           _rows[3] &= unchecked((byte)~0x10);

            // Row 4: 0, 9, 8, 7, 6
            if (kb.IsKeyDown(Keys.D0))           _rows[4] &= unchecked((byte)~0x01);
            if (kb.IsKeyDown(Keys.D9))           _rows[4] &= unchecked((byte)~0x02);
            if (kb.IsKeyDown(Keys.D8))           _rows[4] &= unchecked((byte)~0x04);
            if (kb.IsKeyDown(Keys.D7))           _rows[4] &= unchecked((byte)~0x08);
            if (kb.IsKeyDown(Keys.D6))           _rows[4] &= unchecked((byte)~0x10);

            // Row 5: P, O, I, U, Y
            if (kb.IsKeyDown(Keys.P))            _rows[5] &= unchecked((byte)~0x01);
            if (kb.IsKeyDown(Keys.O))            _rows[5] &= unchecked((byte)~0x02);
            if (kb.IsKeyDown(Keys.I))            _rows[5] &= unchecked((byte)~0x04);
            if (kb.IsKeyDown(Keys.U))            _rows[5] &= unchecked((byte)~0x08);
            if (kb.IsKeyDown(Keys.Y))            _rows[5] &= unchecked((byte)~0x10);

            // Row 6: Enter, L, K, J, H
            if (kb.IsKeyDown(Keys.Enter))        _rows[6] &= unchecked((byte)~0x01);
            if (kb.IsKeyDown(Keys.L))            _rows[6] &= unchecked((byte)~0x02);
            if (kb.IsKeyDown(Keys.K))            _rows[6] &= unchecked((byte)~0x04);
            if (kb.IsKeyDown(Keys.J))            _rows[6] &= unchecked((byte)~0x08);
            if (kb.IsKeyDown(Keys.H))            _rows[6] &= unchecked((byte)~0x10);

            // Row 7: Space, SymShift, M, N, B
            if (kb.IsKeyDown(Keys.Space))        _rows[7] &= unchecked((byte)~0x01);
            if (kb.IsKeyDown(Keys.RightShift) || kb.IsKeyDown(Keys.LeftControl) || kb.IsKeyDown(Keys.RightControl))
                                                 _rows[7] &= unchecked((byte)~0x02);
            if (kb.IsKeyDown(Keys.M))            _rows[7] &= unchecked((byte)~0x04);
            if (kb.IsKeyDown(Keys.N))            _rows[7] &= unchecked((byte)~0x08);
            if (kb.IsKeyDown(Keys.B))            _rows[7] &= unchecked((byte)~0x10);

            // Special key combos: arrow keys = CapsShift + 5/6/7/8
            if (kb.IsKeyDown(Keys.Left))  { _rows[0] &= unchecked((byte)~0x01); _rows[3] &= unchecked((byte)~0x10); }
            if (kb.IsKeyDown(Keys.Down))  { _rows[0] &= unchecked((byte)~0x01); _rows[4] &= unchecked((byte)~0x10); }
            if (kb.IsKeyDown(Keys.Up))    { _rows[0] &= unchecked((byte)~0x01); _rows[4] &= unchecked((byte)~0x08); }
            if (kb.IsKeyDown(Keys.Right)) { _rows[0] &= unchecked((byte)~0x01); _rows[4] &= unchecked((byte)~0x04); }

            // Backspace = CapsShift + 0 (DELETE)
            if (kb.IsKeyDown(Keys.Back))  { _rows[0] &= unchecked((byte)~0x01); _rows[4] &= unchecked((byte)~0x01); }

            // Escape = CapsShift + Space (BREAK)
            if (kb.IsKeyDown(Keys.Escape)){ _rows[0] &= unchecked((byte)~0x01); _rows[7] &= unchecked((byte)~0x01); }
        }

        public byte ReadKeyboard(byte highByte)
        {
            byte result = 0xFF;
            for (int row = 0; row < 8; row++)
            {
                if ((highByte & (1 << row)) == 0)
                    result &= _rows[row];
            }
            return result;
        }

        public bool EarInput => false;
    }
}
