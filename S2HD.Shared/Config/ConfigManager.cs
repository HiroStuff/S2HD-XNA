using System.IO;
using System.Globalization;

namespace S2HD.Shared.Config
{
	public class ConfigManager
	{
		public int FullscreenMode { get; private set; } = 0;
		public int SoundVolumeIndex { get; private set; } = 10;
		public int MusicVolumeIndex { get; private set; } = 10;
		public float MasterVolume { get; private set; } = 1.0f;
		public bool WaterEffects { get; private set; } = true;
		public bool Shadows { get; private set; } = true;
		public bool HeatEffects { get; private set; } = true;

		public static string GetConfigDirectory()
		{
			string docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			return Path.Combine(docs, "SonicOrcaXNA");
		}

		public static string GetConfigPath() => Path.Combine(GetConfigDirectory(), "sonicorca.cfg");

		public static ConfigManager Load()
		{
			var cfg = new ConfigManager();
			try
			{
				string path = GetConfigPath();
				if (!File.Exists(path)) return cfg;
				string section = string.Empty;
				foreach (var rawLine in File.ReadAllLines(path))
				{
					var line = rawLine.Trim();
					if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
					if (line.StartsWith("[") && line.EndsWith("]"))
					{
						section = line.Substring(1, line.Length - 2).ToLowerInvariant();
						continue;
					}
					int eq = line.IndexOf('=');
					if (eq < 0) continue;
					string key = line.Substring(0, eq).Trim().ToLowerInvariant();
					string value = line.Substring(eq + 1).Trim();
					switch (section)
					{
						case "video":
							if (key == "fullscreen") { if (int.TryParse(value, out var v)) cfg.FullscreenMode = v; }
							break;
						case "audio":
							if (key == "sound_volume") { if (int.TryParse(value, out var sv)) cfg.SoundVolumeIndex = sv; }
							else if (key == "music_volume") { if (int.TryParse(value, out var mvIdx)) cfg.MusicVolumeIndex = mvIdx; }
							else if (key == "volume") cfg.MasterVolume = float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var mv) ? mv : cfg.MasterVolume;
							break;
						case "graphics":
							if (key == "water_effects") cfg.WaterEffects = value.ToLowerInvariant().Contains("true");
							else if (key == "shadows") cfg.Shadows = value.ToLowerInvariant().Contains("true");
							else if (key == "heat_effects") cfg.HeatEffects = value.ToLowerInvariant().Contains("true");
							break;
					}
				}
			}
			catch { }
			return cfg;
		}
	}
}
