using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// Collision detection between points, rectangles, circles, polygons and line segments.
    /// </summary>
    /// <remarks>
    /// The methods with a <c>minimumTranslation</c> output return the smallest translation to apply to the first
    /// shape so it stops overlapping the second one (minimum translation vector).
    /// </remarks>
    public static class Collision
    {
        /// <summary>
        /// Checks if a point is inside a rectangle shifted by an origin, as controls are drawn.
        /// </summary>
        public static bool RectangleContainsPoint(Rectangle rectangle, Vector2 origin, Point point)
        {
            var left = rectangle.X - (int)origin.X;
            var top = rectangle.Y - (int)origin.Y;
            return point.X >= left && point.X < left + rectangle.Width && point.Y >= top && point.Y < top + rectangle.Height;
        }

        public static bool RectangleContainsPoint(RectangleF rectangle, Vector2 point) => rectangle.Contains(point);

        public static bool CircleContainsPoint(Circle circle, Vector2 point)
            => Vector2.DistanceSquared(circle.Center, point) <= circle.Radius * circle.Radius;

        /// <summary>
        /// Checks if a point is inside a polygon (convex or concave), using the even-odd rule.
        /// </summary>
        public static bool PolygonContainsPoint(IList<Vector2> polygon, Vector2 point)
        {
            if (polygon == null || polygon.Count < 3)
                return false;

            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var a = polygon[i];
                var b = polygon[j];
                if ((a.Y > point.Y) != (b.Y > point.Y)
                    && point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        public static bool RectanglesIntersect(RectangleF a, RectangleF b) => a.Intersects(b);

        /// <summary>
        /// Checks if two rectangles overlap and computes the translation that separates <paramref name="a"/> from
        /// <paramref name="b"/> along the axis of least overlap.
        /// </summary>
        public static bool RectanglesIntersect(RectangleF a, RectangleF b, out Vector2 minimumTranslation)
        {
            var overlapX = Math.Min(a.Right, b.Right) - Math.Max(a.Left, b.Left);
            var overlapY = Math.Min(a.Bottom, b.Bottom) - Math.Max(a.Top, b.Top);
            if (overlapX <= 0f || overlapY <= 0f)
            {
                minimumTranslation = Vector2.Zero;
                return false;
            }

            minimumTranslation = overlapX < overlapY
                ? new Vector2(a.Center.X < b.Center.X ? -overlapX : overlapX, 0f)
                : new Vector2(0f, a.Center.Y < b.Center.Y ? -overlapY : overlapY);
            return true;
        }

        public static bool RectanglesIntersect(Rectangle a, Rectangle b, out Vector2 minimumTranslation)
            => RectanglesIntersect((RectangleF)a, (RectangleF)b, out minimumTranslation);

        public static bool CirclesIntersect(Circle a, Circle b)
        {
            var radii = a.Radius + b.Radius;
            return Vector2.DistanceSquared(a.Center, b.Center) < radii * radii;
        }

        /// <summary>
        /// Checks if two circles overlap and computes the translation that separates <paramref name="a"/> from
        /// <paramref name="b"/>.
        /// </summary>
        public static bool CirclesIntersect(Circle a, Circle b, out Vector2 minimumTranslation)
        {
            var delta = a.Center - b.Center;
            var radii = a.Radius + b.Radius;
            var distanceSquared = delta.LengthSquared();
            if (distanceSquared >= radii * radii)
            {
                minimumTranslation = Vector2.Zero;
                return false;
            }

            var distance = (float)Math.Sqrt(distanceSquared);
            minimumTranslation = distance > MathUtils.Epsilon
                ? delta / distance * (radii - distance)
                : new Vector2(radii, 0f);
            return true;
        }

        public static bool CircleRectangleIntersect(Circle circle, RectangleF rectangle)
            => CircleRectangleIntersect(circle, rectangle, out _);

        /// <summary>
        /// Checks if a circle and a rectangle overlap and computes the translation that separates the circle from
        /// the rectangle.
        /// </summary>
        public static bool CircleRectangleIntersect(Circle circle, RectangleF rectangle, out Vector2 minimumTranslation)
        {
            var center = circle.Center;
            var closest = new Vector2(
                MathHelper.Clamp(center.X, rectangle.Left, rectangle.Right),
                MathHelper.Clamp(center.Y, rectangle.Top, rectangle.Bottom));
            var delta = center - closest;
            var distanceSquared = delta.LengthSquared();

            if (distanceSquared >= circle.Radius * circle.Radius)
            {
                minimumTranslation = Vector2.Zero;
                return false;
            }

            if (distanceSquared > MathUtils.Epsilon)
            {
                var distance = (float)Math.Sqrt(distanceSquared);
                minimumTranslation = delta / distance * (circle.Radius - distance);
                return true;
            }

            // The center is inside the rectangle: push the circle out through the nearest edge.
            var toLeft = center.X - rectangle.Left;
            var toRight = rectangle.Right - center.X;
            var toTop = center.Y - rectangle.Top;
            var toBottom = rectangle.Bottom - center.Y;
            var nearest = Math.Min(Math.Min(toLeft, toRight), Math.Min(toTop, toBottom));

            if (nearest == toLeft)
                minimumTranslation = new Vector2(-(toLeft + circle.Radius), 0f);
            else if (nearest == toRight)
                minimumTranslation = new Vector2(toRight + circle.Radius, 0f);
            else if (nearest == toTop)
                minimumTranslation = new Vector2(0f, -(toTop + circle.Radius));
            else
                minimumTranslation = new Vector2(0f, toBottom + circle.Radius);
            return true;
        }

        /// <summary>
        /// Checks if two line segments intersect and returns the intersection point (for collinear overlapping
        /// segments, the first overlapping point of <paramref name="a"/>).
        /// </summary>
        public static bool LineSegmentsIntersect(LineSegment a, LineSegment b, out Vector2 intersection)
        {
            var r = a.End - a.Start;
            var s = b.End - b.Start;
            var startDelta = b.Start - a.Start;
            var denominator = r.Cross(s);

            if (Math.Abs(denominator) < MathUtils.Epsilon)
            {
                intersection = Vector2.Zero;
                if (Math.Abs(startDelta.Cross(r)) >= MathUtils.Epsilon && Math.Abs(startDelta.Cross(s)) >= MathUtils.Epsilon)
                    return false; // parallel and not collinear

                var lengthSquared = r.LengthSquared();
                if (lengthSquared < MathUtils.Epsilon)
                {
                    // a is a point
                    if (b.DistanceTo(a.Start) > MathUtils.Epsilon)
                        return false;
                    intersection = a.Start;
                    return true;
                }

                var t0 = Vector2.Dot(startDelta, r) / lengthSquared;
                var t1 = t0 + Vector2.Dot(s, r) / lengthSquared;
                var tMin = Math.Min(t0, t1);
                var tMax = Math.Max(t0, t1);
                if (tMax < 0f || tMin > 1f || Math.Abs(startDelta.Cross(r)) >= MathUtils.Epsilon)
                    return false;

                intersection = a.Start + r * Math.Max(0f, tMin);
                return true;
            }

            var t = startDelta.Cross(s) / denominator;
            var u = startDelta.Cross(r) / denominator;
            if (t < 0f || t > 1f || u < 0f || u > 1f)
            {
                intersection = Vector2.Zero;
                return false;
            }

            intersection = a.Start + r * t;
            return true;
        }

        public static bool LineSegmentIntersectsRectangle(LineSegment segment, RectangleF rectangle)
        {
            if (rectangle.Contains(segment.Start) || rectangle.Contains(segment.End))
                return true;

            var topLeft = new Vector2(rectangle.Left, rectangle.Top);
            var topRight = new Vector2(rectangle.Right, rectangle.Top);
            var bottomLeft = new Vector2(rectangle.Left, rectangle.Bottom);
            var bottomRight = new Vector2(rectangle.Right, rectangle.Bottom);

            return LineSegmentsIntersect(segment, new LineSegment(topLeft, topRight), out _)
                || LineSegmentsIntersect(segment, new LineSegment(topRight, bottomRight), out _)
                || LineSegmentsIntersect(segment, new LineSegment(bottomRight, bottomLeft), out _)
                || LineSegmentsIntersect(segment, new LineSegment(bottomLeft, topLeft), out _);
        }

        public static bool LineSegmentIntersectsCircle(LineSegment segment, Circle circle)
            => Vector2.DistanceSquared(segment.ClosestPoint(circle.Center), circle.Center) <= circle.Radius * circle.Radius;

        /// <summary>
        /// Casts a ray against a rectangle. <paramref name="distance"/> is expressed in multiples of
        /// <paramref name="direction"/> (it is the distance when the direction is normalized).
        /// </summary>
        public static bool RayIntersectsRectangle(Vector2 origin, Vector2 direction, RectangleF rectangle, out float distance)
        {
            if (rectangle.Contains(origin))
            {
                distance = 0f;
                return true;
            }

            var hit = RaySlabs(origin, direction, rectangle, out distance, out _);
            return hit && distance >= 0f;
        }

        /// <summary>
        /// Continuous collision between a moving rectangle and a static one, so fast objects do not go through thin
        /// obstacles (tunneling).
        /// </summary>
        /// <param name="moving">The moving rectangle at its start position.</param>
        /// <param name="displacement">The movement of this frame.</param>
        /// <param name="target">The static rectangle.</param>
        /// <param name="time">
        /// The fraction of <paramref name="displacement"/>, from 0 to 1, at which the rectangles touch.
        /// </param>
        /// <param name="normal">The normal of the face of <paramref name="target"/> that was hit (zero when they already overlap).</param>
        /// <returns>True if the rectangles touch during the movement.</returns>
        public static bool SweptRectangles(RectangleF moving, Vector2 displacement, RectangleF target, out float time, out Vector2 normal)
        {
            var expanded = new RectangleF(
                target.X - moving.Width / 2f,
                target.Y - moving.Height / 2f,
                target.Width + moving.Width,
                target.Height + moving.Height);
            var origin = moving.Center;

            if (origin.X > expanded.Left && origin.X < expanded.Right && origin.Y > expanded.Top && origin.Y < expanded.Bottom)
            {
                time = 0f;
                normal = Vector2.Zero;
                return true; // already overlapping
            }

            if (!RaySlabs(origin, displacement, expanded, out time, out normal) || time < 0f || time > 1f)
            {
                time = 1f;
                normal = Vector2.Zero;
                return false;
            }

            return true;
        }

        private static bool RaySlabs(Vector2 origin, Vector2 direction, RectangleF box, out float tNear, out Vector2 normal)
        {
            tNear = float.NegativeInfinity;
            var tFar = float.PositiveInfinity;
            normal = Vector2.Zero;

            if (Math.Abs(direction.X) < MathUtils.Epsilon)
            {
                if (origin.X <= box.Left || origin.X >= box.Right)
                    return false;
            }
            else
            {
                var inverse = 1f / direction.X;
                var t1 = (box.Left - origin.X) * inverse;
                var t2 = (box.Right - origin.X) * inverse;
                var normalX = -1f;
                if (t1 > t2)
                {
                    var swap = t1;
                    t1 = t2;
                    t2 = swap;
                    normalX = 1f;
                }
                if (t1 > tNear)
                {
                    tNear = t1;
                    normal = new Vector2(normalX, 0f);
                }
                tFar = Math.Min(tFar, t2);
            }

            if (Math.Abs(direction.Y) < MathUtils.Epsilon)
            {
                if (origin.Y <= box.Top || origin.Y >= box.Bottom)
                    return false;
            }
            else
            {
                var inverse = 1f / direction.Y;
                var t1 = (box.Top - origin.Y) * inverse;
                var t2 = (box.Bottom - origin.Y) * inverse;
                var normalY = -1f;
                if (t1 > t2)
                {
                    var swap = t1;
                    t1 = t2;
                    t2 = swap;
                    normalY = 1f;
                }
                if (t1 > tNear)
                {
                    tNear = t1;
                    normal = new Vector2(0f, normalY);
                }
                tFar = Math.Min(tFar, t2);
            }

            return tNear <= tFar && tFar >= 0f && !float.IsNegativeInfinity(tNear);
        }
    }
}
