using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Layout;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// A generated texture that follows the edge of a rounded rectangle (a glow, a rim, a mask), drawn with nine-slice
    /// scaling so it fits any size (see <see cref="ButtonEffectResources"/>).
    /// </summary>
    public readonly struct LightFrame
    {
        public LightFrame(Texture2D texture, int sourceBorder, float destinationBorder, float spread)
        {
            Texture = texture;
            SourceBorder = sourceBorder;
            DestinationBorder = destinationBorder;
            Spread = spread;
        }

        /// <summary>The texture.</summary>
        public Texture2D Texture { get; }

        /// <summary>The size of the corners in texture pixels.</summary>
        public int SourceBorder { get; }

        /// <summary>The size of the corners when drawn, in local units.</summary>
        public float DestinationBorder { get; }

        /// <summary>How far the frame goes outside the shape, in local units.</summary>
        public float Spread { get; }

        /// <summary>The area covered by the frame for a shape.</summary>
        public RectangleF GetArea(RectangleF shape) => shape.Inflate(Spread, Spread);

        /// <summary>Draws the frame around a shape (in the coordinate space of the sprite batch).</summary>
        public void Draw(SpriteBatch spriteBatch, RectangleF shape, Color color)
        {
            if (Texture == null || Texture.IsDisposed)
                return;
            spriteBatch.DrawNineSlice(Texture, null, new Thickness(SourceBorder), GetArea(shape), new Thickness(DestinationBorder), color);
        }
    }
}
