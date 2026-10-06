using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Enums;
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
        private SpriteFont spriteFont;
        private string text = string.Empty;
        private TextAlign textAlign;
        private int textBoxWidth;
        private float lineSpacing;
        private bool areLinesDirty = true;
        private float wrappedScaleX = float.NaN;

        public MultiLineLabel(SpriteFont spriteFont, string text, Vector2 position, Color color, int textBoxWidth)
        {
            SpriteFont = spriteFont;
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

        public SpriteFont SpriteFont
        {
            get => spriteFont;
            set
            {
                spriteFont = value;
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

        public MultiLineLabel SetSpriteFont(SpriteFont spriteFont)
        {
            SpriteFont = spriteFont;
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            EnsureLines();
            if (spriteFont == null || lines.Count == 0)
                return;

            var color = DrawColor;
            var position = GetPosition();
            var width = SizeWithoutScale.X;
            var lineHeight = spriteFont.LineSpacing + lineSpacing;
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length == 0)
                    continue;

                var offsetX = textAlign == TextAlign.Center ? (width - lineWidths[i]) / 2f
                    : textAlign == TextAlign.Right ? width - lineWidths[i]
                    : 0f;

                // Every line rotates and scales around the origin of the whole control.
                var lineOrigin = OriginWithoutScale - new Vector2(offsetX, i * lineHeight);
                spriteBatch.DrawString(spriteFont, lines[i], position, color, Rotation, lineOrigin, NestedScale, SpriteEffects, LayerDepthDraw);
            }
        }

        protected override Vector2 CalculateSize()
        {
            EnsureLines();
            if (spriteFont == null || lines.Count == 0)
                return Vector2.Zero;

            var width = 0f;
            foreach (var lineWidth in lineWidths)
                width = Math.Max(width, lineWidth);
            var height = lines.Count * spriteFont.LineSpacing + (lines.Count - 1) * lineSpacing;
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
            if (spriteFont == null)
                return;

            // The width is in the units of the parent: the scale of the label changes how much text fits.
            var maxWidth = textBoxWidth > 0 && scaleX >= MinimumScaleToWrap ? textBoxWidth / scaleX : float.PositiveInfinity;
            foreach (var line in TextWrapper.Wrap(text, Measure, maxWidth))
            {
                lines.Add(line);
                lineWidths.Add(Measure(line));
            }
        }

        private float Measure(string value) => value.Length == 0 ? 0f : spriteFont.MeasureString(value).X;
    }
}
