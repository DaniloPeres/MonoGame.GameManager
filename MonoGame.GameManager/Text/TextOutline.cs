using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoGame.GameManager.Text
{
    /// <summary>
    /// Draws outlines around text. The font systems loaded by <see cref="Managers.IContentLoader.LoadFontSystem"/>
    /// rasterize round outlines of any thickness (cached per size and thickness) that take any color, drawn in a
    /// single draw; other font systems are outlined by drawing the text several times around its position.
    /// </summary>
    public static class TextOutline
    {
        /// <summary>The thickest outline rasterized, in pixels of the font drawn.</summary>
        public const int MaxThickness = 64;

        private static readonly Vector2[] Directions8 = CreateDirections(8);
        private static readonly Vector2[] Directions16 = CreateDirections(16);

        /// <summary>
        /// A FontStashSharp glyph renderer whose <see cref="FontSystemEffect.Stroked"/> glyphs are round, white
        /// silhouettes grown by the effect amount, so they can be drawn in any color under the text.
        /// </summary>
        public static readonly GlyphRenderer GlyphRenderer = RenderGlyph;

        /// <summary>Settings for a <see cref="FontSystem"/> that supports colored outlines (<see cref="GlyphRenderer"/>).</summary>
        public static FontSystemSettings CreateFontSystemSettings() => new FontSystemSettings { GlyphRenderer = GlyphRenderer };

        /// <summary>True when the outlines of <paramref name="font"/> are rasterized in a single draw.</summary>
        public static bool SupportsColoredStroke(SpriteFontBase font) => font?.FontSystem?.GlyphRenderer == GlyphRenderer;

        /// <summary>
        /// Draws the outline of a text (draw the text itself afterwards, with the same arguments). The thickness is in
        /// pixels of the font (before the scale); the character spacing is in pixels of the font too, the same value
        /// given to the text.
        /// </summary>
        public static void Draw(SpriteBatch spriteBatch, SpriteFontBase font, string text, Vector2 position, Color color, float thickness,
            float rotation, Vector2 origin, Vector2 scale, float layerDepth = 0f, float characterSpacing = 0f)
        {
            if (font == null || string.IsNullOrEmpty(text) || thickness <= 0f || color.A == 0)
                return;

            if (SupportsColoredStroke(font))
            {
                var stroke = Math.Min(MaxThickness, Math.Max(1, (int)Math.Round(thickness)));
                // The stroked glyphs grow by the stroke on every side with the same render offset.
                font.DrawText(spriteBatch, text, position, color, rotation, origin + new Vector2(stroke), scale, layerDepth,
                    characterSpacing: characterSpacing, effect: FontSystemEffect.Stroked, effectAmount: stroke);
                return;
            }

            // Font systems without the glyph renderer: the text drawn around its position, in rings of 2 pixels.
            var directions = thickness > 2.5f ? Directions16 : Directions8;
            for (var ring = thickness; ring > 0f; ring -= 2f)
            {
                for (var i = 0; i < directions.Length; i++)
                    font.DrawText(spriteBatch, text, position, color, rotation, origin - directions[i] * ring, scale, layerDepth, characterSpacing);
            }
        }

        private static void RenderGlyph(byte[] input, byte[] output, GlyphRenderOptions options)
        {
            if (options.Effect != FontSystemEffect.Stroked || options.EffectAmount <= 0 || options.RasterizationMode != FontRasterizationMode.Standard)
            {
                GlyphRenderers.Default(input, output, options);
                return;
            }

            var width = options.Size.X;
            var height = options.Size.Y;
            var radius = options.EffectAmount;
            var distances = SquaredDistanceToGlyph(input, width, height);
            for (var i = 0; i < width * height; i++)
            {
                // Antialiased edge: full inside the radius, fading over the last pixel; the glyph edge keeps its own coverage.
                var coverage = MathHelper.Clamp(radius + 0.5f - (float)Math.Sqrt(distances[i]), 0f, 1f);
                var alpha = (byte)Math.Max(input[i], (int)Math.Round(coverage * 255f));
                var ci = i * 4;
                if (options.GlyphRenderResult == GlyphRenderResult.NonPremultiplied)
                {
                    output[ci] = output[ci + 1] = output[ci + 2] = 255;
                    output[ci + 3] = alpha;
                }
                else
                {
                    if (options.GlyphRenderResult == GlyphRenderResult.NoAntialiasing)
                        alpha = alpha >= 128 ? (byte)255 : (byte)0;
                    output[ci] = output[ci + 1] = output[ci + 2] = output[ci + 3] = alpha;
                }
            }
        }

        /// <summary>
        /// The squared distance of every pixel to the nearest pixel covered by the glyph (exact Euclidean distance
        /// transform of Felzenszwalb and Huttenlocher, linear in the number of pixels).
        /// </summary>
        private static float[] SquaredDistanceToGlyph(byte[] input, int width, int height)
        {
            const float Infinity = 1e20f;
            var size = Math.Max(width, height);
            var distances = new float[width * height];
            var line = new float[size];
            var result = new float[size];
            var parabolas = new int[size];
            var bounds = new float[size + 1];

            for (var i = 0; i < width * height; i++)
                distances[i] = input[i] >= 128 ? 0f : Infinity;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                    line[y] = distances[y * width + x];
                DistanceTransform1D(line, height, result, parabolas, bounds);
                for (var y = 0; y < height; y++)
                    distances[y * width + x] = result[y];
            }

            for (var y = 0; y < height; y++)
            {
                Array.Copy(distances, y * width, line, 0, width);
                DistanceTransform1D(line, width, result, parabolas, bounds);
                Array.Copy(result, 0, distances, y * width, width);
            }

            return distances;
        }

        private static void DistanceTransform1D(float[] f, int n, float[] d, int[] v, float[] z)
        {
            var k = 0;
            v[0] = 0;
            z[0] = float.NegativeInfinity;
            z[1] = float.PositiveInfinity;
            for (var q = 1; q < n; q++)
            {
                var s = ((f[q] + q * q) - (f[v[k]] + v[k] * v[k])) / (2f * q - 2f * v[k]);
                while (s <= z[k])
                {
                    k--;
                    s = ((f[q] + q * q) - (f[v[k]] + v[k] * v[k])) / (2f * q - 2f * v[k]);
                }

                k++;
                v[k] = q;
                z[k] = s;
                z[k + 1] = float.PositiveInfinity;
            }

            k = 0;
            for (var q = 0; q < n; q++)
            {
                while (z[k + 1] < q)
                    k++;
                var offset = q - v[k];
                d[q] = offset * offset + f[v[k]];
            }
        }

        private static Vector2[] CreateDirections(int count)
        {
            var directions = new Vector2[count];
            for (var i = 0; i < count; i++)
            {
                var angle = i * MathHelper.TwoPi / count;
                directions[i] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            }

            return directions;
        }
    }
}
