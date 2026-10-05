using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// A circle, used for collisions and random points.
    /// </summary>
    public struct Circle : IEquatable<Circle>
    {
        public Vector2 Center;
        public float Radius;

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public Circle(float x, float y, float radius) : this(new Vector2(x, y), radius) { }

        /// <summary>The bounding box of the circle.</summary>
        public RectangleF Bounds => new RectangleF(Center.X - Radius, Center.Y - Radius, Radius * 2f, Radius * 2f);

        public bool Contains(Vector2 point) => Collision.CircleContainsPoint(this, point);

        public bool Intersects(Circle other) => Collision.CirclesIntersect(this, other);

        public bool Intersects(RectangleF rectangle) => Collision.CircleRectangleIntersect(this, rectangle);

        public static bool operator ==(Circle a, Circle b) => a.Equals(b);

        public static bool operator !=(Circle a, Circle b) => !a.Equals(b);

        public bool Equals(Circle other) => Center == other.Center && Radius == other.Radius;

        public override bool Equals(object obj) => obj is Circle other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return Center.GetHashCode() * 31 + Radius.GetHashCode();
            }
        }

        public override string ToString()
            => string.Format(CultureInfo.InvariantCulture, "{{Center:{0} Radius:{1}}}", Center, Radius);
    }
}
