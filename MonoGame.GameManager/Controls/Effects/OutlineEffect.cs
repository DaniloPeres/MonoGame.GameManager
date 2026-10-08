using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// A solid rounded border of any thickness, around the button or inside its edge (painted, not a light). Several
    /// outlines make double borders (eg: a light line inside and a dark one outside). It can use a darker or lighter
    /// version of the background color of the button.
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new OutlineEffect().SetColor(Color.White).SetThickness(3));          // a white border around
    /// button.AddEffect(new OutlineEffect().UseButtonColor(0.45f).SetThickness(3));          // a darker border around
    /// button.AddEffect(new OutlineEffect(OutlinePosition.Inside).SetColor(Color.White * 0.5f).SetOffset(3).SetThickness(1.5f));
    /// </code>
    /// </example>
    public class OutlineEffect : ButtonEffect<OutlineEffect>
    {
        public OutlineEffect() : this(OutlinePosition.Outside) { }

        public OutlineEffect(OutlinePosition position)
        {
            Position = position;
            Blend = ButtonEffectBlend.Normal;
            IgnoreStates();
        }

        /// <summary>Around the button or inside its edge.</summary>
        public OutlinePosition Position { get; set; }

        /// <summary>The thickness of the border.</summary>
        public float Thickness { get; set; } = 3f;

        /// <summary>The space between the edge of the button and the border.</summary>
        public float Offset { get; set; }

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => Position == OutlinePosition.Outside ? ButtonEffectLayer.Behind : ButtonEffectLayer.Inside;

        public OutlineEffect SetPosition(OutlinePosition position)
        {
            Position = position;
            return this;
        }

        public OutlineEffect SetThickness(float thickness)
        {
            Thickness = thickness;
            return this;
        }

        public OutlineEffect SetOffset(float offset)
        {
            Offset = offset;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var thickness = Math.Max(0f, Thickness);
            if (thickness <= 0f)
                return;

            var bounds = context.Bounds;
            var outside = Position == OutlinePosition.Outside;
            var grow = outside ? Offset + thickness : -Offset;
            var shape = bounds.Inflate(grow, grow);
            if (shape.Width <= 0f || shape.Height <= 0f)
                return;

            var radius = Math.Max(0f, context.CornerRadius + grow);
            ButtonEffectResources.GetRoundedBorder(radius, thickness, context.TextureDensity).Draw(spriteBatch, shape, GetTint(context));
        }
    }
}
