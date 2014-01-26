using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
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
        Dictionary<string, Texture2D> tileTextures;
        Dictionary<string, Texture2D> objectTextures;
        GameObject movingObject = null, tire1, tire2;
        Vector2 moveSpeed, axel;
        Vector2 mouseInWorld, mouseOnScreen, mouseFix = new Vector2(20, 16);
        Vector2 moveDelta;
        protected World World;
        WheelJoint frontWheelJoint, rearWheelJoint;
        int mouseMiddle;
        Body HiddenBody;
        Texture2D background;
        Rectangle bgRectangle;
        Dictionary<string, GameObject> gameObjects;
        //Body bottom;
        readonly float minScale = 0.4f, maxScale = 1.0f, carPower = 2f, minZoom = 0.7f;
        bool drivingState = false;
        GameObject car, carBridge;
        //Rectangle bottomRectangle;
        Texture2D mouseTexture;
        Joint mouseJoint;
        float acceleration, maxSpeed = 40f;

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
            gameObjects = new Dictionary<string, GameObject>();
            World = new World(new Vector2(0f, 9.82f));
            mouseTexture = Game.Content.Load<Texture2D>("Mouse-cursor-hand-pointer");
            Camera = new Camera2D(Game.GraphicsDevice);
            
            HiddenBody = BodyFactory.CreateBody(World, Vector2.Zero);
            //load texture that will represent the physics body
            objectTextures = new Dictionary<string, Texture2D> {
                { "anvil", Game.Content.Load<Texture2D>("anvil") },
                { "basketBall01", Game.Content.Load<Texture2D>("basketBall01")},
                { "basketBall02", Game.Content.Load<Texture2D>("basketBall02")},
                { "Cat1", Game.Content.Load<Texture2D>("Cat1")},
                { "chainsaw", Game.Content.Load<Texture2D>("chainsaw")},
                { "clownhat", Game.Content.Load<Texture2D>("clownhat")},
                { "coffeeBrewer", Game.Content.Load<Texture2D>("coffeeBrewer")},
                { "crystal", Game.Content.Load<Texture2D>("crystal")},
                { "football", Game.Content.Load<Texture2D>("football")},
                { "goo", Game.Content.Load<Texture2D>("goo")},
                { "pillow01", Game.Content.Load<Texture2D>("pillow01")},
                { "shovel02", Game.Content.Load<Texture2D>("shovel02")},
                { "skates", Game.Content.Load<Texture2D>("skates")},
                { "sword", Game.Content.Load<Texture2D>("sword")},
                { "table", Game.Content.Load<Texture2D>("table")},
                { "tv", Game.Content.Load<Texture2D>("tv")},
                { "washingMachine", Game.Content.Load<Texture2D>("washingMachine")},
                { "woodenBox", Game.Content.Load<Texture2D>("woodenBox")},            
                { "Sprinter2_ulko", Game.Content.Load<Texture2D>("Sprinter2_ulko")},
                { "Sprinter2_luukku", Game.Content.Load<Texture2D>("Sprinter2_luukku")},
                { "Sprinter2_rengas", Game.Content.Load<Texture2D>("Sprinter2_rengas")},
            };

            tileTextures = new Dictionary<string, Texture2D> {
                { "downHill01", Game.Content.Load<Texture2D>("Tiles/downHill01") },
                { "downhillBegin", Game.Content.Load<Texture2D>("Tiles/downHillBegin")},
                { "downhillEnd", Game.Content.Load<Texture2D>("Tiles/downHillEnd")},
                { "flat01", Game.Content.Load<Texture2D>("Tiles/flat01")},
                { "uphill01", Game.Content.Load<Texture2D>("Tiles/uphill01")},
                { "uphillBegin", Game.Content.Load<Texture2D>("Tiles/uphillBegin")},
                { "uphillEnd", Game.Content.Load<Texture2D>("Tiles/uphillEnd")},
            };
            
            float screenWidth = ConvertUnits.ToSimUnits(Game.GraphicsDevice.Viewport.Height), screenHeight = ConvertUnits.ToSimUnits(Game.GraphicsDevice.Viewport.Width);
            float areaWidth = Game.GraphicsDevice.Viewport.Width / 2f, x = 0f;
            Vector2 objPos = new Vector2(0f, Game.GraphicsDevice.Viewport.Height - tileTextures["flat01"].Height);
            foreach (KeyValuePair<string, Texture2D> textureKV in objectTextures)
            {
                AddGameObject(textureKV.Value).Compound.Position = Camera.ConvertScreenToWorld(objPos);
                x += textureKV.Value.Width;
                objPos = new Vector2(x % areaWidth, x / areaWidth);
            }
            mouseMiddle = Mouse.GetState().ScrollWheelValue;
            car = FindGameObject("Sprinter2_ulko");
            Vector2 carStart = new Vector2(Game.Window.ClientBounds.Width / 2f + 130f, Game.GraphicsDevice.Viewport.Height - tileTextures["flat01"].Height - 30f);
            car.Compound.Position = Camera.ConvertScreenToWorld(carStart);
            car.Compound.CollisionGroup = 1;
            car.Compound.Mass = 6000;
            car.Compound.BodyType = BodyType.Static;
            CircleShape wheelShape = new CircleShape(0.5f, 0.8f);
            tire1 = FindGameObject("Sprinter2_rengas");

            tire1.Compound.CreateFixture(wheelShape);
            tire1.Compound.Friction = 0.95f;
            Vector2 tireAxel = new Vector2(220f, 150f);
            tire1.Compound.Position = Camera.ConvertScreenToWorld(carStart + tireAxel);
            tire1.Compound.CollisionGroup = 1;
            frontWheelJoint = JointFactory.CreateWheelJoint(World, car.Compound, tire1.Compound, Vector2.UnitY);
            frontWheelJoint.MaxMotorTorque = 50f;
            frontWheelJoint.MotorEnabled = false;
            frontWheelJoint.Frequency = 4.0f;
            frontWheelJoint.DampingRatio = 0.7f;
            tire2 = AddGameObject(tire1.PolygonTexture, "Sprinter2_rengas_2"); // TODO create method with fixture
            tire2.Compound.CreateFixture(wheelShape);
            tire2.Compound.Friction = 0.95f;
            Vector2 tireAxel2 = new Vector2(-tireAxel.X - 110f, tireAxel.Y);
            tire2.Compound.Position = Camera.ConvertScreenToWorld(carStart + tireAxel2);
            tire2.Compound.CollisionGroup = 1;
            rearWheelJoint = JointFactory.CreateWheelJoint(World, car.Compound, tire2.Compound, Vector2.UnitY);
            rearWheelJoint.MaxMotorTorque = 100f;
            rearWheelJoint.MotorEnabled = true;
            rearWheelJoint.Frequency = 4.0f;
            rearWheelJoint.DampingRatio = 0.7f;
            carBridge = FindGameObject("Sprinter2_luukku");
            carBridge.Compound.Position = Camera.ConvertScreenToWorld(carStart + tireAxel2 - new Vector2(220f, 180f));
            carBridge.Compound.CollisionGroup = 1;
            JointFactory.CreateRevoluteJoint(World, car.Compound, carBridge.Compound, Vector2.Zero);

            GenerateWorld("4444444444444566666674444444444412222222223444444444");
            Camera.Zoom = 1.3f;
            Camera.MoveCamera(ConvertUnits.ToSimUnits(new Vector2(-150f, 150f)));
        }

        public GameObject AddTile(Texture2D texture, String name, ref Vector2 pos, int direction)
        {
            GameObject go1 = AddGameObject(texture, name);
            go1.Compound.Position = pos;
            go1.Compound.BodyType = BodyType.Static;
            go1.Compound.CollisionGroup = 1;
            pos += ConvertUnits.ToSimUnits(new Vector2(go1.PolygonTexture.Width, go1.PolygonTexture.Height * direction));
            return go1;
        }

        /*
         * 1: downhill begin
         * 2: downhill
         * 3: downhill end
         * 4: flat
         * 5: uphill begin
         * 6: uphill
         * 7: uphill end
         */
        public void GenerateWorld(string tiles)
        {
            Vector2 pos = Camera.ConvertScreenToWorld(new Vector2(-200f, Game.GraphicsDevice.Viewport.Height));
            int id = 0;
            foreach (char c in tiles)
            {
                switch (c)
                {
                    case '1':
                        AddTile(tileTextures["downhillBegin"], "tile_" + id, ref pos, 1);                        
                        break;
                    case '2':
                        AddTile(tileTextures["downHill01"], "tile_" + id, ref pos, 1);
                        break;
                    case '3':
                        AddTile(tileTextures["downhillEnd"], "tile_" + id, ref pos, 1);
                        break;
                    case '4':
                        AddTile(tileTextures["flat01"], "tile_" + id, ref pos, 0);
                        break;
                    case '5':
                        AddTile(tileTextures["uphillBegin"], "tile_" + id, ref pos, -1);
                        break;
                    case '6':
                        AddTile(tileTextures["uphill01"], "tile_" + id, ref pos, -1);
                        break;
                    case '7':
                        AddTile(tileTextures["uphillEnd"], "tile_" + id, ref pos, 0);
                        break;
                    default:
                        break;
                }
                id++;
            }
        }

        public GameObject AddGameObject(Texture2D texture, String name = null)
        {
            GameObject gameObject = new GameObject(this, texture, World);
            gameObject.Compound.OnCollision += Compound_OnCollision;
            gameObject.Name = name == null ? texture.Name : name;
            gameObjects.Add(gameObject.Name, gameObject);
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

            foreach (KeyValuePair<string,GameObject> gameObjectKV in gameObjects)
            {
                gameObjectKV.Value.Draw(Game.SpriteBatch, Camera);
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
            foreach (KeyValuePair<string, GameObject> gameObjectKV in gameObjects)
            {
                gameObjectKV.Value.SetScale(CalculateScale(gameObjectKV.Value.Compound.Position));
                gameObjectKV.Value.Update(gameTime);
            }
            if (drivingState && Camera.Zoom > minZoom)
                Camera.Zoom -= 0.01f;
             if (drivingState)
            {
                rearWheelJoint.MotorSpeed = Math.Sign(acceleration) * MathHelper.SmoothStep(0f, maxSpeed, Math.Abs(acceleration));
                frontWheelJoint.MotorSpeed = rearWheelJoint.MotorSpeed;
                if (Math.Abs(rearWheelJoint.MotorSpeed) < maxSpeed * 0.06f)
                {
                    rearWheelJoint.MotorEnabled = false;
                }
                else
                {
                    rearWheelJoint.MotorEnabled = true;
                }
            }
            Camera.Update(gameTime);
        }

        public void HandleMouse(MouseState mouseState, GameTime gameTime)
        {
            mouseOnScreen = new Vector2(mouseState.X + 25, mouseState.Y + 30);
            Vector2 newMouseInWorld = Camera.ConvertScreenToWorld(mouseOnScreen);
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
                    mouseJoint.WorldAnchorB = mouseInWorld;
                 }
            }

            if (movingObject != null && mouseState.LeftButton == ButtonState.Released)
            {
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
            foreach (KeyValuePair<string, GameObject> gameObjectKV in gameObjects)
            {
                if (gameObjectKV.Value.Compound == body)
                    return gameObjectKV.Value;
            }
            return null;
        }

        internal GameObject FindGameObject(String name)
        {
            
            return gameObjects[name];
        }

        public void HandleKeyboard(KeyboardState keyboardState, GameTime gameTime)
        {
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                if (!drivingState)
                {
                    car.Compound.BodyType = BodyType.Dynamic;
                    //car.Compound.LinearVelocity += new Vector2(10f, 0f);
                    drivingState = true;
                    Camera.TrackingBody = car.Compound;
                }
                acceleration = Math.Min(acceleration + (float)(carPower * gameTime.ElapsedGameTime.TotalSeconds), 1f);

            }
            else if (keyboardState.IsKeyDown(Keys.Down))
                acceleration = Math.Max(acceleration - (float)(carPower * gameTime.ElapsedGameTime.TotalSeconds), -1f);
            else
                acceleration = 0f;
            if (keyboardState.IsKeyDown(Keys.Q))
                Camera.Position += new Vector2(0f, 0.1f);
            if (keyboardState.IsKeyDown(Keys.A))
                Camera.Position -= new Vector2(0f, 0.1f);
        }

        public void StartScreen()
        {

        }
    }
}
