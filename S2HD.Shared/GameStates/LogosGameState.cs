using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using S2HD.Graphics;
using System;

namespace S2HD.GameStates
{
    public class LogosGameState : IGameState
    {
        private Texture2D _engineTexture;
        private Texture2D _enginePartialTexture;
        private Texture2D _engineSonicTexture;
        private CustomFont _font;

        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private bool _loaded;

        private bool _smallSonic;
        private int _sonicX;
        private int _sonicVX;
        private int _sonicFrame;
        private int _timer;
        private LogosPhase _currentPhase;

        private float _fadeOpacity;
        private bool _fadeOut;

        private enum LogosPhase
        {
            Loading,
            SmallSonicRun1,
            SmallSonicRun2,
            SmallSonicRun3,
            Wait,
            FadeOut,
            Complete
        }

        public LogosGameState(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _smallSonic = true;
            _sonicX = -256;
            _sonicVX = 100;
            _sonicFrame = 0;
            _timer = 0;
            _currentPhase = LogosPhase.Loading;
            _fadeOpacity = 1.0f;
            _fadeOut = false;
        }

        public void LoadContent(ContentManager content)
        {
            _engineTexture = content.Load<Texture2D>("SONICORCA/ENGINE");
            _enginePartialTexture = content.Load<Texture2D>("SONICORCA/ENGINE/PARTIAL");
            _engineSonicTexture = content.Load<Texture2D>("SONICORCA/ENGINE/SONIC");
            _font = new CustomFont();
            _font.LoadFromXml(content, "Content/SONICORCA/FONTS/HUD_FONT");

            _loaded = true;
            _currentPhase = LogosPhase.SmallSonicRun1;
            _timer = 8;
        }

        public void Update(GameTime gameTime)
        {
            if (!_loaded) return;

            switch (_currentPhase)
            {
                case LogosPhase.SmallSonicRun1:
                    if (_timer > 0)
                    {
                        _timer--;
                        return;
                    }

                    _sonicX += _sonicVX;
                    _sonicFrame = (_sonicFrame + 1) % 8;

                    if (_sonicX >= 2176)
                    {
                        _currentPhase = LogosPhase.SmallSonicRun2;
                        _smallSonic = false;
                        _sonicX = 2944;
                        _sonicVX = -266;
                        _timer = 8;
                    }
                    break;

                case LogosPhase.SmallSonicRun2:
                    if (_timer > 0)
                    {
                        _timer--;
                        return;
                    }

                    _sonicX += _sonicVX;
                    _sonicFrame = (_sonicFrame + 1) % 8;

                    if (_sonicX <= -1024)
                    {
                        _currentPhase = LogosPhase.SmallSonicRun3;
                        _sonicX = -1024;
                        _sonicVX = 200;
                        _timer = 16;
                    }
                    break;

                case LogosPhase.SmallSonicRun3:
                    if (_timer > 0)
                    {
                        _timer--;
                        return;
                    }

                    _sonicX += _sonicVX;
                    _sonicFrame = (_sonicFrame + 1) % 8;

                    if (_sonicX >= 2944)
                    {
                        _currentPhase = LogosPhase.Wait;
                        _timer = 90;
                    }
                    break;

                case LogosPhase.Wait:
                    _timer--;
                    if (_timer <= 0)
                    {
                        _currentPhase = LogosPhase.FadeOut;
                        _fadeOut = true;
                    }
                    break;

                case LogosPhase.FadeOut:
                    _fadeOpacity -= 0.0166666675f;
                    _fadeOpacity = Math.Max(_fadeOpacity, 0.0f);

                    if (_fadeOpacity <= 0.0f)
                    {
                        _currentPhase = LogosPhase.Complete;
                    }
                    break;
            }
        }

        public void Draw()
        {
            if (!_loaded) return;

            _spriteBatch.Begin();

            DrawPoweredBy();

            if (_smallSonic)
            {
                DrawSmallSonic();
            }
            else
            {
                DrawEngineLogo();
                DrawSonic();
            }

            if (_fadeOut)
            {
                Color fadeColor = new Color(0, 0, 0, 1.0f - _fadeOpacity);
                _spriteBatch.Draw(_engineTexture, new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height), fadeColor);
            }

            _spriteBatch.End();
        }

        private void DrawPoweredBy()
        {
            Rectangle textBounds = _font.MeasureString("POWERED BY");
            Vector2 textPosition = new Vector2(
                (_graphicsDevice.Viewport.Width - textBounds.Width) / 2,
                120
            );

            if (_smallSonic)
            {
                Rectangle clipRect = new Rectangle(Math.Max(0, _sonicX), 0, _graphicsDevice.Viewport.Width - Math.Max(0, _sonicX), _graphicsDevice.Viewport.Height);
                _spriteBatch.Draw(_engineTexture, clipRect, Color.Black);
            }
        }

        private void DrawEngineLogo()
        {
            Vector2 logoSize = new Vector2(_engineTexture.Width, _engineTexture.Height);
            Vector2 logoPosition = new Vector2(
                960 - logoSize.X / 2,
                540 - logoSize.Y / 2
            );

            Rectangle destination = new Rectangle(
                (int)logoPosition.X,
                (int)logoPosition.Y,
                (int)logoSize.X,
                (int)logoSize.Y
            );

            int visibleWidth = Math.Max(0, destination.Right - _sonicX);

            if (visibleWidth > 0)
            {
                Rectangle sourceRect = new Rectangle(
                    (int)(logoSize.X - visibleWidth),
                    0,
                    visibleWidth,
                    (int)logoSize.Y
                );

                Rectangle visibleDest = new Rectangle(
                    destination.Right - visibleWidth,
                    destination.Y,
                    visibleWidth,
                    destination.Height
                );

                _spriteBatch.Draw(_enginePartialTexture, visibleDest, sourceRect, Color.White);
            }

            if (_sonicVX >= 0)
            {
                int leftVisibleWidth = Math.Max(0, _sonicX - destination.X);

                if (leftVisibleWidth > 0)
                {
                    Rectangle sourceRect = new Rectangle(
                        0,
                        0,
                        leftVisibleWidth,
                        (int)logoSize.Y
                    );

                    Rectangle visibleDest = new Rectangle(
                        destination.X,
                        destination.Y,
                        leftVisibleWidth,
                        destination.Height
                    );

                    _spriteBatch.Draw(_engineTexture, visibleDest, sourceRect, Color.White);
                }
            }
        }

        private void DrawSmallSonic()
        {
            Rectangle sourceRect = new Rectangle(_sonicFrame * 1024, 0, 1024, 1120);
            Rectangle destRect = new Rectangle(_sonicX - 128, 40, 256, 280);

            if (destRect.Right > 0 && destRect.Left < _graphicsDevice.Viewport.Width)
            {
                SpriteEffects effects = _sonicVX >= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                _spriteBatch.Draw(_engineSonicTexture, destRect, sourceRect, Color.White, 0f, Vector2.Zero, effects, 0f);
            }
        }

        private void DrawSonic()
        {
            Rectangle sourceRect = new Rectangle(_sonicFrame * 1024, 0, 1024, 1120);
            Rectangle destRect = new Rectangle(_sonicX - 512, -20, 1024, 1120);

            if (destRect.Right > 0 && destRect.Left < _graphicsDevice.Viewport.Width)
            {
                SpriteEffects effects = _sonicVX >= 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                _spriteBatch.Draw(_engineSonicTexture, destRect, sourceRect, Color.White, 0f, Vector2.Zero, effects, 0f);
            }
        }

        public bool IsComplete => _currentPhase == LogosPhase.Complete;
    }
}
