using Microsoft.Xna.Framework;
using MonoGame.GameManager.Enums;
using System;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// The outline of a rounded rectangle as a path that can be walked by distance: lights running around a button,
    /// sparkles born on its border, gems on its corners. The path starts at the left end of the top edge and goes
    /// clockwise (top edge, top-right corner, right edge, bottom-right corner...).
    /// </summary>
    /// <example>
    /// <code>
    /// var length = RoundedRectanglePath.PerimeterLength(bounds, 12);
    /// var point = RoundedRectanglePath.PointAt(bounds, 12, time * speed % length, out var normal);
    /// </code>
    /// </example>
    public static class RoundedRectanglePath
    {
        private const float QuarterTurn = MathHelper.PiOver2;

        /// <summary>The corner radius actually used for a rectangle: between 0 and half of its smallest side.</summary>
        public static float ClampRadius(RectangleF bounds, float radius)
            => MathUtils.Clamp(radius, 0f, Math.Max(0f, Math.Min(bounds.Width, bounds.Height) / 2f));

        /// <summary>The length of the outline.</summary>
        public static float PerimeterLength(RectangleF bounds, float radius)
        {
            radius = ClampRadius(bounds, radius);
            return 2f * (Math.Max(0f, bounds.Width) + Math.Max(0f, bounds.Height)) - 8f * radius + MathHelper.TwoPi * radius;
        }

        /// <summary>The point of the outline at a distance from the start (the distance wraps around the outline).</summary>
        public static Vector2 PointAt(RectangleF bounds, float radius, float distance) => PointAt(bounds, radius, distance, out _);

        /// <summary>
        /// The point of the outline at a distance from the start (the distance wraps around the outline), and the
        /// direction that points out of the rectangle at that point.
        /// </summary>
        public static Vector2 PointAt(RectangleF bounds, float radius, float distance, out Vector2 outwardNormal)
        {
            radius = ClampRadius(bounds, radius);
            var width = Math.Max(0f, bounds.Width);
            var height = Math.Max(0f, bounds.Height);
            var length = PerimeterLength(bounds, radius);
            if (length <= 0f)
            {
                outwardNormal = new Vector2(0f, -1f);
                return bounds.Position;
            }

            var d = MathUtils.Wrap(distance, 0f, length);
            var horizontal = width - 2f * radius;
            var vertical = height - 2f * radius;
            var arc = QuarterTurn * radius;
            var left = bounds.X;
            var top = bounds.Y;
            var right = bounds.X + width;
            var bottom = bounds.Y + height;

            // Top edge
            if (d <= horizontal)
            {
                outwardNormal = new Vector2(0f, -1f);
                return new Vector2(left + radius + d, top);
            }
            d -= horizontal;

            // Top-right corner
            if (d <= arc)
                return OnArc(new Vector2(right - radius, top + radius), radius, -QuarterTurn, d, out outwardNormal);
            d -= arc;

            // Right edge
            if (d <= vertical)
            {
                outwardNormal = new Vector2(1f, 0f);
                return new Vector2(right, top + radius + d);
            }
            d -= vertical;

            // Bottom-right corner
            if (d <= arc)
                return OnArc(new Vector2(right - radius, bottom - radius), radius, 0f, d, out outwardNormal);
            d -= arc;

            // Bottom edge
            if (d <= horizontal)
            {
                outwardNormal = new Vector2(0f, 1f);
                return new Vector2(right - radius - d, bottom);
            }
            d -= horizontal;

            // Bottom-left corner
            if (d <= arc)
                return OnArc(new Vector2(left + radius, bottom - radius), radius, QuarterTurn, d, out outwardNormal);
            d -= arc;

            // Left edge
            if (d <= vertical)
            {
                outwardNormal = new Vector2(-1f, 0f);
                return new Vector2(left, bottom - radius - d);
            }
            d -= vertical;

            // Top-left corner
            return OnArc(new Vector2(left + radius, top + radius), radius, MathHelper.Pi, Math.Min(d, arc), out outwardNormal);
        }

        /// <summary>
        /// A remarkable point of the outline: the middle of an edge (TopCenter, CenterRight...), the middle of a corner
        /// (TopLeft, BottomRight...) or the center of the rectangle.
        /// </summary>
        public static Vector2 AnchorPoint(RectangleF bounds, float radius, Anchor anchor) => AnchorPoint(bounds, radius, anchor, out _);

        /// <summary>A remarkable point of the outline (see <see cref="AnchorPoint(RectangleF, float, Anchor)"/>) and the direction that points out of the rectangle there.</summary>
        public static Vector2 AnchorPoint(RectangleF bounds, float radius, Anchor anchor, out Vector2 outwardNormal)
        {
            radius = ClampRadius(bounds, radius);
            var center = bounds.Center;
            var diagonal = new Vector2(0.70710678f);
            switch (anchor)
            {
                case Anchor.TopCenter:
                    outwardNormal = new Vector2(0f, -1f);
                    return new Vector2(center.X, bounds.Top);
                case Anchor.BottomCenter:
                    outwardNormal = new Vector2(0f, 1f);
                    return new Vector2(center.X, bounds.Bottom);
                case Anchor.CenterLeft:
                    outwardNormal = new Vector2(-1f, 0f);
                    return new Vector2(bounds.Left, center.Y);
                case Anchor.CenterRight:
                    outwardNormal = new Vector2(1f, 0f);
                    return new Vector2(bounds.Right, center.Y);
                case Anchor.TopLeft:
                    outwardNormal = diagonal * new Vector2(-1f, -1f);
                    return new Vector2(bounds.Left + radius, bounds.Top + radius) + outwardNormal * radius;
                case Anchor.TopRight:
                    outwardNormal = diagonal * new Vector2(1f, -1f);
                    return new Vector2(bounds.Right - radius, bounds.Top + radius) + outwardNormal * radius;
                case Anchor.BottomLeft:
                    outwardNormal = diagonal * new Vector2(-1f, 1f);
                    return new Vector2(bounds.Left + radius, bounds.Bottom - radius) + outwardNormal * radius;
                case Anchor.BottomRight:
                    outwardNormal = diagonal;
                    return new Vector2(bounds.Right - radius, bounds.Bottom - radius) + outwardNormal * radius;
                default:
                    outwardNormal = Vector2.Zero;
                    return center;
            }
        }

        private static Vector2 OnArc(Vector2 center, float radius, float startAngle, float distance, out Vector2 outwardNormal)
        {
            var angle = startAngle + (radius > 0f ? distance / radius : 0f);
            outwardNormal = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            return center + outwardNormal * radius;
        }
    }
}
