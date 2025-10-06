using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using S2HD.GameStates;
using System.IO;
using S2HD.Shared.Config;
using S2HD.Audio;
using S2HD.Shared.Graphics;

namespace S2HD.Shared
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private GameStateManager _gameStateManager;
		private ConfigManager _config;

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			IsMouseVisible = true;

			_graphics.PreferredBackBufferWidth = 1920;
			_graphics.PreferredBackBufferHeight = 1080;
			_graphics.IsFullScreen = true;

			try
			{
				string docs = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
				string dir = Path.Combine(docs, "SonicOrcaXNA");
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
				_config = ConfigManager.Load();
			}
			catch {}
		}

		private void ApplyConfig()
		{
			VideoService.Apply(_config.FullscreenMode, new Point(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), true);
		}

		protected override void Initialize()
		{
			VideoService.Init(_graphics, this);
			base.Initialize();
		}

		protected override void LoadContent()
		{
			string dataRoot = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data");
			Content.RootDirectory = "data";
			S2HD.Graphics.EffectService.Init(Content);
			AudioService.Init(dataRoot);
			int len = 11;
			float master = _config.MasterVolume;
			float music = (float)_config.MusicVolumeIndex / len;
			float sound = (float)_config.SoundVolumeIndex / len;
			AudioService.Instance.ApplyVolumes(master, music, sound);

			ApplyConfig();

			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_gameStateManager = new GameStateManager(GraphicsDevice, _spriteBatch, dataRoot);

			_gameStateManager.AddState(new DisclaimerGameState(GraphicsDevice, _spriteBatch));
			_gameStateManager.AddState(new LogosGameState(GraphicsDevice, _spriteBatch));
			_gameStateManager.AddState(new TeamLogoGameState(GraphicsDevice, _spriteBatch));
			_gameStateManager.AddState(new TitleGameState(GraphicsDevice, _spriteBatch));
		}

		protected override void Update(GameTime gameTime)
		{
			_gameStateManager.Update(gameTime);

			if (!_gameStateManager.HasActiveState)
				Exit();

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			_gameStateManager.Draw();

			base.Draw(gameTime);
		}
	}
}
