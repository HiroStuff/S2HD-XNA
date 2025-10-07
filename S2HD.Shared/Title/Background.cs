using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using S2HD.Animation;
using System;
using System.IO;

namespace S2HD.Title
{
    public class Background
    {
        private const int BackgroundSkyStartX = 1260;
        private const int BackgroundIslandStartX = 1088;
        private const double BackgroundSkyVelocity = -0.05;
        private const double BackgroundIslandVelocity = -0.1;

        private Texture2D _textureBackgroundSky;
        private Texture2D _textureBackgroundIsland;
        private Texture2D _textureBackgroundDeathEgg;
        private Texture2D _textureWipe;
        private AnimationInstance _waterSparkleAnimationInstance;

        private double _backgroundFlash;
        private double _backgroundSkyCentreX;
        private double _backgroundIslandCentreX;
        private int _ticks;
        private int _wipeHeight;
        private bool _wipeTransitionActive;

        public bool Visible { get; set; }

        public Background(GraphicsDevice graphicsDevice, string dataRoot)
        {
            LoadContent(graphicsDevice, dataRoot);
        }

        private void LoadContent(GraphicsDevice graphicsDevice, string dataRoot)
        {
            using (var s = S2HD.Shared.Data.DataService.OpenRead("SONICORCA/TITLE/BACKGROUND/SKY.png"))
                _textureBackgroundSky = S2HD.Graphics.TextureHelper.LoadTextureFromStream(graphicsDevice, s);
            using (var s = S2HD.Shared.Data.DataService.OpenRead("SONICORCA/TITLE/BACKGROUND/ISLAND.png"))
                _textureBackgroundIsland = S2HD.Graphics.TextureHelper.LoadTextureFromStream(graphicsDevice, s);
            using (var s = S2HD.Shared.Data.DataService.OpenRead("SONICORCA/TITLE/BACKGROUND/DEATHEGG.png"))
                _textureBackgroundDeathEgg = S2HD.Graphics.TextureHelper.LoadTextureFromStream(graphicsDevice, s);
            using (var s = S2HD.Shared.Data.DataService.OpenRead("SONICORCA/TITLE/WIPE.png"))
                _textureWipe = S2HD.Graphics.TextureHelper.LoadTextureFromStream(graphicsDevice, s);
        }

        public void Reset()
        {
            _ticks = 0;
            _backgroundFlash = 0.0;
            _backgroundSkyCentreX = 960.0;
            _backgroundIslandCentreX = BackgroundIslandStartX;
            _waterSparkleAnimationInstance = null;
            Visible = false;
        }

        public void WipeOut()
        {
            _wipeTransitionActive = true;
        }

        public void Update()
        {
            if (_ticks == 0)
            {
                Visible = true;
                _backgroundFlash = 1.0;
                _backgroundSkyCentreX = BackgroundSkyStartX;

                _waterSparkleAnimationInstance = new AnimationInstance(_textureBackgroundSky, new Rectangle[] { new Rectangle(0, 0, 32, 32) });
            }
            else
            {
                _backgroundFlash = Math.Max(0.0, _backgroundFlash - 1.0 / 32.0);
            }

            _backgroundSkyCentreX += BackgroundSkyVelocity;
            _backgroundIslandCentreX += BackgroundIslandVelocity;

            if (_backgroundSkyCentreX + (_textureBackgroundSky.Width / 2) < 0.0)
                _backgroundSkyCentreX = _textureBackgroundSky.Width / 2;

            if (_backgroundIslandCentreX < -_textureBackgroundIsland.Width)
                _backgroundIslandCentreX = 1920 + _textureBackgroundIsland.Width;

            _waterSparkleAnimationInstance?.Animate();

            if (_wipeTransitionActive)
            {
                _wipeHeight += 20;
                if (_wipeHeight >= 600)
                    _wipeTransitionActive = false;
            }

            _ticks++;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Visible)
            {

                int backgroundSkyCentreX = (int)_backgroundSkyCentreX;
                do
                {
                    Vector2 skyPosition = new Vector2(backgroundSkyCentreX - _textureBackgroundSky.Width / 2, 540 - _textureBackgroundSky.Height / 2);
                    spriteBatch.Draw(_textureBackgroundSky, skyPosition, Color.White);
                    backgroundSkyCentreX += _textureBackgroundSky.Width;
                }
                while (backgroundSkyCentreX - _textureBackgroundSky.Width / 2 < 1920);


                Vector2 deathEggPosition = new Vector2(1750 - _textureBackgroundDeathEgg.Width / 2, 192 - _textureBackgroundDeathEgg.Height / 2);
                spriteBatch.Draw(_textureBackgroundDeathEgg, deathEggPosition, Color.White);


                Vector2 islandPosition = new Vector2((float)_backgroundIslandCentreX - _textureBackgroundIsland.Width / 2, 540 - _textureBackgroundIsland.Height / 2);
                spriteBatch.Draw(_textureBackgroundIsland, islandPosition, Color.White);


                if (_waterSparkleAnimationInstance != null)
                {
                    Vector2 sparklePosition = new Vector2((float)(_backgroundIslandCentreX + 38.0), 892);
                    _waterSparkleAnimationInstance.Draw(spriteBatch, sparklePosition);
                }


                if (_backgroundFlash > 0.0)
                {
                    Color flashColor = new Color((float)_backgroundFlash, 1.0f, 1.0f, 1.0f);

                    spriteBatch.Draw(_textureBackgroundSky, new Rectangle(0, 0, 1920, 1080), flashColor);
                }
            }


            if (_wipeHeight > 0)
            {
                Rectangle topWipe = new Rectangle(0, _wipeHeight - _textureWipe.Height, 1920, _textureWipe.Height);
                spriteBatch.Draw(_textureWipe, topWipe, Color.White);

                Rectangle bottomWipe = new Rectangle(0, 1080 - _wipeHeight, 1920, _textureWipe.Height);
                spriteBatch.Draw(_textureWipe, bottomWipe, Color.White);
            }
        }
    }
}
