using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.GameMath;

namespace MonoGame.GameManager.Controls
{
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawTexture(spriteBatch, ShapeExtension.WhitePixelTexture, DestinationRectangle, null, OriginWithoutScale);
        }

        public RectangleControl SetSize(Vector2 size)
        {
            Size = size;
            return this;
        }

        public override RectangleControl SetOriginRate(Vector2 originRate, Vector2 size)
           => SetOrigin(ShapeExtension.WhitePixelTexture.Size().ToVector2() * originRate);
    }
}
