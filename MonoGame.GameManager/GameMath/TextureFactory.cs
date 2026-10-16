using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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

        /// <summary>Creates the light around the edge of a rounded rectangle (see <see cref="CreateRoundedRectangleGlowPixels"/>).</summary>
        public static Texture2D CreateRoundedRectangleGlow(GraphicsDevice graphicsDevice, int width, int height, float cornerRadius, float outerSpread, float innerSpread, float falloff, Color color)
            => FromPixels(graphicsDevice, Math.Max(1, width), Math.Max(1, height), CreateRoundedRectangleGlowPixels(width, height, cornerRadius, outerSpread, innerSpread, falloff, color));

        /// <summary>Creates a soft line inside the edge of a rounded rectangle (see <see cref="CreateRoundedRectangleRimPixels"/>).</summary>
        public static Texture2D CreateRoundedRectangleRim(GraphicsDevice graphicsDevice, int width, int height, float cornerRadius, float thickness, float softness, Color color)
            => FromPixels(graphicsDevice, Math.Max(1, width), Math.Max(1, height), CreateRoundedRectangleRimPixels(width, height, cornerRadius, thickness, softness, color));

        /// <summary>Creates a filled rounded rectangle with a soft edge (see <see cref="CreateSoftRoundedRectanglePixels"/>).</summary>
        public static Texture2D CreateSoftRoundedRectangle(GraphicsDevice graphicsDevice, int width, int height, float cornerRadius, float softness, Color color)
            => FromPixels(graphicsDevice, Math.Max(1, width), Math.Max(1, height), CreateSoftRoundedRectanglePixels(width, height, cornerRadius, softness, color));

        /// <summary>Creates a vertical gradient one pixel wide, opaque at the top (see <see cref="CreateVerticalGradientPixels"/>).</summary>
        public static Texture2D CreateVerticalGradient(GraphicsDevice graphicsDevice, int height, float falloff, Color color)
            => FromPixels(graphicsDevice, 1, Math.Max(2, height), CreateVerticalGradientPixels(height, falloff, color));

        /// <summary>Creates a horizontal soft band one pixel high (see <see cref="CreateSoftBandPixels"/>).</summary>
        /// <summary>Creates a horizontal gradient one pixel high (see <see cref="CreateGradientPixels"/>).</summary>
        public static Texture2D CreateGradient(GraphicsDevice graphicsDevice, int width, IList<Color> colors, IList<float> positions = null, float hardness = 0f, bool keepAlpha = false)
            => FromPixels(graphicsDevice, Math.Max(2, width), 1, CreateGradientPixels(width, colors, positions, hardness, keepAlpha));

        public static Texture2D CreateSoftBand(GraphicsDevice graphicsDevice, int width, Color color)
            => FromPixels(graphicsDevice, Math.Max(3, width), 1, CreateSoftBandPixels(width, color));

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

        /// <summary>
        /// Generates the pixels of the light around (and inside) the edge of a rounded rectangle, used to draw glows that
        /// fit any size with nine-slice scaling. The rounded rectangle is inset by <paramref name="outerSpread"/> from the
        /// edges of the texture; the light is 1 on its edge and fades to 0 at <paramref name="outerSpread"/> outside and
        /// at <paramref name="innerSpread"/> inside it.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="cornerRadius">The corner radius of the rounded rectangle.</param>
        /// <param name="outerSpread">How far the light goes outside the shape (also the margin of the shape in the texture).</param>
        /// <param name="innerSpread">How far the light goes inside the shape (0 = the inside is dark).</param>
        /// <param name="falloff">1 is a linear fade, higher values keep the light closer to the edge.</param>
        /// <param name="color">The color of the light.</param>
        public static Color[] CreateRoundedRectangleGlowPixels(int width, int height, float cornerRadius, float outerSpread, float innerSpread, float falloff, Color color)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            outerSpread = Math.Max(0f, outerSpread);
            innerSpread = Math.Max(0f, innerSpread);
            falloff = Math.Max(0.01f, falloff);
            var halfSize = Vector2.Max(new Vector2(width / 2f - outerSpread, height / 2f - outerSpread), new Vector2(0.5f));
            var radius = MathUtils.Clamp(cornerRadius, 0f, Math.Min(halfSize.X, halfSize.Y));
            var center = new Vector2(width / 2f, height / 2f);
            var pixels = new Color[width * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var distance = RoundedRectangleDistance(new Vector2(x + 0.5f, y + 0.5f) - center, halfSize, radius);
                    float light;
                    if (distance >= 0f)
                        light = outerSpread > 0f ? 1f - distance / outerSpread : 0.5f - distance;
                    else
                        light = innerSpread > 0f ? 1f + distance / innerSpread : 0.5f + distance;
                    pixels[y * width + x] = color * (float)Math.Pow(MathUtils.Clamp01(light), falloff);
                }
            }

            return pixels;
        }

        /// <summary>
        /// Generates the pixels of a soft line that follows the inside of the edge of a rounded rectangle (a rim of
        /// light). The rounded rectangle is inset by <paramref name="softness"/> from the edges of the texture; the
        /// line is <paramref name="thickness"/> wide, inside the shape, and fades over <paramref name="softness"/> on
        /// both sides.
        /// </summary>
        public static Color[] CreateRoundedRectangleRimPixels(int width, int height, float cornerRadius, float thickness, float softness, Color color)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            thickness = Math.Max(0f, thickness);
            softness = Math.Max(0f, softness);
            var halfSize = Vector2.Max(new Vector2(width / 2f - softness, height / 2f - softness), new Vector2(0.5f));
            var radius = MathUtils.Clamp(cornerRadius, 0f, Math.Min(halfSize.X, halfSize.Y));
            var center = new Vector2(width / 2f, height / 2f);
            var halfThickness = thickness / 2f;
            var pixels = new Color[width * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var distance = RoundedRectangleDistance(new Vector2(x + 0.5f, y + 0.5f) - center, halfSize, radius);
                    var fromLine = Math.Abs(distance + halfThickness) - halfThickness; // <= 0 inside the line
                    var light = softness > 0f ? 1f - fromLine / softness : 0.5f - fromLine;
                    pixels[y * width + x] = color * MathUtils.Clamp01(light);
                }
            }

            return pixels;
        }

        /// <summary>
        /// Generates the pixels of a filled rounded rectangle with a soft edge: opaque inside, fading over
        /// <paramref name="softness"/> across its edge (half inside, half outside). The rounded rectangle is inset by
        /// half of the softness from the edges of the texture, so the fade fits in it.
        /// </summary>
        public static Color[] CreateSoftRoundedRectanglePixels(int width, int height, float cornerRadius, float softness, Color color)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            softness = Math.Max(0f, softness);
            var halfSize = Vector2.Max(new Vector2(width / 2f - softness / 2f, height / 2f - softness / 2f), new Vector2(0.5f));
            var radius = MathUtils.Clamp(cornerRadius, 0f, Math.Min(halfSize.X, halfSize.Y));
            var center = new Vector2(width / 2f, height / 2f);
            var pixels = new Color[width * height];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var distance = RoundedRectangleDistance(new Vector2(x + 0.5f, y + 0.5f) - center, halfSize, radius);
                    var light = softness > 1f ? 0.5f - distance / softness : 0.5f - distance;
                    light = MathUtils.Clamp01(light);
                    pixels[y * width + x] = color * (light * light * (3f - 2f * light));
                }
            }

            return pixels;
        }

        /// <summary>
        /// Generates the pixels of a vertical gradient one pixel wide: opaque at the top, transparent at the bottom.
        /// <paramref name="falloff"/> 1 is linear, higher values fade faster.
        /// </summary>
        public static Color[] CreateVerticalGradientPixels(int height, float falloff, Color color)
        {
            height = Math.Max(2, height);
            falloff = Math.Max(0.01f, falloff);
            var pixels = new Color[height];
            for (var y = 0; y < height; y++)
                pixels[y] = color * (float)Math.Pow(1f - y / (float)(height - 1), falloff);
            return pixels;
        }

        /// <summary>
        /// Generates the pixels of a horizontal gradient one pixel high, from the first color (left) to the last (right).
        /// By default it is opaque (the alpha of the colors is ignored, their straight color is used); with
        /// <paramref name="keepAlpha"/> the pixels have the straight color and the alpha of the colors (not premultiplied),
        /// and a fully transparent color takes the color of its nearest visible neighbor, so it fades without darkening.
        /// </summary>
        /// <param name="width">The number of pixels.</param>
        /// <param name="colors">The colors, from the left to the right (one color = a solid line).</param>
        /// <param name="positions">Where each color is, from 0 to 1, in increasing order; null (or a list of another length) spreads them evenly.</param>
        /// <param name="hardness">How sharp the passage from a color to the next is: 0 is linear, 1 is a hard edge in the middle.</param>
        /// <param name="keepAlpha">True to keep the alpha of the colors (straight alpha), false for an opaque gradient.</param>
        public static Color[] CreateGradientPixels(int width, IList<Color> colors, IList<float> positions = null, float hardness = 0f, bool keepAlpha = false)
        {
            width = Math.Max(2, width);
            var pixels = new Color[width];
            if (colors == null || colors.Count == 0)
            {
                for (var x = 0; x < width; x++)
                    pixels[x] = Color.White;
                return pixels;
            }

            var count = colors.Count;
            var stops = new float[count];
            for (var i = 0; i < count; i++)
                stops[i] = positions != null && positions.Count == count ? MathUtils.Clamp01(positions[i]) : count == 1 ? 0f : i / (float)(count - 1);

            var straight = new Color[count];
            for (var i = 0; i < count; i++)
            {
                straight[i] = ToOpaqueStraight(colors[i]);
                if (keepAlpha)
                    straight[i].A = colors[i].A;
            }

            if (keepAlpha)
            {
                // A transparent stop takes the color of its nearest visible neighbor (a fade, not a passage through black).
                for (var i = 0; i < count; i++)
                {
                    if (colors[i].A != 0)
                        continue;
                    for (var distance = 1; distance < count; distance++)
                    {
                        var neighbor = i - distance >= 0 && colors[i - distance].A != 0 ? i - distance
                            : i + distance < count && colors[i + distance].A != 0 ? i + distance
                            : -1;
                        if (neighbor < 0)
                            continue;
                        straight[i] = new Color(straight[neighbor].R, straight[neighbor].G, straight[neighbor].B, (byte)0);
                        break;
                    }
                }
            }

            // A hardness of h squeezes each passage into the middle (1 - h) of its segment.
            var squeeze = 1f - Math.Min(0.999f, MathUtils.Clamp01(hardness));
            for (var x = 0; x < width; x++)
            {
                var t = (x + 0.5f) / width;
                if (t <= stops[0] || count == 1)
                {
                    pixels[x] = straight[0];
                    continue;
                }

                if (t >= stops[count - 1])
                {
                    pixels[x] = straight[count - 1];
                    continue;
                }

                var index = 0;
                while (index < count - 2 && t > stops[index + 1])
                    index++;

                var length = stops[index + 1] - stops[index];
                var amount = length <= 0f ? 1f : (t - stops[index]) / length;
                amount = MathUtils.Clamp01((amount - 0.5f) / squeeze + 0.5f);
                pixels[x] = Color.Lerp(straight[index], straight[index + 1], amount);
            }

            return pixels;
        }

        private static Color ToOpaqueStraight(Color color)
        {
            if (color.A == 255 || color.A == 0)
                return new Color(color.R, color.G, color.B, (byte)255);
            var alpha = color.A / 255f;
            return new Color((int)Math.Min(255f, color.R / alpha + 0.5f), (int)Math.Min(255f, color.G / alpha + 0.5f), (int)Math.Min(255f, color.B / alpha + 0.5f), 255);
        }

        /// <summary>
        /// Generates the pixels of a horizontal soft band one pixel high: transparent at both ends and opaque in the
        /// middle (a smooth bump), used for shine sweeps and light streaks.
        /// </summary>
        public static Color[] CreateSoftBandPixels(int width, Color color)
        {
            width = Math.Max(3, width);
            var pixels = new Color[width];
            for (var x = 0; x < width; x++)
            {
                var light = 0.5f - 0.5f * (float)Math.Cos((x + 0.5f) / width * MathHelper.TwoPi);
                pixels[x] = color * (light * light);
            }

            return pixels;
        }

        /// <summary>
        /// Signed distance from a point to the edge of a rounded rectangle: negative inside, positive outside.
        /// </summary>
        /// <param name="point">The point, relative to the center of the rectangle.</param>
        /// <param name="halfSize">Half of the size of the rectangle.</param>
        /// <param name="radius">The corner radius (at most the smallest half size).</param>
        public static float RoundedRectangleDistance(Vector2 point, Vector2 halfSize, float radius)
        {
            var q = point.Abs() - halfSize + new Vector2(radius);
            var outside = Vector2.Max(q, Vector2.Zero).Length();
            var inside = Math.Min(Math.Max(q.X, q.Y), 0f);
            return outside + inside - radius;
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
    }
}
