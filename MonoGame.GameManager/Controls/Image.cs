using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Extensions;
using System;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// Draws a texture, or a part of it (<see cref="SourceRectangle"/>).
    /// </summary>
    public class Image : ScalableControlAbstract<Image>
    {
        private Texture2D texture;
        private Rectangle? sourceRectangle;

        public Image(Texture2D texture)
        {
            Texture = texture;
        }

        public Texture2D Texture
        {
            get => texture;
            set
            {
                texture = value;
                MarkAsDirty();
            }
        }

        /// <summary>The part of the texture that is drawn (null = the whole texture).</summary>
        public Rectangle? SourceRectangle
        {
            get => sourceRectangle;
            set
            {
                sourceRectangle = value;
                MarkAsDirty();
            }
        }

        /// <summary>
        /// When true, the transparent pixels of the texture do not receive pointer events (pixel-perfect hit test).
        /// </summary>
        public bool IgnoreIntersectionTransparentPixels { get; set; }

        public Image SetTexture(Texture2D texture)
        {
            Texture = texture;
            return this;
        }

        public Image SetSourceRectangle(Rectangle? sourceRectangle)
        {
            SourceRectangle = sourceRectangle;
            return this;
        }

        public Image SetIgnoreIntersectionTransparentPixels(bool ignoreIntersectionTransparentPixels)
        {
            IgnoreIntersectionTransparentPixels = ignoreIntersectionTransparentPixels;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch) => DrawTexture(spriteBatch, texture, DestinationRectangle, sourceRectangle, OriginWithoutScale);

        protected override Vector2 CalculateSize()
        {
            if (sourceRectangle.HasValue)
                return sourceRectangle.Value.Size.ToVector2();
            return texture == null ? Vector2.Zero : texture.Size().ToVector2();
        }

        public override bool Intersects(Point pointToCompare)
        {
            if (!base.Intersects(pointToCompare))
                return false;
            if (!IgnoreIntersectionTransparentPixels || texture == null)
                return true;

            // The local position is in pixels of the drawn part of the texture.
            var local = ToLocalPosition(pointToCompare);
            var source = sourceRectangle ?? texture.Bounds;
            var x = (int)Math.Floor(local.X);
            var y = (int)Math.Floor(local.Y);
            if ((SpriteEffects & SpriteEffects.FlipHorizontally) != 0)
                x = source.Width - 1 - x;
            if ((SpriteEffects & SpriteEffects.FlipVertically) != 0)
                y = source.Height - 1 - y;
            if (x < 0 || y < 0 || x >= source.Width || y >= source.Height)
                return false;

            return TexturePixelCache.GetPixel(texture, source.X + x, source.Y + y).A > 0;
        }
    }
}
