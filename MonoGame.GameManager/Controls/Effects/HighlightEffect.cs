using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Layout;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// A static, soft reflection inside the button: the rounded band of light at the top of candy, jelly and glossy
    /// buttons, a thin strip under the top edge, an oval reflection, or a band at the bottom. The band fades towards
    /// the middle of the button (<see cref="Fade"/>) and its round ends keep their shape at any size. It does not move
    /// nor blink.
    /// </summary>
    /// <remarks>
    /// By default the reflection is painted (<see cref="ButtonEffectBlend.Normal"/>): a white veil that keeps the color
    /// of the button soft, like glass. With <see cref="ButtonEffectBlend.Light"/> it is added, brighter and more saturated.
    /// The insets shrink on small buttons, so the same effect fits an icon and a wide button.
    /// </remarks>
    /// <example>
    /// <code>
    /// button.AddEffect(new HighlightEffect().SetHeightRate(0.42f).SetInset(8, 4).SetIntensity(0.4f));
    /// button.AddEffect(new HighlightEffect(HighlightStyle.Strip).SetThickness(3));
    /// </code>
    /// </example>
    public class HighlightEffect : ButtonEffect<HighlightEffect>
    {
        public HighlightEffect() : this(HighlightStyle.Band) { }

        public HighlightEffect(HighlightStyle style)
        {
            Style = style;
            Blend = ButtonEffectBlend.Normal;
            Intensity = 0.38f;
            SetStateIntensities(1f, 1.2f, 0.75f, 0.5f);
        }

        /// <summary>The shape of the reflection.</summary>
        public HighlightStyle Style { get; set; }

        /// <summary>The space between the reflection and the left and right edges (at most 15% of the width).</summary>
        public float Inset { get; set; } = 8f;

        /// <summary>
        /// The space between the reflection and the top edge (or the bottom edge for <see cref="HighlightStyle.Bottom"/>),
        /// at most 12% of the height.
        /// </summary>
        public float TopInset { get; set; } = 4f;

        /// <summary>The height of the band, as a part of the height of the button.</summary>
        public float HeightRate { get; set; } = 0.42f;

        /// <summary>The height of a <see cref="HighlightStyle.Strip"/>.</summary>
        public float Thickness { get; set; } = 3f;

        /// <summary>How soft the edge of the band is, as a part of its height (0 = sharp, 0.5 = very soft).</summary>
        public float Softness { get; set; } = 0.12f;

        /// <summary>The roundness of the ends of the band, from 0 (square) to 1 (round).</summary>
        public float Roundness { get; set; } = 1f;

        /// <summary>How much the band fades towards the middle of the button (0 = flat, 1 = it vanishes).</summary>
        public float Fade { get; set; } = 0.55f;

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Inside;

        public HighlightEffect SetStyle(HighlightStyle style)
        {
            Style = style;
            return this;
        }

        /// <summary>Sets the space between the reflection and the sides, and the top (or bottom) edge.</summary>
        public HighlightEffect SetInset(float inset, float topInset)
        {
            Inset = inset;
            TopInset = topInset;
            return this;
        }

        public HighlightEffect SetHeightRate(float heightRate)
        {
            HeightRate = heightRate;
            return this;
        }

        public HighlightEffect SetThickness(float thickness)
        {
            Thickness = thickness;
            return this;
        }

        /// <summary>Sets <see cref="Softness"/>, as a part of the height of the band (0 to 0.5).</summary>
        public HighlightEffect SetSoftness(float softness)
        {
            Softness = softness;
            return this;
        }

        public HighlightEffect SetRoundness(float roundness)
        {
            Roundness = roundness;
            return this;
        }

        public HighlightEffect SetFade(float fade)
        {
            Fade = fade;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var bounds = context.Bounds;
            var inset = Math.Min(Math.Max(0f, Inset), bounds.Width * 0.15f);
            var topInset = Math.Min(Math.Max(0f, TopInset), bounds.Height * 0.12f);
            var width = bounds.Width - inset * 2f;
            var height = Style == HighlightStyle.Strip
                ? Math.Min(Math.Max(1f, Thickness), bounds.Height / 3f)
                : (bounds.Height - topInset) * MathUtils.Clamp01(HeightRate);
            if (width <= 0f || height <= 0f)
                return;

            var fromBottom = Style == HighlightStyle.Bottom;
            var y = fromBottom ? bounds.Bottom - topInset - height : bounds.Y + topInset;
            var area = new RectangleF(bounds.X + inset, y, width, height);
            var tint = GetTint(context);

            if (Style == HighlightStyle.Ellipse)
            {
                var glow = ButtonEffectResources.GetShape(LightShape.Glow);
                spriteBatch.Draw(glow, area.Center, null, tint, 0f, new Vector2(glow.Width / 2f, glow.Height / 2f),
                    new Vector2(area.Width * 1.25f / glow.Width, area.Height * 1.6f / glow.Height), SpriteEffects.None, 0f);
                return;
            }

            // A strip is a thin flat band; the band fades towards the middle of the button.
            var strip = Style == HighlightStyle.Strip;
            var texture = ButtonEffectResources.GetBand(Roundness, strip ? 0.3f : Softness, strip ? 0f : Fade, fromBottom, out var cap);
            var capScale = height / ButtonEffectResources.BandHeight;
            var capWidth = Math.Min(cap * capScale, width / 2f);
            spriteBatch.DrawNineSlice(texture, null, new Thickness(cap, 0f, cap, 0f), area, new Thickness(capWidth, 0f, capWidth, 0f), tint);
        }
    }
}
