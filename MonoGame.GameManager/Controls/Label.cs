using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.GameMath;

namespace MonoGame.GameManager.Controls
{
    public class Label : ScalableControlAbstract<Label>
    {
        public SpriteFont SpriteFont
        {
            get => spriteFont;
            set
            {
                spriteFont = value;
                MarkAsDirty();
            }
        }
        private SpriteFont spriteFont;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                MarkAsDirty();
            }
        }
        private string text;

        public Label(SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
            SpriteFont = spriteFont;
            Text = text;
            SetPosition(position);
            SetColor(color);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(SpriteFont, Text, GetPosition(), Color, Rotation, OriginWithoutScale, Scale, SpriteEffects, LayerDepthDraw);
        }

        protected override Vector2 CalculateSize() => SpriteFont.MeasureString(Text);
    }
}
