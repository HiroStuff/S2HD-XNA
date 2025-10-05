using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace S2HD.Animation
{
    public class AnimationInstance
    {
        private Texture2D _texture;
        private Rectangle[] _frames;
        private Animation _animation;
        private AnimationGroup _animationGroup;
        private int _currentFrame;
        private float _frameTime;
        private float _currentTime;
        private bool _isPlaying;
        private bool _additiveBlending;
        private int _cycles;
        private int _index;

        public bool AdditiveBlending
        {
            get => _additiveBlending;
            set => _additiveBlending = value;
        }

        public int Cycles => _cycles;
        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public AnimationInstance(Texture2D texture, Rectangle[] frames)
        {
            _texture = texture;
            _frames = frames;
            _currentFrame = 0;
            _frameTime = 1.0f / 60.0f;
            _currentTime = 0;
            _isPlaying = true;
            _cycles = 0;
        }

        public AnimationInstance(Texture2D texture, Rectangle[] frames, int animationIndex)
        {
            _texture = texture;
            _frames = frames;
            _currentFrame = 0;
            _frameTime = 1.0f / 60.0f;
            _currentTime = 0;
            _isPlaying = true;
            _cycles = 0;
            _index = animationIndex;
        }

        public AnimationInstance(AnimationGroup animationGroup, int animationIndex)
        {
            var animationData = animationGroup.GetAnimation(animationIndex);
            if (animationData != null)
            {
                _animation = animationData.Animation;
                _animationGroup = animationGroup;
            }
            _currentFrame = 0;
            _currentTime = 0;
            _isPlaying = true;
            _cycles = 0;
            _index = animationIndex;
        }

        public void Animate()
        {
            if (!_isPlaying || _animation == null || _animation.Frames.Count == 0)
                return;

            var currentFrameData = _animation.Frames[_currentFrame];
            float frameDelay = (currentFrameData.Delay + 1) / 60.0f;

            _currentTime += 1.0f / 60.0f;

            if (_currentTime >= frameDelay)
            {
                _currentTime = 0;
                _currentFrame++;

                if (_currentFrame >= _animation.Frames.Count)
                {
                    if (_animation.LoopFrameIndex.HasValue)
                    {
                        _currentFrame = _animation.LoopFrameIndex.Value;
                    }
                    else
                    {
                        _currentFrame = 0;
                    }
                    _cycles++;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            Draw(spriteBatch, Color.White, position);
        }

        public void Draw(SpriteBatch spriteBatch, Color color, Vector2 position)
        {
            if (_animation == null || _currentFrame >= _animation.Frames.Count)
                return;

            var frameData = _animation.Frames[_currentFrame];
            var texture = _animationGroup.Textures[frameData.TextureIndex];


            var drawPosition = position + frameData.Offset - new Vector2(frameData.Source.Width / 2, frameData.Source.Height / 2);

            spriteBatch.Draw(texture, drawPosition, frameData.Source, color);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 origin, float scale)
        {
            if (_animation == null || _currentFrame >= _animation.Frames.Count)
                return;

            var frameData = _animation.Frames[_currentFrame];
            var texture = _animationGroup.Textures[frameData.TextureIndex];


            var drawPosition = position + frameData.Offset - new Vector2(frameData.Source.Width / 2, frameData.Source.Height / 2);

            spriteBatch.Draw(texture, drawPosition, frameData.Source, Color.White, rotation, origin, scale, SpriteEffects.None, 0);
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        public void Play()
        {
            _isPlaying = true;
        }

        public void Reset()
        {
            _currentFrame = 0;
            _currentTime = 0;
            _cycles = 0;
        }
    }
}
