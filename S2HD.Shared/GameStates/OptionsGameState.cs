using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using S2HD.Graphics;
using S2HD.Audio;
using System.IO;
using S2HD.Shared.Graphics;
using S2HD.Shared.Config;

namespace S2HD.GameStates
{
	public class OptionsGameState : IGameState
	{
		private readonly GraphicsDevice _graphicsDevice;
		private readonly SpriteBatch _spriteBatch;

#if ANDROID
		private float Scale => System.Math.Min(_graphicsDevice.Viewport.Width / 1920f, _graphicsDevice.Viewport.Height / 1080f);
		private int ScreenWidth => _graphicsDevice.Viewport.Width;
		private int ScreenHeight => _graphicsDevice.Viewport.Height;
#else
		private float Scale => 1f;
		private int ScreenWidth => 1920;
		private int ScreenHeight => 1080;
#endif
		private CustomFont _fontImpactRegular;
		private CustomFont _fontImpactItalic;
		private Texture2D _whiteTexture;
		private Texture2D _backgroundTexture;
		private Texture2D _selectionBarTexture;
		private Texture2D _buttonATexture;
		private Texture2D _buttonBTexture;
		private Texture2D _audioSliderEmptyTexture;
		private Texture2D _audioSliderSilverTexture;
		private Texture2D _audioSliderGoldTexture;
		private bool _loaded;
		private int _selectionIndex;
		private readonly string[] _items = { "AUDIO", "VIDEO" };
		private bool _complete;
		private KeyboardState _prevKeyboard;
		private GamePadState _prevGamepad;
		private AudioManager _audioManager => AudioService.Instance;
		private float _fadeOpacity = 0f;
		private const float TextScale = 0.72f;
		private const float BarScaleX = 0.72f;
		private const float BarScaleY = 0.58f;
		private const float ButtonLabelScale = 0.70f;

		private bool _audioSubmenuActive;
		private int _audioSelectionIndex;
		private float _volMaster;
		private float _volMusic;
		private float _volSound;
		private float _volMasterInitial;
		private float _volMusicInitial;
		private float _volSoundInitial;
		private int _holdDir;
		private int _holdFrames;
		private const int HoldDelayFrames = 10;
		private const int HoldRepeatFrames = 3;
		private static readonly int[] SliderWidths = new int[] { 0, 45, 59, 73, 89, 105, 121, 139, 159, 181, 223 };

		private bool _videoSubmenuActive;
		private int _videoSelectionIndex;
		private int _videoModeIndex;
		private readonly string[] _videoModeOptions = { "WINDOW", "FULLSCREEN", "BORDERLESS WINDOWED" };
		private readonly Point _videoResolutionFixed = new Point(1920, 1080);
		private bool _videoShadows;
		private bool _videoWaterEffects;
		private bool _videoHeatEffects;
		private int _videoModeIndexInitial;
		private bool _videoShadowsInitial;
		private bool _videoWaterEffectsInitial;
		private bool _videoHeatEffectsInitial;

		public OptionsGameState(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			_graphicsDevice = graphicsDevice;
			_spriteBatch = spriteBatch;
		}

		public void LoadContent(ContentManager content)
		{
			_fontImpactRegular = new CustomFont();
			_fontImpactRegular.LoadFromXml(content, "Content/SONICORCA/FONTS/IMPACT/REGULAR_FONT");

			_fontImpactItalic = new CustomFont();
			_fontImpactItalic.LoadFromXml(content, "Content/SONICORCA/FONTS/IMPACT/ITALIC_FONT");

			_backgroundTexture = content.Load<Texture2D>("SONICORCA/MENU/OPTIONS/MENU3");
			_selectionBarTexture = content.Load<Texture2D>("SONICORCA/MENU/OPTIONS/V2/UI/SELECTION/BAR");
			_buttonATexture = content.Load<Texture2D>("SONICORCA/MENU/GAMEPAD/A");
			_buttonBTexture = content.Load<Texture2D>("SONICORCA/MENU/GAMEPAD/B");
			_audioSliderEmptyTexture = content.Load<Texture2D>("SONICORCA/MENU/OPTIONS/AUDIOSLIDER/EMPTY");
			_audioSliderSilverTexture = content.Load<Texture2D>("SONICORCA/MENU/OPTIONS/AUDIOSLIDER/SILVER");
			_audioSliderGoldTexture = content.Load<Texture2D>("SONICORCA/MENU/OPTIONS/AUDIOSLIDER/GOLD");

			_whiteTexture = new Texture2D(_graphicsDevice, 1, 1);
			_whiteTexture.SetData(new[] { Color.White });

			_prevKeyboard = Keyboard.GetState();
			_prevGamepad = GamePad.GetState(PlayerIndex.One);

			_loaded = true;
		}

