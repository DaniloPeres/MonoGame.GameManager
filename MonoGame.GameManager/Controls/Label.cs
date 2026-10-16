using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Text;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A single line of text (use <see cref="MultiLineLabel"/> for text that wraps).
    /// </summary>
    public class Label : ScalableControlAbstract<Label>
    {
        private SpriteFontBase font;
        private string text = string.Empty;
        private Color outlineColor = Color.Black;
        private float outlineThickness;
        private float characterSpacing;
        private RectangleF contentBounds;
        private int contentBoundsKey;

        public Label(SpriteFontBase font, string text, Vector2 position, Color color)
        {
            Font = font;
            Text = text;
            SetPosition(position);
            SetColor(color);
        }

        /// <summary>The font of the text (a size of a FontStashSharp <see cref="FontSystem"/>).</summary>
        public SpriteFontBase Font
        {
            get => font;
            set
            {
                font = value;
                MarkAsDirty();
            }
        }

        /// <summary>The text (never null).</summary>
        public string Text
        {
            get => text;
            set
            {
                value = value ?? string.Empty;
                if (value == text)
                    return;
                text = value;
                MarkAsDirty();
            }
        }

        /// <summary>The color of the outline drawn around the glyphs.</summary>
        public Color OutlineColor
        {
            get => outlineColor;
            set
            {
                outlineColor = value;
                MarkAsDirty();
            }
        }

        /// <summary>
        /// The thickness of the outline in font pixels (0 = no outline). It grows with the scale of the label and is
        /// rounded to whole pixels of the font drawn. The outline does not change the size of the label.
        /// </summary>
        public float OutlineThickness
        {
            get => outlineThickness;
            set
            {
                outlineThickness = System.Math.Max(0f, value);
                MarkAsDirty();
            }
        }

        /// <summary>
        /// Extra space between the characters, in font pixels (0 = the spacing of the font, negative values bring them
        /// closer). It grows with the scale of the label and is part of its size.
        /// </summary>
        public float CharacterSpacing
        {
            get => characterSpacing;
            set
            {
                if (value == characterSpacing)
                    return;
                characterSpacing = value;
                MarkAsDirty();
            }
        }

        public Label SetText(string text)
        {
            Text = text;
            return this;
        }

        public Label SetFont(SpriteFontBase font)
        {
            Font = font;
            return this;
        }

        /// <summary>Sets the extra space between the characters, in font pixels (letter spacing).</summary>
        public Label SetCharacterSpacing(float characterSpacing)
        {
            CharacterSpacing = characterSpacing;
            return this;
        }

        /// <summary>
        /// Draws an outline around the glyphs (thickness in font pixels, 0 = no outline). The outline is part of the
        /// silhouette used by the shading effects: with a <see cref="Shading.GradientFill"/>, use an
        /// <see cref="Shading.Outline"/> effect instead, or the fill covers the outline too.
        /// </summary>
        public Label SetOutline(Color color, float thickness)
        {
            OutlineColor = color;
            OutlineThickness = thickness;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (font == null || text.Length == 0)
                return;
            var scale = NestedScale;
            var drawFont = FontScaling.Resolve(font, ref scale, out var originFactor);
            var origin = OriginWithoutScale * originFactor;
            var position = GetPosition();
            // The spacing is in pixels of the font drawn, which can be a bigger size of the same font.
            var spacing = characterSpacing * originFactor;

            if (outlineThickness > 0f)
                TextOutline.Draw(spriteBatch, drawFont, text, position, outlineColor * NestedOpacity, outlineThickness * originFactor, Rotation, origin, scale, LayerDepthDraw, spacing);

            drawFont.DrawText(spriteBatch, text, position, DrawColor, Rotation, origin, scale, LayerDepthDraw, spacing);
        }

        /// <inheritdoc />
        protected override int? GetContentSignature() => System.HashCode.Combine(font, text, outlineColor, outlineThickness, characterSpacing);

        /// <summary>The box of the glyphs (without the space above and under them), in local units.</summary>
        protected override RectangleF GetContentBounds()
        {
            if (font == null || text.Length == 0)
                return base.GetContentBounds();

            // Measured again only when the text changes (the shading asks for it every frame).
            var key = System.HashCode.Combine(font, text, characterSpacing);
            if (key != contentBoundsKey || contentBounds.IsEmpty)
            {
                contentBounds = TextInk.GetBounds(font, text, Vector2.Zero, characterSpacing);
                contentBoundsKey = key;
            }

            return contentBounds.IsEmpty ? base.GetContentBounds() : contentBounds;
        }

        protected override Vector2 CalculateSize() => font == null || text.Length == 0
            ? Vector2.Zero
            : new Vector2(font.MeasureString(text, characterSpacing: characterSpacing).X, font.LineHeight);
    }
}
