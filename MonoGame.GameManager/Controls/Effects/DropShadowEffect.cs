using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// A soft shadow under the button (painted, not a light), that follows its shape and corner radius.
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new DropShadowEffect().SetOffset(new Vector2(0, 5)).SetBlur(10).SetIntensity(0.5f));
    /// </code>
    /// </example>
    public class DropShadowEffect : ButtonEffect<DropShadowEffect>
    {
        public DropShadowEffect()
        {
            Blend = ButtonEffectBlend.Normal;
            Color = Color.Black;
            Intensity = 0.45f;
            IgnoreStates();
        }

        /// <summary>How far the shadow is moved from the button.</summary>
        public Vector2 Offset { get; set; } = new Vector2(0f, 4f);

        /// <summary>How soft the edge of the shadow is.</summary>
        public float Blur { get; set; } = 8f;

        /// <summary>Makes the shadow bigger (or smaller, when negative) than the button.</summary>
        public float Spread { get; set; }

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Behind;

        public DropShadowEffect SetOffset(Vector2 offset)
        {
            Offset = offset;
            return this;
        }

        public DropShadowEffect SetBlur(float blur)
        {
            Blur = blur;
            return this;
        }

        public DropShadowEffect SetSpread(float spread)
        {
            Spread = spread;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var shape = context.Bounds.Inflate(Spread, Spread).Offset(Offset);
            if (shape.Width <= 0f || shape.Height <= 0f)
                return;

            var radius = Math.Max(0f, context.CornerRadius + Spread);
            ButtonEffectResources.GetSoftFill(radius, Math.Max(0f, Blur), context.TextureDensity).Draw(spriteBatch, shape, GetTint(context));
        }
    }
}
