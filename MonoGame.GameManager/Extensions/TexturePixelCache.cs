using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;

namespace MonoGame.GameManager.Extensions
{
    /// <summary>
    /// Keeps a copy of the pixels of textures in memory, so pixel-perfect hit tests do not read the GPU every time.
    /// The pixels of a texture are read once and released with the texture.
    /// </summary>
    public static class TexturePixelCache
    {
        private static readonly Color[] Unreadable = new Color[0];
        private static readonly ConditionalWeakTable<Texture2D, Color[]> Cache = new ConditionalWeakTable<Texture2D, Color[]>();

        /// <summary>
        /// Returns the color of a pixel. Pixels outside of the texture are transparent; textures that cannot be read
        /// (eg: compressed formats) are considered opaque.
        /// </summary>
        public static Color GetPixel(Texture2D texture, int x, int y)
        {
            if (texture == null || texture.IsDisposed || x < 0 || y < 0 || x >= texture.Width || y >= texture.Height)
                return Color.Transparent;

            var pixels = Cache.GetValue(texture, ReadPixels);
            return pixels.Length == 0 ? Color.White : pixels[y * texture.Width + x];
        }

        /// <summary>Forgets the cached pixels of a texture, after its content was changed with SetData.</summary>
        public static void Invalidate(Texture2D texture)
        {
            if (texture != null)
                Cache.Remove(texture);
        }

        private static Color[] ReadPixels(Texture2D texture)
        {
            if (texture.Format != SurfaceFormat.Color)
                return Unreadable;

            try
            {
                var pixels = new Color[texture.Width * texture.Height];
                texture.GetData(pixels);
                return pixels;
            }
            catch (Exception)
            {
                return Unreadable; // the pixels of this texture cannot be read on this platform
            }
        }
    }
}
