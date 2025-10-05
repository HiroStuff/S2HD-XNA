using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace S2HD.Shared.Graphics
{
	public static class VideoService
	{
		private static GraphicsDeviceManager _gdm;
		private static Game _game;

		public static void Init(GraphicsDeviceManager gdm, Game game)
		{
			_gdm = gdm;
			_game = game;
		}

		public static void Apply(int modeIndex, Point resolution, bool vsync)
		{
			if (_gdm == null || _game == null) return;
			_gdm.SynchronizeWithVerticalRetrace = vsync;
			_gdm.PreferredBackBufferWidth = resolution.X;
			_gdm.PreferredBackBufferHeight = resolution.Y;
			switch (modeIndex)
			{
				case 0:
					_game.Window.IsBorderless = false;
					_gdm.HardwareModeSwitch = true;
					_gdm.IsFullScreen = false;
					break;
				case 1:
					_game.Window.IsBorderless = false;
					_gdm.HardwareModeSwitch = true;
					_gdm.IsFullScreen = true;
					break;
				case 2:
					_gdm.IsFullScreen = false;
					_gdm.HardwareModeSwitch = false;
					_game.Window.IsBorderless = true;
					try { _game.Window.Position = new Point(0, 0); } catch { }
					break;
			}
			_gdm.ApplyChanges();
		}
	}
}
