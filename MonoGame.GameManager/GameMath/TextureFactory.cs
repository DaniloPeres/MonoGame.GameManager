using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoGame.GameManager.GameMath
{
    /// <summary>
    /// Generates simple textures (circles, rectangles, rounded rectangles) at runtime, with anti-aliased edges.
    /// The pixels use premultiplied alpha, like the textures built by the content pipeline.
    /// </summary>
    /// <remarks>
    /// The textures are owned by the caller: dispose them when they are not needed anymore
    /// (eg: with <see cref="Screens.Screen.RegisterDisposable"/>).
    /// </remarks>
    public static class TextureFactory
    {
        public static Texture2D CreateRectangle(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            var pixels = new Color[Math.Max(1, width) * Math.Max(1, height)];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            return FromPixels(graphicsDevice, Math.Max(1, width), Math.Max(1, height), pixels);
        }

        public static Texture2D CreateCircle(GraphicsDevice graphicsDevice, int diameter, Color color)
            => FromPixels(graphicsDevice, Math.Max(1, diameter), Math.Max(1, diameter), CreateCirclePixels(diameter, color));

        public static Texture2D CreateRoundedRectangle(GraphicsDevice graphicsDevice, int width, int height, float radius, Color fillColor, float borderThickness = 0f, Color? borderColor = null)
            => FromPixels(graphicsDevice, Math.Max(1, width), Math.Max(1, height), CreateRoundedRectanglePixels(width, height, radius, fillColor, borderThickness, borderColor));

        /// <summary>Creates a texture from an array of pixels (row by row).</summary>
        public static Texture2D FromPixels(GraphicsDevice graphicsDevice, int width, int height, Color[] pixels)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));
            if (pixels == null || pixels.Length != width * height)
                throw new ArgumentException("The number of pixels must be width * height.", nameof(pixels));

            var texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
            texture.SetData(pixels);
            return texture;
        }

        /// <summary>Generates the pixels of an anti-aliased circle.</summary>
        public static Color[] CreateCirclePixels(int diameter, Color color)
        {
            diameter = Math.Max(1, diameter);
            var pixels = new Color[diameter * diameter];
            var radius = diameter / 2f;
            var center = new Vector2(radius);

            for (var y = 0; y < diameter; y++)
            {
                for (var x = 0; x < diameter; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    pixels[y * diameter + x] = color * MathUtils.Clamp01(radius - distance + 0.5f);
                }
            }

            return pixels;
        }

        /// <summary>Generates the pixels of an anti-aliased rounded rectangle, with an optional border.</summary>
        public static Color[] CreateRoundedRectanglePixels(int width, int height, float radius, Color fillColor, float borderThickness = 0f, Color? borderColor = null)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            radius = MathUtils.Clamp(radius, 0f, Math.Min(width, height) / 2f);
            var border = borderColor ?? fillColor;
            var pixels = new Color[width * height];
            var halfSize = new Vector2(width / 2f, height / 2f);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var point = new Vector2(x + 0.5f, y + 0.5f) - halfSize;
                    var distance = RoundedRectangleDistance(point, halfSize, radius);
                    var outerCoverage = MathUtils.Clamp01(0.5f - distance);
                    var innerCoverage = borderThickness > 0f ? MathUtils.Clamp01(0.5f - (distance + borderThickness)) : outerCoverage;
                    pixels[y * width + x] = Add(border * (outerCoverage - innerCoverage), fillColor * innerCoverage);
                }
            }

            return pixels;
        }

        /// <summary>Adds two premultiplied colors (MonoGame 3.8.0 has no Color + Color operator).</summary>
        private static Color Add(Color a, Color b) => new Color(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);

        /// <summary>Signed distance from a point (relative to the center) to a rounded rectangle.</summary>
        private static float RoundedRectangleDistance(Vector2 point, Vector2 halfSize, float radius)
        {
            var q = point.Abs() - halfSize + new Vector2(radius);
            var outside = Vector2.Max(q, Vector2.Zero).Length();
            var inside = Math.Min(Math.Max(q.X, q.Y), 0f);
            return outside + inside - radius;
        }
    }
}
