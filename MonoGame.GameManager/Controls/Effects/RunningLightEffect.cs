using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// Lights that run around the outline of the button, each one with a fading tail, like a comet chasing the border.
    /// The speed is in local units per second, so the lights move the same way on a small and on a big button.
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new RunningLightEffect().SetColor(Color.Cyan).SetCount(2).SetSpeed(260));
    /// </code>
    /// </example>
    public class RunningLightEffect : ButtonEffect<RunningLightEffect>
    {
        /// <summary>The number of lights, spread evenly around the outline.</summary>
        public int Count { get; set; } = 1;

        /// <summary>The speed of the lights, in local units per second.</summary>
        public float Speed { get; set; } = 220f;

        /// <summary>The size of the head of a light.</summary>
        public float Size { get; set; } = 14f;

        /// <summary>The length of the tail of a light.</summary>
        public float TailLength { get; set; } = 70f;

        /// <summary>The number of glows that make the tail (more is smoother).</summary>
        public int TailSteps { get; set; } = 12;

        /// <summary>The shape of the bright center of the head (it uses <see cref="CoreColor"/>).</summary>
        public LightShape HeadShape { get; set; } = LightShape.Flare;

        /// <summary>The color of the center of the head.</summary>
        public Color CoreColor { get; set; } = Color.White;

        /// <summary>When true, the lights run counterclockwise.</summary>
        public bool Reverse { get; set; }

        /// <summary>Moves the path of the lights away from the outline (positive values outwards, negative inwards).</summary>
        public float Offset { get; set; }

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Front;

        public RunningLightEffect SetCount(int count)
        {
            Count = count;
            return this;
        }

        public RunningLightEffect SetSpeed(float speed)
        {
            Speed = speed;
            return this;
        }

        public RunningLightEffect SetSize(float size)
        {
            Size = size;
            return this;
        }

        /// <summary>Sets the length of the tail and the number of glows that make it.</summary>
        public RunningLightEffect SetTail(float length, int steps = 12)
        {
            TailLength = length;
            TailSteps = steps;
            return this;
        }

        public RunningLightEffect SetHeadShape(LightShape headShape)
        {
            HeadShape = headShape;
            return this;
        }

        public RunningLightEffect SetCoreColor(Color coreColor)
        {
            CoreColor = coreColor;
            return this;
        }

        public RunningLightEffect SetReverse(bool reverse)
        {
            Reverse = reverse;
            return this;
        }

        public RunningLightEffect SetOffset(float offset)
        {
            Offset = offset;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var count = Math.Max(0, Count);
            var length = context.PerimeterLength;
            if (count == 0 || length <= 0f || Size <= 0f)
                return;

            var glow = ButtonEffectResources.GetShape(LightShape.Glow);
            var head = ButtonEffectResources.GetShape(HeadShape);
            var glowOrigin = new Vector2(glow.Width / 2f, glow.Height / 2f);
            var headOrigin = new Vector2(head.Width / 2f, head.Height / 2f);
            var tint = GetTint(context);
            var core = ApplyIntensity(CoreColor, context);
            var direction = Reverse ? -1f : 1f;
            var time = GetLocalTime(context);
            var steps = Math.Max(0, TailSteps);
            var tailLength = Math.Max(0f, Math.Min(TailLength, length));

            for (var light = 0; light < count; light++)
            {
                var headDistance = direction * time * Speed + light * length / count;

                // The tail first, from its end to the head, so the head is drawn on top.
                for (var step = steps; step >= 1; step--)
                {
                    var rate = step / (float)(steps + 1);
                    var position = context.PointOnBorder(headDistance - direction * tailLength * rate, out var normal) + normal * Offset;
                    var fade = (1f - rate) * (1f - rate);
                    var size = Size * 2f * (1f - 0.55f * rate);
                    spriteBatch.Draw(glow, position, null, tint * fade, 0f, glowOrigin, size / glow.Width, SpriteEffects.None, 0f);
                }

                var headPosition = context.PointOnBorder(headDistance, out var headNormal) + headNormal * Offset;
                spriteBatch.Draw(glow, headPosition, null, tint, 0f, glowOrigin, Size * 2.6f / glow.Width, SpriteEffects.None, 0f);
                spriteBatch.Draw(head, headPosition, null, core, time * 2f, headOrigin, Size / head.Width, SpriteEffects.None, 0f);
            }
        }
    }
}
