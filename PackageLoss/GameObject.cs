using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PackageLoss
{
    internal class GameObject
    {
        internal Body Compound { get; set; }
        private Texture2D polygonTexture;
        World world;
        GameScreen gameScreen;

        private Vector2 _origin;

        public GameObject(GameScreen gameScreen, Texture2D texture2D, World world)
        {
            this.world = world;
            this.gameScreen = gameScreen;
            this.polygonTexture = texture2D;
            //Create an array to hold the data from the texture
            uint[] data = new uint[polygonTexture.Width * polygonTexture.Height];

            //Transfer the texture data to the array
            polygonTexture.GetData(data);

            //Find the vertices that makes up the outline of the shape in the texture
            Vertices textureVertices = PolygonTools.CreatePolygon(data, polygonTexture.Width, false);

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
            Vector2 vertScale = new Vector2(ConvertUnits.ToSimUnits(1));
            foreach (Vertices vertices in list)
            {
                vertices.Scale(ref vertScale);
            }

            //Create a single body with multiple fixtures
            Compound = BodyFactory.CreateCompoundPolygon(world, list, 1f, BodyType.Dynamic);
            Compound.BodyType = BodyType.Dynamic;
            Compound.Inertia = 100.0f;
        }

        public void Draw(SpriteBatch spriteBatch, Camera2D camera)
        {
            gameScreen.Game.SpriteBatch.Begin(0, null, null, null, null, null, camera.View);
            gameScreen.Game.SpriteBatch.Draw(polygonTexture, ConvertUnits.ToDisplayUnits(Compound.Position), null, Color.White, Compound.Rotation, _origin, 1.0f, SpriteEffects.None, 0f);
            gameScreen.Game.SpriteBatch.End();
        }
    }
}
