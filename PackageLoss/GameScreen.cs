using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
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
            "anvil",
            "basketBall01",
            "basketBall02",
            "Cat1",
            "chainsaw",
            "clownhat",
            "coffeeBrewer",
            "crystal",
            "football",
            "goo",
            "pillow01",
            "shovel02",
            "skates",
            "sword",
            "table",
            "tv",
            "washingMachine",
            "woodenBox",            
            "Sprinter2_ulko",
            //"Sprinter2_ulko_alfa",
            "Sprinter2_luukku",
            "Sprinter2_rengas",
            "bottom",
            //"Mouse-cursor-hand-pointer"
        };
        GameObject movingObject = null, tire1, tire2;
        Vector2 moveSpeed, axel;
        Vector2 mouseInWorld, mouseOnScreen, mouseFix = new Vector2(20, 16);
        Vector2 moveDelta;
        protected World World;
        int mouseMiddle;
        Body HiddenBody;
        Texture2D background;
        Texture2D[] textures;
        Rectangle bgRectangle;
        List<GameObject> gameObjects;
        //Body bottom;
        readonly float minScale = 0.4f, maxScale = 1.0f, carPower = 0.3f, minZoom = 0.7f;
        bool drivingState = false;
        GameObject car, carBridge;
        //Rectangle bottomRectangle;
        Texture2D mouseTexture;
        Joint mouseJoint;

        public Camera2D Camera { get; set; }
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
            mouseTexture = Game.Content.Load<Texture2D>("Mouse-cursor-hand-pointer");
            Camera = new Camera2D(Game.GraphicsDevice);
            
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
            mouseMiddle = Mouse.GetState().ScrollWheelValue;
            //bottom = BodyFactory.CreateRectangle(World, ConvertUnits.ToSimUnits(Game.Window.ClientBounds.Width * 10.0f), ConvertUnits.ToSimUnits(20f), 10.0f);
            //bottomRectangle = new Rectangle(0, 0, Game.Window.ClientBounds.Width, 10);
            //bottom.Position = new Vector2(ConvertUnits.ToSimUnits(0f), ConvertUnits.ToSimUnits(Game.Window.ClientBounds.Height / 2f));
            //bottom.OnCollision += Compound_OnCollision;

            // Special characterics for objects
            //FindGameObject("basketBall01").Compound.Restitution = FindGameObject("basketBall02").Compound.Restitution = FindGameObject("football").Compound.Restitution = 0.8f;
            Vector2 carStart = new Vector2(Game.Window.ClientBounds.Width / 2f + 130f, 50f);
            car = FindGameObject("Sprinter2_ulko");
            car.Compound.BodyType = BodyType.Dynamic;
            car.Compound.Position = Camera.ConvertScreenToWorld(carStart);
            car.Compound.CollisionGroup = 1;
            //car.Compound.IgnoreCCD = true;
            //car.Compound.OnCollision -= Compound_OnCollision;
            car.Compound.Mass = 6000;
            //axel = ConvertUnits.ToSimUnits(new Vector2(car.PolygonTexture.Width / 2 - 50f, 10f));

            tire1 = FindGameObject("Sprinter2_rengas");
            tire1.Compound.Friction = 0.9f;
            tire1.Compound.Mass = 20f;
            Vector2 tireAxel = new Vector2(220f, 150f);
            tire1.Compound.Position = Camera.ConvertScreenToWorld(carStart + tireAxel);
            tire1.Compound.CollisionGroup = 1;
            JointFactory.CreateRevoluteJoint(World, car.Compound, tire1.Compound, Vector2.Zero);

            tire2 = AddGameObject(tire1.PolygonTexture);
            tire2.Compound.Friction = 0.9f;
            tire2.Compound.Mass = 20f;
            Vector2 tireAxel2 = new Vector2(-tireAxel.X - 100f, tireAxel.Y);
            tire2.Compound.Position = Camera.ConvertScreenToWorld(carStart + tireAxel2);
            tire2.Compound.CollisionGroup = 1;
            JointFactory.CreateRevoluteJoint(World, car.Compound, tire2.Compound, Vector2.Zero);
            //JointFactory.CreateRevoluteJoint(World, car.Compound, tire1.Compound, axis + new Vector2(0f, 1.2f), axis + new Vector2(0f, -1.2f)).Breakpoint = float.MaxValue;
            //JointFactory.CreateWheelJoint(World, car.Compound, tire1.Compound, Vector2.Zero);
            //JointFactory.CreateAngleJoint(World, car.Compound, tire1.Compound);
            

            //tire2 = FindGameObject("Sprinter2_rengas");
            //tire1.Compound.Friction = 0.9f;
            //tire2.Compound.Mass = 20f;
            
            //tire2.Compound.Position = car.Compound.Position - axis;
            //tire2.Compound.CollisionGroup = 1;
            //JointFactory.CreateWheelJoint(World, car.Compound, tire2.Compound, -axis, Vector2.UnitY, true);
            //JointFactory.CreateRevoluteJoint(World, car.Compound, tire2.Compound, Vector2.Zero);
            //tire1.Compound.IgnoreCollisionWith(car.Compound);
            //JointFactory.CreateWheelJoint(World, car.Compound, tire1.Compound, axis);
            
            //WheelJoint wheelJoint1 = new WheelJoint(car.Compound, tire1.Compound, tire1.Compound.Position, axis, true);
            //wheelJoint1.MotorSpeed = 0f;
            //wheelJoint1.MaxMotorTorque = 20.0f;
            //wheelJoint1.MotorEnabled = true;
            //wheelJoint1.Frequency = 4.0f;
            //wheelJoint1.DampingRatio = 0.7f;
            //World.AddJoint(wheelJoint1);
             

            //tire2 = AddGameObject(tire1.PolygonTexture);
            //tire2.Compound.Friction = 0.9f;
            //Vector2 axis2 = ConvertUnits.ToSimUnits(new Vector2(150f, 10f));
            //tire2.Compound.Position = car.Compound.Position + axis2;
            //tire2.Compound.CollisionGroup = 5;
            //tire2.Compound.IgnoreCollisionWith(car.Compound);
            //gameObjects.Remove(tire2);
            //JointFactory.CreateWheelJoint(World, car.Compound, tire2.Compound, Vector2.Zero);
            //WheelJoint wheelJoint2 = new WheelJoint(car.Compound, tire2.Compound, tire2.Compound.Position, axis2, true);
            //wheelJoint2.MotorSpeed = 0f;
            //wheelJoint2.MaxMotorTorque = 20.0f;
            //wheelJoint2.MotorEnabled = true;
            //wheelJoint2.Frequency = 4.0f;
            //wheelJoint2.DampingRatio = 0.7f;
            //World.AddJoint(wheelJoint2);
            //JointFactory.CreateWheelJoint(World, car.Compound, tire1.Compound, new Vector2(200f, 0f), true);
            //JointFactory.CreateWheelJoint(World, car.Compound, AddGameObject(tire1.PolygonTexture, "Sprinter2_rengas_2").Compound, new Vector2(-200f, 0f)); 


            carBridge = FindGameObject("Sprinter2_luukku");
            carBridge.Compound.Position = Camera.ConvertScreenToWorld(carStart + tireAxel2 - new Vector2(220f, 180f));
            carBridge.Compound.CollisionGroup = 1;
            JointFactory.CreateRevoluteJoint(World, car.Compound, carBridge.Compound, Vector2.Zero);
            //JointFactory.CreateRevoluteJoint(World, car.Compound, carBridge.Compound, new Vector2(0f, -50f));
            //JointFactory.CreateAngleJoint(World, car.Compound, carBridge.Compound).TargetAngle = 90f;
            //JointFactory.

            GameObject bottom = FindGameObject("bottom");
            bottom.Compound.Position = Camera.ConvertScreenToWorld(new Vector2(3500f, Game.Window.ClientBounds.Height - 200f));
            bottom.Compound.BodyType = BodyType.Static;
            bottom.Compound.CollisionGroup = 1;
            
            //cursor = FindGameObject("Mouse-cursor-hand-pointer");
            //cursor.Compound.CollisionGroup = 4;
            //cursor.Compound.CollisionCategories = Category.Cat30;
            //cursor.Compound.IgnoreCCD = true;
            //cursor.Compound.BodyType = BodyType.Kinematic;
            //cursor.Compound.Mass = 0f;
            Camera.Zoom = 1.3f;
            Camera.MoveCamera(ConvertUnits.ToSimUnits(new Vector2(-150f, 150f)));
            //Camera.TrackingBody = car.Compound;
        }

        public GameObject AddGameObject(Texture2D texture, String name = null)
        {
            GameObject gameObject = new GameObject(this, texture, World);
            gameObject.Compound.OnCollision += Compound_OnCollision;
            gameObjects.Add(gameObject);
            if (name == null)
                gameObject.Name = texture.Name;
            else
                gameObject.Name = name;
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
                gameObject.Draw(Game.SpriteBatch, Camera);
            }
            Game.SpriteBatch.Begin();
            Game.SpriteBatch.Draw(mouseTexture, mouseOnScreen - mouseFix, Color.White);
            Game.SpriteBatch.End();
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
            if (drivingState && Camera.Zoom > minZoom)
                Camera.Zoom -= 0.01f;
            //Matrix carMatrix = Matrix.CreateRotationZ(car.Compound.Rotation) * Matrix.CreateTranslation(new Vector3(car.Compound.Position, 0));
            
            //tire1.Compound.Position = Vector2.Transform(axel, carMatrix * Matrix.CreateTranslation(new Vector3(axel, 0)));
            //tire1.Compound.Rotation += car.Compound.LinearVelocity.X;
            //tire2.Compound.Position = Vector2.Transform(axel, carMatrix * Matrix.CreateTranslation(new Vector3(-axel * 2f, 0)));
            //tire2.Compound.Rotation += car.Compound.LinearVelocity.X;
            Camera.Update(gameTime);
        }

        public void HandleMouse(MouseState mouseState, GameTime gameTime)
        {
            mouseOnScreen = new Vector2(mouseState.X + 25, mouseState.Y + 30);
            Vector2 newMouseInWorld = Camera.ConvertScreenToWorld(mouseOnScreen);
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
                        if (gameObject != null)
                        {
                            movingObject = gameObject;
                            moveDelta = movingObject.Compound.Position - mouseInWorld;
                        }
                        mouseJoint = JointFactory.CreateFixedMouseJoint(World, gameObject.Compound, mouseInWorld);
                        mouseJoint.CollideConnected = true;
                        gameObject.Compound.Awake = true;
                    }
                }
                else
                {
                    //if (mouseState.RightButton == ButtonState.Pressed)
                        //movingObject.Compound.Rotation += moveSpeed.X / 10.0f;
                    mouseJoint.WorldAnchorB = mouseInWorld;
                    //movingObject.Compound.Position = mouseInWorld + moveDelta;                 
                }
            }

            if (movingObject != null && mouseState.LeftButton == ButtonState.Released)
            {
                //movingObject.Compound.LinearVelocity = -moveSpeed;
                if (mouseJoint != null)
                {
                    World.RemoveJoint(mouseJoint);
                    mouseJoint = null;
                }
                movingObject = null;
            }

            Camera.Zoom += (mouseMiddle - mouseState.ScrollWheelValue) / 10000.0f;
            mouseMiddle = mouseState.ScrollWheelValue;
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
                    Camera.TrackingBody = car.Compound;
                }
                car.Compound.LinearVelocity += new Vector2(carPower, 0.0f);
            }
            if (keyboardState.IsKeyDown(Keys.Q))
                Camera.Position += new Vector2(0f, 0.1f);
            if (keyboardState.IsKeyDown(Keys.A))
                Camera.Position -= new Vector2(0f, 0.1f);
        }

    }
}
