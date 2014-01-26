using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PackageLoss
{
    internal class GameObject
    {
        public SoundEffect SoundEffectHit { get; set; }
        internal Body Compound { get; set; }
        public String Name { get; set; }
        public Texture2D PolygonTexture { get; set; }
        TimeSpan lastHitSound = TimeSpan.FromSeconds(0);
        World world;
        GameScreen gameScreen;

        private Vector2 _origin;

        public delegate void OnColliding(GameObject go1, GameObject go2);

        public GameObject(GameScreen gameScreen, Texture2D texture2D, World world, bool staticObject = false)
        {
            this.world = world;
            
            this.gameScreen = gameScreen;
            this.PolygonTexture = texture2D;
            //Create an array to hold the data from the texture
            uint[] data = new uint[PolygonTexture.Width * PolygonTexture.Height];

            //Transfer the texture data to the array
            PolygonTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices textureVertices = PolygonTools.CreatePolygon(data, PolygonTexture.Width, false);

            //The tool return vertices as they were found in the texture.
            //We need to find the real center (centroid) of the vertices for 2 reasons:

            //1. To translate the vertices so the polygon is centered around the centroid.
            Vector2 centroid = -textureVertices.GetCentroid();
            textureVertices.Translate(ref centroid);

            //2. To draw the texture the correct place.
            _origin = -centroid;

            //We simplify the vertices found in the texture.
            textureVertices = SimplifyTools.ReduceByDistance(textureVertices, 4f);

            //Since it is a concave polygon, we need to partition it into several smaller convex polygons
            List<Vertices> list = Triangulate.ConvexPartition(textureVertices, TriangulationAlgorithm.Bayazit);

            //Adjust the scale of the object for WP7's lower resolution

            //scale the vertices from graphics space to sim space
            Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1) * gameScreen.Camera.Zoom);
            foreach (Vertices vertices in list)
            {
                vertices.Scale(ref vertScale);
            }

            //Create a single body with multiple fixtures
            Compound = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);
            if (staticObject)
                Compound.BodyType = BodyType.Static;
            else
            {
                Compound.BodyType = BodyType.Dynamic;
                Compound.Restitution = 0.4f;
            }
            Compound.Inertia = 100.0f;

            //Compound.Mass = PolygonTexture.Width * PolygonTexture.Height / 1000f;
        }

        public void Draw(SpriteBatch spriteBatch, Camera2D camera)
        {
            gameScreen.Game.SpriteBatch.Begin(0, null, null, null, null, null, camera.View);
            gameScreen.Game.SpriteBatch.Draw(PolygonTexture, ConvertUnits.ToDisplayUnits(Compound.Position), null, Color.White, Compound.Rotation, _origin, 1f, SpriteEffects.None, 0f);
            gameScreen.Game.SpriteBatch.End();
        }

        public void Update(GameTime gameTime)
        {

        }

        internal void SetScale(float p)
        {
            
        }

        public void PlayHit(GameTime gameTime)
        {
            if (SoundEffectHit != null && (gameTime.TotalGameTime - lastHitSound).TotalSeconds > 0.2)
            {
                SoundEffectHit.Play(1f, 1.2f - gameScreen.random.Next(40) / 100f, 0f);
                lastHitSound = gameTime.TotalGameTime;
            }


        }
    }
}
