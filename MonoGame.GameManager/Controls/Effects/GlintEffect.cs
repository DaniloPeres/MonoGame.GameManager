using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Enums;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// Small static reflections near a corner of the button: a white oval followed by one or two smaller dots, like the
    /// shine on glossy, candy and bubble buttons. They do not move (unless <see cref="TwinkleSpeed"/> is set).
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new GlintEffect().SetCorners(Anchor.TopLeft).SetDots(2, 7));
    /// button.AddEffect(new GlintEffect().SetCorners(Anchor.TopRight).SetDots(3, 5).SetShape(LightShape.Flare));
    /// </code>
    /// </example>
    public class GlintEffect : ButtonEffect<GlintEffect>
    {
        public GlintEffect()
        {
            // Painted white dots stay crisp on light colors too.
            Blend = ButtonEffectBlend.Normal;
            Intensity = 0.85f;
            SetStateIntensities(1f, 1.1f, 0.85f, 0.5f);
        }

        /// <summary>The corners with reflections (TopLeft, TopRight, BottomLeft, BottomRight).</summary>
        public IList<Anchor> Corners { get; set; } = new List<Anchor> { Anchor.TopLeft };

        /// <summary>The number of dots at each corner (1 to 3), each one smaller than the previous.</summary>
        public int Count { get; set; } = 2;

        /// <summary>The height of the biggest dot (at most 14% of the height of the button).</summary>
        public float Size { get; set; } = 7f;

        /// <summary>How much the dots are stretched into ovals (1 = circles).</summary>
        public float Stretch { get; set; } = 1.7f;

        /// <summary>The tilt of the ovals, in degrees (mirrored on the right corners).</summary>
        public float AngleDegrees { get; set; } = -25f;

        /// <summary>The distance between the dots, as a part of their size.</summary>
        public float Spacing { get; set; } = 1.1f;

        /// <summary>The distance between the first dot and the corner (at most 20% of the height of the button).</summary>
        public float Inset { get; set; } = 9f;

        /// <summary>The shape of the dots.</summary>
        public LightShape Shape { get; set; } = LightShape.Circle;

        /// <summary>The number of twinkles per second (0 = static).</summary>
        public float TwinkleSpeed { get; set; }

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Front;

        /// <summary>Sets the corners with reflections.</summary>
        public GlintEffect SetCorners(params Anchor[] corners)
        {
            Corners = new List<Anchor>(corners ?? Array.Empty<Anchor>());
            return this;
        }

        /// <summary>Sets the number of dots and the size of the biggest one.</summary>
        public GlintEffect SetDots(int count, float size)
        {
            Count = count;
            Size = size;
            return this;
        }

        /// <summary>Sets the stretch and the tilt of the ovals.</summary>
        public GlintEffect SetOval(float stretch, float angleDegrees)
        {
            Stretch = stretch;
            AngleDegrees = angleDegrees;
            return this;
        }

        public GlintEffect SetSpacing(float spacing)
        {
            Spacing = spacing;
            return this;
        }

        public GlintEffect SetInset(float inset)
        {
            Inset = inset;
            return this;
        }

        public GlintEffect SetShape(LightShape shape)
        {
            Shape = shape;
            return this;
        }

        public GlintEffect SetTwinkleSpeed(float twinkleSpeed)
        {
            TwinkleSpeed = twinkleSpeed;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var corners = Corners;
            var count = Math.Max(0, Math.Min(3, Count));
            if (corners == null || corners.Count == 0 || count == 0 || Size <= 0f)
                return;

            var bounds = context.Bounds;
            var texture = ButtonEffectResources.GetShape(Shape);
            var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            var tint = GetTint(context);
            var cornerOffset = context.CornerRadius * 0.29f; // the arc of a rounded corner at 45 degrees
            var twinkle = TwinkleSpeed > 0f ? 0.75f + 0.25f * (float)Math.Sin(GetLocalTime(context) * TwinkleSpeed * MathHelper.TwoPi) : 1f;

            for (var c = 0; c < corners.Count; c++)
            {
                var corner = corners[c];
                var right = corner == Anchor.TopRight || corner == Anchor.BottomRight || corner == Anchor.CenterRight;
                var bottom = corner == Anchor.BottomLeft || corner == Anchor.BottomRight || corner == Anchor.BottomCenter;
                var inset = Math.Min(Inset, bounds.Height * 0.2f) + cornerOffset;
                var position = new Vector2(right ? bounds.Right - inset : bounds.X + inset, bottom ? bounds.Bottom - inset : bounds.Y + inset);
                var direction = new Vector2(right ? -1f : 1f, 0f); // the next dots go towards the center
                var angle = MathHelper.ToRadians(right ^ bottom ? -AngleDegrees : AngleDegrees);
                var size = Math.Min(Size, bounds.Height * 0.14f); // small buttons get small reflections

                for (var i = 0; i < count; i++)
                {
                    var stretch = i == 0 ? Math.Max(1f, Stretch) : 1f;
                    spriteBatch.Draw(texture, position, null, tint * (i == 0 ? twinkle : 1f), angle, origin,
                        new Vector2(size * stretch / texture.Width, size / texture.Height) * (i == 0 ? twinkle : 1f), SpriteEffects.None, 0f);

                    var step = size * stretch / 2f;
                    size *= 0.55f;
                    position += direction * (step + size / 2f + size * Spacing);
                }
            }
        }
    }
}
