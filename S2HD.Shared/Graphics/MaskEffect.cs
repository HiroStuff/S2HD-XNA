using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace S2HD.Graphics
{
    public class MaskEffect : Effect
    {
        private EffectParameter _inputTextureParam;
        private EffectParameter _maskTextureParam;
        private EffectParameter _maskInputParam;
        private EffectParameter _inputColourParam;
        private EffectParameter _maskColorMultiplyParam;

        public Texture2D InputTexture
        {
            get => _inputTextureParam?.GetValueTexture2D();
            set => _inputTextureParam?.SetValue(value);
        }

        public Texture2D MaskTexture
        {
            get => _maskTextureParam?.GetValueTexture2D();
            set => _maskTextureParam?.SetValue(value);
        }

        public bool MaskInput
        {
            get => _maskInputParam?.GetValueInt32() != 0;
            set => _maskInputParam?.SetValue(value ? 1 : 0);
        }

        public Color InputColour
        {
            get
            {
                if (_inputColourParam == null) return Color.White;
                var vector = _inputColourParam.GetValueVector4();
                return new Color(vector.X, vector.Y, vector.Z, vector.W);
            }
            set => _inputColourParam?.SetValue(new Vector4(value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f));
        }

        public bool MaskColorMultiply
        {
            get => _maskColorMultiplyParam?.GetValueInt32() != 0;
            set => _maskColorMultiplyParam?.SetValue(value ? 1 : 0);
        }

        public MaskEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
            CacheEffectParameters();
        }

        private void CacheEffectParameters()
        {
            _inputTextureParam = Parameters["InputTexture"];
            _maskTextureParam = Parameters["InputTextureMask"];
            _maskInputParam = Parameters["MaskInput"];
            _inputColourParam = Parameters["InputColour"];
            _maskColorMultiplyParam = Parameters["MaskColorMultiply"];
        }

        public MaskEffect(Effect cloneSource) : base(cloneSource)
        {
            CacheEffectParameters();
        }
    }
}
