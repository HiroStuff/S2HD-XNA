using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using S2HD.GameStates;

namespace S2HD
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameStateManager _gameStateManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameStateManager = new GameStateManager(GraphicsDevice, _spriteBatch, Content);

            _gameStateManager.AddState(new DisclaimerGameState(GraphicsDevice, _spriteBatch));
            _gameStateManager.AddState(new LogosGameState(GraphicsDevice, _spriteBatch));
            _gameStateManager.AddState(new TeamLogoGameState(GraphicsDevice, _spriteBatch));
            _gameStateManager.AddState(new TitleGameState(GraphicsDevice, _spriteBatch, Content));
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
