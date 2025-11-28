using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace S2HD.Graphics
{
    public class CustomFont
    {
        private Dictionary<char, CharacterDefinition> _characterDefinitions;
        private Texture2D _shapeTexture;
        private Texture2D[] _overlayTextures;

        public int DefaultWidth { get; private set; }
        public int Height { get; private set; }
        public int Tracking { get; private set; }
        public Vector2? DefaultShadow { get; private set; }

        public CustomFont()
        {
            _characterDefinitions = new Dictionary<char, CharacterDefinition>();
        }

        public void LoadFromXml(ContentManager content, string fontPath)
        {
            var xmlDocument = new XmlDocument();
            
            Stream stream = null;
            try
            {
                stream = TitleContainer.OpenStream(fontPath);
            }
            catch
            {
                try
                {
                    string altPath = fontPath.Replace("Content/", "");
                    stream = TitleContainer.OpenStream(altPath);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to load font from '{fontPath}': {ex.Message}", ex);
                }
            }
            
            using (stream)
            {
                xmlDocument.Load(stream);
            }

            XmlNode root = xmlDocument.SelectSingleNode("font");
            if (root == null)
                throw new XmlException($"Missing <font> root node in '{fontPath}'.");

            string fontDir = fontPath.Replace("Content/", "").Replace("_FONT", "");

            string shapePath = root.SelectSingleNode("shape")?.InnerText;
            if (!string.IsNullOrEmpty(shapePath))
            {
                string cleanPath = shapePath.TrimStart('/');
                _shapeTexture = content.Load<Texture2D>($"{fontDir}/{cleanPath}");
            }

            var overlayNodes = root.SelectNodes("overlay");
            if (overlayNodes != null)
            {
                _overlayTextures = new Texture2D[overlayNodes.Count];
                for (int i = 0; i < overlayNodes.Count; i++)
                {
                    string overlayPath = overlayNodes[i].InnerText;

                    string cleanPath = overlayPath.TrimStart('/');
                    _overlayTextures[i] = content.Load<Texture2D>($"{fontDir}/{cleanPath}");
                }
            }

            DefaultWidth = int.Parse(root.GetNodeInnerText("width", "0"));
            Height = int.Parse(root.GetNodeInnerText("height", "0"));
            Tracking = int.Parse(root.GetNodeInnerText("tracking", "0"));

            var shadowNode = root.SelectSingleNode("shadow");
            if (shadowNode != null)
            {
                DefaultShadow = new Vector2(
                    int.Parse(shadowNode.Attributes["x"].Value),
                    int.Parse(shadowNode.Attributes["y"].Value)
                );
            }

            var chardefNodes = root.SelectNodes("chardefs/chardef");
            if (chardefNodes != null)
            {
                foreach (XmlNode chardefNode in chardefNodes)
                {
                    ParseCharacterDefinition(chardefNode);
                }
            }
        }

        private void ParseCharacterDefinition(XmlNode chardefNode)
        {
            char character = chardefNode.Attributes["char"].Value[0];

            var rectNode = chardefNode.SelectSingleNode("rect");
            var rect = new Rectangle(
                int.Parse(rectNode.Attributes["x"].Value),
                int.Parse(rectNode.Attributes["y"].Value),
                int.Parse(rectNode.Attributes["w"].Value),
                int.Parse(rectNode.Attributes["h"].Value)
            );

            var offsetNode = chardefNode.SelectSingleNode("offset");
            var offset = offsetNode != null
                ? new Vector2(
                    int.Parse(offsetNode.Attributes["x"].Value),
                    int.Parse(offsetNode.Attributes["y"].Value))
                : Vector2.Zero;

            int width = chardefNode.TryGetNodeInnerText("width", out string widthStr)
                ? int.Parse(widthStr)
                : rect.Width;

            _characterDefinitions[character] = new CharacterDefinition(character, rect, offset, width);
        }

        public Rectangle MeasureString(string text)
        {
            return MeasureString(text, new Rectangle(), FontAlignment.Left);
        }

        public Rectangle MeasureString(string text, Rectangle boundary, FontAlignment alignment)
        {
            float width = 0;
            float height = Height;

            foreach (char c in text)
            {
                if (_characterDefinitions.TryGetValue(c, out CharacterDefinition charDef))
                {
                    width += charDef.Width;
                }
                else
                {
                    width += DefaultWidth;
                }
                width += Tracking;
            }

            if (text.Length > 0)
                width -= Tracking;

            float x = 0;
            float y = 0;

            switch (alignment & FontAlignment.HorizontalMask)
            {
                case FontAlignment.Left:
                    x = boundary.X;
                    break;
                case FontAlignment.Center:
                    x = boundary.X + (boundary.Width - width) / 2;
                    break;
                case FontAlignment.Right:
                    x = boundary.Right - width;
                    break;
            }

            switch (alignment & FontAlignment.VerticalMask)
            {
                case FontAlignment.Top:
                    y = boundary.Y;
                    break;
                case FontAlignment.Middle:
                    y = boundary.Y + (boundary.Height - height) / 2;
                    break;
                case FontAlignment.Bottom:
                    y = boundary.Bottom - height;
                    break;
            }

            return new Rectangle((int)x, (int)y, (int)width, (int)height);
        }

        public void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color, int overlay = -1)
        {
            DrawString(spriteBatch, text, position, color, overlay, false);
        }

        public void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color, int overlay, bool centerAlign)
        {
            Vector2 currentPosition = position;

            if (centerAlign)
            {
                float textWidth = MeasureString(text).Width;
                currentPosition.X -= textWidth / 2;
            }

            foreach (char c in text)
            {
                if (_characterDefinitions.TryGetValue(c, out CharacterDefinition charDef))
                {
                    Vector2 charPosition = currentPosition + charDef.Offset;


                    if (DefaultShadow.HasValue && DefaultShadow.Value != Vector2.Zero)
                    {
                        Vector2 shadowPosition = charPosition + DefaultShadow.Value;
                        Color shadowColor = new Color((byte)0, (byte)0, (byte)0, color.A);
                        spriteBatch.Draw(_shapeTexture, shadowPosition, charDef.SourceRectangle, shadowColor);
                    }

                    spriteBatch.Draw(_shapeTexture, charPosition, charDef.SourceRectangle, color);

                    if (overlay >= 0 && overlay < _overlayTextures.Length && _overlayTextures[overlay] != null)
                    {
                        spriteBatch.Draw(_overlayTextures[overlay], charPosition, charDef.SourceRectangle, color);
                    }

                    currentPosition.X += charDef.Width + Tracking;
                }
                else
                {
                    currentPosition.X += DefaultWidth + Tracking;
                }
            }
        }

        public CharacterDefinition GetCharacter(char c)
        {
            return _characterDefinitions.TryGetValue(c, out CharacterDefinition charDef) ? charDef : null;
        }

        public class CharacterDefinition
        {
            public char Key { get; }
            public Rectangle SourceRectangle { get; }
            public Vector2 Offset { get; }
            public int Width { get; }

            public CharacterDefinition(char key, Rectangle sourceRectangle, Vector2 offset, int width)
            {
                Key = key;
                SourceRectangle = sourceRectangle;
                Offset = offset;
                Width = width;
            }
        }
    }

    [Flags]
    public enum FontAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Top = 0,
        Middle = 4,
        Bottom = 8,
        HorizontalMask = 3,
        VerticalMask = 12
    }
}
