using Microsoft.Xna.Framework;
using System;
using System.Globalization;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// A line segment between two points.
    /// </summary>
    public struct LineSegment : IEquatable<LineSegment>
    {
        public Vector2 Start;
        public Vector2 End;

        public LineSegment(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }

        /// <summary>The vector from <see cref="Start"/> to <see cref="End"/>.</summary>
        public Vector2 Vector => End - Start;

        public float Length => Vector.Length();

        public float LengthSquared => Vector.LengthSquared();

        /// <summary>The normalized direction, or zero for a segment of length 0.</summary>
        public Vector2 Direction => Vector.NormalizedOrZero();

        public Vector2 Center => (Start + End) / 2f;

        /// <summary>The point of the segment closest to <paramref name="point"/>.</summary>
        public Vector2 ClosestPoint(Vector2 point)
        {
            var vector = Vector;
            var lengthSquared = vector.LengthSquared();
            if (lengthSquared <= MathUtils.Epsilon)
                return Start;

            var t = MathHelper.Clamp(Vector2.Dot(point - Start, vector) / lengthSquared, 0f, 1f);
            return Start + vector * t;
        }

        /// <summary>The distance from the segment to <paramref name="point"/>.</summary>
        public float DistanceTo(Vector2 point) => Vector2.Distance(ClosestPoint(point), point);

        public bool Intersects(LineSegment other, out Vector2 intersection) => Collision.LineSegmentsIntersect(this, other, out intersection);

        public static bool operator ==(LineSegment a, LineSegment b) => a.Equals(b);

        public static bool operator !=(LineSegment a, LineSegment b) => !a.Equals(b);

        public bool Equals(LineSegment other) => Start == other.Start && End == other.End;

        public override bool Equals(object obj) => obj is LineSegment other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return Start.GetHashCode() * 31 + End.GetHashCode();
            }
        }

        public override string ToString()
            => string.Format(CultureInfo.InvariantCulture, "{{Start:{0} End:{1}}}", Start, End);
    }
}
