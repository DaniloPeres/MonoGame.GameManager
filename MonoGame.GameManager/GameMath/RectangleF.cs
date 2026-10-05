using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// An axis-aligned rectangle with float coordinates (sub-pixel precision for game logic and collisions).
    /// </summary>
    public struct RectangleF : IEquatable<RectangleF>
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectangleF(Vector2 position, Vector2 size) : this(position.X, position.Y, size.X, size.Y) { }

        public static RectangleF Empty => new RectangleF();

        public float Left => X;

        public float Right => X + Width;

        public float Top => Y;

        public float Bottom => Y + Height;

        public Vector2 Position => new Vector2(X, Y);

        public Vector2 Size => new Vector2(Width, Height);

        public Vector2 Center => new Vector2(X + Width / 2f, Y + Height / 2f);

        public bool IsEmpty => Width <= 0f || Height <= 0f;

        /// <summary>True if the point is inside the rectangle (left/top edges included, right/bottom excluded).</summary>
        public bool Contains(Vector2 point) => point.X >= X && point.X < Right && point.Y >= Y && point.Y < Bottom;

        public bool Contains(RectangleF other) => other.X >= X && other.Right <= Right && other.Y >= Y && other.Bottom <= Bottom;

        /// <summary>True if the rectangles overlap (touching edges do not count).</summary>
        public bool Intersects(RectangleF other) => other.Left < Right && Left < other.Right && other.Top < Bottom && Top < other.Bottom;

        public RectangleF Offset(Vector2 offset) => new RectangleF(X + offset.X, Y + offset.Y, Width, Height);

        public RectangleF Inflate(float horizontalAmount, float verticalAmount)
            => new RectangleF(X - horizontalAmount, Y - verticalAmount, Width + horizontalAmount * 2f, Height + verticalAmount * 2f);

        /// <summary>The overlapping area of two rectangles, or <see cref="Empty"/>.</summary>
        public static RectangleF Intersect(RectangleF a, RectangleF b)
        {
            var left = Math.Max(a.Left, b.Left);
            var top = Math.Max(a.Top, b.Top);
            var right = Math.Min(a.Right, b.Right);
            var bottom = Math.Min(a.Bottom, b.Bottom);
            return right > left && bottom > top ? new RectangleF(left, top, right - left, bottom - top) : Empty;
        }

        /// <summary>The smallest rectangle containing both rectangles.</summary>
        public static RectangleF Union(RectangleF a, RectangleF b)
        {
            var left = Math.Min(a.Left, b.Left);
            var top = Math.Min(a.Top, b.Top);
            return new RectangleF(left, top, Math.Max(a.Right, b.Right) - left, Math.Max(a.Bottom, b.Bottom) - top);
        }

        /// <summary>The smallest integer rectangle containing this rectangle.</summary>
        public Rectangle ToRectangle()
        {
            var left = (int)Math.Floor(X);
            var top = (int)Math.Floor(Y);
            return new Rectangle(left, top, (int)Math.Ceiling(Right) - left, (int)Math.Ceiling(Bottom) - top);
        }

        public static implicit operator RectangleF(Rectangle rectangle) => new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

        public static explicit operator Rectangle(RectangleF rectangle) => rectangle.ToRectangle();

        public static bool operator ==(RectangleF a, RectangleF b) => a.Equals(b);

        public static bool operator !=(RectangleF a, RectangleF b) => !a.Equals(b);

        public bool Equals(RectangleF other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

        public override bool Equals(object obj) => obj is RectangleF other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + X.GetHashCode();
                hash = hash * 31 + Y.GetHashCode();
                hash = hash * 31 + Width.GetHashCode();
                hash = hash * 31 + Height.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
            => string.Format(CultureInfo.InvariantCulture, "{{X:{0} Y:{1} Width:{2} Height:{3}}}", X, Y, Width, Height);
    }
}
