using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Enums;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// Points of light placed on the outline of the button: gems on the middle of the top edge, sparkles on the corners,
    /// lights on the sides... Each light is a soft glow with a bright shape in its center, that twinkles and can have a
    /// horizontal streak.
    /// </summary>
    /// <example>
    /// <code>
    /// // A gem on the top and sparkles on the four corners
    /// button.AddEffect(new CornerLightEffect().SetAnchors(Anchor.TopCenter).SetShape(LightShape.Diamond).SetStreak(90, 4));
    /// button.AddEffect(new CornerLightEffect().SetAnchors(Anchor.TopLeft, Anchor.TopRight, Anchor.BottomLeft, Anchor.BottomRight));
    /// </code>
    /// </example>
    public class CornerLightEffect : ButtonEffect<CornerLightEffect>
    {
        /// <summary>The four corners.</summary>
        public static readonly Anchor[] Corners = { Anchor.TopLeft, Anchor.TopRight, Anchor.BottomRight, Anchor.BottomLeft };

        /// <summary>The middle of the four edges.</summary>
        public static readonly Anchor[] Edges = { Anchor.TopCenter, Anchor.CenterRight, Anchor.BottomCenter, Anchor.CenterLeft };

        /// <summary>The four corners and the middle of the four edges.</summary>
        public static readonly Anchor[] All =
        {
            Anchor.TopLeft, Anchor.TopCenter, Anchor.TopRight, Anchor.CenterRight,
            Anchor.BottomRight, Anchor.BottomCenter, Anchor.BottomLeft, Anchor.CenterLeft
        };

        private const float GoldenAngle = 2.39996f;

        /// <summary>Where the lights are (see <see cref="Corners"/>, <see cref="Edges"/> and <see cref="All"/>).</summary>
        public IList<Anchor> Anchors { get; set; } = new List<Anchor> { Anchor.TopCenter };

        /// <summary>The shape in the center of each light.</summary>
        public LightShape Shape { get; set; } = LightShape.Flare;

        /// <summary>The size of the shape.</summary>
        public float Size { get; set; } = 16f;

        /// <summary>The size of the soft glow around the shape (0 = no glow).</summary>
        public float GlowSize { get; set; } = 40f;

        /// <summary>The color of the shape (the glow uses <see cref="ButtonEffect.Color"/>).</summary>
        public Color CoreColor { get; set; } = Color.White;

        /// <summary>The number of twinkles per second.</summary>
        public float TwinkleSpeed { get; set; } = 1.2f;

        /// <summary>How much the lights shrink and dim when they twinkle (0 to 1).</summary>
        public float TwinkleAmount { get; set; } = 0.35f;

        /// <summary>The length of a horizontal streak of light through each light (0 = no streak).</summary>
        public float StreakLength { get; set; }

        /// <summary>The thickness of the streak.</summary>
        public float StreakThickness { get; set; } = 4f;

        /// <summary>The rotation speed of the shapes, in degrees per second.</summary>
        public float SpinSpeed { get; set; }

        /// <summary>Moves the lights away from the outline (positive values outwards, negative inwards).</summary>
        public float Offset { get; set; }

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Front;

        /// <summary>Sets where the lights are.</summary>
        public CornerLightEffect SetAnchors(params Anchor[] anchors)
        {
            Anchors = new List<Anchor>(anchors ?? Array.Empty<Anchor>());
            return this;
        }

        public CornerLightEffect SetShape(LightShape shape)
        {
            Shape = shape;
            return this;
        }

        /// <summary>Sets the size of the shape and of its glow.</summary>
        public CornerLightEffect SetSize(float size, float glowSize)
        {
            Size = size;
            GlowSize = glowSize;
            return this;
        }

        public CornerLightEffect SetCoreColor(Color coreColor)
        {
            CoreColor = coreColor;
            return this;
        }

        public CornerLightEffect SetTwinkle(float speed, float amount)
        {
            TwinkleSpeed = speed;
            TwinkleAmount = amount;
            return this;
        }

        /// <summary>Sets a horizontal streak of light through each light (length 0 = no streak).</summary>
        public CornerLightEffect SetStreak(float length, float thickness = 4f)
        {
            StreakLength = length;
            StreakThickness = thickness;
            return this;
        }

        public CornerLightEffect SetSpinSpeed(float spinSpeed)
        {
            SpinSpeed = spinSpeed;
            return this;
        }

        public CornerLightEffect SetOffset(float offset)
        {
            Offset = offset;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var anchors = Anchors;
            if (anchors == null || anchors.Count == 0)
                return;

            var time = GetLocalTime(context);
            var tint = GetTint(context);
            var core = ApplyIntensity(CoreColor, context);
            var glow = ButtonEffectResources.GetShape(LightShape.Glow);
            var shape = ButtonEffectResources.GetShape(Shape);
            var glowOrigin = new Vector2(glow.Width / 2f, glow.Height / 2f);
            var shapeOrigin = new Vector2(shape.Width / 2f, shape.Height / 2f);
            var amount = Math.Max(0f, Math.Min(1f, TwinkleAmount));

            for (var i = 0; i < anchors.Count; i++)
            {
                var position = context.AnchorPoint(anchors[i], out var normal) + normal * Offset;
                var twinkle = 1f - amount * (0.5f + 0.5f * (float)Math.Sin(time * TwinkleSpeed * MathHelper.TwoPi + i * GoldenAngle));
                var rotation = SpinSpeed != 0f ? MathHelper.ToRadians(SpinSpeed * time) + i * GoldenAngle : 0f;

                if (GlowSize > 0f)
                    spriteBatch.Draw(glow, position, null, tint * twinkle, 0f, glowOrigin, GlowSize * twinkle / glow.Width, SpriteEffects.None, 0f);
                if (StreakLength > 0f && StreakThickness > 0f)
                    spriteBatch.Draw(glow, position, null, tint * twinkle, 0f, glowOrigin,
                        new Vector2(StreakLength / glow.Width, StreakThickness * 2f / glow.Height) * (0.6f + 0.4f * twinkle), SpriteEffects.None, 0f);
                if (Size > 0f)
                    spriteBatch.Draw(shape, position, null, core * (0.5f + 0.5f * twinkle), rotation, shapeOrigin,
                        Size * (0.75f + 0.25f * twinkle) / shape.Width, SpriteEffects.None, 0f);
            }
        }
    }
}
