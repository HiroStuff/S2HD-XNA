using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace S2HD.Animation
{
    public class Animation
    {
        private AnimationFrame[] _frames = new AnimationFrame[0];

        public IReadOnlyList<AnimationFrame> Frames
        {
            get => _frames;
            set => _frames = value.ToArray();
        }

        public int? NextFrameIndex { get; set; }
        public int? LoopFrameIndex { get; set; }

        public int Duration
        {
            get
            {
                return _frames.Sum(x => x.Delay + 1);
            }
        }

        public Animation()
        {
        }

        public Animation(IEnumerable<AnimationFrame> frames)
        {
            _frames = frames.ToArray();
        }

        public Animation(IEnumerable<AnimationFrame> frames, int? nextFrameIndex, int? loopFrameIndex)
        {
            _frames = frames.ToArray();
            NextFrameIndex = nextFrameIndex;
            LoopFrameIndex = loopFrameIndex;
        }

        public struct AnimationFrame
        {
            public int TextureIndex { get; set; }
            public Rectangle Source { get; set; }
            public Vector2 Offset { get; set; }
            public int Delay { get; set; }
        }
    }
}
