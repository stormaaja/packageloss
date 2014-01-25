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
    internal class GameScreen : BaseScreen
    {
        readonly String[] textureFilenames = new String[] {
            "basketBall01",
            "basketBall02",
            "chainsaw",
            "crystal",
            "football",
            "pillow01",
            "skates",
            "sword",
            "table",
            "tv",
            "washingMachine",
            "sprinter",
            "bottom",
            //"Mouse-cursor-hand-pointer"
        };
        GameObject movingObject = null, cursor;
        Vector2 moveSpeed;
        Vector2 mouseInWorld;
        Vector2 moveDelta;
        protected World World;
        Body HiddenBody;
        Texture2D background;
        Texture2D[] textures;
        Rectangle bgRectangle;
        List<GameObject> gameObjects;
        //Body bottom;
        readonly float minScale = 0.4f, maxScale = 1.0f, carPower = 0.3f;
        bool drivingState = false;
        GameObject car;
        //Rectangle bottomRectangle;
        
        Camera2D camera;
        public Game1 Game { get; set; }

        public GameScreen(Game1 game)
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
            bgRectangle = new Rectangle(0, 0, Game.Window.ClientBounds.Width, Game.Window.ClientBounds.Height);
            background = Game.Content.Load<Texture2D>("background");
            gameObjects = new List<GameObject>();
            World = new World(new Vector2(0f, 9.82f));

            camera = new Camera2D(Game.GraphicsDevice);
            HiddenBody = BodyFactory.CreateBody(World, Vector2.Zero);
            //load texture that will represent the physics body
            textures = new Texture2D[textureFilenames.Length];
            for (int i = 0; i < textureFilenames.Length; i++)
            {
                textures[i] = Game.Content.Load<Texture2D>(textureFilenames[i]);
                textures[i].Name = textureFilenames[i]; // XNA hack
            }
            float screenWidth = ConvertUnits.ToSimUnits(Game.GraphicsDevice.Viewport.Height), screenHeight = ConvertUnits.ToSimUnits(Game.GraphicsDevice.Viewport.Width);
            Random rand = new Random();
            foreach (Texture2D texture in textures)
            {
                AddGameObject(texture).Compound.Position = new Vector2((float)(rand.NextDouble() - 0.5) * screenWidth, (float)(rand.NextDouble() - 0.5) * screenHeight);
            }

            //bottom = BodyFactory.CreateRectangle(World, ConvertUnits.ToSimUnits(Game.Window.ClientBounds.Width * 10.0f), ConvertUnits.ToSimUnits(20f), 10.0f);
            //bottomRectangle = new Rectangle(0, 0, Game.Window.ClientBounds.Width, 10);
            //bottom.Position = new Vector2(ConvertUnits.ToSimUnits(0f), ConvertUnits.ToSimUnits(Game.Window.ClientBounds.Height / 2f));
            //bottom.OnCollision += Compound_OnCollision;

            // Special characterics for objects
            FindGameObject("basketBall01").Compound.Restitution = FindGameObject("basketBall02").Compound.Restitution = FindGameObject("football").Compound.Restitution = 0.8f;
            car = FindGameObject("sprinter");
            car.Compound.BodyType = BodyType.Dynamic;
            car.Compound.Position = new Vector2(ConvertUnits.ToSimUnits(Game.Window.ClientBounds.Width / 4f), ConvertUnits.ToSimUnits(Game.Window.ClientBounds.Height / 2f) - ConvertUnits.ToSimUnits(80.0f));
            car.Compound.CollisionGroup = 1;
            car.Compound.IgnoreCCD = true;
            car.Compound.OnCollision -= Compound_OnCollision;

            GameObject bottom = FindGameObject("bottom");
            bottom.Compound.Position = new Vector2(ConvertUnits.ToSimUnits(2500f), ConvertUnits.ToSimUnits(Game.Window.ClientBounds.Height / 2f - 150f));
            bottom.Compound.BodyType = BodyType.Static;
            bottom.Compound.CollisionGroup = 1;

            //cursor = FindGameObject("Mouse-cursor-hand-pointer");
            //cursor.Compound.CollisionGroup = 4;
            //cursor.Compound.CollisionCategories = Category.Cat30;
            //cursor.Compound.IgnoreCCD = true;
            //cursor.Compound.BodyType = BodyType.Kinematic;
            //cursor.Compound.Mass = 0f;
        }

        public GameObject AddGameObject(Texture2D texture)
        {
            GameObject gameObject = new GameObject(this, texture, World);
            gameObject.Compound.OnCollision += Compound_OnCollision;
            gameObjects.Add(gameObject);
            gameObject.Name = texture.Name;
            gameObject.Compound.CollisionGroup = 2;
            return gameObject;
        }

        bool Compound_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            return true;
        }

        public void Draw(GameTime gameTime)
        {
            Game.SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Opaque, null, DepthStencilState.Default, RasterizerState.CullNone);
            Game.SpriteBatch.Draw(background, Vector2.Zero, bgRectangle, Color.White, 0, Vector2.Zero, 4.0f, SpriteEffects.None, 0);
            Game.SpriteBatch.End();

            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Draw(Game.SpriteBatch, camera);
            }
            
        }

        float CalculateScale(Vector2 position)
        {
            float scale = Vector2.Distance(position, car.Compound.Position) / 10.0f;
            if (scale < minScale)
                return minScale;
            else if (scale > maxScale)
                return maxScale;
            return scale;
        }

        public void Update(GameTime gameTime)
        {            
            World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f)));
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.SetScale(CalculateScale(gameObject.Compound.Position));
                gameObject.Update(gameTime);
            }
            camera.Update(gameTime);
        }

        public void HandleMouse(MouseState mouseState, GameTime gameTime)
        {
            Vector2 newMouseInWorld = camera.ConvertScreenToWorld(new Vector2(mouseState.X + 25, mouseState.Y + 30));
            //cursor.Compound.Position = newMouseInWorld;
            if (drivingState)
                return;
            if (mouseState.LeftButton == ButtonState.Pressed)
            {                
                
                moveSpeed = (mouseInWorld - newMouseInWorld) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                mouseInWorld = newMouseInWorld;
                if (movingObject == null)
                {
                    Fixture fixture = World.TestPoint(mouseInWorld);
                    if (fixture != null && fixture.CollisionGroup != 1)
                    {
                        GameObject gameObject = FindGameObject(fixture.Body);
                        if (gameObject != null && gameObject != car)
                        {
                            movingObject = gameObject;
                            moveDelta = movingObject.Compound.Position - mouseInWorld;
                        }
                    }
                }
                else
                {
                    if (mouseState.RightButton == ButtonState.Pressed)
                        movingObject.Compound.Rotation += moveSpeed.X / 10.0f;
                    movingObject.Compound.Position = mouseInWorld + moveDelta;                    
                }
            }

            if (movingObject != null && mouseState.LeftButton == ButtonState.Released)
            {
                movingObject.Compound.LinearVelocity = -moveSpeed;
                movingObject = null;
            }
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

        internal GameObject FindGameObject(String name)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject.Name.Equals(name))
                    return gameObject;
            }
            return null;
        }

        public void HandleKeyboard(KeyboardState keyboardState, GameTime gameTime)
        {
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                if (!drivingState) {
                    drivingState = true;
                    camera.TrackingBody = car.Compound;
                }
                car.Compound.LinearVelocity += new Vector2(carPower, 0.0f);
            }
        }

    }
}
