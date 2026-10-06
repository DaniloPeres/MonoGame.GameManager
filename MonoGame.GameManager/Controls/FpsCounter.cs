using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using System.Diagnostics;
using System.Globalization;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// Shows the number of frames drawn per second, measured with the real time.
    /// </summary>
    /// <example>
    /// <code>
    /// new FpsCounter(font, new Vector2(10, 10), Color.Yellow).SetZIndex(ZIndexLayers.Overlay).AddToScreen();
    /// </code>
    /// </example>
    public class FpsCounter : ScalableControlAbstract<FpsCounter>
    {
        private long intervalStart = Stopwatch.GetTimestamp();
        private int frames;
        private string text = string.Empty;

        public FpsCounter(SpriteFont spriteFont, Vector2 position, Color color)
        {
            SpriteFont = spriteFont;
            SetPosition(position);
            Color = color;
            UpdateText();
        }

        public SpriteFont SpriteFont { get; set; }

        /// <summary>How often the value is updated, in seconds.</summary>
        public float UpdateInterval { get; set; } = 0.5f;

        /// <summary>The format of the text; {0} is the number of frames per second.</summary>
        public string Format { get; set; } = "FPS: {0:0}";

        /// <summary>The frames per second measured in the last interval.</summary>
        public float CurrentFps { get; private set; }

        public override void Draw(SpriteBatch spriteBatch)
        {
            frames++;
            var now = Stopwatch.GetTimestamp();
            var elapsed = (now - intervalStart) / (double)Stopwatch.Frequency;
            if (elapsed >= UpdateInterval && elapsed > 0)
            {
                CurrentFps = (float)(frames / elapsed);
                frames = 0;
                intervalStart = now;
                UpdateText();
            }

            if (SpriteFont != null && text.Length > 0)
                spriteBatch.DrawString(SpriteFont, text, GetPosition(), DrawColor, Rotation, OriginWithoutScale, NestedScale, SpriteEffects, LayerDepthDraw);
        }

        protected override Vector2 CalculateSize() => SpriteFont == null || text.Length == 0 ? Vector2.Zero : SpriteFont.MeasureString(text);

        private void UpdateText()
        {
            var newText = string.Format(CultureInfo.InvariantCulture, Format ?? "{0:0}", CurrentFps);
            if (newText == text)
                return;
            text = newText;
            MarkAsDirty();
        }
    }
}
