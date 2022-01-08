using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using MonoGame.GameManager.Services;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    ///     Sprite batch extensions for drawing primitive shapes
    /// </summary>
    public static class ShapeExtension
    {
        public static Texture2D WhitePixelTexture => lazyWhitePixelTexture.Value;
        public static Texture2D TransparentPixelTexture => lazyTransparentPixelTexture.Value;
        private static readonly Lazy<Texture2D> lazyWhitePixelTexture = new Lazy<Texture2D>(() => CreatePixelTexture(Color.White));
        private static readonly Lazy<Texture2D> lazyTransparentPixelTexture = new Lazy<Texture2D>(() => CreatePixelTexture(Color.Transparent));

        private static Texture2D CreatePixelTexture(Color color)
        {
            var graphicsDevice = ServiceProvider.ScreenManager.GraphicsDevice;
            var whitePixelTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            whitePixelTexture.SetData(new[] { color });
            
            return whitePixelTexture;
        }
    }
}
