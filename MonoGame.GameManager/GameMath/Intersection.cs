using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.GameMath
{
    [Obsolete("Use the Collision class instead.")]
    public static class Intersection
    {
        [Obsolete("Use Collision.RectangleContainsPoint instead.")]
        public static bool IntersectsWithPoint(Rectangle destinationRectangleControl, Vector2 originControl, Point pointToCompare)
            => Collision.RectangleContainsPoint(destinationRectangleControl, originControl, pointToCompare);
    }
}