		public void Update(GameTime gameTime)
		{
			if (!_loaded) return;

			if (_fadeOpacity < 1f)
			{
				_fadeOpacity += 1f / 60f;
				if (_fadeOpacity > 1f) _fadeOpacity = 1f;
			}

			var ks = Keyboard.GetState();
			var gp = GamePad.GetState(PlayerIndex.One);

			bool up = IsKeyJustPressed(ks, Keys.Up) || IsDPadUpJustPressed(gp);
			bool down = IsKeyJustPressed(ks, Keys.Down) || IsDPadDownJustPressed(gp);
			bool leftJust = IsKeyJustPressed(ks, Keys.Left) || (gp.DPad.Left == ButtonState.Pressed && _prevGamepad.DPad.Left != ButtonState.Pressed);
			bool rightJust = IsKeyJustPressed(ks, Keys.Right) || (gp.DPad.Right == ButtonState.Pressed && _prevGamepad.DPad.Right != ButtonState.Pressed);
			bool back = IsKeyJustPressed(ks, Keys.Escape) || IsButtonJustPressed(gp, Buttons.Back) || IsButtonJustPressed(gp, Buttons.B);
			bool confirm = IsKeyJustPressed(ks, Keys.Enter) || IsButtonJustPressed(gp, Buttons.Start) || IsButtonJustPressed(gp, Buttons.A);

			if (_audioSubmenuActive)
			{
				HandleAudioSubmenuInput(up, down, leftJust, rightJust, ks.IsKeyDown(Keys.Left) || gp.DPad.Left == ButtonState.Pressed, ks.IsKeyDown(Keys.Right) || gp.DPad.Right == ButtonState.Pressed, back, confirm);
			}
			else if (_videoSubmenuActive)
			{
				HandleVideoSubmenuInput(up, down, leftJust, rightJust, back, confirm);
			}
			else
			{
				HandleMainMenuInput(up, down, back, confirm);
			}

			_prevKeyboard = ks;
			_prevGamepad = gp;
		}

		private static int ValueToIndex(float v)
		{
			int len = SliderWidths.Length;
			int index = (int)System.Math.Floor(MathHelper.Clamp(v, 0f, 1f) * len);
			if (index >= len) index = len - 1;
			if (index < 0) index = 0;
			return index;
		}

		private static float IndexToValue(int index)
		{
			int len = SliderWidths.Length;
			index = System.Math.Max(0, System.Math.Min(len - 1, index));
			return (float)index / len;
		}

		private void HandleMainMenuInput(bool up, bool down, bool back, bool confirm)
		{
			if (back)
			{
				_audioManager.PlaySound("NAVIGATE/BACK");
				_complete = true;
				return;
			}
			if (up)
			{
				_selectionIndex = (_selectionIndex - 1 + _items.Length) % _items.Length;
				_audioManager.PlaySound("NAVIGATE/CURSOR");
			}
			else if (down)
			{
				_selectionIndex = (_selectionIndex + 1) % _items.Length;
				_audioManager.PlaySound("NAVIGATE/CURSOR");
			}
			if (confirm)
			{
				_audioManager.PlaySound("NAVIGATE/YES");
				if (_selectionIndex == 0)
				{
					_audioSubmenuActive = true;
					_audioSelectionIndex = 0;
					_volMaster = _volMasterInitial = _audioManager.GetMasterVolume();
					_volMusic = _volMusicInitial = _audioManager.GetMusicVolume();
					_volSound = _volSoundInitial = _audioManager.GetSoundVolume();
				}
				else if (_selectionIndex == 1)
				{
					EnterVideoSubmenu();
				}
			}
		}

