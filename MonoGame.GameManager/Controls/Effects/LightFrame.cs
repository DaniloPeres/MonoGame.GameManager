using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Layout;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// A generated image that follows the edge of a rounded rectangle (a glow, a rim, a mask), drawn with nine-slice
    /// scaling so it fits any size (see <see cref="ButtonEffectResources"/>). Do not keep it between frames: the image may
    /// live in the atlas of the effects, which is rebuilt when it is full.
    /// </summary>
    public readonly struct LightFrame
    {
        public LightFrame(EffectSprite sprite, int sourceBorder, float destinationBorder, float spread)
        {
            Sprite = sprite;
            SourceBorder = sourceBorder;
            DestinationBorder = destinationBorder;
            Spread = spread;
        }

        public LightFrame(Texture2D texture, int sourceBorder, float destinationBorder, float spread)
            : this(new EffectSprite(texture, texture?.Bounds ?? Rectangle.Empty), sourceBorder, destinationBorder, spread)
        {
        }

        /// <summary>The image.</summary>
        public EffectSprite Sprite { get; }

        /// <summary>The texture that holds the image (an atlas page or a texture of its own).</summary>
        public Texture2D Texture => Sprite.Texture;

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
            if (!Sprite.IsValid)
                return;
            spriteBatch.DrawNineSlice(Texture, Sprite.Source, new Thickness(SourceBorder), GetArea(shape), new Thickness(DestinationBorder), color);
        }

        /// <summary>Draws the frame around a shape in the local space of a button (see <see cref="ButtonEffectContext"/>).</summary>
        public void Draw(SpriteBatch spriteBatch, ButtonEffectContext context, RectangleF shape, Color color)
        {
            if (!Sprite.IsValid)
                return;
            context.DrawNineSlice(spriteBatch, Texture, Sprite.Source, new Thickness(SourceBorder), GetArea(shape), new Thickness(DestinationBorder), color);
        }
    }
}
