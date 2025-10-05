using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace S2HD.Graphics
{
    public class MaskRenderer : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private MaskEffect _maskEffect;
        private Texture2D _texture;
        private Rectangle _source;
        private Rectangle _destination;
        private Texture2D _maskTexture;
        private Rectangle _maskSource;
        private Rectangle _maskDestination;
        private BlendState _blendMode = BlendState.AlphaBlend;
        private Color _color = Color.White;
        private Matrix _maskModelMatrix = Matrix.Identity;
        private Matrix _targetModelMatrix = Matrix.Identity;
        private Matrix _intersectionModelMatrix = Matrix.Identity;

        public Texture2D Texture
        {
            get => _texture;
            set => _texture = value;
        }

        public Rectangle Source
        {
            get => _source;
            set => _source = value;
        }

        public Rectangle Destination
        {
            get => _destination;
            set => _destination = value;
        }

        public Texture2D MaskTexture
        {
            get => _maskTexture;
            set => _maskTexture = value;
        }

        public Rectangle MaskSource
        {
            get => _maskSource;
            set => _maskSource = value;
        }

        public Rectangle MaskDestination
        {
            get => _maskDestination;
            set => _maskDestination = value;
        }

        public BlendState BlendMode
        {
            get => _blendMode;
            set => _blendMode = value;
        }

        public Color Colour
        {
            get => _color;
            set => _color = value;
        }

        public Matrix MaskModelMatrix
        {
            get => _maskModelMatrix;
            set => _maskModelMatrix = value;
        }

        public Matrix TargetModelMatrix
        {
            get => _targetModelMatrix;
            set => _targetModelMatrix = value;
        }

        public Matrix IntersectionModelMatrix
        {
            get => _intersectionModelMatrix;
            set => _intersectionModelMatrix = value;
        }

        public MaskRenderer(GraphicsDevice graphicsDevice, ContentManager content)
        {
            _graphicsDevice = graphicsDevice;

            _maskEffect = new MaskEffect(content.Load<Effect>("Effects/MaskEffect"));
        }

        public void Render(SpriteBatch spriteBatch, bool maskColorMultiply = false)
        {
            if (_texture == null)
                throw new InvalidOperationException("Texture was null.");


            Rectangle clippedDestination = Rectangle.Intersect(_destination, _maskDestination);

            if (clippedDestination.Width <= 0 || clippedDestination.Height <= 0)
                return;


            _maskEffect.InputTexture = _texture;
            _maskEffect.MaskTexture = _maskTexture;
            _maskEffect.MaskInput = _maskTexture != null;
            _maskEffect.InputColour = _color;
            _maskEffect.MaskColorMultiply = maskColorMultiply;


            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, _blendMode, null, null, null, _maskEffect);


            spriteBatch.Draw(_texture, clippedDestination, _source, _color);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, _blendMode);
        }

        public void Deactivate()
        {

        }

        public void Dispose()
        {
            _maskEffect?.Dispose();
        }
    }
}
