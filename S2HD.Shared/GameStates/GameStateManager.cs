using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using S2HD.Title;

namespace S2HD.GameStates
{
    public class GameStateManager
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;
        private readonly ContentManager _contentManager;

        private IGameState _currentState;
        private readonly Queue<IGameState> _stateQueue;

        public GameStateManager(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager contentManager)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _contentManager = contentManager;
            _stateQueue = new Queue<IGameState>();
        }

        public void AddState(IGameState state)
        {
            _stateQueue.Enqueue(state);
        }

        public void Update(GameTime gameTime)
        {
            if (_currentState == null && _stateQueue.Count > 0)
            {
                _currentState = _stateQueue.Dequeue();
                _currentState.LoadContent(_contentManager);
            }

            if (_currentState != null)
            {
                _currentState.Update(gameTime);

                if (_currentState.IsComplete)
                {
                    if (_currentState is TitleGameState title)
                    {
                        switch (title.Result)
                        {
                            case TitleGameState.ResultType.ShowOptions:
                                _stateQueue.Enqueue(new OptionsGameState(_graphicsDevice, _spriteBatch));
                                break;
                            case TitleGameState.ResultType.Quit:
                                break;
                        }
                    }
                    else if (_currentState is OptionsGameState)
                    {
                        _stateQueue.Enqueue(new TitleGameState(_graphicsDevice, _spriteBatch, _contentManager));
                    }

                    _currentState = null;
                }
            }
        }

        public void Draw()
        {
            _currentState?.Draw();
        }

        public bool HasActiveState => _currentState != null || _stateQueue.Count > 0;
    }
}
