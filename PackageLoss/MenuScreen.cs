using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PackageLoss
{
    internal class MenuScreen : BaseScreen
    {
        readonly String[] menuEntries = new String[] {
                "Start game",
                "Help",
                "Credits",
                "Quit",
            };

        SpriteFont font;
        Texture2D background;
        Rectangle bgRectangle;

        Vector2 menuEntriesPosition;

        public Game1 Game { get; set; }

        public MenuScreen(Game1 game)
        {
            SetGame(game);
        }

        public void SetGame(Game1 game)
        {
            this.Game = game;
        }

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Package Loss";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TODO: Add sample description!");

            return sb.ToString();
        }

        #endregion

        public void LoadContent()
        {
            //Texture2D texture = new Texture2D(graphicsDevice, 200, 20);
            this.background = Game.Content.Load<Texture2D>("background");
            bgRectangle = new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
            
            this.font = Game.Content.Load<SpriteFont>("Segoe UI Mono");

            menuEntriesPosition = new Vector2(100, 100);
            

        }

        internal void DrawTexts()
        {
            Game.SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, null, DepthStencilState.Default, RasterizerState.CullNone);

            for (int i=0; i < menuEntries.Length; i++)
            {
                // TODO take the font height into account and don't just use some constant int
                int fontHeight = 48;
                Game.SpriteBatch.DrawString(font, menuEntries[i], new Vector2(menuEntriesPosition.X, menuEntriesPosition.Y + (i * fontHeight)), Color.White);
            }
            Game.SpriteBatch.End();
        }

        public void Draw(GameTime gameTime)
        {
            DrawBackground();
            DrawTexts();
        }

        private void DrawBackground()
        {
            Game.SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, null, DepthStencilState.Default, RasterizerState.CullNone);
            Game.SpriteBatch.Draw(background, Vector2.Zero, bgRectangle, Color.White, 0, Vector2.Zero, 4.0f, SpriteEffects.None, 0);
            Game.SpriteBatch.End();
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void HandleMouse(MouseState mouseState, GameTime gameTime)
        {
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                // TODO check if mouse is hovering over an entry
                for (int i = 0; i < menuEntries.Length; i++)
                {
                    // TODO don't use constant int
                    if (mouseState.Y > i + menuEntriesPosition.Y && mouseState.Y < i + (menuEntriesPosition.Y + 48))
                    {
                        // TODO id's should be enums
                        this.Game.SelectScreen(1);
                    }
                }
            }

        }
        public void HandleKeyboard(KeyboardState keyboardState, GameTime gameTime)
        {
        }

    }
}

