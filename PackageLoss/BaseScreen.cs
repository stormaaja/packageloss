using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PackageLoss
{
    interface BaseScreen
    {
        void SetGame(Game1 game);
        string GetTitle();
        string GetDetails();
        void LoadContent();
        void Draw(GameTime gameTime);
        void Update(GameTime gameTime);
        void HandleMouse(MouseState mouseState, GameTime gameTime);
    }
}
