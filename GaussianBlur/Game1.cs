using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GaussianBlur
{
    public class Game1 : Game
    {
        /************************************************
         * A conversion of dhpoware Gaussian Blur Demo
         * 
         * Feel free to use for Commercial use, 
         * Distribution, Modification and/or Private use
         * Please respect MIT License in GaussianBlur.cs
         ************************************************/

        // WARNING:
        // If you change the BLUR_RADIUS you *MUST* also change the RADIUS
        // constant in GaussianBlur.fx. Both values *MUST* be the same.

        private const int BLUR_RADIUS = 7;
        private const float BLUR_AMOUNT = 2.0f;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private Texture2D _texture;
        private RenderTarget2D _renderTarget1;
        private RenderTarget2D _renderTarget2;
        private Vector2 _fontPos;
        private KeyboardState _currentKeyboardState;
        private KeyboardState _prevKeyboardState;
        private GaussianBlur _gaussianBlur;
        private int _windowWidth;
        private int _windowHeight;
        private int _renderTargetWidth;
        private int _renderTargetHeight;
        private int _frames;
        private int _framesPerSecond;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private bool _displayHelp;
        private bool _enableGaussianBlur;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.Title = "Gaussian Blur";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            // Setup the window to be a quarter the size of the desktop.

            _windowWidth = GraphicsDevice.DisplayMode.Width / 2;
            _windowHeight = GraphicsDevice.DisplayMode.Height / 2;

            // Setup frame buffer.

            _graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.PreferredBackBufferWidth = _windowWidth;
            _graphics.PreferredBackBufferHeight = _windowHeight;
            _graphics.ApplyChanges();

            // Position the text.

            _fontPos = new Vector2(1.0f, 1.0f);

            // Setup the initial input states.

            _currentKeyboardState = Keyboard.GetState();

            // Create the Gaussian blur filter kernel.

            _gaussianBlur = new GaussianBlur(this);
            _gaussianBlur.ComputeKernel(BLUR_RADIUS, BLUR_AMOUNT);

            base.Initialize();
        }

        private void InitRenderTargets()
        {
            // Since we're performing a Gaussian blur on a texture image the
            // render targets are half the size of the source texture image.
            // This will help improve the blurring effect.

            _renderTargetWidth = _texture.Width / 2;
            _renderTargetHeight = _texture.Height / 2;

            _renderTarget1 = new RenderTarget2D(GraphicsDevice,
                _renderTargetWidth, _renderTargetHeight, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);

            _renderTarget2 = new RenderTarget2D(GraphicsDevice,
                _renderTargetWidth, _renderTargetHeight, false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.None);

            // The texture offsets used by the Gaussian blur shader depends
            // on the dimensions of the render targets. The offsets need to be
            // recalculated whenever the render targets are recreated.

            _gaussianBlur.ComputeOffsets(_renderTargetWidth, _renderTargetHeight);
        }

        private bool KeyJustPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _prevKeyboardState.IsKeyUp(key);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteFont = Content.Load<SpriteFont>("DemoFont");

            _texture = Content.Load<Texture2D>("lena");

            InitRenderTargets();
        }

        private void ProcessKeyboard()
        {
            _prevKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            if (KeyJustPressed(Keys.Escape))
                this.Exit();

            if (KeyJustPressed(Keys.Space))
                _enableGaussianBlur = !_enableGaussianBlur;

            if (KeyJustPressed(Keys.H))
                _displayHelp = !_displayHelp;

            if (_currentKeyboardState.IsKeyDown(Keys.LeftAlt) ||
                _currentKeyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (KeyJustPressed(Keys.Enter))
                    ToggleFullScreen();
            }
        }

        private void ToggleFullScreen()
        {
            int newWidth = 0;
            int newHeight = 0;

            _graphics.IsFullScreen = !_graphics.IsFullScreen;

            if (_graphics.IsFullScreen)
            {
                newWidth = GraphicsDevice.DisplayMode.Width;
                newHeight = GraphicsDevice.DisplayMode.Height;
            }
            else
            {
                newWidth = _windowWidth;
                newHeight = _windowHeight;
            }

            _graphics.PreferredBackBufferWidth = newWidth;
            _graphics.PreferredBackBufferHeight = newHeight;
            _graphics.ApplyChanges();
        }

        protected override void UnloadContent()
        {
            _renderTarget1.Dispose();
            _renderTarget1 = null;

            _renderTarget2.Dispose();
            _renderTarget2 = null;
        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive)
                return;

            ProcessKeyboard();
            UpdateFrameRate(gameTime);

            base.Update(gameTime);
        }

        private void UpdateFrameRate(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime;

            if (_elapsedTime > TimeSpan.FromSeconds(1))
            {
                _elapsedTime -= TimeSpan.FromSeconds(1);
                _framesPerSecond = _frames;
                _frames = 0;
            }
        }

        private void IncrementFrameCounter()
        {
            ++_frames;
        }

        private void DrawText()
        {
            StringBuilder buffer = new StringBuilder();

            if (_displayHelp)
            {
                buffer.AppendLine("Press SPACE to enable/disable Gaussian blur");
                buffer.AppendLine("Press ALT and ENTER to toggle full screen");
                buffer.AppendLine("Press ESCAPE to exit");
                buffer.AppendLine();
                buffer.AppendLine("Press H to hide help");
            }
            else
            {
                buffer.AppendFormat("FPS: {0}\n", _framesPerSecond);
                buffer.AppendLine();
                buffer.AppendFormat("Radius: {0}\n", _gaussianBlur.Radius);
                buffer.AppendFormat("Sigma: {0}\n", _gaussianBlur.Sigma.ToString("f2"));
                buffer.AppendLine();
                buffer.AppendLine("Press H to display help");
            }

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            _spriteBatch.DrawString(_spriteFont, buffer.ToString(), _fontPos, Color.Yellow);
            _spriteBatch.End();
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!IsActive)
                return;

            if (_enableGaussianBlur)
            {
                Texture2D result = _gaussianBlur.PerformGaussianBlur(_texture, _renderTarget1, _renderTarget2, _spriteBatch);
                Rectangle rectangle = new Rectangle(0, 0, _texture.Width, _texture.Height);

                GraphicsDevice.Clear(Color.CornflowerBlue);

                _spriteBatch.Begin();
                _spriteBatch.Draw(result, rectangle, Color.White);
                _spriteBatch.End();
            }
            else
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);

                _spriteBatch.Begin();
                _spriteBatch.Draw(_texture, new Rectangle(0, 0, _texture.Width, _texture.Height), Color.White);
                _spriteBatch.End();
            }

            DrawText();

            base.Draw(gameTime);
            IncrementFrameCounter();
        }
    }
}