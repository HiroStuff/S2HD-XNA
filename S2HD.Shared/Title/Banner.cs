using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using S2HD.Animation;
using S2HD.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace S2HD.Title
{
    public class Banner
    {
        private const int BannerStartTime = 332;
        private static readonly Vector2[] SpinningStarOffsets = new Vector2[]
        {
            new Vector2(-214, -41),
            new Vector2(-190, -129),
            new Vector2(-130, -193),
            new Vector2(-50, -233),
            new Vector2(50, -233),
            new Vector2(130, -193),
            new Vector2(190, -129),
            new Vector2(214, -41)
        };

        private AnimationGroup _animationGroup;
        private Texture2D _bannerTexture;
        private Texture2D _bannerInsideTexture;
        private Texture2D _hdStarTexture;
        private Texture2D _maskTexture;
        private Texture2D _titleOutlineTexture;

        private AnimationInstance[] _preBannerStarSpin;
        private AnimationInstance _sonicAnimationInstance;
        private AnimationInstance _sonicHandAnimationInstance;
        private AnimationInstance _tailsAnimationInstance;
        private AnimationInstance _tailsTailsAnimationInstance;
        private AnimationInstance _sonic2AnimationInstance;
        private AnimationInstance _theHedgehogAnimationInstance;
        private AnimationInstance _hdAnimationInstance;

        private readonly EffectEventManager _effectEventManager = new EffectEventManager();
        private readonly MaskRenderer _maskRenderer;
        private int _ticks;
        private double _bannerInsideOpacity;
        private double _bannerOpacity;
        private double _sonic2Opacity;
        private double _maskOffset;

        public Vector2 Position { get; set; }
        public bool ShowStarLensFare { get; set; }

        public Banner(GraphicsDevice graphicsDevice, string dataRoot)
        {
            _maskRenderer = new MaskRenderer(graphicsDevice, dataRoot);
            LoadContent(graphicsDevice, dataRoot);
            Position = new Vector2(960, 476);
        }

        private void LoadContent(GraphicsDevice graphicsDevice, string dataRoot)
        {
            _animationGroup = new AnimationGroup();
            _animationGroup.LoadFromXml(graphicsDevice, Path.Combine(dataRoot, "SONICORCA/TITLE/ANIGROUP.anim"));

            using (var s = S2HD.Shared.Data.DataService.OpenRead("SONICORCA/TITLE/FRAMES/0.png"))
                _bannerTexture = Texture2D.FromStream(graphicsDevice, s);
            using (var s = S2HD.Shared.Data.DataService.OpenRead("SONICORCA/TITLE/FRAMES/1.png"))
                _bannerInsideTexture = Texture2D.FromStream(graphicsDevice, s);
            using (var s = S2HD.Shared.Data.DataService.OpenRead("SONICORCA/TITLE/ADDFRAMES/0.png"))
                _titleOutlineTexture = Texture2D.FromStream(graphicsDevice, s);
            using (var s = S2HD.Shared.Data.DataService.OpenRead("SONICORCA/TITLE/ADDFRAMES/1.png"))
                _maskTexture = Texture2D.FromStream(graphicsDevice, s);
            using (var s = S2HD.Shared.Data.DataService.OpenRead("SONICORCA/TITLE/ADDFRAMES/2.png"))
                _hdStarTexture = Texture2D.FromStream(graphicsDevice, s);
        }

        public void Reset()
        {
            _ticks = 0;
            _preBannerStarSpin = new AnimationInstance[8];
            _sonicAnimationInstance = null;
            _sonicHandAnimationInstance = null;
            _tailsAnimationInstance = null;
            _tailsTailsAnimationInstance = null;
            _sonic2AnimationInstance = null;
            _theHedgehogAnimationInstance = null;
            _hdAnimationInstance = null;
            _bannerInsideOpacity = 0.0;
            _bannerOpacity = 0.0;
            _sonic2Opacity = 0.0;
            _maskOffset = -1.0;
            ShowStarLensFare = false;
        }

        public void DoShine(int duration)
        {
            _effectEventManager.BeginEvent(EffectShine(duration));
        }

        private IEnumerable<UpdateResult> EffectShine(int duration)
        {
            _maskOffset = -1.0;
            do
            {
                yield return UpdateResult.Next;
                _maskOffset += 2.0 / duration;
            }
            while (_maskOffset < 1.0);
            _maskOffset = 1.0;
        }

        public void Update()
        {
            if (_ticks == 268)
                DoShine(368);

            if (_ticks >= 296)
                _bannerInsideOpacity = Math.Min(1.0, _bannerInsideOpacity + 1.0 / 72.0);

            if (_ticks >= 332)
                _bannerOpacity = Math.Min(1.0, _bannerOpacity + 1.0 / 32.0);


            for (int index = 0; index < _preBannerStarSpin.Length; index++)
            {
                if (_preBannerStarSpin[index] == null)
                {
                    if (_ticks >= 298 + index * 4)
                        _preBannerStarSpin[index] = new AnimationInstance(_animationGroup, 3);
                }
                else if (_preBannerStarSpin[index].Cycles == 0)
                    _preBannerStarSpin[index].Animate();
            }

            if (_ticks >= 346)
            {
                _sonic2Opacity = Math.Min(1.0, _sonic2Opacity + 1.0 / 16.0);
                if (_sonic2AnimationInstance == null)
                    _sonic2AnimationInstance = new AnimationInstance(_animationGroup, 2);
            }

            if (_ticks >= 360)
            {
                if (_theHedgehogAnimationInstance == null)
                    _theHedgehogAnimationInstance = new AnimationInstance(_animationGroup, 1);
                _theHedgehogAnimationInstance.Animate();
            }

            if (_ticks >= 364)
            {
                if (_sonicAnimationInstance == null)
                    _sonicAnimationInstance = new AnimationInstance(_animationGroup, 4);
                _sonicAnimationInstance.Animate();
            }

            if (_ticks >= 413)
            {
                if (_sonicHandAnimationInstance == null)
                    _sonicHandAnimationInstance = new AnimationInstance(_animationGroup, 5);
                _sonicHandAnimationInstance.Animate();
            }

            if (_ticks >= 400)
            {
                if (_tailsAnimationInstance == null)
                {
                    _tailsAnimationInstance = new AnimationInstance(_animationGroup, 6);
                    _tailsTailsAnimationInstance = new AnimationInstance(_animationGroup, 7);
                }
                _tailsAnimationInstance.Animate();
                _tailsTailsAnimationInstance.Animate();
            }

            if (_ticks >= 436)
                _sonic2AnimationInstance?.Animate();

            if (_ticks >= 454)
            {
                if (_hdAnimationInstance == null)
                    _hdAnimationInstance = new AnimationInstance(_animationGroup, 0);
                _hdAnimationInstance.Animate();
            }

            _effectEventManager.Update();
            _ticks++;
        }

        public void Draw(SpriteBatch spriteBatch)
        {

            if (_bannerInsideOpacity > 0.0 && _bannerOpacity < 1.0)
            {
                Color insideColor = new Color((float)_bannerInsideOpacity, 1.0f, 1.0f, 1.0f);
                Vector2 insidePosition = Position + new Vector2(-_bannerInsideTexture.Width / 2, -114 - _bannerInsideTexture.Height / 2);
                spriteBatch.Draw(_bannerInsideTexture, insidePosition, insideColor);
            }


            if (_bannerOpacity > 0.0)
            {
                Color bannerColor = new Color((float)_bannerOpacity, 1.0f, 1.0f, 1.0f);
                Vector2 bannerPosition = Position + new Vector2(-_bannerTexture.Width / 2, -_bannerTexture.Height / 2);
                spriteBatch.Draw(_bannerTexture, bannerPosition, bannerColor);
            }


            for (int index = 0; index < 8; index++)
            {
                if (_preBannerStarSpin[index] != null && _preBannerStarSpin[index].Cycles == 0)
                    _preBannerStarSpin[index].Draw(spriteBatch, Position + SpinningStarOffsets[index]);
            }


            DrawSonicAndTails(spriteBatch);


            if (_sonic2AnimationInstance != null)
            {
                Color sonic2Color = new Color((float)_sonic2Opacity, 1.0f, 1.0f, 1.0f);
                _sonic2AnimationInstance.Draw(spriteBatch, sonic2Color, Position + new Vector2(0, 77));
            }


            if (_theHedgehogAnimationInstance != null)
                _theHedgehogAnimationInstance.Draw(spriteBatch, Position + new Vector2(0, 153));


            if (_hdAnimationInstance != null)
                _hdAnimationInstance.Draw(spriteBatch, Position + new Vector2(0, 266));

            if (ShowStarLensFare)
            {
                Vector2 starPosition = Position + new Vector2(-_hdStarTexture.Width / 2, 188 - _hdStarTexture.Height / 2);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                spriteBatch.Draw(_hdStarTexture, starPosition, Color.White);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            }
        }

        public void DrawUnfaded(SpriteBatch spriteBatch)
        {
            DrawTitleShine(spriteBatch);
        }

        private void DrawSonicAndTails(SpriteBatch spriteBatch)
        {
            if (_sonicAnimationInstance != null)
                _sonicAnimationInstance.Draw(spriteBatch, Position + new Vector2(89, -158));

            if (_sonicHandAnimationInstance != null)
                _sonicHandAnimationInstance.Draw(spriteBatch, Position + new Vector2(230, -89));

            if (_tailsTailsAnimationInstance != null)
                _tailsTailsAnimationInstance.Draw(spriteBatch, Position + new Vector2(-264, -65));

            if (_tailsAnimationInstance != null)
                _tailsAnimationInstance.Draw(spriteBatch, Position + new Vector2(-108, -121));
        }

        private void DrawTitleShine(SpriteBatch spriteBatch)
        {
            if (_maskOffset <= -1.0 || _maskOffset >= 1.0)
                return;


            _maskRenderer.Texture = _titleOutlineTexture;
            _maskRenderer.Source = new Rectangle(0, 0, _titleOutlineTexture.Width, _titleOutlineTexture.Height);
            _maskRenderer.Destination = new Rectangle(
                (int)(Position.X - _titleOutlineTexture.Width / 2),
                (int)(Position.Y - _titleOutlineTexture.Height / 2),
                _titleOutlineTexture.Width,
                _titleOutlineTexture.Height
            );

            _maskRenderer.MaskTexture = _maskTexture;
            _maskRenderer.MaskSource = new Rectangle(0, 0, _maskTexture.Width, _maskTexture.Height);
            _maskRenderer.MaskDestination = new Rectangle(
                (int)(Position.X + (_maskOffset * 1920.0)),
                (int)(Position.Y + (_maskOffset * 1080.0)),
                1920,
                1080
            );

            _maskRenderer.BlendMode = BlendState.Additive;
            _maskRenderer.Colour = Color.White;


            _maskRenderer.Render(spriteBatch, true);
        }
    }
}
