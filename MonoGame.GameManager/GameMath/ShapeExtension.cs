using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// Shared 1x1 textures used to draw solid shapes (see <see cref="Primitives"/>).
    /// </summary>
    public static class ShapeExtension
    {
        private static Texture2D whitePixelTexture;
        private static Texture2D transparentPixelTexture;

        /// <summary>A 1x1 white texture of the current graphics device.</summary>
        public static Texture2D WhitePixelTexture => GetOrCreate(ref whitePixelTexture, Color.White);

        /// <summary>A 1x1 transparent texture of the current graphics device.</summary>
        public static Texture2D TransparentPixelTexture => GetOrCreate(ref transparentPixelTexture, Color.Transparent);

        /// <summary>Creates a 1x1 texture of the given color.</summary>
        public static Texture2D CreatePixelTexture(GraphicsDevice graphicsDevice, Color color)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            var texture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            texture.SetData(new[] { color });
            return texture;
        }

        /// <summary>Disposes the shared textures (they are recreated on demand).</summary>
        internal static void Reset()
        {
            whitePixelTexture?.Dispose();
            whitePixelTexture = null;
            transparentPixelTexture?.Dispose();
            transparentPixelTexture = null;
        }

        private static Texture2D GetOrCreate(ref Texture2D texture, Color color)
        {
            var graphicsDevice = ServiceProvider.GraphicsDevice
                ?? throw new InvalidOperationException("The graphics device is not available yet.");

            if (texture == null || texture.IsDisposed || texture.GraphicsDevice != graphicsDevice)
                texture = CreatePixelTexture(graphicsDevice, color);

            return texture;
        }
    }
}
