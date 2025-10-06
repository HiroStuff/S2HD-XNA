using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System;

namespace S2HD.GameStates
{
    public class DisclaimerGameState : IGameState
    {
        private const int FadeTime = 60;
        private const int ShowTime = 240;

        private Texture2D _disclaimerTexture;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private bool _loaded;
        private float _opacity;
        private int _timer;
        private DisclaimerPhase _currentPhase;

        private enum DisclaimerPhase
        {
            Loading,
            Wait,
            FadeIn,
            Showing,
            FadeOut,
            Complete
        }

        public DisclaimerGameState(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _opacity = 0.0f;
            _timer = 0;
            _currentPhase = DisclaimerPhase.Loading;
        }

        public void LoadContent(string dataRoot)
        {
            using (var s = File.OpenRead(Path.Combine(dataRoot, "SONICORCA/DISCLAIMER.png")))
                _disclaimerTexture = Texture2D.FromStream(_graphicsDevice, s);
            _loaded = true;
            _currentPhase = DisclaimerPhase.Wait;
            _timer = FadeTime;
        }

        public void Update(GameTime gameTime)
        {
            if (!_loaded) return;

            switch (_currentPhase)
            {
                case DisclaimerPhase.Wait:
                    _timer--;
                    if (_timer <= 0)
                    {
                        _currentPhase = DisclaimerPhase.FadeIn;
                    }
                    break;

                case DisclaimerPhase.FadeIn:
                    _opacity += 0.0166666675f;
                    _opacity = Math.Min(_opacity, 1f);

                    if (_opacity >= 1.0f)
                    {
                        _currentPhase = DisclaimerPhase.Showing;
                        _timer = ShowTime;
                    }
                    break;

                case DisclaimerPhase.Showing:
                    _timer--;
                    if (_timer <= 0)
                    {
                        _currentPhase = DisclaimerPhase.FadeOut;
                    }
                    break;

                case DisclaimerPhase.FadeOut:
                    _opacity -= 0.0166666675f;
                    _opacity = Math.Max(_opacity, 0.0f);

                    if (_opacity <= 0.0f)
                    {
                        _currentPhase = DisclaimerPhase.Complete;
                    }
                    break;
            }
        }

        public void Draw()
        {
            if (!_loaded) return;

            _spriteBatch.Begin();

            Vector2 screenCenter = new Vector2(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 2);
            Vector2 disclaimerSize = new Vector2(_disclaimerTexture.Width, _disclaimerTexture.Height);
            Vector2 disclaimerPosition = screenCenter - disclaimerSize / 2;

            _spriteBatch.Draw(_disclaimerTexture, disclaimerPosition, Color.White * _opacity);

            if (_opacity < 1.0f)
            {
                Color fadeColor = new Color(0, 0, 0, 1.0f - _opacity);
                _spriteBatch.Draw(_disclaimerTexture, new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height), fadeColor);
            }

            _spriteBatch.End();
        }

        public bool IsComplete => _currentPhase == DisclaimerPhase.Complete;
    }
}
