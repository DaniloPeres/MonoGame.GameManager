using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// Helpers for <see cref="Vector2"/>.
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>Returns the vector rotated around the origin. Unlike MonoGame's <c>Vector2.Rotate</c>, it does not change the vector.</summary>
        public static Vector2 Rotated(this Vector2 vector, float radians)
        {
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);
            return new Vector2(vector.X * cos - vector.Y * sin, vector.X * sin + vector.Y * cos);
        }

        /// <summary>Rotates the point around a pivot.</summary>
        public static Vector2 RotateAround(this Vector2 point, Vector2 pivot, float radians) => (point - pivot).Rotated(radians) + pivot;

        /// <summary>The angle of the vector in radians (0 = right, positive = clockwise on the screen).</summary>
        public static float ToAngle(this Vector2 vector) => (float)Math.Atan2(vector.Y, vector.X);

        /// <summary>The vector rotated by 90 degrees.</summary>
        public static Vector2 Perpendicular(this Vector2 vector) => new Vector2(-vector.Y, vector.X);

        /// <summary>The normalized vector, or zero when its length is (almost) zero.</summary>
        public static Vector2 NormalizedOrZero(this Vector2 vector)
        {
            var length = vector.Length();
            return length > MathUtils.Epsilon ? vector / length : Vector2.Zero;
        }

        /// <summary>The vector with its length limited to <paramref name="maxLength"/>.</summary>
        public static Vector2 Truncate(this Vector2 vector, float maxLength)
        {
            var lengthSquared = vector.LengthSquared();
            if (lengthSquared <= maxLength * maxLength)
                return vector;
            return vector / (float)Math.Sqrt(lengthSquared) * maxLength;
        }

        /// <summary>Moves a point towards a target without overshooting it.</summary>
        public static Vector2 MoveTowards(this Vector2 current, Vector2 target, float maxDistanceDelta)
        {
            var delta = target - current;
            var distance = delta.Length();
            if (distance <= maxDistanceDelta || distance <= MathUtils.Epsilon)
                return target;
            return current + delta / distance * maxDistanceDelta;
        }

        /// <summary>Converts to a point rounding each coordinate (instead of truncating it).</summary>
        /// <summary>Rounds to the nearest point; halves are rounded up, so pixel snapping is the same everywhere.</summary>
        public static Point ToPointRounded(this Vector2 vector) => new Point((int)Math.Floor(vector.X + 0.5f), (int)Math.Floor(vector.Y + 0.5f));

        public static Vector2 WithX(this Vector2 vector, float x) => new Vector2(x, vector.Y);

        public static Vector2 WithY(this Vector2 vector, float y) => new Vector2(vector.X, y);

        public static Vector2 Abs(this Vector2 vector) => new Vector2(Math.Abs(vector.X), Math.Abs(vector.Y));

        /// <summary>The projection of the vector on another vector.</summary>
        public static Vector2 Project(this Vector2 vector, Vector2 onto)
        {
            var lengthSquared = onto.LengthSquared();
            return lengthSquared <= MathUtils.Epsilon ? Vector2.Zero : onto * (Vector2.Dot(vector, onto) / lengthSquared);
        }

        /// <summary>The 2D cross product (the Z component of the 3D cross product).</summary>
        public static float Cross(this Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;

        public static bool IsNearlyZero(this Vector2 vector, float epsilon = MathUtils.Epsilon) => vector.LengthSquared() <= epsilon * epsilon;
    }
}
