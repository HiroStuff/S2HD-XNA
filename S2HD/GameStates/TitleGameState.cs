using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using S2HD.Title;
using S2HD.Graphics;
using S2HD.Animation;
using System;
using System.Collections.Generic;

namespace S2HD.GameStates
{
    public class TitleGameState : IGameState
    {
        private static readonly EaseTimeline IntroTextOpacity = new EaseTimeline(new EaseTimeline.Entry[]
        {
            new EaseTimeline.Entry(32, 0.0),
            new EaseTimeline.Entry(64, 1.0),
            new EaseTimeline.Entry(144, 1.0),
            new EaseTimeline.Entry(176, 0.0)
        });

        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;
        private ContentManager _content;

        private CustomFont _font;
        private AnimationInstance _sparkleAnimationInstance;
        private AnimationInstance _shootingStarAnimationInstance;

        private readonly Tuple<int, Vector2>[] SparkleTable = new Tuple<int, Vector2>[]
        {
            new Tuple<int, Vector2>(204, new Vector2(-126, -320)),
            new Tuple<int, Vector2>(220, new Vector2(-280, 366)),
            new Tuple<int, Vector2>(236, new Vector2(324, -4)),
            new Tuple<int, Vector2>(252, new Vector2(80, 168))
        };

        private const int BannerStartTime = 332;
        private int _ticks;
        private Vector2 _sparklePosition;
        private float _fadeOutOpacity;
        private bool _fadingOut;
        private Vector2 _shootingStarPosition;
        private bool _loaded;

        private Background _background;
        private Banner _banner;
        private UserInterface _userInterface;
        private string _versionText;

        public Background Background => _background;
        public TitleGameState.ResultType Result { get; set; }

        public TitleGameState(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _content = content;
            _versionText = "XNA Port 1.0";
        }

        public void LoadContent(ContentManager content)
        {

            _font = new CustomFont();
            _font.LoadFromXml(content, "Content/SONICORCA/FONTS/HUD_FONT");


            var sparkleTexture = content.Load<Texture2D>("SONICORCA/TITLE/FRAMES/0");
            _sparkleAnimationInstance = new AnimationInstance(sparkleTexture, new Rectangle[] { new Rectangle(0, 0, 32, 32) }, 8);


            _shootingStarAnimationInstance = new AnimationInstance(sparkleTexture, new Rectangle[] { new Rectangle(0, 0, 64, 64) }, 9);

            _loaded = true;
            _background = new Background(_graphicsDevice, content);
            _banner = new Banner(_graphicsDevice, content);
            _userInterface = new UserInterface(_graphicsDevice, content, this);
            RestartEvents();
        }

        public void Update(GameTime gameTime)
        {
            if (!_loaded) return;

            if (_fadingOut)
            {
                if (_fadeOutOpacity > 0.0f)
                {
                    _fadeOutOpacity -= 0.0166666675f;
                }
                else
                {

                    return;
                }
            }


            if (_ticks == 268)
            {

            }


            if (_ticks == 458)
            {
                _background.Visible = true;
                _banner.ShowStarLensFare = true;
                _userInterface.Visible = true;
            }

            _banner.Update();
            if (_background.Visible)
                _background.Update();
            _userInterface.Update();


            if (_ticks >= 662)
            {
                if (_shootingStarAnimationInstance != null)
                {
                    _shootingStarAnimationInstance.Animate();
                    _shootingStarPosition += new Vector2(-16.0f, 8.0f);
                }
            }

            UpdateSparkle();
            _ticks++;
        }

        public void Draw()
        {
            if (!_loaded) return;

            _spriteBatch.Begin();

            DrawIntroText();
            _background.Draw(_spriteBatch);
            DrawShootingStar();
            _banner.Draw(_spriteBatch);
            DrawSparkle();
            _userInterface.Draw(_spriteBatch);

            if (_background.Visible)
                DrawVersion();

            if (_fadeOutOpacity != 1.0f)
            {
                Color fadeColor = new Color(0, 0, 0, 1.0f - _fadeOutOpacity);

                var fadeTexture = _content.Load<Texture2D>("SONICORCA/TITLE/FRAMES/0");
                _spriteBatch.Draw(fadeTexture, new Rectangle(0, 0, 1920, 1080), fadeColor);
            }

            _banner.DrawUnfaded(_spriteBatch);

            _spriteBatch.End();
        }

