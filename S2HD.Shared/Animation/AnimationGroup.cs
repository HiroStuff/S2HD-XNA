using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.IO;

namespace S2HD.Animation
{
    public class AnimationGroup
    {
        private Dictionary<int, AnimationData> _animations = new Dictionary<int, AnimationData>();
        private List<Texture2D> _textures = new List<Texture2D>();

        public IReadOnlyList<Texture2D> Textures => _textures;

        public void LoadFromXml(GraphicsDevice graphicsDevice, string xmlPath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);

            XmlNode root = xmlDocument.SelectSingleNode("anigroup");
            if (root == null)
                throw new XmlException("Missing <anigroup> root node.");


            var textureNodes = root.SelectNodes("textures/texture");
            if (textureNodes != null)
            {
                foreach (XmlNode textureNode in textureNodes)
                {
                    string texturePath = textureNode.InnerText.TrimStart('/');
                    string fullPath = Path.Combine(Path.GetDirectoryName(xmlPath), texturePath + ".png");
                    using (var s = File.OpenRead(fullPath))
                        _textures.Add(Texture2D.FromStream(graphicsDevice, s));
                }
            }


            var animationNodes = root.SelectNodes("animations/animation");
            if (animationNodes != null)
            {
                int animationIndex = 0;
                foreach (XmlNode animationNode in animationNodes)
                {
                    var animation = GetAnimationFromXmlNode(animationNode);
                    _animations[animationIndex] = new AnimationData(animation);
                    animationIndex++;
                }
            }
        }

        private Animation GetAnimationFromXmlNode(XmlNode node)
        {
            int? nextFrameIndex = node.Attributes["next"]?.Value != null ? int.Parse(node.Attributes["next"].Value) : (int?)null;
            int? loopFrameIndex = node.Attributes["loop"]?.Value != null ? int.Parse(node.Attributes["loop"].Value) : (int?)null;

            int defaultTexture = node.Attributes["texture"]?.Value != null ? int.Parse(node.Attributes["texture"].Value) : 0;
            int defaultWidth = node.Attributes["w"]?.Value != null ? int.Parse(node.Attributes["w"].Value) : 0;
            int defaultHeight = node.Attributes["h"]?.Value != null ? int.Parse(node.Attributes["h"].Value) : 0;
            int defaultOffsetX = node.Attributes["offset_x"]?.Value != null ? int.Parse(node.Attributes["offset_x"].Value) : 0;
            int defaultOffsetY = node.Attributes["offset_y"]?.Value != null ? int.Parse(node.Attributes["offset_y"].Value) : 0;
            int defaultDelay = node.Attributes["delay"]?.Value != null ? int.Parse(node.Attributes["delay"].Value) : 0;

            var frames = new List<Animation.AnimationFrame>();
            var frameNodes = node.SelectNodes("frame");
            if (frameNodes != null)
            {
                foreach (XmlNode frameNode in frameNodes)
                {
                    frames.Add(GetFrameFromXmlNode(frameNode, defaultTexture, defaultWidth, defaultHeight, defaultOffsetX, defaultOffsetY, defaultDelay));
                }
            }

            return new Animation(frames, nextFrameIndex, loopFrameIndex);
        }

        private Animation.AnimationFrame GetFrameFromXmlNode(
            XmlNode node,
            int defaultTexture,
            int defaultWidth,
            int defaultHeight,
            int defaultOffsetX,
            int defaultOffsetY,
            int defaultDelay)
        {
            int x = int.Parse(node.Attributes["x"].Value);
            int y = int.Parse(node.Attributes["y"].Value);

            int width = node.Attributes["w"]?.Value != null ? int.Parse(node.Attributes["w"].Value) : defaultWidth;
            int height = node.Attributes["h"]?.Value != null ? int.Parse(node.Attributes["h"].Value) : defaultHeight;
            int texture = node.Attributes["texture"]?.Value != null ? int.Parse(node.Attributes["texture"].Value) : defaultTexture;
            int offsetX = node.Attributes["offset_x"]?.Value != null ? int.Parse(node.Attributes["offset_x"].Value) : defaultOffsetX;
            int offsetY = node.Attributes["offset_y"]?.Value != null ? int.Parse(node.Attributes["offset_y"].Value) : defaultOffsetY;
            int delay = node.Attributes["delay"]?.Value != null ? int.Parse(node.Attributes["delay"].Value) : defaultDelay;

            return new Animation.AnimationFrame
            {
                TextureIndex = texture,
                Offset = new Vector2(offsetX, offsetY),
                Source = new Rectangle(x, y, width, height),
                Delay = delay
            };
        }

        public void AddAnimation(int index, AnimationData animationData)
        {
            _animations[index] = animationData;
        }

        public AnimationData GetAnimation(int index)
        {
            return _animations.TryGetValue(index, out var animation) ? animation : null;
        }
    }

    public class AnimationData
    {
        public Animation Animation { get; set; }

        public AnimationData(Animation animation)
        {
            Animation = animation;
        }
    }
}