		private void EnterVideoSubmenu()
		{
			_videoSubmenuActive = true;
			_videoSelectionIndex = 0;
			var cfg = ConfigManager.Load();
			_videoModeIndex = cfg.FullscreenMode;
			_videoShadows = cfg.Shadows;
			_videoWaterEffects = cfg.WaterEffects;
			_videoHeatEffects = cfg.HeatEffects;
			_videoModeIndexInitial = _videoModeIndex;
			_videoShadowsInitial = _videoShadows;
			_videoWaterEffectsInitial = _videoWaterEffects;
			_videoHeatEffectsInitial = _videoHeatEffects;
		}

		private string GetConfigPath()
		{
			string docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			string dir = Path.Combine(docs, "SonicOrcaXNA");
			return Path.Combine(dir, "sonicorca.cfg");
		}

		private void SaveConfig()
		{
			try
			{
				string path = GetConfigPath();
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				using (var sw = new StreamWriter(path, false))
				{
					sw.WriteLine("[video]");
					sw.WriteLine($"fullscreen = {_videoModeIndex}");
					sw.WriteLine();
					sw.WriteLine("[audio]");
					int soundVol = ValueToIndex(_volSound);
					int musicVol = ValueToIndex(_volMusic);
					sw.WriteLine($"sound_volume = {soundVol}");
					sw.WriteLine($"music_volume = {musicVol}");
					sw.WriteLine($"volume = {_volMaster.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
					sw.WriteLine();
					sw.WriteLine("[graphics]");
					sw.WriteLine($"water_effects = {_videoWaterEffects.ToString().ToLowerInvariant()}");
					sw.WriteLine($"shadows = {_videoShadows.ToString().ToLowerInvariant()}");
				}
			}
			catch { }
		}

		private void HandleAudioSubmenuInput(bool up, bool down, bool leftJust, bool rightJust, bool leftHeld, bool rightHeld, bool back, bool confirm)
		{
			if (back)
			{
				_volMaster = _volMasterInitial;
				_volMusic = _volMusicInitial;
				_volSound = _volSoundInitial;
				_audioManager.ApplyVolumes(_volMaster, _volMusic, _volSound);
				_audioManager.PlaySound("NAVIGATE/BACK");
				_audioSubmenuActive = false;
				_holdDir = 0; _holdFrames = 0;
				return;
			}
			if (up)
			{
				_audioSelectionIndex = (_audioSelectionIndex - 1 + 3) % 3;
				_audioManager.PlaySound("NAVIGATE/CURSOR");
			}
			else if (down)
			{
				_audioSelectionIndex = (_audioSelectionIndex + 1) % 3;
				_audioManager.PlaySound("NAVIGATE/CURSOR");
			}

			if (leftJust)
			{
				StepAudio(-1);
				_holdDir = -1; _holdFrames = 0;
			}
			else if (rightJust)
			{
				StepAudio(1);
				_holdDir = 1; _holdFrames = 0;
			}
			else
			{
				if (_holdDir != 0 && ((_holdDir < 0 && leftHeld) || (_holdDir > 0 && rightHeld)))
				{
					_holdFrames++;
					if (_holdFrames > HoldDelayFrames && (_holdFrames - HoldDelayFrames) % HoldRepeatFrames == 0)
					{
						StepAudio(_holdDir);
					}
				}
				else
				{
					_holdDir = 0; _holdFrames = 0;
				}
			}

			if (confirm)
			{
				_audioManager.ApplyVolumes(_volMaster, _volMusic, _volSound);
				SaveConfig();
				_audioManager.PlaySound("NAVIGATE/YES");
				_audioSubmenuActive = false;
				_holdDir = 0; _holdFrames = 0;
			}
		}

		private void HandleVideoSubmenuInput(bool up, bool down, bool leftJust, bool rightJust, bool back, bool confirm)
		{
			if (back)
			{
				_videoModeIndex = _videoModeIndexInitial;
				_videoShadows = _videoShadowsInitial;
				_videoWaterEffects = _videoWaterEffectsInitial;
				_videoHeatEffects = _videoHeatEffectsInitial;
				_audioManager.PlaySound("NAVIGATE/BACK");
				_videoSubmenuActive = false;
				return;
			}
			if (up)
			{
				_videoSelectionIndex = (_videoSelectionIndex - 1 + 5) % 5;
				_audioManager.PlaySound("NAVIGATE/CURSOR");
			}
			else if (down)
			{
				_videoSelectionIndex = (_videoSelectionIndex + 1) % 5;
				_audioManager.PlaySound("NAVIGATE/CURSOR");
			}
			if (leftJust)
			{
				AdjustVideo(-1);
			}
			else if (rightJust)
			{
				AdjustVideo(1);
			}
			if (confirm)
			{
				SaveConfig();
				VideoService.Apply(_videoModeIndex, _videoResolutionFixed, true);
				_videoModeIndexInitial = _videoModeIndex;
				_videoShadowsInitial = _videoShadows;
				_videoWaterEffectsInitial = _videoWaterEffects;
				_videoHeatEffectsInitial = _videoHeatEffects;
				_audioManager.PlaySound("NAVIGATE/YES");
				_videoSubmenuActive = false;
			}
		}

		private void StepAudio(int dir)
		{
			switch (_audioSelectionIndex)
			{
				case 0:
				{
					int idx = ValueToIndex(_volMaster);
					idx = System.Math.Max(0, System.Math.Min(SliderWidths.Length - 1, idx + dir));
					_volMaster = IndexToValue(idx);
					break;
				}
				case 1:
				{
					int idx = ValueToIndex(_volMusic);
					idx = System.Math.Max(0, System.Math.Min(SliderWidths.Length - 1, idx + dir));
					_volMusic = IndexToValue(idx);
					break;
				}
				case 2:
				{
					int idx = ValueToIndex(_volSound);
					idx = System.Math.Max(0, System.Math.Min(SliderWidths.Length - 1, idx + dir));
					_volSound = IndexToValue(idx);
					break;
				}
			}
			_audioManager.ApplyVolumes(_volMaster, _volMusic, _volSound);
		}

		private void AdjustVideo(int dir)
		{
			switch (_videoSelectionIndex)
			{
				case 0:
					_videoModeIndex = (_videoModeIndex + dir + _videoModeOptions.Length) % _videoModeOptions.Length;
					break;
				case 1:
					break;
				case 2:
					_videoShadows = !_videoShadows;
					break;
				case 3:
					_videoWaterEffects = !_videoWaterEffects;
					break;
				case 4:
					_videoHeatEffects = !_videoHeatEffects;
					break;
			}
		}

		public void Draw()
		{
			if (!_loaded) return;

			_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
			int texW = _backgroundTexture.Width;
			int texH = _backgroundTexture.Height;
			for (int y = 0; y < ScreenHeight; y += texH)
			{
				for (int x = 0; x < ScreenWidth; x += texW)
				{
					_spriteBatch.Draw(_backgroundTexture, new Rectangle(x, y, texW, texH), Color.White);
				}
			}
			_spriteBatch.End();

			_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

			if (_audioSubmenuActive)
			{
				DrawAudioSubmenu();
			}
			else if (_videoSubmenuActive)
			{
				DrawVideoSubmenu();
			}
			else
			{
				DrawMainMenu();
			}

			_spriteBatch.End();
		}

		private void DrawMainMenu()
		{
			float scale = Scale;
			Vector2 firstPos = new Vector2(ScreenWidth / 2, 360 * scale);
			Vector2 secondPos = new Vector2(ScreenWidth / 2, 480 * scale);
			Vector2 selectedPos = _selectionIndex == 0 ? firstPos : secondPos;

			int barW = (int)(_selectionBarTexture.Width * BarScaleX * scale);
			int barH = (int)(_selectionBarTexture.Height * BarScaleY * scale);
			int barYOffset = (int)(28 * scale);
			var barDest = new Rectangle((int)(selectedPos.X - barW / 2), (int)(selectedPos.Y - barH / 2 + barYOffset), barW, barH);
			_spriteBatch.Draw(_selectionBarTexture, barDest, Color.White * _fadeOpacity);

			DrawMenuItemScaled("AUDIO", firstPos, _selectionIndex == 0);
			DrawMenuItemScaled("VIDEO", secondPos, _selectionIndex == 1);

			var stripRect = new Rectangle(0, (int)(1000 * scale), ScreenWidth, (int)(80 * scale));
			_spriteBatch.Draw(_whiteTexture, stripRect, new Color(0f, 0f, 0f, 0.35f * _fadeOpacity));
			Vector2 aIconPos = new Vector2(180 * scale, 1008 * scale);
			Vector2 bIconPos = new Vector2(ScreenWidth - 240 * scale, 1008 * scale);
			_spriteBatch.Draw(_buttonATexture, aIconPos, Color.White * _fadeOpacity);
			_spriteBatch.Draw(_buttonBTexture, bIconPos, Color.White * _fadeOpacity);
			AlignButtonLabel("APPLY", aIconPos, _buttonATexture, ButtonLabelScale * scale);
			AlignButtonLabel("CANCEL", bIconPos, _buttonBTexture, ButtonLabelScale * scale);
		}

		private void DrawAudioSubmenu()
		{
			float scale = Scale;
			Vector2 titlePos = new Vector2(ScreenWidth / 2, 300 * scale);
			DrawScaledString(_fontImpactItalic, "AUDIO", titlePos, Color.White * _fadeOpacity, 0.8f * scale);

			var labels = new[] { "MASTER", "MUSIC", "SFX" };
			float[] values = new[] { _volMaster, _volMusic, _volSound };
			int startY = (int)(380 * scale);
			int spacing = (int)(120 * scale);
			for (int i = 0; i < 3; i++)
			{
				int y = startY + i * spacing;
				bool selected = i == _audioSelectionIndex;
				DrawScaledString(_fontImpactRegular, labels[i], new Vector2(640 * scale, y), selected ? new Color(1.0f * _fadeOpacity, 0.85f * _fadeOpacity, 0.2f * _fadeOpacity, 1f) : Color.White * _fadeOpacity, 0.7f * scale);
				DrawSlider(new Vector2(1220 * scale, y), values[i], selected);
			}

			var stripRect = new Rectangle(0, (int)(1000 * scale), ScreenWidth, (int)(80 * scale));
			_spriteBatch.Draw(_whiteTexture, stripRect, new Color(0f, 0f, 0f, 0.35f * _fadeOpacity));
			Vector2 aIconPos = new Vector2(180 * scale, 1008 * scale);
			Vector2 bIconPos = new Vector2(ScreenWidth - 240 * scale, 1008 * scale);
			_spriteBatch.Draw(_buttonATexture, aIconPos, Color.White * _fadeOpacity);
			_spriteBatch.Draw(_buttonBTexture, bIconPos, Color.White * _fadeOpacity);
			AlignButtonLabel("APPLY", aIconPos, _buttonATexture, ButtonLabelScale * scale);
			AlignButtonLabel("CANCEL", bIconPos, _buttonBTexture, ButtonLabelScale * scale);
		}

		private void DrawVideoSubmenu()
		{
			float scale = Scale;
			Vector2 titlePos = new Vector2(ScreenWidth / 2, 300 * scale);
			DrawScaledString(_fontImpactItalic, "VIDEO", titlePos, Color.White * _fadeOpacity, 0.8f * scale);
			int startY = (int)(380 * scale);
			int spacing = (int)(120 * scale);
			for (int i = 0; i < 5; i++)
			{
				int y = startY + i * spacing;
				bool selected = i == _videoSelectionIndex;
				string left = i == 0 ? "MODE" : i == 1 ? "RESOLUTION" : i == 2 ? "SHADOWS" : i == 3 ? "WATER EFFECTS" : "HEAT EFFECTS";
				string value = i == 0 ? _videoModeOptions[_videoModeIndex]
					: i == 1 ? ($"{_videoResolutionFixed.X}x{_videoResolutionFixed.Y}")
					: (i == 2 ? (_videoShadows ? "ON" : "OFF") : (i == 3 ? (_videoWaterEffects ? "ON" : "OFF") : (_videoHeatEffects ? "ON" : "OFF")));
				Color leftColor = selected ? new Color(1.0f * _fadeOpacity, 0.85f * _fadeOpacity, 0.2f * _fadeOpacity, 1f) : Color.White * _fadeOpacity;
				DrawScaledString(_fontImpactRegular, left, new Vector2(640 * scale, y), leftColor, 0.7f * scale);
				string rightText = selected ? $"< {value} >" : value;
				DrawScaledString(_fontImpactRegular, rightText, new Vector2(1280 * scale, y), Color.White * _fadeOpacity, 0.7f * scale);
			}
			var stripRect = new Rectangle(0, (int)(1000 * scale), ScreenWidth, (int)(80 * scale));
			_spriteBatch.Draw(_whiteTexture, stripRect, new Color(0f, 0f, 0f, 0.35f * _fadeOpacity));
			Vector2 aIconPos = new Vector2(180 * scale, 1008 * scale);
			Vector2 bIconPos = new Vector2(ScreenWidth - 240 * scale, 1008 * scale);
			_spriteBatch.Draw(_buttonATexture, aIconPos, Color.White * _fadeOpacity);
			_spriteBatch.Draw(_buttonBTexture, bIconPos, Color.White * _fadeOpacity);
			AlignButtonLabel("APPLY", aIconPos, _buttonATexture, ButtonLabelScale * scale);
			AlignButtonLabel("CANCEL", bIconPos, _buttonBTexture, ButtonLabelScale * scale);
		}

		private void DrawSlider(Vector2 center, float value, bool selected)
		{
			int w = _audioSliderEmptyTexture.Width;
			int h = _audioSliderEmptyTexture.Height;
			int x = (int)(center.X - w / 2);
			int y = (int)(center.Y - h / 2);
			var emptyDest = new Rectangle(x, y, w, h);
			_spriteBatch.Draw(_audioSliderEmptyTexture, emptyDest, Color.White * _fadeOpacity);
			int idx = ValueToIndex(value);
			int fillW = SliderWidths[idx];
			if (fillW > 0)
			{
				fillW = System.Math.Min(fillW, w);
				var src = new Rectangle(0, 0, fillW, h);
				var dest = new Rectangle(x, y, fillW, h);
				_spriteBatch.Draw(selected ? _audioSliderGoldTexture : _audioSliderSilverTexture, dest, src, Color.White * _fadeOpacity);
			}
		}

		private void AlignButtonLabel(string text, Vector2 iconPos, Texture2D icon, float scale)
		{
			int centerY = (int)(iconPos.Y + icon.Height / 2);
			int textH = (int)(_fontImpactRegular.MeasureString(text).Height * scale);
			int textY = centerY - textH / 2;
			DrawScaledStringLeft(_fontImpactRegular, text, new Vector2(iconPos.X + icon.Width + 24, textY), Color.White * _fadeOpacity, scale);
		}

		private void DrawMenuItemScaled(string text, Vector2 pos, bool selected)
		{
			DrawScaledString(_fontImpactRegular, text, pos + new Vector2(2, 2), new Color(0f, 0f, 0f, 0.6f) * _fadeOpacity, TextScale);
			Color color = selected ? new Color(1.0f * _fadeOpacity, 0.85f * _fadeOpacity, 0.2f * _fadeOpacity, 1f) : Color.White * _fadeOpacity;
			DrawScaledString(_fontImpactRegular, text, pos, color, TextScale);
		}

		private void DrawScaledString(CustomFont font, string text, Vector2 pos, Color color, float scale)
		{
			_spriteBatch.End();
			_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateScale(scale));
			font.DrawString(_spriteBatch, text, pos / scale, color, -1, true);
			_spriteBatch.End();
			_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		}

		private void DrawScaledStringLeft(CustomFont font, string text, Vector2 pos, Color color, float scale)
		{
			_spriteBatch.End();
			_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.CreateScale(scale));
			font.DrawString(_spriteBatch, text, pos / scale, color, -1);
			_spriteBatch.End();
			_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
		}

		public bool IsComplete => _complete;

		private bool IsKeyJustPressed(KeyboardState current, Keys key)
		{
			return current.IsKeyDown(key) && !_prevKeyboard.IsKeyDown(key);
		}

		private bool IsButtonJustPressed(GamePadState current, Buttons button)
		{
			return current.IsButtonDown(button) && !_prevGamepad.IsButtonDown(button);
		}

		private bool IsDPadUpJustPressed(GamePadState current)
		{
			return current.DPad.Up == ButtonState.Pressed && _prevGamepad.DPad.Up != ButtonState.Pressed;
		}

		private bool IsDPadDownJustPressed(GamePadState current)
		{
			return current.DPad.Down == ButtonState.Pressed && _prevGamepad.DPad.Down != ButtonState.Pressed;
		}
	}
}

