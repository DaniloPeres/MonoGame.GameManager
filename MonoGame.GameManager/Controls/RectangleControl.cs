using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.GameMath;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A rectangle of a solid <see cref="Control{TControl}.Color"/>, with an optional border.
    /// </summary>
    /// <remarks>
    /// Since version 2.0 the origin is in pixels, like every other control (it used to be a rate from 0 to 1).
    /// <c>SetOriginRate</c> works as before.
    /// </remarks>
    public class RectangleControl : ScalableControlAbstract<RectangleControl>
    {
        public RectangleControl(Rectangle destinationRectangle, Color color)
            : this(destinationRectangle.Location.ToVector2(), destinationRectangle.Size.ToVector2(), color) { }

        public RectangleControl(Vector2 position, Vector2 size, Color color)
        {
            PositionAnchor = position;
            Size = size;
            Color = color;
        }

        /// <summary>The color of the border (null = no border).</summary>
        public Color? BorderColor { get; set; }

        /// <summary>The thickness of the border, in local units.</summary>
        public float BorderThickness { get; set; } = 1f;

        public RectangleControl SetSize(Vector2 size)
        {
            Size = size;
            return this;
        }

        public RectangleControl SetBorder(Color color, float thickness = 1f)
        {
            BorderColor = color;
            BorderThickness = thickness;
            return this;
        }

        public RectangleControl RemoveBorder()
        {
            BorderColor = null;
            return this;
        }

        /// <inheritdoc />
        protected override int? GetContentSignature() => System.HashCode.Combine(BorderColor, BorderThickness);

        public override void Draw(SpriteBatch spriteBatch)
        {
            var size = SizeWithoutScale;
            if (size.X <= 0f || size.Y <= 0f)
                return;

            // The texture is a single pixel: its origin is the origin of the control relative to its size.
            DrawTexture(spriteBatch, ShapeExtension.WhitePixelTexture, DestinationRectangle, null, OriginWithoutScale / size);

            if (BorderColor.HasValue)
                DrawLocalBorder(spriteBatch, LocalBounds, BorderColor.Value * NestedOpacity, BorderThickness);
        }
    }
}
