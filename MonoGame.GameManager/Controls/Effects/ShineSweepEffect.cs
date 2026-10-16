using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// A diagonal band of light that crosses the button from time to time, like light reflected on glass or metal.
    /// It can also cross it once when the pointer enters the button (<see cref="SweepOnHover"/>).
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new ShineSweepEffect().SetAngle(25).SetWidth(30).SetTiming(0.6f, 2.5f));
    /// </code>
    /// </example>
    public class ShineSweepEffect : ButtonEffect<ShineSweepEffect>
    {
        private const float SecondaryBandDistance = 1.7f;
        private float hoverSweepTime = -1f;

        public ShineSweepEffect()
        {
            Intensity = 0.7f;
        }

        /// <summary>The tilt of the band, in degrees (0 = vertical; positive values lean the top to the right).</summary>
        public float AngleDegrees { get; set; } = 20f;

        /// <summary>The width of the band.</summary>
        public float Width { get; set; } = 36f;

        /// <summary>How long the band takes to cross the button, in seconds.</summary>
        public float Duration { get; set; } = 0.7f;

        /// <summary>The time between two sweeps, in seconds (a negative value disables the automatic sweeps).</summary>
        public float Interval { get; set; } = 2.5f;

        /// <summary>When true, the band crosses the button from right to left.</summary>
        public bool Reverse { get; set; }

        /// <summary>The width of a thinner band that follows the main one, as a part of its width (0 = no second band).</summary>
        public float SecondaryBandRate { get; set; } = 0.35f;

        /// <summary>When true, the band also crosses the button when the pointer enters it.</summary>
        public bool SweepOnHover { get; set; }

        /// <summary>The movement of the band.</summary>
        public EasingFunction Easing { get; set; } = Animations.Easing.SineInOut;

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Inside;

        /// <summary>Sets <see cref="AngleDegrees"/>.</summary>
        public ShineSweepEffect SetAngle(float angleDegrees)
        {
            AngleDegrees = angleDegrees;
            return this;
        }

        public ShineSweepEffect SetWidth(float width)
        {
            Width = width;
            return this;
        }

        /// <summary>Sets the duration of a sweep and the time between two sweeps (negative = only on hover).</summary>
        public ShineSweepEffect SetTiming(float duration, float interval)
        {
            Duration = duration;
            Interval = interval;
            return this;
        }

        public ShineSweepEffect SetReverse(bool reverse)
        {
            Reverse = reverse;
            return this;
        }

        public ShineSweepEffect SetSecondaryBandRate(float secondaryBandRate)
        {
            SecondaryBandRate = secondaryBandRate;
            return this;
        }

        public ShineSweepEffect SetSweepOnHover(bool sweepOnHover)
        {
            SweepOnHover = sweepOnHover;
            return this;
        }

        public ShineSweepEffect SetEasing(EasingFunction easing)
        {
            Easing = easing;
            return this;
        }

        /// <summary>Starts a sweep now.</summary>
        public void Sweep() => hoverSweepTime = 0f;

        /// <inheritdoc />
        public override void Update(ButtonEffectContext context)
        {
            base.Update(context);
            if (hoverSweepTime >= 0f)
            {
                hoverSweepTime += context.DeltaSeconds;
                if (hoverSweepTime > Math.Max(0.01f, Duration))
                    hoverSweepTime = -1f;
            }
        }

        /// <inheritdoc />
        public override void OnStateChanged(ButtonEffectContext context, ButtonVisualState previousState)
        {
            if (SweepOnHover && context.State == ButtonVisualState.Hover && previousState == ButtonVisualState.Normal)
                Sweep();
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var duration = Math.Max(0.01f, Duration);
            float progress;
            if (hoverSweepTime >= 0f)
                progress = hoverSweepTime / duration;
            else if (Interval >= 0f)
            {
                var time = MathUtils.Wrap(GetLocalTime(context), 0f, duration + Interval);
                if (time > duration)
                    return;
                progress = time / duration;
            }
            else
                return;

            progress = MathUtils.Clamp01(Easing != null ? Easing(MathUtils.Clamp01(progress)) : progress);
            if (Reverse)
                progress = 1f - progress;

            var bounds = context.Bounds;
            var angle = MathHelper.ToRadians(MathUtils.Clamp(AngleDegrees, -80f, 80f));
            var width = Math.Max(1f, Width);
            var slant = Math.Abs(bounds.Height * (float)Math.Tan(angle)) / 2f;
            var travel = slant + width * (1f + (SecondaryBandRate > 0f ? SecondaryBandDistance : 0f));
            var x = MathUtils.Lerp(bounds.X - travel, bounds.Right + travel, progress);
            var length = bounds.Height / (float)Math.Cos(angle) + width;
            var tint = GetTint(context);

            DrawBand(spriteBatch, x, bounds.Center.Y, width, length, angle, tint);
            if (SecondaryBandRate > 0f)
            {
                var behind = Reverse ? 1f : -1f;
                DrawBand(spriteBatch, x + behind * width * SecondaryBandDistance, bounds.Center.Y, width * SecondaryBandRate, length, angle, tint * 0.8f);
            }
        }

        private static void DrawBand(SpriteBatch spriteBatch, float x, float y, float width, float length, float angle, Color color)
        {
            var band = ButtonEffectResources.SoftBand;
            spriteBatch.Draw(band, new Vector2(x, y), null, color, angle, new Vector2(band.Width / 2f, 0.5f),
                new Vector2(width / band.Width, length), SpriteEffects.None, 0f);
        }
    }
}
