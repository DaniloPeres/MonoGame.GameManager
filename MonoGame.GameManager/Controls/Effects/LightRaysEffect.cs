using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Enums;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// Rays of light that slowly turn behind the button, from its center or from a point of its outline (eg: a burst of
    /// light above the top edge), like a legendary reward.
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new LightRaysEffect().SetColor(Color.Gold).SetRays(12, 140, 16).SetRotationSpeed(15));
    /// button.AddEffect(new LightRaysEffect().SetAnchor(Anchor.TopCenter).SetRays(8, 60, 8));
    /// </code>
    /// </example>
    public class LightRaysEffect : ButtonEffect<LightRaysEffect>
    {
        public LightRaysEffect()
        {
            Intensity = 0.55f;
        }

        /// <summary>The number of rays.</summary>
        public int Count { get; set; } = 10;

        /// <summary>The length of the longest rays, from the center (0 = three quarters of the size of the button).</summary>
        public float Length { get; set; }

        /// <summary>The width of the rays.</summary>
        public float Width { get; set; } = 14f;

        /// <summary>The length of every other ray, as a part of <see cref="Length"/> (1 = all the same).</summary>
        public float ShortRayRate { get; set; } = 0.65f;

        /// <summary>The rotation speed, in degrees per second.</summary>
        public float RotationSpeed { get; set; } = 12f;

        /// <summary>Where the rays come from (<see cref="Anchor.Center"/> = the center of the button).</summary>
        public Anchor Anchor { get; set; } = Anchor.Center;

        /// <summary>How much the rays flicker (0 to 1).</summary>
        public float FlickerAmount { get; set; } = 0.3f;

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Behind;

        /// <summary>Sets the number, the length (0 = from the size of the button) and the width of the rays.</summary>
        public LightRaysEffect SetRays(int count, float length, float width)
        {
            Count = count;
            Length = length;
            Width = width;
            return this;
        }

        public LightRaysEffect SetShortRayRate(float shortRayRate)
        {
            ShortRayRate = shortRayRate;
            return this;
        }

        public LightRaysEffect SetRotationSpeed(float rotationSpeed)
        {
            RotationSpeed = rotationSpeed;
            return this;
        }

        public LightRaysEffect SetAnchor(Anchor anchor)
        {
            Anchor = anchor;
            return this;
        }

        public LightRaysEffect SetFlicker(float flickerAmount)
        {
            FlickerAmount = flickerAmount;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var count = Math.Max(0, Count);
            if (count == 0 || Width <= 0f)
                return;

            var bounds = context.Bounds;
            var length = Length > 0f ? Length : Math.Max(bounds.Width, bounds.Height) * 0.75f;
            var center = context.AnchorPoint(Anchor, out _);
            var glow = ButtonEffectResources.GetShape(LightShape.Glow);
            var origin = new Vector2(glow.Width / 2f, glow.Height / 2f);
            var time = GetLocalTime(context);
            var baseAngle = MathHelper.ToRadians(RotationSpeed * time);
            var tint = GetTint(context);
            var flicker = Math.Max(0f, Math.Min(1f, FlickerAmount));

            // Each glow is stretched into a ray that goes through the center: count glows make 2 * count rays.
            for (var i = 0; i < count; i++)
            {
                var rate = i % 2 == 0 ? 1f : ShortRayRate;
                var angle = baseAngle + i * MathHelper.Pi / count;
                var light = 1f - flicker * (0.5f + 0.5f * (float)Math.Sin(time * 2.3f + i * 1.7f));
                spriteBatch.Draw(glow, center, null, tint * light, angle, origin,
                    new Vector2(length * 2f * rate / glow.Width, Width / glow.Height), SpriteEffects.None, 0f);
            }
        }
    }
}
