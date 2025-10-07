using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace S2HD.Audio
{
	public class AudioManager
	{
		private readonly string _dataRoot;
		private Dictionary<string, SoundEffect> _soundEffects;
		private Dictionary<string, Song> _songs;
		private Song _currentSong;
		private bool _shouldLoop = true;
		private float _masterVolume = 1.0f;
		private float _musicVolume = 0.2f;
		private float _soundVolume = 1.0f;

		public AudioManager(string dataRoot)
		{
			_dataRoot = dataRoot;
			_soundEffects = new Dictionary<string, SoundEffect>();
			_songs = new Dictionary<string, Song>();            
			MediaPlayer.Volume = _masterVolume * _musicVolume;
			MediaPlayer.MediaStateChanged += OnMediaStateChanged;
		}
		
		private void OnMediaStateChanged(object sender, EventArgs e)
		{
			if (MediaPlayer.State == MediaState.Stopped && _currentSong != null && _shouldLoop)
			{
				MediaPlayer.Play(_currentSong);
			}
		}

		public void LoadContent()
		{
			LoadSoundEffect("NAVIGATE/CURSOR", "SONICORCA/SOUND/NAVIGATE/CURSOR");
			LoadSoundEffect("NAVIGATE/YES", "SONICORCA/SOUND/NAVIGATE/YES");
			LoadSoundEffect("NAVIGATE/BACK", "SONICORCA/SOUND/NAVIGATE/BACK");
			LoadSoundEffect("NAVIGATE/NO", "SONICORCA/SOUND/NAVIGATE/NO");

			LoadSoundEffect("SPARKLE", "SONICORCA/SOUND/SPARKLE");
			LoadSoundEffect("SHOOTINGSTAR", "SONICORCA/SOUND/SHOOTINGSTAR");

			LoadSong("TITLE/MUSIC", "SONICORCA/TITLE/MUSIC");
			LoadSong("OPTIONS/MUSIC", "SONICORCA/MUSIC/OPTIONS");
		}

		private void LoadSoundEffect(string key, string relativePathWithoutExtension)
		{
			try
			{
				string sinfoPath = relativePathWithoutExtension + ".sinfo";
				if (TryLoadSampleInfo(sinfoPath, out var sampleInfo))
				{
					string oggPath = sampleInfo.SamplePath;
					using (var stream = S2HD.Shared.Data.DataService.OpenRead(oggPath))
					{
						var soundEffect = SoundEffect.FromStream(stream);
						_soundEffects[key] = soundEffect;
					}
				}
				else
				{
					string oggPath = relativePathWithoutExtension + ".ogg";
					using (var stream = S2HD.Shared.Data.DataService.OpenRead(oggPath))
					{
						var soundEffect = SoundEffect.FromStream(stream);
						_soundEffects[key] = soundEffect;
					}
				}
			}
			catch {}
		}

		private void LoadSong(string key, string relativePathWithoutExtension)
		{
			try
			{
				string sinfoPath = relativePathWithoutExtension + ".sinfo";
				if (TryLoadSampleInfo(sinfoPath, out var sampleInfo))
				{
					string oggPath = sampleInfo.SamplePath;
					using (var stream = S2HD.Shared.Data.DataService.OpenRead(oggPath))
					{
						string tempPath = Path.GetTempFileName() + ".ogg";
						using (var fileStream = File.Create(tempPath))
						{
							stream.CopyTo(fileStream);
						}
						var song = Song.FromUri(key, new Uri(tempPath));
						_songs[key] = song;
					}
				}
				else
				{
					string oggPath = relativePathWithoutExtension + ".ogg";
					using (var stream = S2HD.Shared.Data.DataService.OpenRead(oggPath))
					{
						string tempPath = Path.GetTempFileName() + ".ogg";
						using (var fileStream = File.Create(tempPath))
						{
							stream.CopyTo(fileStream);
						}
						var song = Song.FromUri(key, new Uri(tempPath));
						_songs[key] = song;
					}
				}
			}
			catch {}
		}

		public void PlaySound(string key)
		{
			if (_soundEffects.TryGetValue(key, out var soundEffect))
			{
				soundEffect.Play(_masterVolume * _soundVolume, 0.0f, 0.0f);
			}
		}

		public void PlayMusic(string key, bool loop = true)
		{
			if (_songs.TryGetValue(key, out var song))
			{
				if (_currentSong != null && MediaPlayer.State == MediaState.Playing)
				{
					MediaPlayer.Stop();
				}
				_currentSong = song;
				_shouldLoop = loop;
				MediaPlayer.IsRepeating = loop;
				MediaPlayer.Volume = _masterVolume * _musicVolume;
				if (_masterVolume * _musicVolume <= 0.0001f) return;
				MediaPlayer.Play(song);
			}
		}

		public void StopMusic()
		{
			if (MediaPlayer.State == MediaState.Playing)
			{
				MediaPlayer.Stop();
			}
		}

		public void StopAll()
		{
			StopMusic();
		}

		public void SetMusicVolume(float volume)
		{
			_musicVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
			MediaPlayer.Volume = _masterVolume * _musicVolume;
			if (_masterVolume * _musicVolume <= 0.0001f && MediaPlayer.State == MediaState.Playing)
			{
				MediaPlayer.Pause();
			}
			else if (_currentSong != null && MediaPlayer.State == MediaState.Paused && _masterVolume * _musicVolume > 0.0001f)
			{
				MediaPlayer.Resume();
			}
		}

		public void SetSoundVolume(float volume)
		{
			_soundVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
		}

		public void SetMasterVolume(float volume)
		{
			_masterVolume = MathHelper.Clamp(volume, 0.0f, 1.0f);
			MediaPlayer.Volume = _masterVolume * _musicVolume;
			if (_masterVolume * _musicVolume <= 0.0001f && MediaPlayer.State == MediaState.Playing)
			{
				MediaPlayer.Pause();
			}
			else if (_currentSong != null && MediaPlayer.State == MediaState.Paused && _masterVolume * _musicVolume > 0.0001f)
			{
				MediaPlayer.Resume();
			}
		}

		public void ApplyVolumes(float master, float music, float sound)
		{
			SetMasterVolume(master);
			SetMusicVolume(music);
			SetSoundVolume(sound);
		}

		public float GetMasterVolume() => _masterVolume;
		public float GetMusicVolume() => _musicVolume;
		public float GetSoundVolume() => _soundVolume;

		private bool TryLoadSampleInfo(string sinfoPath, out SampleInfo sampleInfo)
		{
			sampleInfo = null;
			try
			{
				using (var stream = S2HD.Shared.Data.DataService.OpenRead(sinfoPath))
				{
					var xmlDocument = new XmlDocument();
					xmlDocument.Load(stream);
					
					var sampleNode = xmlDocument.SelectSingleNode("sampleinfo/sample");
					if (sampleNode == null) return false;
					
					string samplePath = sampleNode.InnerText.TrimStart('/');
					
					int? loopSampleIndex = null;
					var loopNode = xmlDocument.SelectSingleNode("sampleinfo/loop");
					if (loopNode != null && int.TryParse(loopNode.InnerText, out int loopIndex))
					{
						loopSampleIndex = loopIndex;
					}
					
					sampleInfo = new SampleInfo
					{
						SamplePath = samplePath,
						LoopSampleIndex = loopSampleIndex
					};
					return true;
				}
			}
			catch
			{
				return false;
			}
		}
	}

	public class SampleInfo
	{
		public string SamplePath { get; set; }
		public int? LoopSampleIndex { get; set; }
		public bool HasLoopPoint => LoopSampleIndex.HasValue;
	}
}
