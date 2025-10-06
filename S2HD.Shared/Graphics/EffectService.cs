using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace S2HD.Graphics
{
	public static class EffectService
	{
		private static ContentManager _content;

		public static void Init(ContentManager content)
		{
			_content = content;
		}

		public static Effect LoadEffect(string assetPath)
		{
			return _content.Load<Effect>(assetPath);
		}
	}
}
