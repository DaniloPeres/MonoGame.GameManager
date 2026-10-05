using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A single line of text (use <see cref="MultiLineLabel"/> for text that wraps).
    /// </summary>
    public class Label : ScalableControlAbstract<Label>
    {
        private SpriteFont spriteFont;
        private string text = string.Empty;

        public Label(SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
            SpriteFont = spriteFont;
            Text = text;
            SetPosition(position);
            SetColor(color);
        }

        public SpriteFont SpriteFont
        {
            get => spriteFont;
            set
            {
                spriteFont = value;
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

        public Label SetText(string text)
        {
            Text = text;
            return this;
        }

        public Label SetSpriteFont(SpriteFont spriteFont)
        {
            SpriteFont = spriteFont;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (spriteFont == null || text.Length == 0)
                return;
            spriteBatch.DrawString(spriteFont, text, GetPosition(), DrawColor, Rotation, OriginWithoutScale, NestedScale, SpriteEffects, LayerDepthDraw);
        }

        protected override Vector2 CalculateSize() => spriteFont == null || text.Length == 0 ? Vector2.Zero : spriteFont.MeasureString(text);
    }
}
