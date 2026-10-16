using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Text;
using System;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// An outline and a shadow under the text of the button (the label created by <c>SetText</c>), like the bold
    /// outlined titles of casual games. It is painted under the text, and it is not drawn while the button or the label
    /// is rotated (labels do not rotate with their button).
    /// </summary>
    /// <example>
    /// <code>
    /// button.AddEffect(new TextOutlineEffect().SetOutline(new Color(90, 40, 0), 2).SetShadow(new Vector2(0, 3), Color.Black * 0.4f));
    /// </code>
    /// </example>
    public class TextOutlineEffect : ButtonEffect<TextOutlineEffect>
    {
        public TextOutlineEffect()
        {
            Blend = ButtonEffectBlend.Normal;
            Color = new Color(30, 30, 30);
            IgnoreStates();
        }

        /// <summary>The thickness of the outline in pixels (0 = no outline), rounded to whole font pixels.</summary>
        public float Thickness { get; set; } = 2f;

        /// <summary>How far the shadow is moved from the text.</summary>
        public Vector2 ShadowOffset { get; set; } = new Vector2(0f, 3f);

        /// <summary>The color of the shadow (transparent = no shadow).</summary>
        public Color ShadowColor { get; set; } = Color.Black * 0.35f;

        /// <inheritdoc />
        public override ButtonEffectLayer Layer => ButtonEffectLayer.Inside;

        /// <summary>Sets the color and the thickness of the outline.</summary>
        public TextOutlineEffect SetOutline(Color color, float thickness)
        {
            Color = color;
            Thickness = thickness;
            return this;
        }

        /// <summary>Sets the offset and the color of the shadow (transparent = no shadow).</summary>
        public TextOutlineEffect SetShadow(Vector2 offset, Color color)
        {
            ShadowOffset = offset;
            ShadowColor = color;
            return this;
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch spriteBatch, ButtonEffectContext context)
        {
            var label = context.TextLabel;
            if (label == null || !label.IsVisible || label.Font == null || string.IsNullOrEmpty(label.Text)
                || context.Rotation != 0f || label.Rotation != 0f || context.Scale.X <= 0f || context.Scale.Y <= 0f)
                return;

            var text = label.Text;
            var position = context.ToLocalPosition(label.GetPosition());
            var scale = label.NestedScale / context.Scale;
            var font = FontScaling.Resolve(label.Font, ref scale, out var originFactor);
            var origin = label.OriginWithoutScale * originFactor;
            var spacing = label.CharacterSpacing * originFactor;
            var opacity = label.NestedOpacity;

            if (ShadowColor.A > 0 && ShadowOffset != Vector2.Zero)
                font.DrawText(spriteBatch, text, position + ShadowOffset, ApplyIntensity(ShadowColor, context) * opacity, 0f, origin, scale, 0f, spacing);

            if (Thickness <= 0f || scale.X == 0f)
                return;

            TextOutline.Draw(spriteBatch, font, text, position, GetTint(context) * opacity, Thickness / Math.Abs(scale.X), 0f, origin, scale, 0f, spacing);
        }
    }
}
