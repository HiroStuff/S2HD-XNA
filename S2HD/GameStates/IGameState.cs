using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace S2HD.GameStates
{
    public interface IGameState
    {
        void LoadContent(ContentManager content);
        void Update(GameTime gameTime);
        void Draw();
        bool IsComplete { get; }
    }
}
