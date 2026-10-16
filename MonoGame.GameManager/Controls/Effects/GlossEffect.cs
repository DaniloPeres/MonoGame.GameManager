using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// A glossy reflection: light at the top of the button that fades towards the middle (or at the bottom, with
    /// <see cref="FromBottom"/>), like polished glass or enamel.
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new GlossEffect().SetHeightRate(0.45f).SetIntensity(0.5f));
    /// </code>
    /// </example>
    public class GlossEffect : ButtonEffect<GlossEffect>
    {
        public GlossEffect()
        {
            Intensity = 0.45f;
        }

        /// <summary>The height of the reflection, as a part of the height of the button (0 to 1).</summary>
        public float HeightRate { get; set; } = 0.5f;

        /// <summary>1 is a linear fade, higher values fade faster.</summary>
        public float Falloff { get; set; } = 1.5f;

        /// <summary>Space left on the left, the right and the top (or bottom) of the reflection.</summary>
        public float Inset { get; set; }

        /// <summary>When true, the light starts at the bottom of the button and fades upwards.</summary>
        public bool FromBottom { get; set; }

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Inside;

        public GlossEffect SetHeightRate(float heightRate)
        {
            HeightRate = heightRate;
            return this;
        }

        public GlossEffect SetFalloff(float falloff)
        {
            Falloff = falloff;
            return this;
        }

        public GlossEffect SetInset(float inset)
        {
            Inset = inset;
            return this;
        }

        public GlossEffect SetFromBottom(bool fromBottom)
        {
            FromBottom = fromBottom;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var bounds = context.Bounds;
            var width = bounds.Width - Inset * 2f;
            var height = (bounds.Height - Inset) * Math.Max(0f, Math.Min(1f, HeightRate));
            if (width <= 0f || height <= 0f)
                return;

            var texture = ButtonEffectResources.GetVerticalGradient(Falloff);
            var y = FromBottom ? bounds.Bottom - Inset - height : bounds.Y + Inset;
            spriteBatch.Draw(texture, new Vector2(bounds.X + Inset, y), null, GetTint(context), 0f, Vector2.Zero,
                new Vector2(width / texture.Width, height / texture.Height), FromBottom ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
        }
    }
}
