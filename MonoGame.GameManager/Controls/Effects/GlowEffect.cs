using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// Light that follows the edge of the button: a halo around it (<see cref="GlowPlacement.Outer"/>), light along
    /// the inside of the edge (<see cref="GlowPlacement.Inner"/>) or a lit border (<see cref="GlowPlacement.Rim"/>).
    /// The light is a generated texture drawn with nine-slice scaling, so the corners stay round at any size.
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new GlowEffect().SetColor(Color.Gold).SetRadius(18).SetPulse(0.8f, 0.4f));
    /// button.AddEffect(new GlowEffect(GlowPlacement.Rim).SetColor(Color.LightYellow).SetThickness(2).SetOffset(-4));
    /// </code>
    /// </example>
    public class GlowEffect : ButtonEffect<GlowEffect>
    {
        public GlowEffect() { }

        public GlowEffect(GlowPlacement placement)
        {
            Placement = placement;
        }

        /// <summary>Where the light shines.</summary>
        public GlowPlacement Placement { get; set; } = GlowPlacement.Outer;

        /// <summary>How far the light goes from the edge (outside for an outer glow, inside for an inner glow).</summary>
        public float Radius { get; set; } = 12f;

        /// <summary>1 is a linear fade, higher values keep the light closer to the edge.</summary>
        public float Falloff { get; set; } = 2f;

        /// <summary>The width of the line of a rim.</summary>
        public float Thickness { get; set; } = 3f;

        /// <summary>The soft light on both sides of the line of a rim.</summary>
        public float Softness { get; set; } = 2f;

        /// <summary>Which edges an inner glow lights: all of them, or only the top or the bottom one (a gradient).</summary>
        public GlowEdges Edges { get; set; } = GlowEdges.All;

        /// <summary>Moves the edge followed by the light: positive values outwards, negative values inwards (eg: an inner frame).</summary>
        public float Offset { get; set; }

        /// <inheritdoc />
        public override ButtonEffectLayer Layer
            => Placement == GlowPlacement.Outer ? ButtonEffectLayer.Behind
                : Placement == GlowPlacement.Inner ? ButtonEffectLayer.Inside
                : ButtonEffectLayer.Front;

        public GlowEffect SetPlacement(GlowPlacement placement)
        {
            Placement = placement;
            return this;
        }

        public GlowEffect SetRadius(float radius)
        {
            Radius = radius;
            return this;
        }

        public GlowEffect SetFalloff(float falloff)
        {
            Falloff = falloff;
            return this;
        }

        public GlowEffect SetThickness(float thickness)
        {
            Thickness = thickness;
            return this;
        }

        public GlowEffect SetSoftness(float softness)
        {
            Softness = softness;
            return this;
        }

        /// <summary>Sets which edges an inner glow lights.</summary>
        public GlowEffect SetEdges(GlowEdges edges)
        {
            Edges = edges;
            return this;
        }

        public GlowEffect SetOffset(float offset)
        {
            Offset = offset;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var shape = context.Bounds.Inflate(Offset, Offset);
            if (shape.Width <= 0f || shape.Height <= 0f)
                return;

            var radius = RoundedRectanglePath.ClampRadius(shape, context.CornerRadius + Offset);
            var density = context.TextureDensity;
            LightFrame frame;
            switch (Placement)
            {
                case GlowPlacement.Inner when Edges != GlowEdges.All:
                    DrawEdgeGradient(spriteBatch, shape, context);
                    return;
                case GlowPlacement.Inner:
                    var depth = Math.Min(Math.Max(0f, Radius), Math.Min(shape.Width, shape.Height) / 2f);
                    frame = ButtonEffectResources.GetGlowFrame(radius, 0f, depth, Falloff, density);
                    break;
                case GlowPlacement.Rim:
                    frame = ButtonEffectResources.GetRimFrame(radius, Thickness, Softness, density);
                    break;
                default:
                    frame = ButtonEffectResources.GetGlowFrame(radius, Math.Max(0f, Radius), 1f, Falloff, density);
                    break;
            }

            frame.Draw(spriteBatch, shape, GetTint(context));
        }

        /// <summary>An inner glow along the top or the bottom edge only: a gradient clipped by the shape of the button.</summary>
        private void DrawEdgeGradient(SpriteBatch spriteBatch, RectangleF shape, ButtonEffectContext context)
        {
            var height = Math.Min(Math.Max(0f, Radius), shape.Height);
            if (height <= 0f)
                return;

            var gradient = ButtonEffectResources.GetVerticalGradient(Falloff);
            var fromBottom = Edges == GlowEdges.Bottom;
            spriteBatch.Draw(gradient, new Vector2(shape.X, fromBottom ? shape.Bottom - height : shape.Y), null, GetTint(context), 0f, Vector2.Zero,
                new Vector2(shape.Width / gradient.Width, height / gradient.Height), fromBottom ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
        }
    }
}
