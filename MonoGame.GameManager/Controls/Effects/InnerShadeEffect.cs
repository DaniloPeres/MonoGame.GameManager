using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// Darkens a part of the button (painted, not a light): the bottom for volume, the top for a hollow look, the edges
    /// for an inner shadow, or the whole button. It is clipped to the shape of the button and does not move.
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new InnerShadeEffect().SetHeightRate(0.5f).SetIntensity(0.25f));
    /// button.AddEffect(new InnerShadeEffect(ShadePlacement.Edges).SetRadius(10).SetIntensity(0.3f));
    /// </code>
    /// </example>
    public class InnerShadeEffect : ButtonEffect<InnerShadeEffect>
    {
        public InnerShadeEffect() : this(ShadePlacement.Bottom) { }

        public InnerShadeEffect(ShadePlacement placement)
        {
            Placement = placement;
            Blend = ButtonEffectBlend.Normal;
            Color = Color.Black;
            Intensity = 0.25f;
            IgnoreStates();
        }

        /// <summary>Where the button is darkened.</summary>
        public ShadePlacement Placement { get; set; }

        /// <summary>The height of the shade, as a part of the height of the button (for the top and the bottom).</summary>
        public float HeightRate { get; set; } = 0.5f;

        /// <summary>1 is a linear fade, higher values fade faster.</summary>
        public float Falloff { get; set; } = 1.4f;

        /// <summary>How far the shade goes from the edges (for <see cref="ShadePlacement.Edges"/>).</summary>
        public float Radius { get; set; } = 10f;

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Inside;

        public InnerShadeEffect SetPlacement(ShadePlacement placement)
        {
            Placement = placement;
            return this;
        }

        public InnerShadeEffect SetHeightRate(float heightRate)
        {
            HeightRate = heightRate;
            return this;
        }

        public InnerShadeEffect SetFalloff(float falloff)
        {
            Falloff = falloff;
            return this;
        }

        public InnerShadeEffect SetRadius(float radius)
        {
            Radius = radius;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var bounds = context.Bounds;
            var tint = GetTint(context);
            switch (Placement)
            {
                case ShadePlacement.Full:
                    spriteBatch.FillRectangle(bounds, tint);
                    break;
                case ShadePlacement.Edges:
                    var depth = Math.Min(Math.Max(0f, Radius), Math.Min(bounds.Width, bounds.Height) / 2f);
                    ButtonEffectResources.GetGlowFrame(context.CornerRadius, 0f, depth, Falloff, context.TextureDensity).Draw(spriteBatch, bounds, tint);
                    break;
                default:
                    var height = bounds.Height * MathUtils.Clamp01(HeightRate);
                    if (height <= 0f || bounds.Width <= 0f)
                        return;
                    var gradient = ButtonEffectResources.GetVerticalGradient(Falloff);
                    var fromBottom = Placement == ShadePlacement.Bottom;
                    spriteBatch.Draw(gradient, new Vector2(bounds.X, fromBottom ? bounds.Bottom - height : bounds.Y), null, tint, 0f, Vector2.Zero,
                        new Vector2(bounds.Width / gradient.Width, height / gradient.Height), fromBottom ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
                    break;
            }
        }
    }
}
