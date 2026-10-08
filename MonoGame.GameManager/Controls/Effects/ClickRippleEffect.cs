using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// A ring of light that grows from where the button is pressed, with a short flash of the whole button. It is
    /// clipped to the shape of the button.
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new ClickRippleEffect().SetColor(Color.White).SetDuration(0.5f));
    /// </code>
    /// </example>
    public class ClickRippleEffect : ButtonEffect<ClickRippleEffect>
    {
        private const int Capacity = 8;
        private readonly Vector2[] origins = new Vector2[Capacity];
        private readonly float[] ages = new float[Capacity];
        private readonly float[] radii = new float[Capacity];
        private int next;

        public ClickRippleEffect()
        {
            Intensity = 0.8f;
            for (var i = 0; i < Capacity; i++)
                ages[i] = -1f;
        }

        /// <summary>How long a ripple lasts, in seconds.</summary>
        public float Duration { get; set; } = 0.55f;

        /// <summary>The final size of a ripple, as a part of the distance to the farthest corner of the button.</summary>
        public float RadiusRate { get; set; } = 1f;

        /// <summary>The strength of the flash of the whole button (0 = no flash).</summary>
        public float FlashIntensity { get; set; } = 0.3f;

        /// <summary>The strength of the soft light inside the ring (0 = only the ring).</summary>
        public float FillIntensity { get; set; } = 0.35f;

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Inside;

        public ClickRippleEffect SetDuration(float duration)
        {
            Duration = duration;
            return this;
        }

        public ClickRippleEffect SetRadiusRate(float radiusRate)
        {
            RadiusRate = radiusRate;
            return this;
        }

        public ClickRippleEffect SetFlash(float flashIntensity, float fillIntensity = 0.35f)
        {
            FlashIntensity = flashIntensity;
            FillIntensity = fillIntensity;
            return this;
        }

        /// <summary>Starts a ripple at a point of the local space of the button.</summary>
        public void Ripple(ButtonEffectContext context, Vector2 localPosition)
        {
            var bounds = context.Bounds;
            var farthest = Math.Max(
                Math.Max(Vector2.Distance(localPosition, bounds.Position), Vector2.Distance(localPosition, new Vector2(bounds.Right, bounds.Y))),
                Math.Max(Vector2.Distance(localPosition, new Vector2(bounds.X, bounds.Bottom)), Vector2.Distance(localPosition, new Vector2(bounds.Right, bounds.Bottom))));
            origins[next] = localPosition;
            radii[next] = Math.Max(4f, farthest * RadiusRate);
            ages[next] = 0f;
            next = (next + 1) % Capacity;
        }

        /// <inheritdoc />
        public override void OnPressed(ButtonEffectContext context, Vector2 localPosition) => Ripple(context, localPosition);

        /// <inheritdoc />
        public override void Update(ButtonEffectContext context)
        {
            base.Update(context);
            var duration = Math.Max(0.01f, Duration);
            for (var i = 0; i < Capacity; i++)
            {
                if (ages[i] < 0f)
                    continue;
                ages[i] += context.DeltaSeconds;
                if (ages[i] >= duration)
                    ages[i] = -1f;
            }
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var duration = Math.Max(0.01f, Duration);
            Texture2D ring = null;
            Texture2D glow = null;
            var tint = GetTint(context);

            for (var i = 0; i < Capacity; i++)
            {
                if (ages[i] < 0f)
                    continue;

                ring ??= ButtonEffectResources.GetShape(LightShape.Ring);
                glow ??= ButtonEffectResources.GetShape(LightShape.Glow);
                var progress = MathUtils.Clamp01(ages[i] / duration);
                var fade = 1f - progress;
                var radius = radii[i] * Easing.CubicOut(progress);
                var origin = new Vector2(ring.Width / 2f, ring.Height / 2f);

                if (FlashIntensity > 0f)
                    spriteBatch.FillRectangle(context.Bounds, tint * (FlashIntensity * fade * fade));
                if (FillIntensity > 0f)
                    spriteBatch.Draw(glow, origins[i], null, tint * (FillIntensity * fade), 0f, origin, radius * 2.2f / glow.Width, SpriteEffects.None, 0f);
                spriteBatch.Draw(ring, origins[i], null, tint * fade, 0f, origin, radius * 2f / ring.Width, SpriteEffects.None, 0f);
            }
        }
    }
}
