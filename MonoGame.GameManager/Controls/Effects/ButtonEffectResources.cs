using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Particles;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>
    /// Shared resources of the button effects, created on demand for the current graphics device and released with the
    /// screen manager: the blend states, the light shapes, and the generated frames (glows, rims and masks that follow a
    /// rounded rectangle and fit any size with nine-slice scaling).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The frames are cached by corner radius, spread and resolution. When the cache is full, the frames not used for
    /// the longest time are removed (their textures are released before the next frame is drawn), so animating a radius
    /// does not use more and more memory.
    /// </para>
    /// <para>
    /// The small images (light shapes, gradients, bands and frames up to <see cref="EffectAtlas.MaxImageSize"/> pixels)
    /// share the pages of an atlas, so the effects are drawn without switching textures. Get them again when drawing
    /// (<see cref="EffectSprite"/>, <see cref="LightFrame"/>): the atlas is rebuilt when it is full.
    /// </para>
    /// </remarks>
    public static class ButtonEffectResources
    {
        /// <summary>The maximum number of cached frames.</summary>
        public const int MaxCachedFrames = 512;

        private const int ShapeTextureSize = ParticleResources.ShapeTextureSize;
        private const int GradientHeight = 64;
        private const int SoftBandWidth = 64;

        private static readonly Dictionary<(int kind, int radius, int a, int b, int c, int density), LightFrame> frames
            = new Dictionary<(int, int, int, int, int, int), LightFrame>();
        private static readonly Dictionary<(int kind, int radius, int a, int b, int c, int density), long> frameUses
            = new Dictionary<(int, int, int, int, int, int), long>();
        private static readonly List<(int, int, int, int, int, int)> evictionCandidates = new List<(int, int, int, int, int, int)>();
        private static readonly Dictionary<int, Texture2D> gradients = new Dictionary<int, Texture2D>();
        private static readonly Dictionary<int, EffectSprite> gradientSprites = new Dictionary<int, EffectSprite>();
        private static readonly EffectSprite[] shapeSprites = new EffectSprite[Enum.GetValues(typeof(LightShape)).Length];
        private static readonly List<Texture2D> pendingDisposal = new List<Texture2D>();
        private static readonly List<(int, int, int, int, int, int)> invalidFrames = new List<(int, int, int, int, int, int)>();
        private static EffectSprite softBandSprite;
        private static EffectSprite whiteSprite;
        private static Texture2D flareTexture;
        private static Texture2D softBandTexture;
        private static BlendState lightBlendState;
        private static BlendState maskBlendState;
        private static GraphicsDevice graphicsDevice;

        /// <summary>
        /// Adds the color to what is below and keeps its alpha (source One, destination One for the color; the alpha of
        /// the destination is kept). Lights drawn with it work over opaque screens and inside transparent render
        /// targets, with the premultiplied textures of the content pipeline and of <see cref="TextureFactory"/>.
        /// </summary>
        public static BlendState LightBlendState
        {
            get
            {
                if (lightBlendState == null || lightBlendState.IsDisposed)
                {
                    lightBlendState = new BlendState
                    {
                        Name = "ButtonEffectResources.Light",
                        ColorSourceBlend = Blend.One,
                        ColorDestinationBlend = Blend.One,
                        AlphaSourceBlend = Blend.Zero,
                        AlphaDestinationBlend = Blend.One
                    };
                }

                return lightBlendState;
            }
        }

        /// <summary>Multiplies what is below by the alpha of what is drawn (used to clip the lights to a shape).</summary>
        public static BlendState MaskBlendState
        {
            get
            {
                if (maskBlendState == null || maskBlendState.IsDisposed)
                {
                    maskBlendState = new BlendState
                    {
                        Name = "ButtonEffectResources.Mask",
                        ColorSourceBlend = Blend.Zero,
                        ColorDestinationBlend = Blend.SourceAlpha,
                        AlphaSourceBlend = Blend.Zero,
                        AlphaDestinationBlend = Blend.SourceAlpha
                    };
                }

                return maskBlendState;
            }
        }

        /// <summary>
        /// The number of texture pixels per local unit for a control drawn at a scale: 1 at its size, up to 3 when it
        /// is scaled up, so the generated frames stay sharp.
        /// </summary>
        public static int GetDensity(Vector2 scale)
            => Math.Max(1, Math.Min(3, (int)Math.Ceiling(Math.Max(Math.Abs(scale.X), Math.Abs(scale.Y)) - 0.05f)));

        /// <summary>The texture of a light shape: white, <see cref="ParticleResources.ShapeTextureSize"/> pixels wide.</summary>
        public static Texture2D GetShape(LightShape shape)
        {
            switch (shape)
            {
                case LightShape.Flare:
                    var device = EnsureGraphicsDevice();
                    if (flareTexture == null || flareTexture.IsDisposed)
                        flareTexture = CreateFlare(device);
                    return flareTexture;
                case LightShape.Star:
                    return ParticleResources.GetTexture(ParticleShape.Star);
                case LightShape.Diamond:
                    return ParticleResources.GetTexture(ParticleShape.Diamond);
                case LightShape.Circle:
                    return ParticleResources.GetTexture(ParticleShape.Circle);
                case LightShape.Ring:
                    return ParticleResources.GetTexture(ParticleShape.Ring);
                default:
                    return ParticleResources.GetTexture(ParticleShape.Glow);
            }
        }

        /// <summary>
        /// The image of a light shape (the same as <see cref="GetShape"/>) in the atlas of the effects:
        /// <see cref="ParticleResources.ShapeTextureSize"/> pixels wide.
        /// </summary>
        public static EffectSprite GetShapeSprite(LightShape shape)
        {
            var device = EnsureGraphicsDevice();
            var index = (int)shape;
            if (index < 0 || index >= shapeSprites.Length)
                index = (int)LightShape.Glow;
            ref var sprite = ref shapeSprites[index];
            if (!sprite.IsValid)
                sprite = CreateSprite(device, ShapeTextureSize, ShapeTextureSize, CreateShapePixels((LightShape)index));
            return sprite;
        }

        /// <summary>A white pixel in the atlas of the effects, to fill rectangles.</summary>
        public static EffectSprite WhitePixel
        {
            get
            {
                var device = EnsureGraphicsDevice();
                if (!whiteSprite.IsValid)
                    whiteSprite = CreateSprite(device, 1, 1, new[] { Color.White });
                return whiteSprite;
            }
        }

        /// <summary>The soft band of <see cref="SoftBand"/> in the atlas of the effects.</summary>
        public static EffectSprite SoftBandSprite
        {
            get
            {
                var device = EnsureGraphicsDevice();
                if (!softBandSprite.IsValid)
                    softBandSprite = CreateSprite(device, SoftBandWidth, 1, TextureFactory.CreateSoftBandPixels(SoftBandWidth, Color.White));
                return softBandSprite;
            }
        }

        /// <summary>The vertical gradient of <see cref="GetVerticalGradient"/> in the atlas of the effects.</summary>
        public static EffectSprite GetVerticalGradientSprite(float falloff)
        {
            var device = EnsureGraphicsDevice();
            var key = (int)Math.Round(MathUtils.Clamp(falloff, 0.1f, 10f) * 10f);
            if (!gradientSprites.TryGetValue(key, out var sprite) || !sprite.IsValid)
                gradientSprites[key] = sprite = CreateSprite(device, 1, GradientHeight, TextureFactory.CreateVerticalGradientPixels(GradientHeight, key / 10f, Color.White));
            return sprite;
        }

        /// <summary>A horizontal soft band, <c>64x1</c> pixels: transparent at both ends, white in the middle.</summary>
        public static Texture2D SoftBand
        {
            get
            {
                var device = EnsureGraphicsDevice();
                if (softBandTexture == null || softBandTexture.IsDisposed)
                    softBandTexture = TextureFactory.CreateSoftBand(device, SoftBandWidth, Color.White);
                return softBandTexture;
            }
        }

        /// <summary>A vertical gradient, <c>1x64</c> pixels: white at the top, transparent at the bottom.</summary>
        /// <param name="falloff">1 is linear, higher values fade faster (rounded to a tenth).</param>
        public static Texture2D GetVerticalGradient(float falloff)
        {
            var device = EnsureGraphicsDevice();
            var key = (int)Math.Round(MathUtils.Clamp(falloff, 0.1f, 10f) * 10f);
            if (!gradients.TryGetValue(key, out var texture) || texture.IsDisposed)
                gradients[key] = texture = TextureFactory.CreateVerticalGradient(device, GradientHeight, key / 10f, Color.White);
            return texture;
        }

        /// <summary>
        /// A white glow that follows the edge of a rounded rectangle: it fades over <paramref name="outerSpread"/>
        /// outside the shape and over <paramref name="innerSpread"/> inside it.
        /// </summary>
        /// <param name="cornerRadius">The corner radius of the shape, in local units.</param>
        /// <param name="outerSpread">The size of the light outside the shape, in local units.</param>
        /// <param name="innerSpread">The size of the light inside the shape, in local units.</param>
        /// <param name="falloff">1 is a linear fade, higher values keep the light closer to the edge.</param>
        /// <param name="density">The number of texture pixels per local unit (1 to 3).</param>
        public static LightFrame GetGlowFrame(float cornerRadius, float outerSpread, float innerSpread, float falloff, int density = 1)
        {
            var radius = Quantize(cornerRadius);
            var outer = Quantize(outerSpread);
            var inner = Quantize(innerSpread);
            var falloffKey = (int)Math.Round(MathUtils.Clamp(falloff, 0.1f, 10f) * 10f);
            density = MathUtils.Clamp(density, 1, 4);
            var key = (0, radius, outer, inner, falloffKey, density);
            if (TryGetFrame(key, out var frame))
                return frame;

            var border = outer + radius + inner + 1;
            var size = (border * 2 + 2) * density;
            var sprite = CreateSprite(EnsureGraphicsDevice(), size, size, TextureFactory.CreateRoundedRectangleGlowPixels(size, size, radius * density,
                outer * density, inner * density, falloffKey / 10f, Color.White));
            return AddFrame(key, new LightFrame(sprite, border * density, border, outer));
        }

        /// <summary>
        /// A white line of light inside the edge of a rounded rectangle, <paramref name="thickness"/> wide, fading over
        /// <paramref name="softness"/> on both sides.
        /// </summary>
        public static LightFrame GetRimFrame(float cornerRadius, float thickness, float softness, int density = 1)
        {
            var radius = Quantize(cornerRadius);
            var thicknessKey = (int)Math.Round(MathUtils.Clamp(thickness, 0f, 200f) * 2f); // half units
            var softnessKey = (int)Math.Round(MathUtils.Clamp(softness, 0f, 200f) * 2f);
            density = MathUtils.Clamp(density, 1, 4);
            var key = (1, radius, thicknessKey, softnessKey, 0, density);
            if (TryGetFrame(key, out var frame))
                return frame;

            var line = thicknessKey / 2f;
            var soft = softnessKey / 2f;
            var border = (int)Math.Ceiling(soft * 2f + line) + radius + 1;
            var size = (border * 2 + 2) * density;
            var sprite = CreateSprite(EnsureGraphicsDevice(), size, size, TextureFactory.CreateRoundedRectangleRimPixels(size, size, radius * density,
                line * density, soft * density, Color.White));
            return AddFrame(key, new LightFrame(sprite, border * density, border, soft));
        }

        /// <summary>A white rounded rectangle (anti-aliased), used to draw rounded backgrounds and to clip the lights.</summary>
        public static LightFrame GetRoundedMask(float cornerRadius, int density = 1)
        {
            var radius = Quantize(cornerRadius);
            density = MathUtils.Clamp(density, 1, 4);
            var key = (2, radius, 0, 0, 0, density);
            if (TryGetFrame(key, out var frame))
                return frame;

            var border = radius + 1;
            var size = (border * 2 + 2) * density;
            var sprite = CreateSprite(EnsureGraphicsDevice(), size, size, TextureFactory.CreateRoundedRectanglePixels(size, size, radius * density, Color.White));
            return AddFrame(key, new LightFrame(sprite, border * density, border, 0f));
        }

        /// <summary>
        /// A white filled rounded rectangle with a soft edge that fades over <paramref name="softness"/> (half inside,
        /// half outside the shape): highlights, shadows, lips.
        /// </summary>
        public static LightFrame GetSoftFill(float cornerRadius, float softness, int density = 1)
        {
            var radius = Quantize(cornerRadius);
            var softnessKey = Quantize(softness);
            density = MathUtils.Clamp(density, 1, 4);
            var key = (4, radius, softnessKey, 0, 0, density);
            if (TryGetFrame(key, out var frame))
                return frame;

            var border = softnessKey + radius + 1;
            var size = (border * 2 + 2) * density;
            var sprite = CreateSprite(EnsureGraphicsDevice(), size, size, TextureFactory.CreateSoftRoundedRectanglePixels(size, size, radius * density, softnessKey * density, Color.White));
            return AddFrame(key, new LightFrame(sprite, border * density, border, softnessKey / 2f));
        }

        /// <summary>The height in pixels of the textures of <see cref="GetBand"/>.</summary>
        public const int BandHeight = 64;

        /// <summary>
        /// A white rounded band <see cref="BandHeight"/> pixels high that fades from one side to the other: the glossy
        /// reflection of buttons. Draw it with three slices (<paramref name="capWidth"/> pixels on the left and on the
        /// right) and scale the caps with the height, so the round ends keep their shape at any size.
        /// </summary>
        /// <param name="roundness">0 = square ends, 1 = round ends (a pill).</param>
        /// <param name="softness">The width of the soft edge, as a part of the height (0 to 0.5).</param>
        /// <param name="fade">How much the light fades from the top to the bottom (0 = flat, 1 = transparent at the bottom).</param>
        /// <param name="fromBottom">When true, the band fades from the bottom to the top.</param>
        /// <param name="capWidth">Receives the width of the round ends, in texture pixels.</param>
        /// <returns>The image of the band (in the atlas of the effects when it fits).</returns>
        public static EffectSprite GetBand(float roundness, float softness, float fade, bool fromBottom, out int capWidth)
        {
            var roundnessKey = (int)Math.Round(MathUtils.Clamp01(roundness) * 20f);
            var softnessKey = (int)Math.Round(MathUtils.Clamp(softness, 0f, 0.5f) * 40f);
            var fadeKey = (int)Math.Round(MathUtils.Clamp01(fade) * 20f);
            var key = (5, roundnessKey, softnessKey, fadeKey, fromBottom ? 1 : 0, 1);

            var margin = softnessKey / 40f * BandHeight / 2f;
            var radius = (BandHeight / 2f - margin) * roundnessKey / 20f;
            capWidth = (int)Math.Ceiling(radius + margin) + 2;
            if (TryGetFrame(key, out var frame))
                return frame.Sprite;

            var width = capWidth * 2 + 4;
            var softnessPixels = Math.Max(1f, softnessKey / 40f * BandHeight);
            var halfSize = new Vector2(width / 2f - margin, BandHeight / 2f - margin);
            var center = new Vector2(width / 2f, BandHeight / 2f);
            var fadeAmount = fadeKey / 20f;
            var pixels = new Color[width * BandHeight];
            for (var y = 0; y < BandHeight; y++)
            {
                var t = (y + 0.5f) / BandHeight;
                if (fromBottom)
                    t = 1f - t;
                var vertical = 1f - fadeAmount * (float)Math.Pow(t, 1.3f);
                for (var x = 0; x < width; x++)
                {
                    var distance = TextureFactory.RoundedRectangleDistance(new Vector2(x + 0.5f, y + 0.5f) - center, halfSize, radius);
                    var light = MathUtils.Clamp01(0.5f - distance / softnessPixels);
                    light = light * light * (3f - 2f * light);
                    pixels[y * width + x] = Color.White * (light * vertical);
                }
            }

            var sprite = CreateSprite(EnsureGraphicsDevice(), width, BandHeight, pixels);
            return AddFrame(key, new LightFrame(sprite, capWidth, capWidth, 0f)).Sprite;
        }

        /// <summary>
        /// A rounded outline (anti-aliased) of a given thickness inside the edge, used to draw rounded borders.
        /// </summary>
        public static LightFrame GetRoundedBorder(float cornerRadius, float thickness, int density = 1)
        {
            var radius = Quantize(cornerRadius);
            var thicknessKey = (int)Math.Round(MathUtils.Clamp(thickness, 0f, 200f) * 2f);
            density = MathUtils.Clamp(density, 1, 4);
            var key = (3, radius, thicknessKey, 0, 0, density);
            if (TryGetFrame(key, out var frame))
                return frame;

            var line = thicknessKey / 2f;
            var border = (int)Math.Ceiling(line) + radius + 1;
            var size = (border * 2 + 2) * density;
            var sprite = CreateSprite(EnsureGraphicsDevice(), size, size, TextureFactory.CreateRoundedRectanglePixels(size, size, radius * density,
                Color.Transparent, line * density, Color.White));
            return AddFrame(key, new LightFrame(sprite, border * density, border, 0f));
        }

        /// <summary>Releases the frames removed from the cache (called before the controls are drawn).</summary>
        internal static void ReleaseUnused()
        {
            if (EffectAtlas.ReleaseIfFull())
                ForgetReleasedImages();
            if (pendingDisposal.Count == 0)
                return;
            for (var i = 0; i < pendingDisposal.Count; i++)
                pendingDisposal[i].Dispose();
            pendingDisposal.Clear();
        }

        /// <summary>Disposes the shared resources (they are recreated on demand).</summary>
        internal static void Reset()
        {
            ResetTextures();
            lightBlendState?.Dispose();
            lightBlendState = null;
            maskBlendState?.Dispose();
            maskBlendState = null;
        }

        /// <summary>Removes from the cache the frames whose texture was released with the atlas (the sprites check it themselves).</summary>
        private static void ForgetReleasedImages()
        {
            invalidFrames.Clear();
            foreach (var pair in frames)
            {
                if (pair.Value.Texture == null || pair.Value.Texture.IsDisposed)
                    invalidFrames.Add(pair.Key);
            }

            for (var i = 0; i < invalidFrames.Count; i++)
            {
                frames.Remove(invalidFrames[i]);
                frameUses.Remove(invalidFrames[i]);
            }

            invalidFrames.Clear();
        }

        /// <summary>An image in the atlas of the effects, or in a texture of its own when it does not fit.</summary>
        private static EffectSprite CreateSprite(GraphicsDevice device, int width, int height, Color[] pixels)
        {
            if (EffectAtlas.TryAdd(device, width, height, pixels, out var sprite))
                return sprite;
            return new EffectSprite(TextureFactory.FromPixels(device, width, height, pixels), new Rectangle(0, 0, width, height));
        }

        /// <summary>The pixels of a light shape, the same as the textures of <see cref="GetShape"/>.</summary>
        private static Color[] CreateShapePixels(LightShape shape)
        {
            switch (shape)
            {
                case LightShape.Flare:
                    return CreateFlarePixels();
                case LightShape.Star:
                    return TextureFactory.CreateStarPixels(ShapeTextureSize, Color.White);
                case LightShape.Diamond:
                    return TextureFactory.CreateDiamondPixels(ShapeTextureSize, Color.White);
                case LightShape.Circle:
                    return TextureFactory.CreateCirclePixels(ShapeTextureSize, Color.White);
                case LightShape.Ring:
                    return TextureFactory.CreateRingPixels(ShapeTextureSize, ShapeTextureSize * 0.12f, Color.White);
                default:
                    return TextureFactory.CreateGlowPixels(ShapeTextureSize, Color.White, 2f);
            }
        }

        private static int Quantize(float value) => (int)Math.Round(MathUtils.Clamp(value, 0f, 512f));

        private static bool TryGetFrame((int, int, int, int, int, int) key, out LightFrame frame)
        {
            EnsureGraphicsDevice();
            if (!frames.TryGetValue(key, out frame) || frame.Texture == null || frame.Texture.IsDisposed)
                return false;
            frameUses[key] = CurrentFrame;
            return true;
        }

        private static LightFrame AddFrame((int, int, int, int, int, int) key, LightFrame frame)
        {
            if (frames.Count >= MaxCachedFrames)
                EvictLeastRecentlyUsed();

            frames[key] = frame;
            frameUses[key] = CurrentFrame;
            return frame;
        }

        private static long CurrentFrame => ServiceProvider.Clock?.FrameCount ?? 0;

        /// <summary>
        /// Removes the frames that were not used for the longest time (never those used in this frame), down to three
        /// quarters of <see cref="MaxCachedFrames"/>. The textures may still be in a sprite batch of this frame: they are
        /// released before the next frame is drawn.
        /// </summary>
        private static void EvictLeastRecentlyUsed()
        {
            var now = CurrentFrame;
            evictionCandidates.Clear();
            foreach (var pair in frameUses)
            {
                if (pair.Value < now)
                    evictionCandidates.Add(pair.Key);
            }

            evictionCandidates.Sort((a, b) => frameUses[a].CompareTo(frameUses[b]));
            var toRemove = Math.Min(evictionCandidates.Count, frames.Count - MaxCachedFrames * 3 / 4);
            for (var i = 0; i < toRemove; i++)
            {
                var key = evictionCandidates[i];
                if (frames.TryGetValue(key, out var cached) && cached.Texture != null && !EffectAtlas.IsPage(cached.Texture))
                    pendingDisposal.Add(cached.Texture);
                frames.Remove(key);
                frameUses.Remove(key);
            }

            evictionCandidates.Clear();
        }

        private static Texture2D CreateFlare(GraphicsDevice device)
            => TextureFactory.FromPixels(device, ShapeTextureSize, ShapeTextureSize, CreateFlarePixels());

        private static Color[] CreateFlarePixels()
        {
            var star = TextureFactory.CreateStarPixels(ShapeTextureSize, Color.White, 4, 0.14f);
            var glow = TextureFactory.CreateGlowPixels(ShapeTextureSize, Color.White, 3f);
            for (var i = 0; i < star.Length; i++)
            {
                var a = star[i];
                var b = glow[i] * 0.85f;
                star[i] = new Color(Math.Min(255, a.R + b.R), Math.Min(255, a.G + b.G), Math.Min(255, a.B + b.B), Math.Min(255, a.A + b.A));
            }

            return star;
        }

        private static void ResetTextures()
        {
            foreach (var frame in frames.Values)
            {
                if (frame.Texture != null && !EffectAtlas.IsPage(frame.Texture))
                    frame.Texture.Dispose();
            }

            EffectAtlas.Reset();
            Array.Clear(shapeSprites, 0, shapeSprites.Length);
            gradientSprites.Clear();
            softBandSprite = default;
            whiteSprite = default;
            frames.Clear();
            frameUses.Clear();
            foreach (var gradient in gradients.Values)
                gradient.Dispose();
            gradients.Clear();
            ReleaseUnused();
            flareTexture?.Dispose();
            flareTexture = null;
            softBandTexture?.Dispose();
            softBandTexture = null;
            graphicsDevice = null;
        }

        /// <summary>The textures belong to a graphics device: when it changes they are released.</summary>
        private static GraphicsDevice EnsureGraphicsDevice()
        {
            var device = ServiceProvider.GraphicsDevice
                ?? throw new InvalidOperationException("The graphics device is not available yet.");

            if (graphicsDevice != device)
            {
                ResetTextures();
                graphicsDevice = device;
            }

            return device;
        }
    }
}
