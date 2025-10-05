using Microsoft.Xna.Framework.Content;

namespace S2HD.Audio
{
	public static class AudioService
	{
		private static AudioManager _instance;
		public static AudioManager Instance => _instance;

		public static void Init(ContentManager content)
		{
			if (_instance == null)
			{
				_instance = new AudioManager(content);
				_instance.LoadContent();
			}
		}
	}
}
