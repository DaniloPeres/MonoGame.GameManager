using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Extensions;

namespace MonoGame.GameManager.GameMath
{
    public static class Intersection
    {
        public static bool IntersectsWithPoint(Rectangle destinationRectangleControl, Vector2 originControl, Point pointToCompare)
        {
            var rec = destinationRectangleControl;
            rec.X -= (int)(originControl.X);
            rec.Y -= (int)(originControl.Y);

            var pointRectangleToCompare = new Rectangle(pointToCompare, new Point(1));

            return rec.Intersects(pointRectangleToCompare);
        }

        public static bool IntersectsTextureWithPoint(Texture2D texture, Vector2 position, Vector2 originControl, Point pointToCompare)
        {
            return IntersectsWithPoint(new Rectangle(position.ToPoint(), texture.Size()), originControl, pointToCompare);
        }
    }
}
