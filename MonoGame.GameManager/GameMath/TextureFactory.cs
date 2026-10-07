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

        /// <summary>Creates a soft circle, opaque in the center and transparent at the edge (lights, fire, magic).</summary>
        public static Texture2D CreateGlow(GraphicsDevice graphicsDevice, int diameter, Color color, float falloff = 2f)
            => FromPixels(graphicsDevice, Math.Max(1, diameter), Math.Max(1, diameter), CreateGlowPixels(diameter, color, falloff));

        /// <summary>Creates the anti-aliased outline of a circle.</summary>
        public static Texture2D CreateRing(GraphicsDevice graphicsDevice, int diameter, float thickness, Color color)
            => FromPixels(graphicsDevice, Math.Max(1, diameter), Math.Max(1, diameter), CreateRingPixels(diameter, thickness, color));

        /// <summary>Creates an anti-aliased star.</summary>
        public static Texture2D CreateStar(GraphicsDevice graphicsDevice, int size, Color color, int points = 5, float innerRadiusRate = 0.5f)
            => FromPixels(graphicsDevice, Math.Max(1, size), Math.Max(1, size), CreateStarPixels(size, color, points, innerRadiusRate));

        /// <summary>Creates an anti-aliased diamond (a square rotated by 45 degrees).</summary>
        public static Texture2D CreateDiamond(GraphicsDevice graphicsDevice, int size, Color color)
            => FromPixels(graphicsDevice, Math.Max(1, size), Math.Max(1, size), CreateDiamondPixels(size, color));

        /// <summary>
        /// Generates the pixels of a soft circle. <paramref name="falloff"/> controls how fast it fades from the center:
        /// 1 is linear, higher values keep the light in a smaller center.
        /// </summary>
        public static Color[] CreateGlowPixels(int diameter, Color color, float falloff = 2f)
        {
            diameter = Math.Max(1, diameter);
            falloff = Math.Max(0.01f, falloff);
            var pixels = new Color[diameter * diameter];
            var radius = diameter / 2f;
            var center = new Vector2(radius);

            for (var y = 0; y < diameter; y++)
            {
                for (var x = 0; x < diameter; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    var light = MathUtils.Clamp01(1f - distance / radius);
                    pixels[y * diameter + x] = color * (float)Math.Pow(light, falloff);
                }
            }

            return pixels;
        }

        /// <summary>Generates the pixels of an anti-aliased circle outline.</summary>
        public static Color[] CreateRingPixels(int diameter, float thickness, Color color)
        {
            diameter = Math.Max(1, diameter);
            var pixels = new Color[diameter * diameter];
            var radius = diameter / 2f;
            thickness = MathUtils.Clamp(thickness, 0.5f, radius);
            var center = new Vector2(radius);

            for (var y = 0; y < diameter; y++)
            {
                for (var x = 0; x < diameter; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    var coverage = MathUtils.Clamp01(radius - distance + 0.5f) - MathUtils.Clamp01(radius - thickness - distance + 0.5f);
                    pixels[y * diameter + x] = color * coverage;
                }
            }

            return pixels;
        }

        /// <summary>Generates the pixels of an anti-aliased star with <paramref name="points"/> points.</summary>
        public static Color[] CreateStarPixels(int size, Color color, int points = 5, float innerRadiusRate = 0.5f)
        {
            size = Math.Max(1, size);
            points = Math.Max(3, points);
            innerRadiusRate = MathUtils.Clamp(innerRadiusRate, 0.05f, 1f);

            var center = new Vector2(size / 2f);
            var outerRadius = size / 2f;
            var innerRadius = outerRadius * innerRadiusRate;
            var vertices = new Vector2[points * 2];
            for (var i = 0; i < vertices.Length; i++)
            {
                var angle = -MathHelper.PiOver2 + i * MathHelper.Pi / points;
                vertices[i] = center + MathUtils.AngleToVector(angle, i % 2 == 0 ? outerRadius : innerRadius);
            }

            const int samplesPerAxis = 4;
            const float sampleWeight = 1f / (samplesPerAxis * samplesPerAxis);
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var inside = 0;
                    for (var sy = 0; sy < samplesPerAxis; sy++)
                        for (var sx = 0; sx < samplesPerAxis; sx++)
                            if (IsInsidePolygon(new Vector2(x + (sx + 0.5f) / samplesPerAxis, y + (sy + 0.5f) / samplesPerAxis), vertices))
                                inside++;
                    pixels[y * size + x] = color * (inside * sampleWeight);
                }
            }

            return pixels;
        }

        /// <summary>Generates the pixels of an anti-aliased diamond.</summary>
        public static Color[] CreateDiamondPixels(int size, Color color)
        {
            size = Math.Max(1, size);
            var pixels = new Color[size * size];
            var half = size / 2f;
            var center = new Vector2(half);

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var point = new Vector2(x + 0.5f, y + 0.5f) - center;
                    var distance = (Math.Abs(point.X) + Math.Abs(point.Y) - half) * 0.7071f; // distance to the edge
                    pixels[y * size + x] = color * MathUtils.Clamp01(0.5f - distance);
                }
            }

            return pixels;
        }

        /// <summary>Adds two premultiplied colors (MonoGame 3.8.0 has no Color + Color operator).</summary>
        private static Color Add(Color a, Color b) => new Color(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);

        /// <summary>Even-odd test of a point against a polygon.</summary>
        private static bool IsInsidePolygon(Vector2 point, Vector2[] vertices)
        {
            var inside = false;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                var a = vertices[i];
                var b = vertices[j];
                if ((a.Y > point.Y) != (b.Y > point.Y) && point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X)
                    inside = !inside;
            }

            return inside;
        }

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
