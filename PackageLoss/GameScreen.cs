using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PackageLoss
{
    internal class GameScreen
    {
        GameObject movingObject = null;
        Vector2 moveDelta;
        protected World World;
        Body HiddenBody;
        List<GameObject> gameObjects;
        
        Camera2D camera;
        public Game1 Game { get; set; }

        public GameScreen(Game1 game)
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
            gameObjects = new List<GameObject>();
            World = new World(Vector2.Zero);
            World.Gravity = Vector2.Zero;

            camera = new Camera2D(Game.GraphicsDevice);
            HiddenBody = BodyFactory.CreateBody(World, Vector2.Zero);
            //load texture that will represent the physics body
            gameObjects.Add(new GameObject(this, Game.Content.Load<Texture2D>("basketBall01"), World));

            
        }

        public void Draw(GameTime gameTime)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Draw(Game.SpriteBatch, camera);
            }
        }


        internal void Update(GameTime gameTime)
        {
            World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));
            camera.Update(gameTime);
        }

        internal void HandleMouse(MouseState mouseState)
        {
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 mouseInWorld = camera.ConvertScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
                if (movingObject == null)
                {
                    Fixture fixture = World.TestPoint(mouseInWorld);
                    if (fixture != null)
                    {
                        GameObject gameObject = FindGameObject(fixture.Body);
                        if (gameObject != null)
                        {
                            movingObject = gameObject;
                            moveDelta = movingObject.Compound.Position - mouseInWorld;
                        }
                    }
                }
                else
                {
                    movingObject.Compound.Position = mouseInWorld + moveDelta;
                }
            }

            if (movingObject != null && mouseState.LeftButton == ButtonState.Released)
                movingObject = null;
        }

        internal GameObject FindGameObject(Body body)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject.Compound == body)
                    return gameObject;
            }
            return null;
        }
    }
}
