using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// The atlas of the small generated images of the button effects (light shapes, gradients, bands, frames): a few
    /// shared textures, so consecutive effects (of one button or of many) do not switch textures, which would cost one
    /// draw call each.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The images are packed in rows. Each one is surrounded by a copy of its border pixels (<see cref="Gutter"/>), so
    /// linear filtering at its edges reads what the clamped texture of its own would read: drawn from the atlas it looks
    /// exactly the same.
    /// </para>
    /// <para>
    /// The images are never removed one by one. When the pages are full, the image is given a texture of its own and the
    /// atlas is emptied before the next frame is drawn (<see cref="ReleaseIfFull"/>); the owners then create their images
    /// again on demand.
    /// </para>
    /// </remarks>
    internal static class EffectAtlas
    {
        /// <summary>The size of a page, in pixels.</summary>
        public const int PageSize = 1024;

        /// <summary>The largest side of an image put in the atlas; bigger images get a texture of their own.</summary>
        public const int MaxImageSize = 256;

        /// <summary>The number of border pixels copied around each image.</summary>
        public const int Gutter = 2;

        /// <summary>The maximum number of pages.</summary>
        public const int MaxPages = 4;

        private static readonly List<Page> pages = new List<Page>();
        private static bool isFull;

        /// <summary>True when <paramref name="texture"/> is a page of the atlas.</summary>
        public static bool IsPage(Texture2D texture)
        {
            if (texture == null)
                return false;
            for (var i = 0; i < pages.Count; i++)
            {
                if (ReferenceEquals(pages[i].Texture, texture))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Puts an image in the atlas. Returns false when it is too big or the atlas is full (it will be emptied before the
        /// next frame); the caller then creates a texture of its own.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="pixels">The pixels of the image, row by row.</param>
        /// <param name="sprite">Receives the page and the area of the image.</param>
        public static bool TryAdd(GraphicsDevice graphicsDevice, int width, int height, Color[] pixels, out EffectSprite sprite)
        {
            sprite = default;
            if (width <= 0 || height <= 0 || width > MaxImageSize || height > MaxImageSize || pixels == null || pixels.Length != width * height)
                return false;

            var paddedWidth = width + Gutter * 2;
            var paddedHeight = height + Gutter * 2;
            Page page = null;
            Point position = default;
            for (var i = 0; i < pages.Count && page == null; i++)
            {
                if (pages[i].TryAllocate(paddedWidth, paddedHeight, out position))
                    page = pages[i];
            }

            if (page == null)
            {
                if (pages.Count >= MaxPages)
                {
                    isFull = true;
                    return false;
                }

                page = new Page(new Texture2D(graphicsDevice, PageSize, PageSize, false, SurfaceFormat.Color));
                pages.Add(page);
                if (!page.TryAllocate(paddedWidth, paddedHeight, out position))
                    return false;
            }

            page.Texture.SetData(0, new Rectangle(position.X, position.Y, paddedWidth, paddedHeight), Pad(width, height, pixels), 0, paddedWidth * paddedHeight);
            sprite = new EffectSprite(page.Texture, new Rectangle(position.X + Gutter, position.Y + Gutter, width, height));
            return true;
        }

        /// <summary>
        /// Empties the atlas when it became full (called before the controls are drawn, when no sprite batch holds its
        /// pages). Returns true when it was emptied: every image taken from it must be created again.
        /// </summary>
        public static bool ReleaseIfFull()
        {
            if (!isFull)
                return false;
            Reset();
            return true;
        }

        /// <summary>Disposes the pages.</summary>
        public static void Reset()
        {
            for (var i = 0; i < pages.Count; i++)
                pages[i].Texture.Dispose();
            pages.Clear();
            isFull = false;
        }

        /// <summary>The image surrounded by <see cref="Gutter"/> copies of its border pixels.</summary>
        private static Color[] Pad(int width, int height, Color[] pixels)
        {
            var paddedWidth = width + Gutter * 2;
            var paddedHeight = height + Gutter * 2;
            var padded = new Color[paddedWidth * paddedHeight];
            for (var y = 0; y < paddedHeight; y++)
            {
                var sourceY = Math.Clamp(y - Gutter, 0, height - 1);
                for (var x = 0; x < paddedWidth; x++)
                {
                    var sourceX = Math.Clamp(x - Gutter, 0, width - 1);
                    padded[y * paddedWidth + x] = pixels[sourceY * width + sourceX];
                }
            }

            return padded;
        }

        /// <summary>A page packed in rows (shelves): each image goes in the first row high enough with room left.</summary>
        private sealed class Page
        {
            private readonly List<Shelf> shelves = new List<Shelf>();
            private int nextY;

            public Page(Texture2D texture)
            {
                Texture = texture;
            }

            public Texture2D Texture { get; }

            public bool TryAllocate(int width, int height, out Point position)
            {
                // The lowest row that fits wastes the least height.
                var best = -1;
                for (var i = 0; i < shelves.Count; i++)
                {
                    var shelf = shelves[i];
                    if (shelf.Height >= height && shelf.Height <= height * 2 + 8 && PageSize - shelf.UsedWidth >= width && (best < 0 || shelf.Height < shelves[best].Height))
                        best = i;
                }

                if (best < 0)
                {
                    if (nextY + height > PageSize || width > PageSize)
                    {
                        position = default;
                        return false;
                    }

                    shelves.Add(new Shelf { Y = nextY, Height = height });
                    nextY += height;
                    best = shelves.Count - 1;
                }

                var target = shelves[best];
                position = new Point(target.UsedWidth, target.Y);
                target.UsedWidth += width;
                shelves[best] = target;
                return true;
            }

            private struct Shelf
            {
                public int Y;
                public int Height;
                public int UsedWidth;
            }
        }
    }
}
