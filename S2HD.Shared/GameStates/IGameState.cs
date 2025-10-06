using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace S2HD.GameStates
{
    public interface IGameState
    {
        void LoadContent(string dataRoot);
        void Update(GameTime gameTime);
        void Draw();
        bool IsComplete { get; }
    }
}
