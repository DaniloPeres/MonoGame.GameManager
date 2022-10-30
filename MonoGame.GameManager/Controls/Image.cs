using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls
{
    public class Image : ScalableControlAbstract<Image>
    {
        private Rectangle? SourceRectangle { get; set; }

        private Texture2D texture;
        public Texture2D Texture {
            get => texture;
            set {
                texture = value;
                MarkAsDirty();
            }
        }

        /// <summary>
        /// Gets or sets the information during intersection if we should ignore transparent pixels
        /// </summary>
        public bool IgnoreIntersectionTransparentPixels { get; set; }

        public Image(Texture2D texture)
        {
            Texture = texture;
        }

        public Image SetSourceRectangle(Rectangle? sourceRectangle)
        {
            SourceRectangle = sourceRectangle;
            return this;
        }

        public override void Draw(SpriteBatch spriteBatch) => DrawTexture(spriteBatch, Texture, DestinationRectangle, SourceRectangle, OriginWithoutScale);
        protected override Vector2 CalculateSize() {
            return SourceRectangle != null
                ? SourceRectangle.Value.Size.ToVector2()
                : Texture.Size().ToVector2();
        }

        public override bool Intersects(Point pointToCompare)
        {
            var intersects = Intersection.IntersectsWithPoint(DestinationRectangle, Origin, pointToCompare);

            if (intersects && IgnoreIntersectionTransparentPixels)
            {
                Vector2 posTexture = pointToCompare.ToVector2() - DestinationRectangle.Location.ToVector2();
                posTexture -= Origin;
                posTexture /= Scale;

                if (posTexture.X < 0 || posTexture.Y < 0 || posTexture.X >= texture.Width || posTexture.Y >= texture.Height)
                {
#if DEBUG
                    throw new Exception("Image.cs, Intersects with invalid posTexture");
#endif
                    return false;
                }

                var pixelColor = texture.GetPixelColor((int)posTexture.X, (int)posTexture.Y);
                if (pixelColor.A == 0)
                {
                    return false;
                }
            }

            return intersects;
        }

        public Image SetIgnoreIntersectionTransparentPixels(bool ignoreIntersectionTransparentPixels)
        {
            IgnoreIntersectionTransparentPixels = ignoreIntersectionTransparentPixels;
            return this;
        }
    }
}
