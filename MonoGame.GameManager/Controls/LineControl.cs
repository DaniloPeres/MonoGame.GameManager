using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A straight line between two points of the parent (laser, connection, rope, debug drawing...).
    /// </summary>
    /// <remarks>
    /// The position and the size of the control are the bounding box of the line. Pointer events hit the line itself
    /// (with a small tolerance), not its bounding box.
    /// </remarks>
    public class LineControl : ScalableControlAbstract<LineControl>
    {
        private const float HitTolerance = 3f;
        private Vector2 start;
        private Vector2 end;

        public LineControl(Vector2 start, Vector2 end, Color color, float thickness = 1f)
        {
            Color = color;
            Thickness = thickness;
            SetPoints(start, end);
        }

        /// <summary>The first point, in the units of the parent.</summary>
        public Vector2 Start => start;

        /// <summary>The second point, in the units of the parent.</summary>
        public Vector2 End => end;

        /// <summary>The thickness, in local units.</summary>
        public float Thickness { get; set; }

        public LineControl SetPoints(Vector2 start, Vector2 end)
        {
            this.start = start;
            this.end = end;
            PositionAnchor = Vector2.Min(start, end);
            Size = Vector2.Max(start, end) - Vector2.Min(start, end);
            return this;
        }

        public LineControl SetThickness(float thickness)
        {
            Thickness = thickness;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            GetScreenPoints(out var from, out var to);
            spriteBatch.DrawLine(from, to, DrawColor, Thickness * Math.Abs(NestedScale.Y), LayerDepthDraw);
        }

        public override bool Intersects(Point pointToCompare)
        {
            GetScreenPoints(out var from, out var to);
            var halfThickness = Math.Max(Thickness * Math.Abs(NestedScale.Y) / 2f, HitTolerance);
            return new LineSegment(from, to).DistanceTo(pointToCompare.ToVector2()) <= halfThickness;
        }

        private void GetScreenPoints(out Vector2 from, out Vector2 to)
        {
            var topLeft = Vector2.Min(start, end);
            from = LocalToParentPosition(start - topLeft);
            to = LocalToParentPosition(end - topLeft);
        }
    }
}
