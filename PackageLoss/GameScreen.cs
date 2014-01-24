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
        Texture2D background;
        Rectangle bgRectangle;
        List<GameObject> gameObjects;
        Body bottom;
        Rectangle bottomRectangle;
        Texture2D testTexture;
        
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
            bgRectangle = new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
            background = Game.Content.Load<Texture2D>("background");
            gameObjects = new List<GameObject>();
            World = new World(Vector2.Zero);
            World.Gravity = new Vector2(0.0f, 4.0f);

            camera = new Camera2D(Game.GraphicsDevice);
            HiddenBody = BodyFactory.CreateBody(World, Vector2.Zero);
            //load texture that will represent the physics body
            GameObject go1 = new GameObject(this, Game.Content.Load<Texture2D>("basketBall01"), World);
            go1.Compound.OnCollision += Compound_OnCollision;
            gameObjects.Add(go1);
            GameObject go2 = new GameObject(this, Game.Content.Load<Texture2D>("basketBall01"), World);
            go2.Compound.OnCollision += Compound_OnCollision;
            gameObjects.Add(go2);
            testTexture = Game.Content.Load<Texture2D>("goo");
            bottom = BodyFactory.CreateRectangle(World, Game.Window.ClientBounds.Width, 1.0f, 10.0f);
            bottomRectangle = new Rectangle(0, 0, Game.Window.ClientBounds.Width, 10);
            bottom.Position = new Vector2(-20.0f, 10.0f);
            bottom.OnCollision += Compound_OnCollision;
        }

        bool Compound_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            return true;
        }

        public void Draw(GameTime gameTime)
        {
            Game.SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone);
            Game.SpriteBatch.Draw(background, Vector2.Zero, bgRectangle, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            Game.SpriteBatch.End();

            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Draw(Game.SpriteBatch, camera);
            }
            Game.SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone, null, camera.View);
            Game.SpriteBatch.Draw(testTexture, ConvertUnits.ToDisplayUnits(bottom.Position), bottomRectangle, Color.White, bottom.Rotation, Vector2.Zero, 1.0f, SpriteEffects.None, 0f);
            Game.SpriteBatch.End();
        }


        internal void Update(GameTime gameTime)
        {
            World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Update(gameTime);
            }
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
                    movingObject.Compound.LinearVelocity = Vector2.Zero;
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
