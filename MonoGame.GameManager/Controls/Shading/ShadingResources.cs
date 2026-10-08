using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// Shared resources of the shading effects (<see cref="Shadow"/>, <see cref="Glow"/>, <see cref="Outline"/>,
    /// <see cref="GradientFill"/>, <see cref="InnerShadow"/>, <see cref="InnerGlow"/>), created
    /// on demand for the current graphics device and released with the screen manager: the blend states, a sprite batch
    /// and the scratch render targets used while the shapes are rendered.
    /// </summary>
    /// <remarks>
    /// The shapes are rendered as white textures with straight alpha: every intermediate pass writes only the alpha
    /// channel of a target cleared to transparent white, so the colors of what the control draws never leak into its
    /// shading. They are drawn with <see cref="PaintBlendState"/> or <see cref="LightBlendState"/>.
    /// </remarks>
    public static class ShadingResources
    {
        /// <summary>The maximum width and height of the render targets.</summary>
        public const int MaxTargetSize = 2048;

        /// <summary>Transparent white: the clear color of the shape targets (RGB stays white, only the alpha is written).</summary>
        internal static readonly Color TransparentWhite = new Color(255, 255, 255, 0);

        /// <summary>Opaque white: the clear color of the inner shapes (the blurred silhouette is subtracted from it).</summary>
        internal static readonly Color OpaqueWhite = new Color(255, 255, 255, 255);

        private const int ScratchCount = 3;
        private const int SizeStep = 64;

        private static readonly RenderTarget2D[] scratchTargets = new RenderTarget2D[ScratchCount];
        private static BlendState paintBlendState;
        private static BlendState lightBlendState;
        private static BlendState alphaUnionBlendState;
        private static BlendState alphaAddBlendState;
        private static BlendState alphaSubtractBlendState;
        private static BlendState alphaMultiplyBlendState;
        private static BlendState colorReplaceBlendState;
        private static SpriteBatch spriteBatch;
        private static Texture2D sparkleTexture;
        private static readonly Dictionary<ShadingPattern, Texture2D> patternTextures = new Dictionary<ShadingPattern, Texture2D>();

        /// <summary>The size of the generated pattern and sparkle textures (a power of two, so they can be tiled).</summary>
        internal const int PatternSize = 64;
        private static GraphicsDevice graphicsDevice;

        /// <summary>
        /// Paints a white texture with straight alpha, tinted by a straight color: color source alpha / inverse source
        /// alpha, alpha one / inverse source alpha (correct over opaque screens and inside transparent render targets).
        /// </summary>
        public static BlendState PaintBlendState
        {
            get
            {
                if (paintBlendState == null || paintBlendState.IsDisposed)
                {
                    paintBlendState = new BlendState
                    {
                        Name = "ShadingResources.Paint",
                        ColorSourceBlend = Blend.SourceAlpha,
                        ColorDestinationBlend = Blend.InverseSourceAlpha,
                        AlphaSourceBlend = Blend.One,
                        AlphaDestinationBlend = Blend.InverseSourceAlpha
                    };
                }

                return paintBlendState;
            }
        }

        /// <summary>
        /// Adds a white texture with straight alpha, tinted by a straight color, to what is below and keeps its alpha
        /// (color source alpha / one, alpha zero / one).
        /// </summary>
        public static BlendState LightBlendState
        {
            get
            {
                if (lightBlendState == null || lightBlendState.IsDisposed)
                {
                    lightBlendState = new BlendState
                    {
                        Name = "ShadingResources.Light",
                        ColorSourceBlend = Blend.SourceAlpha,
                        ColorDestinationBlend = Blend.One,
                        AlphaSourceBlend = Blend.Zero,
                        AlphaDestinationBlend = Blend.One
                    };
                }

                return lightBlendState;
            }
        }

        /// <summary>Writes only the alpha: the union of what is drawn and what is below (one / inverse source alpha).</summary>
        internal static BlendState AlphaUnionBlendState
        {
            get
            {
                if (alphaUnionBlendState == null || alphaUnionBlendState.IsDisposed)
                {
                    alphaUnionBlendState = new BlendState
                    {
                        Name = "ShadingResources.AlphaUnion",
                        ColorSourceBlend = Blend.One,
                        ColorDestinationBlend = Blend.Zero,
                        AlphaSourceBlend = Blend.One,
                        AlphaDestinationBlend = Blend.InverseSourceAlpha,
                        ColorWriteChannels = ColorWriteChannels.Alpha
                    };
                }

                return alphaUnionBlendState;
            }
        }

        /// <summary>Writes only the alpha: the sum of what is drawn and what is below (weighted taps of a blur).</summary>
        internal static BlendState AlphaAddBlendState
        {
            get
            {
                if (alphaAddBlendState == null || alphaAddBlendState.IsDisposed)
                {
                    alphaAddBlendState = new BlendState
                    {
                        Name = "ShadingResources.AlphaAdd",
                        ColorSourceBlend = Blend.One,
                        ColorDestinationBlend = Blend.Zero,
                        AlphaSourceBlend = Blend.One,
                        AlphaDestinationBlend = Blend.One,
                        ColorWriteChannels = ColorWriteChannels.Alpha
                    };
                }

                return alphaAddBlendState;
            }
        }

        /// <summary>Writes only the alpha: what is below minus what is drawn (destination - source).</summary>
        internal static BlendState AlphaSubtractBlendState
        {
            get
            {
                if (alphaSubtractBlendState == null || alphaSubtractBlendState.IsDisposed)
                {
                    alphaSubtractBlendState = new BlendState
                    {
                        Name = "ShadingResources.AlphaSubtract",
                        ColorSourceBlend = Blend.One,
                        ColorDestinationBlend = Blend.Zero,
                        AlphaBlendFunction = BlendFunction.ReverseSubtract,
                        AlphaSourceBlend = Blend.One,
                        AlphaDestinationBlend = Blend.One,
                        ColorWriteChannels = ColorWriteChannels.Alpha
                    };
                }

                return alphaSubtractBlendState;
            }
        }

        /// <summary>Writes only the alpha: what is below multiplied by what is drawn.</summary>
        internal static BlendState AlphaMultiplyBlendState
        {
            get
            {
                if (alphaMultiplyBlendState == null || alphaMultiplyBlendState.IsDisposed)
                {
                    alphaMultiplyBlendState = new BlendState
                    {
                        Name = "ShadingResources.AlphaMultiply",
                        ColorSourceBlend = Blend.One,
                        ColorDestinationBlend = Blend.Zero,
                        AlphaSourceBlend = Blend.Zero,
                        AlphaDestinationBlend = Blend.SourceAlpha,
                        ColorWriteChannels = ColorWriteChannels.Alpha
                    };
                }

                return alphaMultiplyBlendState;
            }
        }

        /// <summary>Writes only the color (red, green and blue) of what is drawn, and keeps the alpha below (gradient fills).</summary>
        internal static BlendState ColorReplaceBlendState
        {
            get
            {
                if (colorReplaceBlendState == null || colorReplaceBlendState.IsDisposed)
                {
                    colorReplaceBlendState = new BlendState
                    {
                        Name = "ShadingResources.ColorReplace",
                        ColorSourceBlend = Blend.One,
                        ColorDestinationBlend = Blend.Zero,
                        AlphaSourceBlend = Blend.Zero,
                        AlphaDestinationBlend = Blend.One,
                        ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue
                    };
                }

                return colorReplaceBlendState;
            }
        }

        /// <summary>The sprite batch used to render the shapes.</summary>
        internal static SpriteBatch SpriteBatch
        {
            get
            {
                var device = EnsureGraphicsDevice();
                if (spriteBatch == null || spriteBatch.IsDisposed)
                    spriteBatch = new SpriteBatch(device);
                return spriteBatch;
            }
        }

        /// <summary>
        /// A scratch render target of at least <paramref name="width"/> x <paramref name="height"/> pixels (it only grows;
        /// its content is discarded, clear it before use).
        /// </summary>
        internal static RenderTarget2D GetScratch(int index, int width, int height)
        {
            var device = EnsureGraphicsDevice();
            var target = scratchTargets[index];
            if (target != null && !target.IsDisposed && target.Width >= width && target.Height >= height)
                return target;

            var newWidth = Math.Min(MaxTargetSize, RoundUp(Math.Max(width, target?.Width ?? 0)));
            var newHeight = Math.Min(MaxTargetSize, RoundUp(Math.Max(height, target?.Height ?? 0)));
            target?.Dispose();
            scratchTargets[index] = target = new RenderTarget2D(device, newWidth, newHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            return target;
        }

        /// <summary>Disposes the shared resources (they are recreated on demand).</summary>
        internal static void Reset()
        {
            ResetGraphics();
            paintBlendState?.Dispose();
            paintBlendState = null;
            lightBlendState?.Dispose();
            lightBlendState = null;
            alphaUnionBlendState?.Dispose();
            alphaUnionBlendState = null;
            alphaAddBlendState?.Dispose();
            alphaAddBlendState = null;
            alphaSubtractBlendState?.Dispose();
            alphaSubtractBlendState = null;
            alphaMultiplyBlendState?.Dispose();
            alphaMultiplyBlendState = null;
            colorReplaceBlendState?.Dispose();
            colorReplaceBlendState = null;
        }

        private static int RoundUp(int value) => (value + SizeStep - 1) / SizeStep * SizeStep;

        /// <summary>A white four-pointed star with a soft core (alpha only), <see cref="PatternSize"/> pixels wide.</summary>
        internal static Texture2D SparkleTexture
        {
            get
            {
                var device = EnsureGraphicsDevice();
                if (sparkleTexture == null || sparkleTexture.IsDisposed)
                    sparkleTexture = TextureFactory.FromPixels(device, PatternSize, PatternSize, CreateSparklePixels(PatternSize));
                return sparkleTexture;
            }
        }

        /// <summary>The tile of a pattern (alpha only, seamless), <see cref="PatternSize"/> pixels wide.</summary>
        internal static Texture2D GetPattern(ShadingPattern pattern)
        {
            var device = EnsureGraphicsDevice();
            if (patternTextures.TryGetValue(pattern, out var texture) && !texture.IsDisposed)
                return texture;
            texture = TextureFactory.FromPixels(device, PatternSize, PatternSize, CreatePatternPixels(pattern, PatternSize));
            patternTextures[pattern] = texture;
            return texture;
        }

        private static Color[] CreateSparklePixels(int size)
        {
            var pixels = new Color[size * size];
            var half = size / 2f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = Math.Abs(x + 0.5f - half) / half;
                    var dy = Math.Abs(y + 0.5f - half) / half;
                    // Four rays that thin out towards their tips, four shorter diagonal rays and a round core.
                    var rays = Math.Max((float)Math.Exp(-dy * 9f) * (float)Math.Pow(Math.Max(0f, 1f - dx), 1.5),
                        (float)Math.Exp(-dx * 9f) * (float)Math.Pow(Math.Max(0f, 1f - dy), 1.5));
                    var diagonalX = Math.Abs(dx - dy) * 0.7071f;
                    var diagonal = (float)Math.Exp(-diagonalX * 14f) * Math.Max(0f, 1f - (dx + dy) * 1.1f) * 0.4f;
                    var core = (float)Math.Exp(-(dx * dx + dy * dy) * 10f);
                    var alpha = MathUtils.Clamp01(Math.Max(Math.Max(rays, diagonal), core));
                    pixels[y * size + x] = new Color(255, 255, 255, (int)(alpha * 255f + 0.5f));
                }
            }

            return pixels;
        }

        private static Color[] CreatePatternPixels(ShadingPattern pattern, int size)
        {
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var u = (x + 0.5f) / size;
                    var v = (y + 0.5f) / size;
                    float alpha;
                    switch (pattern)
                    {
                        case ShadingPattern.Dots:
                            alpha = Smooth(0.3f - Distance(u, v, 0.5f, 0.5f), 1.5f / size);
                            break;
                        case ShadingPattern.Checker:
                            alpha = (u < 0.5f) != (v < 0.5f) ? 1f : 0f;
                            break;
                        case ShadingPattern.Diagonal:
                            {
                                var band = Fraction((u + v) * 2f);
                                alpha = Smooth(0.2f - Math.Abs(band - 0.5f), 2f / size);
                                break;
                            }
                        case ShadingPattern.Grid:
                            alpha = Smooth(1.5f / size - Math.Min(Math.Min(u, 1f - u), Math.Min(v, 1f - v)), 1f / size);
                            break;
                        case ShadingPattern.Scales:
                            alpha = ScaleAt(u, v);
                            break;
                        case ShadingPattern.ScanLines:
                            alpha = Fraction(v * 8f) < 0.4f ? 1f : 0f;
                            break;
                        case ShadingPattern.Blotches:
                            {
                                var noise = TileNoise(u, v, 3, 11) * 0.7f + TileNoise(u, v, 6, 23) * 0.3f;
                                alpha = MathUtils.Clamp01((noise - 0.42f) / 0.16f);
                                break;
                            }
                        default:
                            alpha = TileNoise(u, v, 8, 7) * 0.5f + TileNoise(u, v, 16, 13) * 0.3f + TileNoise(u, v, 32, 29) * 0.2f;
                            alpha = MathUtils.Clamp01((alpha - 0.2f) / 0.6f);
                            break;
                    }

                    pixels[y * size + x] = new Color(255, 255, 255, (int)(MathUtils.Clamp01(alpha) * 255f + 0.5f));
                }
            }

            return pixels;
        }

        private static float Fraction(float value) => value - (float)Math.Floor(value);

        private static float Distance(float x0, float y0, float x1, float y1) => (float)Math.Sqrt((x0 - x1) * (x0 - x1) + (y0 - y1) * (y0 - y1));

        /// <summary>1 inside (positive value), 0 outside, with an anti-aliased edge of the given width.</summary>
        private static float Smooth(float value, float edge) => MathUtils.Clamp01(value / edge + 0.5f);

        /// <summary>Fish scales: rows of circles, each row half a circle lower and shifted; darker towards their edge.</summary>
        private static float ScaleAt(float u, float v)
        {
            // Centers of the circles of the tile and of its neighbors (rows every half tile, shifted by half a tile).
            var bestY = float.MinValue;
            var value = 0f;
            for (var row = -1; row <= 3; row++)
            {
                var cy = row * 0.5f;
                var shift = (row & 1) == 0 ? 0f : 0.5f;
                for (var column = -1; column <= 2; column++)
                {
                    var cx = column + shift;
                    var distance = Distance(u, v, cx, cy) / 0.5f;
                    if (distance <= 1f && cy > bestY)
                    {
                        bestY = cy;
                        value = distance;
                    }
                }
            }

            return MathUtils.Clamp01((value - 0.55f) / 0.45f);
        }

        /// <summary>Seamless value noise with <paramref name="cells"/> cells across the tile, from 0 to 1.</summary>
        private static float TileNoise(float u, float v, int cells, int seed)
        {
            var x = u * cells;
            var y = v * cells;
            var x0 = (int)Math.Floor(x);
            var y0 = (int)Math.Floor(y);
            var fx = x - x0;
            var fy = y - y0;
            fx = fx * fx * (3f - 2f * fx);
            fy = fy * fy * (3f - 2f * fy);
            float Corner(int cx, int cy) => Hash(((cx % cells) + cells) % cells, ((cy % cells) + cells) % cells, seed);
            var top = Corner(x0, y0) + (Corner(x0 + 1, y0) - Corner(x0, y0)) * fx;
            var bottom = Corner(x0, y0 + 1) + (Corner(x0 + 1, y0 + 1) - Corner(x0, y0 + 1)) * fx;
            return top + (bottom - top) * fy;
        }

        private static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                var h = (uint)(x * 374761393 + y * 668265263 + seed * 2246822519);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / (float)0xFFFFFF;
            }
        }

        private static void ResetGraphics()
        {
            for (var i = 0; i < scratchTargets.Length; i++)
            {
                scratchTargets[i]?.Dispose();
                scratchTargets[i] = null;
            }

            sparkleTexture?.Dispose();
            sparkleTexture = null;
            foreach (var texture in patternTextures.Values)
                texture.Dispose();
            patternTextures.Clear();

            spriteBatch?.Dispose();
            spriteBatch = null;
            graphicsDevice = null;
        }

        /// <summary>The render targets belong to a graphics device: when it changes they are released.</summary>
        private static GraphicsDevice EnsureGraphicsDevice()
        {
            var device = ServiceProvider.GraphicsDevice
                ?? throw new InvalidOperationException("The graphics device is not available yet.");

            if (graphicsDevice != device)
            {
                ResetGraphics();
                graphicsDevice = device;
            }

            return device;
        }
    }
}
