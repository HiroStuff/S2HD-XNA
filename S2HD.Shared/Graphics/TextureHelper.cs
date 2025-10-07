using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace S2HD.Graphics
{
    public static class TextureHelper
    {
        public static Texture2D LoadTextureFromStream(GraphicsDevice graphicsDevice, System.IO.Stream stream)
        {
            var texture = Texture2D.FromStream(graphicsDevice, stream);
            FixTextureAlpha(texture);
            return texture;
        }

        public static void FixTextureAlpha(Texture2D texture)
        {
            if (texture.Format != SurfaceFormat.Color)
                return;

            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                var color = data[i];
                if (color.A > 0)
                {
                    float alpha = color.A / 255.0f;
                    data[i] = new Color(
                        (byte)(color.R * alpha),
                        (byte)(color.G * alpha),
                        (byte)(color.B * alpha),
                        color.A
                    );
                }
            }

            texture.SetData(data);
        }
    }
}