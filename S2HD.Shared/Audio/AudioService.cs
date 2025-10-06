using System;

namespace S2HD.Audio
{
	public static class AudioService
	{
		private static AudioManager _instance;
		public static AudioManager Instance => _instance;

		public static void Init(string dataRoot)
		{
			if (_instance == null)
			{
				_instance = new AudioManager(dataRoot);
				_instance.LoadContent();
			}
		}
	}
}
