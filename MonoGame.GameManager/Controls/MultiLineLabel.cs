using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Text;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A text that wraps to fit <see cref="TextBoxWidth"/>, aligned to the left, the center or the right.
    /// </summary>
    /// <remarks>
    /// <see cref="TextBoxWidth"/> is in the units of the parent: a label scaled down fits more words per line.
    /// Line breaks ("\n") are kept. The lines are calculated again only when the text, the font, the width, the
    /// alignment or the scale change.
    /// </remarks>
    public class MultiLineLabel : ScalableControlAbstract<MultiLineLabel>
    {
        private const float MinimumScaleToWrap = 0.0001f;
        private readonly List<string> lines = new List<string>();
        private readonly List<float> lineWidths = new List<float>();
        private SpriteFontBase font;
        private string text = string.Empty;
        private TextAlign textAlign;
        private int textBoxWidth;
        private float lineSpacing;
        private float characterSpacing;
        private Color outlineColor = Color.Black;
        private float outlineThickness;
        private bool areLinesDirty = true;
        private float wrappedScaleX = float.NaN;

        public MultiLineLabel(SpriteFontBase font, string text, Vector2 position, Color color, int textBoxWidth)
        {
            Font = font;
            Text = text;
            SetPosition(position);
            Color = color;
            TextBoxWidth = textBoxWidth;
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
                MarkLinesAsDirty();
            }
        }

        /// <summary>The font of the text (a size of a FontStashSharp <see cref="FontSystem"/>).</summary>
        public SpriteFontBase Font
        {
            get => font;
            set
            {
                font = value;
                MarkLinesAsDirty();
            }
        }

        public TextAlign TextAlign
        {
            get => textAlign;
            set
            {
                textAlign = value;
                MarkAsDirty();
            }
        }

        /// <summary>The maximum width of a line, in the units of the parent (0 = no wrapping).</summary>
        public int TextBoxWidth
        {
            get => textBoxWidth;
            set
            {
                textBoxWidth = value;
                MarkLinesAsDirty();
            }
        }

        /// <summary>Extra space between two lines, in font pixels (can be negative).</summary>
        public float LineSpacing
        {
            get => lineSpacing;
            set
            {
                lineSpacing = value;
                MarkAsDirty();
            }
        }

        /// <summary>
        /// Extra space between the characters, in font pixels (0 = the spacing of the font, negative values bring them
        /// closer). The lines are wrapped with it.
        /// </summary>
        public float CharacterSpacing
        {
            get => characterSpacing;
            set
            {
                if (value == characterSpacing)
                    return;
                characterSpacing = value;
                MarkLinesAsDirty();
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
                outlineThickness = Math.Max(0f, value);
                MarkAsDirty();
            }
        }

        /// <summary>The lines after wrapping.</summary>
        public IReadOnlyList<string> Lines
        {
            get
            {
                EnsureLines();
                return lines;
            }
        }

        public MultiLineLabel SetText(string text)
        {
            Text = text;
            return this;
        }

        public MultiLineLabel SetFont(SpriteFontBase font)
        {
            Font = font;
            return this;
        }

        public MultiLineLabel SetTextAlign(TextAlign textAlign)
        {
            TextAlign = textAlign;
            return this;
        }

        public MultiLineLabel SetTextBoxWidth(int textBoxWidth)
        {
            TextBoxWidth = textBoxWidth;
            return this;
        }

        public MultiLineLabel SetLineSpacing(float lineSpacing)
        {
            LineSpacing = lineSpacing;
            return this;
        }

        /// <summary>Sets the extra space between the characters, in font pixels (letter spacing).</summary>
        public MultiLineLabel SetCharacterSpacing(float characterSpacing)
        {
            CharacterSpacing = characterSpacing;
            return this;
        }

        /// <summary>Draws an outline around the glyphs (thickness in font pixels, 0 = no outline).</summary>
        public MultiLineLabel SetOutline(Color color, float thickness)
        {
            OutlineColor = color;
            OutlineThickness = thickness;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            EnsureLines();
            if (font == null || lines.Count == 0)
                return;

            var color = DrawColor;
            var position = GetPosition();
            var width = SizeWithoutScale.X;
            var lineHeight = font.LineHeight + lineSpacing;
            var scale = NestedScale;
            var drawFont = FontScaling.Resolve(font, ref scale, out var originFactor);
            var spacing = characterSpacing * originFactor;

            // The outlines of every line first, so an outline never covers the text of the line above (negative line spacing).
            if (outlineThickness > 0f)
            {
                var outline = outlineColor * NestedOpacity;
                for (var i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Length > 0)
                        TextOutline.Draw(spriteBatch, drawFont, lines[i], position, outline, outlineThickness * originFactor, Rotation,
                            GetLineOrigin(i, width, lineHeight) * originFactor, scale, LayerDepthDraw, spacing);
                }
            }

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length > 0)
                    drawFont.DrawText(spriteBatch, lines[i], position, color, Rotation, GetLineOrigin(i, width, lineHeight) * originFactor, scale, LayerDepthDraw, spacing);
            }
        }

        /// <inheritdoc />
        protected override int? GetContentSignature()
            => HashCode.Combine(font, text, textAlign, textBoxWidth, lineSpacing, NestedScale.X, HashCode.Combine(characterSpacing, outlineColor, outlineThickness));

        /// <summary>The box of the glyphs of every line, in local units.</summary>
        protected override RectangleF GetContentBounds()
        {
            EnsureLines();
            if (font == null || lines.Count == 0)
                return base.GetContentBounds();

            var width = SizeWithoutScale.X;
            var lineHeight = font.LineHeight + lineSpacing;
            RectangleF? area = null;
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length == 0)
                    continue;
                var line = TextInk.GetBounds(font, lines[i], OriginWithoutScale - GetLineOrigin(i, width, lineHeight), characterSpacing);
                area = area.HasValue ? RectangleF.Union(area.Value, line) : line;
            }

            return area.HasValue && !area.Value.IsEmpty ? area.Value : base.GetContentBounds();
        }

        /// <summary>The origin of a line (unscaled): every line rotates and scales around the origin of the whole control.</summary>
        private Vector2 GetLineOrigin(int index, float width, float lineHeight)
        {
            var offsetX = textAlign == TextAlign.Center ? (width - lineWidths[index]) / 2f
                : textAlign == TextAlign.Right ? width - lineWidths[index]
                : 0f;
            return OriginWithoutScale - new Vector2(offsetX, index * lineHeight);
        }

        protected override Vector2 CalculateSize()
        {
            EnsureLines();
            if (font == null || lines.Count == 0)
                return Vector2.Zero;

            var width = 0f;
            foreach (var lineWidth in lineWidths)
                width = Math.Max(width, lineWidth);
            var height = lines.Count * font.LineHeight + (lines.Count - 1) * lineSpacing;
            return new Vector2(width, Math.Max(0f, height));
        }

        private void MarkLinesAsDirty()
        {
            areLinesDirty = true;
            MarkAsDirty();
        }

        private void EnsureLines()
        {
            var scaleX = Math.Abs(Scale.X);
            var scaleChanged = scaleX != wrappedScaleX;
            if (!areLinesDirty && (!scaleChanged || scaleX < MinimumScaleToWrap))
                return; // keep the lines while the label is scaled to zero (eg: during a scale animation)

            lines.Clear();
            lineWidths.Clear();
            areLinesDirty = false;
            wrappedScaleX = scaleX;
            if (font == null)
                return;

            // The width is in the units of the parent: the scale of the label changes how much text fits.
            var maxWidth = textBoxWidth > 0 && scaleX >= MinimumScaleToWrap ? textBoxWidth / scaleX : float.PositiveInfinity;
            foreach (var line in TextWrapper.Wrap(text, Measure, maxWidth))
            {
                lines.Add(line);
                lineWidths.Add(Measure(line));
            }
        }

        private float Measure(string value) => value.Length == 0 ? 0f : font.MeasureString(value, characterSpacing: characterSpacing).X;
    }
}
