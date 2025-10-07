using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System;

namespace S2HD.GameStates
{
    public class TeamLogoGameState : IGameState
    {
        private const int FadeTime = 60;
        private const int ShowTime = 240;

        private Texture2D _teamLogoTexture;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private bool _loaded;
        private float _opacity;
        private int _timer;
        private TeamLogoPhase _currentPhase;

        private enum TeamLogoPhase
        {
            Loading,
            FadeIn,
            Showing,
            FadeOut,
            Complete
        }

        public TeamLogoGameState(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _opacity = 0.0f;
            _timer = 0;
            _currentPhase = TeamLogoPhase.Loading;
        }

        public void LoadContent(string dataRoot)
        {
            using (var s = S2HD.Shared.Data.DataService.OpenRead("SONICORCA/TEAMLOGO.png"))
                _teamLogoTexture = S2HD.Graphics.TextureHelper.LoadTextureFromStream(_graphicsDevice, s);
            _loaded = true;
            _currentPhase = TeamLogoPhase.FadeIn;
        }

        public void Update(GameTime gameTime)
        {
            if (!_loaded) return;

            switch (_currentPhase)
            {
                case TeamLogoPhase.FadeIn:
                    _opacity += 0.0166666675f;
                    _opacity = Math.Min(_opacity, 1f);

                    if (_opacity >= 1.0f)
                    {
                        _currentPhase = TeamLogoPhase.Showing;
                        _timer = ShowTime;
                    }
                    break;

                case TeamLogoPhase.Showing:
                    _timer--;
                    if (_timer <= 0)
                    {
                        _currentPhase = TeamLogoPhase.FadeOut;
                    }
                    break;

                case TeamLogoPhase.FadeOut:
                    _opacity -= 0.0166666675f;
                    _opacity = Math.Max(_opacity, 0.0f);

                    if (_opacity <= 0.0f)
                    {
                        _currentPhase = TeamLogoPhase.Complete;
                    }
                    break;
            }
        }

        public void Draw()
        {
            if (!_loaded) return;

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            Vector2 screenCenter = new Vector2(_graphicsDevice.Viewport.Width / 2, _graphicsDevice.Viewport.Height / 2);
            Vector2 logoSize = new Vector2(_teamLogoTexture.Width, _teamLogoTexture.Height);
            Vector2 logoPosition = screenCenter - logoSize / 2;

            _spriteBatch.Draw(_teamLogoTexture, logoPosition, Color.White * _opacity);

            if (_opacity < 1.0f)
            {
                Color fadeColor = new Color(0, 0, 0, 1.0f - _opacity);
                _spriteBatch.Draw(_teamLogoTexture, new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height), fadeColor);
            }

            _spriteBatch.End();
        }

        public bool IsComplete => _currentPhase == TeamLogoPhase.Complete;
    }
}
