using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.GameMath;
using System;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// Repeats a texture to fill an area (backgrounds, floors, water...). <see cref="Offset"/> scrolls the pattern,
    /// for example to make an endless scrolling background.
    /// </summary>
    /// <example>
    /// <code>
    /// var sky = new TiledImage(cloudsTexture, Vector2.Zero, ScreenManager.ScreenSize.ToVector2()).AddToScreen();
    /// sky.AddOnUpdateEvent(gameTime => sky.Offset += new Vector2(30, 0) * (float)gameTime.ElapsedGameTime.TotalSeconds);
    /// </code>
    /// </example>
    public class TiledImage : ScalableControlAbstract<TiledImage>
    {
        /// <summary>The maximum number of tiles drawn, to protect against tiny tiles over huge areas.</summary>
        private const int MaxTiles = 4096;

        public TiledImage(Texture2D texture, Vector2 position, Vector2 size)
        {
            Texture = texture;
            PositionAnchor = position;
            Size = size;
        }

        public Texture2D Texture { get; set; }

        /// <summary>The part of the texture that is repeated (null = the whole texture).</summary>
        public Rectangle? SourceRectangle { get; set; }

        /// <summary>Scrolls the pattern, in local units (it wraps around).</summary>
        public Vector2 Offset { get; set; }

        /// <summary>The scale of every tile.</summary>
        public Vector2 TileScale { get; set; } = Vector2.One;

        public TiledImage SetTexture(Texture2D texture, Rectangle? sourceRectangle = null)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle;
            return this;
        }

        public TiledImage SetOffset(Vector2 offset)
        {
            Offset = offset;
            return this;
        }

        public TiledImage SetTileScale(Vector2 tileScale)
        {
            TileScale = tileScale;
            return this;
        }

        public TiledImage SetSize(Vector2 size)
        {
            Size = size;
            return this;
        }

        /// <inheritdoc />
        protected override int? GetContentSignature() => HashCode.Combine(Texture, SourceRectangle, Offset, TileScale);

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Texture == null || Texture.IsDisposed)
                return;

            var source = SourceRectangle ?? Texture.Bounds;
            var tile = new Vector2(source.Width * TileScale.X, source.Height * TileScale.Y);
            var size = SizeWithoutScale;
            if (tile.X <= 0f || tile.Y <= 0f || size.X <= 0f || size.Y <= 0f)
                return;
            if (Math.Ceiling(size.X / tile.X + 1) * Math.Ceiling(size.Y / tile.Y + 1) > MaxTiles)
                return;

            // The first tile starts before the area so the offset scrolls the pattern in both directions.
            var start = new Vector2(-Wrap(Offset.X, tile.X), -Wrap(Offset.Y, tile.Y));
            var color = DrawColor;
            for (var y = start.Y; y < size.Y; y += tile.Y)
            {
                for (var x = start.X; x < size.X; x += tile.X)
                {
                    // The part of the tile inside the area, and the matching part of the texture.
                    var left = Math.Max(x, 0f);
                    var top = Math.Max(y, 0f);
                    var right = Math.Min(x + tile.X, size.X);
                    var bottom = Math.Min(y + tile.Y, size.Y);
                    if (right <= left || bottom <= top)
                        continue;

                    var sourceLeft = (int)Math.Round((left - x) / TileScale.X);
                    var sourceTop = (int)Math.Round((top - y) / TileScale.Y);
                    var sourceRight = (int)Math.Round((right - x) / TileScale.X);
                    var sourceBottom = (int)Math.Round((bottom - y) / TileScale.Y);
                    if (sourceRight <= sourceLeft || sourceBottom <= sourceTop)
                        continue;

                    var part = new Rectangle(source.X + sourceLeft, source.Y + sourceTop, sourceRight - sourceLeft, sourceBottom - sourceTop);
                    DrawLocalTexture(spriteBatch, Texture, part, new RectangleF(left, top, right - left, bottom - top), color);
                }
            }
        }

        private static float Wrap(float value, float length)
        {
            var result = value % length;
            return result < 0f ? result + length : result;
        }
    }
}