        private void RestartEvents()
        {
            _ticks = 0;
            _sparkleAnimationInstance = null;
            _fadeOutOpacity = 1f;
            _fadingOut = false;
            _background.Reset();
            _banner.Reset();
            _userInterface.Reset();
        }

        public void FadeOut()
        {
            _fadingOut = true;
            _banner.DoShine(210);
        }

        private void CreateSparkle(Vector2 position)
        {
            var sparkleTexture = _content.Load<Texture2D>("SONICORCA/TITLE/FRAMES/0");
            _sparkleAnimationInstance = new AnimationInstance(sparkleTexture, new Rectangle[] { new Rectangle(0, 0, 32, 32) }, 8);
            _sparklePosition = position;
        }

        private void UpdateSparkle()
        {
            foreach (var tuple in SparkleTable)
            {
                if (tuple.Item1 == _ticks)
                    CreateSparkle(_banner.Position + tuple.Item2);
            }

            if (_sparkleAnimationInstance == null)
                return;
            _sparkleAnimationInstance.Animate();
        }

        private void DrawIntroText()
        {
            double valueAt = IntroTextOpacity.GetValueAt(_ticks);
            if (valueAt <= 0.0)
                return;

            string[] stringList = { "SONIC", "AND", "MILES \"TAILS\" PROWER", "IN" };
            int y = 540 - stringList.Length * 128 / 2;
            Color colour = new Color((float)valueAt, 1.0f, 1.0f, 1.0f);

            foreach (string text in stringList)
            {
                Vector2 textPosition = new Vector2(960, y);
                _font.DrawString(_spriteBatch, text, textPosition, colour, 0, true);
                y += 128;
            }
        }

        private void DrawSparkle()
        {
            if (_sparkleAnimationInstance == null || _sparkleAnimationInstance.Cycles != 0)
                return;
            _sparkleAnimationInstance.Draw(_spriteBatch, _sparklePosition);
        }

        private void DrawShootingStar()
        {
            if (_shootingStarAnimationInstance == null)
                return;
            _shootingStarAnimationInstance.Draw(_spriteBatch, _shootingStarPosition);
        }

        private void DrawVersion()
        {
            Color colour = new Color(_fadeOutOpacity / 2.0f, 1.0f, 1.0f, 1.0f);
            Vector2 versionPosition = new Vector2(8, 1052);            
            _spriteBatch.End();
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateScale(0.5f, 0.5f, 1.0f) * Matrix.CreateTranslation(8, 1052, 0));
            _font.DrawString(_spriteBatch, _versionText.ToUpper(), Vector2.Zero, colour);
            _spriteBatch.End();
            _spriteBatch.Begin();
        }

        public bool IsComplete => _fadingOut && _fadeOutOpacity <= 0.0f;

        public enum ResultType
        {
            NewGame,
            LevelSelect,
            ShowOptions,
            ShowAchievements,
            StartDemo,
            Quit,
        }
    }


    public class EaseTimeline
    {
        public class Entry
        {
            public int Time { get; set; }
            public double Value { get; set; }

            public Entry(int time, double value)
            {
                Time = time;
                Value = value;
            }
        }

        private Entry[] _entries;

        public EaseTimeline(Entry[] entries)
        {
            _entries = entries;
        }

        public double GetValueAt(int time)
        {
            if (_entries.Length == 0) return 0.0;

            for (int i = 0; i < _entries.Length - 1; i++)
            {
                if (time >= _entries[i].Time && time <= _entries[i + 1].Time)
                {
                    double t = (double)(time - _entries[i].Time) / (_entries[i + 1].Time - _entries[i].Time);
                    return _entries[i].Value + t * (_entries[i + 1].Value - _entries[i].Value);
                }
            }

            if (time <= _entries[0].Time) return _entries[0].Value;
            if (time >= _entries[_entries.Length - 1].Time) return _entries[_entries.Length - 1].Value;

            return 0.0;
        }
    }
}
