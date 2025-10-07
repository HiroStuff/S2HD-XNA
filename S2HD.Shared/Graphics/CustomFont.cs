using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

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

        public void LoadFromXml(GraphicsDevice graphicsDevice, string fontPath)
        {
            var xmlDocument = new XmlDocument();
            if (File.Exists(fontPath))
            {
                xmlDocument.Load(fontPath);
            }
            else
            {
                string rel = fontPath.Replace('\\','/');
                if (rel.StartsWith("data/", StringComparison.OrdinalIgnoreCase))
                    rel = rel.Substring(5);
                using (var s = S2HD.Shared.Data.DataService.OpenRead(rel))
                {
                    xmlDocument.Load(s);
                }
            }

            XmlNode root = xmlDocument.SelectSingleNode("font");
            if (root == null)
                throw new XmlException("Missing <font> root node.");


            string fontDirFs = Path.Combine(Path.GetDirectoryName(fontPath) ?? string.Empty, Path.GetFileNameWithoutExtension(fontPath));
            string fontDirKey = (Path.GetDirectoryName(fontPath) ?? string.Empty).Replace('\\','/');
            if (fontDirKey.StartsWith("data/", StringComparison.OrdinalIgnoreCase))
                fontDirKey = fontDirKey.Substring(5);
            fontDirKey += "/" + Path.GetFileNameWithoutExtension(fontPath);

            string shapePath = root.SelectSingleNode("shape")?.InnerText;
            if (!string.IsNullOrEmpty(shapePath))
            {
                string cleanPath = shapePath.TrimStart('/');
                string fullFs = Path.Combine(fontDirFs, cleanPath + ".png");
                if (File.Exists(fullFs))
                {
                    using (var s = File.OpenRead(fullFs))
                        _shapeTexture = Texture2D.FromStream(graphicsDevice, s);
                }
                else
                {
                    string key = (fontDirKey + "/" + cleanPath + ".png").Replace("\\","/");
                    using (var s = S2HD.Shared.Data.DataService.OpenRead(key))
                        _shapeTexture = Texture2D.FromStream(graphicsDevice, s);
                }
                PremultiplyAlpha(_shapeTexture);
            }

            var overlayNodes = root.SelectNodes("overlay");
            if (overlayNodes != null)
            {
                _overlayTextures = new Texture2D[overlayNodes.Count];
                for (int i = 0; i < overlayNodes.Count; i++)
                {
                    string overlayPath = overlayNodes[i].InnerText;

                    string cleanPath = overlayPath.TrimStart('/');
                    string fullFs = Path.Combine(fontDirFs, cleanPath + ".png");
                    if (File.Exists(fullFs))
                    {
                        using (var s = File.OpenRead(fullFs))
                            _overlayTextures[i] = Texture2D.FromStream(graphicsDevice, s);
                    }
                    else
                    {
                        string key = (fontDirKey + "/" + cleanPath + ".png").Replace("\\","/");
                        using (var s = S2HD.Shared.Data.DataService.OpenRead(key))
                            _overlayTextures[i] = Texture2D.FromStream(graphicsDevice, s);
                    }
                    PremultiplyAlpha(_overlayTextures[i]);
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

        private static void PremultiplyAlpha(Texture2D texture)
        {
            if (texture == null)
                return;
            var data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            for (int i = 0; i < data.Length; i++)
            {
                byte a = data[i].A;
                if (a == 255) continue;
                if (a == 0) { data[i] = new Color(0, 0, 0, 0); continue; }
                data[i] = new Color(
                    (byte)(data[i].R * a / 255),
                    (byte)(data[i].G * a / 255),
                    (byte)(data[i].B * a / 255),
                    a
                );
            }
            texture.SetData(data);
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
