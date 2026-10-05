using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Layout;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A bar filled from 0 to 1 (health, loading, experience...), drawn with colors or textures. A fill texture is
    /// revealed (cropped), not stretched.
    /// </summary>
    /// <example>
    /// <code>
    /// var health = new ProgressBar(new Vector2(20, 20), new Vector2(200, 16))
    ///     .SetColors(Color.DarkRed * 0.5f, Color.Red)
    ///     .SetValue(1f)
    ///     .AddToScreen();
    /// health.AnimateTo(player.Health / player.MaxHealth, 0.3f);
    /// </code>
    /// </example>
    public class ProgressBar : ScalableControlAbstract<ProgressBar>
    {
        private float value;
        private Tween<float> valueTween;

        public ProgressBar(Vector2 position, Vector2 size)
        {
            PositionAnchor = position;
            Size = size;
        }

        /// <summary>The filled part, from 0 to 1.</summary>
        public float Value
        {
            get => value;
            set => this.value = MathHelper.Clamp(value, 0f, 1f);
        }

        public FillDirection FillDirection { get; set; } = FillDirection.LeftToRight;

        public Color BackgroundColor { get; set; } = new Color(40, 40, 40);

        public Color FillColor { get; set; } = new Color(80, 190, 90);

        /// <summary>Drawn instead of <see cref="BackgroundColor"/> (tinted with the color of the control).</summary>
        public Texture2D BackgroundTexture { get; set; }

        /// <summary>Drawn instead of <see cref="FillColor"/> (tinted with the color of the control).</summary>
        public Texture2D FillTexture { get; set; }

        /// <summary>The color of the border (null = no border).</summary>
        public Color? BorderColor { get; set; }

        public float BorderThickness { get; set; } = 1f;

        public ProgressBar SetValue(float value)
        {
            valueTween?.Stop();
            Value = value;
            return this;
        }

        /// <summary>Animates the value (the previous animation of the value is stopped).</summary>
        public ProgressBar AnimateTo(float value, float duration, EasingFunction easing = null)
        {
            valueTween?.Stop();
            valueTween = Tween.Float(v => Value = v, Value, MathHelper.Clamp(value, 0f, 1f), duration, this)
                .SetEasing(easing ?? Easing.CubicOut)
                .Play();
            return this;
        }

        public ProgressBar SetFillDirection(FillDirection fillDirection)
        {
            FillDirection = fillDirection;
            return this;
        }

        public ProgressBar SetColors(Color background, Color fill)
        {
            BackgroundColor = background;
            FillColor = fill;
            return this;
        }

        public ProgressBar SetTextures(Texture2D background, Texture2D fill)
        {
            BackgroundTexture = background;
            FillTexture = fill;
            return this;
        }

        public ProgressBar SetBorder(Color color, float thickness = 1f)
        {
            BorderColor = color;
            BorderThickness = thickness;
            return this;
        }

        public ProgressBar SetSize(Vector2 size)
        {
            Size = size;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var opacity = NestedOpacity;
            var bounds = LocalBounds;

            if (BackgroundTexture != null)
                DrawLocalTexture(spriteBatch, BackgroundTexture, null, bounds, Color * opacity);
            else
                DrawLocalRectangle(spriteBatch, bounds, BackgroundColor * opacity);

            if (value > 0f)
            {
                var fillArea = GetFillArea(bounds, value);
                if (FillTexture != null)
                    DrawLocalTexture(spriteBatch, FillTexture, GetFillArea(FillTexture.Bounds, value).ToRectangle(), fillArea, Color * opacity);
                else
                    DrawLocalRectangle(spriteBatch, fillArea, FillColor * opacity);
            }

            if (BorderColor.HasValue)
                DrawLocalBorder(spriteBatch, bounds, BorderColor.Value * opacity, BorderThickness);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                valueTween?.Stop();
            base.Dispose(disposing);
        }

        private RectangleF GetFillArea(RectangleF area, float amount)
        {
            switch (FillDirection)
            {
                case FillDirection.RightToLeft:
                    return new RectangleF(area.X + area.Width * (1f - amount), area.Y, area.Width * amount, area.Height);
                case FillDirection.TopToBottom:
                    return new RectangleF(area.X, area.Y, area.Width, area.Height * amount);
                case FillDirection.BottomToTop:
                    return new RectangleF(area.X, area.Y + area.Height * (1f - amount), area.Width, area.Height * amount);
                default:
                    return new RectangleF(area.X, area.Y, area.Width * amount, area.Height);
            }
        }
    }
}
