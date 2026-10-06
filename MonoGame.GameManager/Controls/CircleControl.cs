using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A filled circle or the outline of a circle. Its position is its center.
    /// </summary>
    public class CircleControl : ScalableControlAbstract<CircleControl>
    {
        private float radius;

        public CircleControl(Vector2 center, float radius, Color color, bool filled = true)
        {
            Color = color;
            IsFilled = filled;
            SetRadius(radius);
            PositionAnchor = center;
        }

        public float Radius => radius;

        /// <summary>True for a filled circle, false for an outline.</summary>
        public bool IsFilled { get; set; }

        /// <summary>The thickness of the outline, in local units.</summary>
        public float Thickness { get; set; } = 1f;

        /// <summary>The number of segments of the outline.</summary>
        public int Segments { get; set; } = 48;

        /// <summary>Changes the radius (the center stays in place).</summary>
        public CircleControl SetRadius(float radius)
        {
            this.radius = Math.Max(0f, radius);
            Size = new Vector2(this.radius * 2f);
            Origin = new Vector2(this.radius);
            return this;
        }

        public CircleControl SetIsFilled(bool isFilled)
        {
            IsFilled = isFilled;
            return this;
        }

        public CircleControl SetThickness(float thickness)
        {
            Thickness = thickness;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (radius <= 0f)
                return;

            var center = GetCenter();
            var scaledRadius = radius * Math.Abs(NestedScale.X);
            if (IsFilled)
                spriteBatch.FillCircle(center, scaledRadius, DrawColor, LayerDepthDraw);
            else
                spriteBatch.DrawCircle(center, scaledRadius, DrawColor, Thickness * Math.Abs(NestedScale.X), Math.Max(3, Segments), LayerDepthDraw);
        }

        public override bool Intersects(Point pointToCompare)
            => Vector2.Distance(pointToCompare.ToVector2(), GetCenter()) <= radius * Math.Abs(NestedScale.X);

        private Vector2 GetCenter() => LocalToParentPosition(new Vector2(radius));
    }
}
