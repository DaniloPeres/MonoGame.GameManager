using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls.Effects;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>What the <see cref="ShadingRenderer"/> needs from its control.</summary>
    internal interface IShadingHost
    {
        IControl Control { get; }
        Vector2 SizeWithoutScale { get; }
        Vector2 OriginWithoutScale { get; }
        Vector2 NestedScale { get; }
        float Rotation { get; }
        float NestedOpacity { get; }
        Color Color { get; }
        SpriteEffects SpriteEffects { get; }

        /// <summary>How far the shading effects reach outside the control, in screen units (0 without effects).</summary>
        float ShadingMargin { get; }

        Vector2 GetPosition();

        /// <summary>A value that changes when what the control draws changes, or null when it is unknown.</summary>
        int? GetShadingContentSignature();

        /// <summary>The box of what the control draws (the glyphs of a text), in local units: the area of gradient fills.</summary>
        RectangleF GetShadingContentBounds();

        /// <summary>
        /// True when what the control draws fills its area and stretches with it (a circle, a rectangle): a change of size
        /// within <see cref="ShadingRenderer.SizeStep"/> stretches the shapes instead of rendering them again.
        /// </summary>
        bool ShadingStretchesWithSize { get; }

        /// <summary>Draws the control itself (without its shading effects).</summary>
        void DrawShadingContent(SpriteBatch spriteBatch);

        /// <summary>Draws the shading effects behind the control, the control and the effects in front of it.</summary>
        void DrawWithShading(SpriteBatch spriteBatch);
    }

    /// <summary>
    /// Renders and draws the shading effects of a control.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Before the frame is drawn (<see cref="Prepare"/>), the control is drawn into a scratch render target in its local
    /// space, without its rotation, at about the resolution it has on the screen. For every effect, that silhouette is
    /// downsampled (big blurs), grown by stamping it around a disc (outlines and spreads), and blurred with a separable
    /// gaussian of weighted taps, into a render target of the effect. Every pass writes only the alpha channel of
    /// targets cleared to transparent white, so the shapes are white with straight alpha and the colors of the control
    /// never leak into them. The shapes are kept until the control changes.
    /// </para>
    /// <para>
    /// Inner shapes (<see cref="InnerShadow"/>, <see cref="InnerGlow"/>) are the silhouette multiplied by one minus a
    /// blurred, moved copy of it: a target cleared to opaque white, the blur subtracted from its alpha, then multiplied
    /// by the silhouette. Fills (<see cref="GradientFill"/>) keep the silhouette as alpha and write the colors of a
    /// gradient texture into the red, green and blue channels of the whole target, so the edges never blend with white.
    /// </para>
    /// <para>
    /// While the frame is drawn (<see cref="DrawLayer"/>), the shapes are drawn with the transformation of the control
    /// (position, scale and rotation), tinted by the color, intensity and pulse of their effect. The shapes are premultiplied
    /// when rendered and the lights are drawn with an alpha of 0 (added to what is below), so they go in the batch of the
    /// controls without a new one.
    /// </para>
    /// <para>
    /// Rendering a shape switches render targets several times (slow on phones), so it happens only when needed: a change of
    /// scale alone keeps the shapes while they are sharp enough; an intensity above 1 and the opacity are kept in steps
    /// (<see cref="GainStep"/>, <see cref="OpacitySteps"/>) and the two nearest steps are blended, so a pulsing or fading
    /// effect renders each step once; circles and rectangles stretch their shapes with small changes of size.
    /// </para>
    /// </remarks>
    internal sealed class ShadingRenderer : IDisposable
    {
        private const float MinPixelScale = 0.125f;
        private const float MaxPixelScale = 4f;
        private const float MaxDilationPixels = 32f;
        private const int GradientWidth = 256;
        private const float MaxSigmaPixels = 4f;
        private const int MaxDownsample = 8;
        private const int MaxTapsPerSide = 12;

        /// <summary>How much sharper than the screen needs the shapes can be before a smaller scale renders them again.</summary>
        private const float MaxScaleReuse = 1.3f;

        /// <summary>The extra resolution of the shapes rendered again while a control grows.</summary>
        private const float GrowHeadroom = 1.15f;

        /// <summary>The frames a scale must stay the same before the shapes get its exact resolution.</summary>
        private const int SettleFrames = 20;

        /// <summary>
        /// The step between the gains (intensities above 1) rendered for an effect: an intensity between two steps blends their
        /// shapes, so an intensity that moves (a pulsing or breathing glow) renders each step once.
        /// </summary>
        private const float GainStep = 0.25f;

        /// <summary>
        /// The opacity levels of the silhouette (a step is 1/OpacitySteps). The shapes depend on the opacity in a way that is
        /// not linear (an outline stamps the silhouette many times), so a control that fades or breathes renders each level
        /// once and is drawn with the two nearest ones blended.
        /// </summary>
        private const int OpacitySteps = 16;

        /// <summary>
        /// The relative step of the sizes rendered for a control whose content stretches with its size
        /// (<see cref="IShadingHost.ShadingStretchesWithSize"/>): a circle that grows is rendered every 6% and stretched in
        /// between (its glow is at most 3% wider or thinner).
        /// </summary>
        internal const float SizeStep = 1.06f;

        /// <summary>The step of a size (see <see cref="SizeStep"/>).</summary>
        private static int SizeBucket(float size) => size <= 0f ? int.MinValue : (int)Math.Round(Math.Log(size) / Math.Log(SizeStep));

        private readonly IShadingHost host;
        private readonly List<ShadingEffect> effects = new List<ShadingEffect>();
        private readonly Dictionary<(ShadingEffect Effect, int Level, int Opacity), ShapeResult> results = new Dictionary<(ShadingEffect, int, int), ShapeResult>();
        private readonly List<(ShadingEffect Effect, int Level, int Opacity)> staleResults = new List<(ShadingEffect, int, int)>();

        /// <summary>The opacity levels the control is drawn with now (see <see cref="OpacitySteps"/>) and the weight of the upper one.</summary>
        private int opacityLow = OpacitySteps;
        private int opacityHigh = OpacitySteps;
        private float opacityWeight;

        /// <summary>The opacity level of the silhouette being rendered.</summary>
        private int renderOpacity = OpacitySteps;

        /// <summary>The shapes rendered for the current content and scale (older ones are stale).</summary>
        private int generation;

        /// <summary>The gain level and gain of the shape being rendered.</summary>
        private int renderLevel;
        private float renderGain = 1f;
        private readonly int[] taps = new int[MaxTapsPerSide + 1];
        private float time;
        private bool hasKey;
        private int lastKey;
        private float pixelScale = 1f;

        private Matrix drawMatrix = Matrix.Identity;
        private Vector2 drawScale = Vector2.One;
        private float lastIdeal;
        private int stableFrames;
        private int padding;
        private bool isReady;
        private bool isCulled;

        public ShadingRenderer(IShadingHost host)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
            // Each control starts at another time, so a group of controls does not pulse in lockstep.
            time = ServiceProvider.Random.NextFloat(0f, 100f);
        }

        /// <summary>True while a control is drawn into a silhouette: the shading of its children is not drawn.</summary>
        public static bool IsCapturing { get; private set; }

        /// <summary>
        /// The control drawn into a silhouette, or null: it is drawn at the opacity level being rendered
        /// (<see cref="CaptureOpacity"/> is its <see cref="IRenderable.NestedOpacity"/> meanwhile).
        /// </summary>
        internal static IControl CaptureHost { get; private set; }

        /// <summary>The opacity of <see cref="CaptureHost"/> while its silhouette is drawn.</summary>
        internal static float CaptureOpacity { get; private set; } = 1f;

        public IReadOnlyList<ShadingEffect> Effects => effects;

        public bool HasEffects => effects.Count > 0;

        /// <summary>How far the enabled effects reach outside the control, offsets included, in local units.</summary>
        public float MaxExtentLocal
        {
            get
            {
                var extent = 0f;
                for (var i = 0; i < effects.Count; i++)
                {
                    var effect = effects[i];
                    if (effect.IsEnabled && effect.IsAttached)
                        extent = Math.Max(extent, effect.Extent);
                    else if (effect.IsEnabled)
                        extent = Math.Max(extent, effect.Extent + effect.Offset.Length() + Math.Abs(effect.OrbitRadius)
                            + Math.Max(0f, effect.BreatheAmount) * Math.Max(host.SizeWithoutScale.X, host.SizeWithoutScale.Y) / 2f);
                }

                return extent + GetContentOverflow();
            }
        }

        /// <summary>
        /// How far what the control draws goes outside its area, in local units (eg: the swashes of a decorative font
        /// that go past the width measured for the text): the silhouette is captured with this much more room.
        /// </summary>
        private float GetContentOverflow()
        {
            var size = host.SizeWithoutScale;
            var bounds = host.GetShadingContentBounds();
            if (bounds.IsEmpty)
                return 0f;
            return Math.Max(0f, Math.Max(Math.Max(-bounds.X, -bounds.Y), Math.Max(bounds.Right - size.X, bounds.Bottom - size.Y)));
        }

        /// <summary>Draws a control with its shading effects when it has some.</summary>
        public static void DrawControl(IControl control, SpriteBatch spriteBatch)
        {
            if (control is IShadingHost host)
                host.DrawWithShading(spriteBatch);
            else
                control.Draw(spriteBatch);
        }

        public void Add(ShadingEffect effect)
        {
            effects.Add(effect);
            hasKey = false;
        }

        public void Remove(ShadingEffect effect)
        {
            effects.Remove(effect);
            staleResults.Clear();
            foreach (var key in results.Keys)
            {
                if (ReferenceEquals(key.Effect, effect))
                    staleResults.Add(key);
            }


            foreach (var key in staleResults)
            {
                results[key].Dispose();
                results.Remove(key);
            }

            staleResults.Clear();
            hasKey = false;
        }

        public void Clear()
        {
            effects.Clear();
            ReleaseResults();
        }

        /// <summary>Renders the shapes again before the next frame.</summary>
        public void Invalidate() => hasKey = false;

        public void Update(float deltaSeconds) => time += Math.Max(0f, deltaSeconds);

        /// <summary>Renders the shapes of the effects when the control changed (before the frame is drawn).</summary>
        public void Prepare()
        {
            isReady = false;
            isCulled = false;
            if (effects.Count == 0)
                return;

            var graphicsDevice = ServiceProvider.GraphicsDevice;
            var controlManager = ServiceProvider.ControlManager;
            if (graphicsDevice == null || controlManager == null)
                return;

            var size = host.SizeWithoutScale;
            var scale = host.NestedScale;
            if (size.X <= 0f || size.Y <= 0f || scale.X == 0f || scale.Y == 0f || host.NestedOpacity <= 0f)
                return;

            var extent = 0f;
            var anyEnabled = false;
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (!effect.IsEnabled)
                    continue;
                anyEnabled = true;
                extent = Math.Max(extent, effect.Extent);
            }

            if (!anyEnabled)
                return;

            // Glyphs that go past the area of the control are captured too.
            extent += GetContentOverflow();

            // A control hidden by a clipping parent (eg: scrolled out of a scroll viewer) keeps its shapes but draws nothing.
            isCulled = EffectCulling.IsOutsideClippingParents(host.Control, host.GetPosition(), size, host.OriginWithoutScale, scale, host.Rotation,
                MaxExtentLocal + 2f);
            if (isCulled)
                return;

            // About one pixel of the target per pixel on the screen, smaller for big controls (big outlines are grown at
            // a lower resolution in RenderShape, so the fills and the other shapes stay sharp).
            var largest = Math.Max(size.X, size.Y) + extent * 2f;
            var ideal = PixelScaleFor(Math.Max(Math.Abs(scale.X), Math.Abs(scale.Y)), largest);

            // The shapes are rendered again when the control changed (all of them) or when the options of an effect
            // changed (only its shape: a scrolling fill does not blur the glows again). The opacity and the gain are not
            // part of the key: each step of them has its own shapes (see OpacitySteps and GainStep).
            var signature = host.GetShadingContentSignature();
            var key = ComputeKey(signature, size, extent);
            var sameContent = signature != null && hasKey && key == lastKey;

            // A change of scale alone (breathing, pop-ins, a lunge) keeps the shapes while they are at least as sharp as
            // the screen needs and at most MaxScaleReuse times sharper; a control that stops changing gets shapes of its
            // exact resolution after SettleFrames.
            stableFrames = ideal == lastIdeal ? stableFrames + 1 : 0;
            lastIdeal = ideal;
            var s = ideal;
            if (sameContent && pixelScale >= ideal && pixelScale <= ideal * MaxScaleReuse && (stableFrames < SettleFrames || pixelScale == ideal))
                s = pixelScale;
            else if (sameContent && ideal > pixelScale && stableFrames < SettleFrames)
                s = PixelScaleFor(Math.Max(Math.Abs(scale.X), Math.Abs(scale.Y)) * GrowHeadroom, largest); // still growing: room to grow

            var pad = (int)Math.Ceiling(extent * s) + 2;
            var width = (int)Math.Ceiling(size.X * s) + pad * 2;
            var height = (int)Math.Ceiling(size.Y * s) + pad * 2;
            if ((width > ShadingResources.MaxTargetSize || height > ShadingResources.MaxTargetSize) && s != ideal)
            {
                s = ideal;
                pad = (int)Math.Ceiling(extent * s) + 2;
                width = (int)Math.Ceiling(size.X * s) + pad * 2;
                height = (int)Math.Ceiling(size.Y * s) + pad * 2;
            }

            if (width > ShadingResources.MaxTargetSize || height > ShadingResources.MaxTargetSize)
                return;

            var renderAll = !sameContent || s != pixelScale || pad != padding;
            if (renderAll)
                generation++;
            GetOpacityLevels(host.NestedOpacity, out opacityLow, out opacityHigh, out opacityWeight);
            var needsRender = renderAll;
            for (var i = 0; i < effects.Count && !needsRender; i++)
            {
                var effect = effects[i];
                if (!effect.IsEnabled)
                    continue;
                GetLevels(effect, out var low, out var high, out _);
                var effectKey = ComputeEffectKey(effect);
                needsRender = !IsCurrent(effect, low, opacityLow, effectKey) || !IsCurrent(effect, high, opacityLow, effectKey)
                    || !IsCurrent(effect, low, opacityHigh, effectKey) || !IsCurrent(effect, high, opacityHigh, effectKey);
            }

            pixelScale = s;
            padding = pad;
            if (needsRender)
            {
                Render(graphicsDevice, controlManager, width, height, renderAll);
                lastKey = key;
                hasKey = true;
            }

            isReady = true;
        }

        /// <summary>Draws the effects of a layer, with the transformation of the control; false when nothing was drawn.</summary>
        public bool DrawLayer(SpriteBatch spriteBatch, ShadingLayer layer)
        {
            if (!isReady || isCulled || !HasVisibleEffects(layer))
                return false;

            var controlManager = ServiceProvider.ControlManager;
            if (controlManager == null)
                return false;

            // The shapes are premultiplied and the lights are drawn with an alpha of 0 (added to what is below), so every
            // shape goes in the batch that draws the controls: no new batch when it already uses the premultiplied blend
            // and a linear sampler, one for the whole layer otherwise.
            var current = controlManager.CurrentState;
            var scale = host.NestedScale;
            var inBatch = ReferenceEquals(current.BlendState, BlendState.AlphaBlend) && ReferenceEquals(current.SamplerState, SamplerState.LinearClamp)
                && current.Effect == null;
            if (!inBatch)
                controlManager.PushState(spriteBatch, current.WithBlendState(BlendState.AlphaBlend).WithSamplerState(SamplerState.LinearClamp).WithEffect(null));

            drawMatrix = GetLocalTransform(null);
            drawScale = scale;
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (IsVisible(effect, layer, out var result))
                    DrawShape(spriteBatch, effect, result);
            }

            if (!inBatch)
                controlManager.PopState(spriteBatch);
            return true;
        }

        /// <summary>
        /// Draws a part of a shape given in local units (its top-left <paramref name="localPosition"/>, <paramref name="scale"/>
        /// local units per pixel) with the transformation of the control (<see cref="drawMatrix"/>).
        /// </summary>
        private void DrawLocal(SpriteBatch spriteBatch, Texture2D texture, Vector2 localPosition, Rectangle? source, Color tint, Vector2 scale)
        {
            var position = Vector2.Transform(localPosition, drawMatrix);
            var spriteEffects = SpriteEffects.None;
            var drawScale = this.drawScale * scale;
            if (drawScale.X < 0f || drawScale.Y < 0f)
            {
                // A mirrored control: the texture is drawn flipped from the other corner.
                var size = source.HasValue ? new Vector2(source.Value.Width, source.Value.Height) : new Vector2(texture.Width, texture.Height);
                var origin = Vector2.Zero;
                if (drawScale.X < 0f)
                {
                    spriteEffects |= SpriteEffects.FlipHorizontally;
                    origin.X = size.X;
                    drawScale.X = -drawScale.X;
                }

                if (drawScale.Y < 0f)
                {
                    spriteEffects |= SpriteEffects.FlipVertically;
                    origin.Y = size.Y;
                    drawScale.Y = -drawScale.Y;
                }

                spriteBatch.Draw(texture, position, source, tint, host.Rotation, origin, drawScale, spriteEffects, 0f);
                return;
            }

            spriteBatch.Draw(texture, position, source, tint, host.Rotation, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// The color a shape is drawn with in a premultiplied batch: the straight color times the strength, with the strength
        /// as alpha (painted) or 0 (a light: added to what is below, its alpha is kept).
        /// </summary>
        private static Color Tint(Color straight, float strength, ShadingBlend blend)
        {
            var s = MathUtils.Clamp01(strength);
            return new Color((int)(straight.R * s + 0.5f), (int)(straight.G * s + 0.5f), (int)(straight.B * s + 0.5f),
                blend == ShadingBlend.Light ? 0 : (int)(s * 255f + 0.5f));
        }

        public void Dispose()
        {
            ReleaseResults();
        }

        /// <summary>
        /// The pixels of the target per local unit for a control drawn at <paramref name="scale"/> (its largest side and
        /// effects <paramref name="largest"/> local units long), in steps of 1/64.
        /// </summary>
        private static float PixelScaleFor(float scale, float largest)
        {
            var s = MathUtils.Clamp(scale, MinPixelScale, MaxPixelScale);
            if (largest * s + 8f > ShadingResources.MaxTargetSize)
                s = (ShadingResources.MaxTargetSize - 8f) / largest;
            return Math.Max(1f / 64f, (float)Math.Floor(s * 64f) / 64f); // steps, so a slow zoom does not render every frame
        }

        /// <summary>The key of what the control draws (without its scale, which only changes the resolution of the shapes).</summary>
        private int ComputeKey(int? signature, Vector2 size, float extent)
        {
            var hash = new HashCode();
            hash.Add(signature ?? 0);
            if (host.ShadingStretchesWithSize)
            {
                hash.Add(SizeBucket(size.X));
                hash.Add(SizeBucket(size.Y));
            }
            else
            {
                hash.Add(size);
            }

            hash.Add(host.Color.A);
            hash.Add(host.SpriteEffects);
            hash.Add((int)Math.Ceiling(extent * 8f));
            hash.Add(effects.Count);
            return hash.ToHashCode();
        }

        /// <summary>The key of the shape of an effect: its options and the animations baked into it.</summary>
        private int ComputeEffectKey(ShadingEffect effect) => HashCode.Combine(effect.GetShapeKey(), effect.GetDynamicShapeKey(time));

        private bool HasVisibleEffects(ShadingLayer layer)
        {
            for (var i = 0; i < effects.Count; i++)
            {
                if (IsVisible(effects[i], layer, out _))
                    return true;
            }

            return false;
        }

        private bool IsVisible(ShadingEffect effect, ShadingLayer layer, out ShapeResult result)
        {
            result = null;
            if (!effect.IsEnabled || effect.Layer != layer || effect.Intensity <= 0f || effect.Color.A == 0)
                return false;
            GetLevels(effect, out var low, out _, out _);
            return TryGetCurrent(effect, low, opacityLow, out result);
        }

        /// <summary>The shape of an effect at a gain and opacity level, when it is rendered for the current content.</summary>
        private bool TryGetCurrent(ShadingEffect effect, int level, int opacity, out ShapeResult result)
            => results.TryGetValue((effect, level, opacity), out result) && result.IsValid && result.Generation == generation;

        /// <summary>The opacity levels around <paramref name="opacity"/> and the weight of the upper one (0 on a level).</summary>
        private static void GetOpacityLevels(float opacity, out int low, out int high, out float weight)
        {
            var steps = MathUtils.Clamp01(opacity) * OpacitySteps;
            low = Math.Min(OpacitySteps, (int)Math.Floor(steps + 0.001f));
            weight = steps - low;
            if (weight < 0.004f || low >= OpacitySteps)
            {
                weight = 0f;
                high = low;
                return;
            }

            high = low + 1;
        }

        /// <summary>
        /// The gain levels of the shapes an effect is drawn with: the step at or under its gain, the step over it, and the
        /// weight of the upper one (0 when the gain is on a step). Shapes that do not use the gain have only level 0.
        /// </summary>
        private static void GetLevels(ShadingEffect effect, out int low, out int high, out float weight)
        {
            if (effect.Shape != ShadingShape.Outer && effect.Shape != ShadingShape.Inner)
            {
                low = high = 0;
                weight = 0f;
                return;
            }

            var steps = (effect.BakedGain - 1f) / GainStep;
            low = (int)Math.Floor(steps + 0.001f);
            weight = steps - low;
            if (weight < 0.004f)
            {
                weight = 0f;
                high = low;
                return;
            }

            high = low + 1;
        }

        private static float GainOf(int level) => 1f + level * GainStep;

        /// <summary>True when the shape of an effect at a gain and opacity level is rendered for the current content and options.</summary>
        private bool IsCurrent(ShadingEffect effect, int level, int opacity, int effectKey)
            => TryGetCurrent(effect, level, opacity, out var result) && result.Key == effectKey;

        /// <summary>
        /// Draws an effect: the shape of its gain and opacity, or, between two gain steps or two opacity levels, the nearest
        /// shapes blended.
        /// </summary>
        private void DrawShape(SpriteBatch spriteBatch, ShadingEffect effect, ShapeResult lowest)
        {
            GetLevels(effect, out var lowLevel, out var highLevel, out var gainWeight);
            for (var o = 0; o < 2; o++)
            {
                var opacity = o == 0 ? opacityLow : opacityHigh;
                var opacityPart = o == 0 ? 1f - opacityWeight : opacityWeight;
                if (opacityPart <= 0f || o == 1 && opacityHigh == opacityLow)
                    continue;

                for (var g = 0; g < 2; g++)
                {
                    var level = g == 0 ? lowLevel : highLevel;
                    var gainPart = g == 0 ? 1f - gainWeight : gainWeight;
                    if (gainPart <= 0f || g == 1 && highLevel == lowLevel)
                        continue;

                    if (o == 0 && g == 0)
                        DrawShape(spriteBatch, effect, lowest, opacityPart * gainPart);
                    else if (TryGetCurrent(effect, level, opacity, out var shape))
                        DrawShape(spriteBatch, effect, shape, opacityPart * gainPart);
                }
            }
        }

        private void DrawShape(SpriteBatch spriteBatch, ShadingEffect effect, ShapeResult result, float weight)
        {
            var color = effect.GetColor(time);
            if (color.A == 0)
                return;

            // The colors of XNA are premultiplied; the shapes are drawn with a straight color.
            var alpha = color.A / 255f;
            var strength = alpha * Math.Min(1f, effect.Intensity) * effect.GetAnimatedIntensity(time) * effect.Strength * weight;
            var straight = new Color(
                (int)Math.Min(255f, color.R / alpha + 0.5f),
                (int)Math.Min(255f, color.G / alpha + 0.5f),
                (int)Math.Min(255f, color.B / alpha + 0.5f));
            if (strength <= 0.002f)
                return;

            // The breathing scales the shape around the center of the control. Attached shapes (fills, inner shadows)
            // stay on the silhouette.
            var attached = effect.IsAttached;
            var breathe = attached ? 1f : effect.GetBreathe(time);
            var size = host.SizeWithoutScale;
            // Each shape is stretched from the size it was rendered at: the opacity and gain levels of one size step are
            // rendered at different sizes while a circle grows, so one shared size would draw some of them too small.
            var stretch = host.ShadingStretchesWithSize && result.Size.X > 0f && result.Size.Y > 0f ? size / result.Size : Vector2.One;
            var center = size / 2f;
            var origin = new Vector2(-padding / pixelScale) * stretch;
            var position = center + (origin - center) * breathe + (attached ? Vector2.Zero : ToLocalOffset(effect.GetAnimatedOffset(time)));
            var scale = stretch * (result.Downsample / pixelScale * breathe);

            if (effect is Shine shine)
            {
                DrawShine(spriteBatch, shine, result, straight, strength, position, scale);
                return;
            }

            DrawLocal(spriteBatch, result.Target, position, null, Tint(straight, strength, effect.Blend), scale);
        }

        /// <summary>
        /// Draws the part of the silhouette under a slanted band of light: the band is cut in horizontal strips (shifted
        /// to slant it) and vertical slices (brighter in the middle of the band).
        /// </summary>
        private void DrawShine(SpriteBatch spriteBatch, Shine shine, ShapeResult result, Color straight, float strength, Vector2 position, Vector2 scale)
        {
            var progress = shine.GetProgress(time);
            if (progress < 0f || progress > 1f)
                return;

            const int Strips = 8;
            const int Slices = 9;
            var texture = result.Target;
            var size = host.SizeWithoutScale;
            var slant = (float)Math.Tan(MathHelper.ToRadians(MathHelper.Clamp(shine.Angle, -70f, 70f)));
            var width = shine.Width;
            var spread = Math.Abs(slant) * size.Y;
            var travel = size.X + width + spread;
            var start = -width / 2f - spread / 2f;
            var middle = start + (shine.Reverse ? 1f - progress : progress) * travel;

            // Local units to pixels of the shape: pixel = (local - origin) / scale.
            int ToPixelX(float local) => MathUtils.Clamp((int)Math.Round((local - position.X) / scale.X), 0, texture.Width);
            int ToPixelY(float local) => MathUtils.Clamp((int)Math.Round((local - position.Y) / scale.Y), 0, texture.Height);

            Span<int> columns = stackalloc int[Slices + 1];
            for (var strip = 0; strip < Strips; strip++)
            {
                var top = ToPixelY(size.Y * strip / Strips);
                var bottom = ToPixelY(size.Y * (strip + 1) / Strips);
                if (strip == 0)
                    top = 0;
                if (strip == Strips - 1)
                    bottom = texture.Height;
                if (bottom <= top)
                    continue;

                // The band leans: its middle moves with the height of the strip.
                var stripMiddle = (strip + 0.5f) / Strips * size.Y;
                var bandCenter = middle - slant * (stripMiddle - size.Y / 2f);
                for (var i = 0; i <= Slices; i++)
                    columns[i] = ToPixelX(bandCenter - width / 2f + width * i / Slices);

                for (var i = 0; i < Slices; i++)
                {
                    if (columns[i + 1] <= columns[i])
                        continue;
                    var weight = (float)Math.Sin(MathHelper.Pi * (i + 0.5f) / Slices);
                    var source = new Rectangle(columns[i], top, columns[i + 1] - columns[i], bottom - top);
                    var at = position + new Vector2(source.X, source.Y) * scale;
                    DrawLocal(spriteBatch, texture, at, source, Tint(straight, strength * weight * weight, shine.Blend), scale);
                }
            }
        }

        /// <summary>
        /// Converts an offset (local units, in a fixed direction on the screen) to the local space of the control,
        /// which can be rotated and scaled differently on each axis.
        /// </summary>
        private Vector2 ToLocalOffset(Vector2 offset)
        {
            if (offset == Vector2.Zero)
                return Vector2.Zero;

            var scale = host.NestedScale;
            var onScreen = offset * new Vector2(Math.Abs(scale.X), Math.Abs(scale.Y));
            if (host.Rotation != 0f)
                onScreen = onScreen.Rotated(-host.Rotation);
            return new Vector2(onScreen.X / scale.X, onScreen.Y / scale.Y);
        }

        /// <summary>Draws the silhouette of the control and renders the shape of every enabled effect.</summary>
        private void Render(GraphicsDevice graphicsDevice, ControlManager controlManager, int width, int height, bool renderAll)
        {
            var batch = ShadingResources.SpriteBatch;
            var previousTargets = graphicsDevice.GetRenderTargets();

            RenderOpacity(graphicsDevice, controlManager, batch, opacityLow, width, height);
            if (opacityHigh != opacityLow)
                RenderOpacity(graphicsDevice, controlManager, batch, opacityHigh, width, height);

            // The shapes of an older content or scale are useless now.
            if (renderAll)
            {
                staleResults.Clear();
                foreach (var pair in results)
                {
                    if (pair.Value.Generation != generation)
                        staleResults.Add(pair.Key);
                }

                foreach (var key in staleResults)
                {
                    results[key].Dispose();
                    results.Remove(key);
                }

                staleResults.Clear();
            }

            graphicsDevice.SetRenderTargets(previousTargets);
        }

        /// <summary>
        /// Draws the silhouette of the control at an opacity level and renders from it every shape of that level that is
        /// missing.
        /// </summary>
        private void RenderOpacity(GraphicsDevice graphicsDevice, ControlManager controlManager, SpriteBatch batch, int opacity, int width, int height)
        {
            var missing = false;
            for (var i = 0; i < effects.Count && !missing; i++)
            {
                var effect = effects[i];
                if (!effect.IsEnabled)
                    continue;
                var effectKey = ComputeEffectKey(effect);
                GetLevels(effect, out var low, out var high, out _);
                missing = !IsCurrent(effect, low, opacity, effectKey) || !IsCurrent(effect, high, opacity, effectKey);
            }

            if (!missing)
                return;

            // The silhouette: the control drawn in its local space (unrotated), pixelScale pixels per unit, with a
            // transparent margin of padding pixels for the effects.
            var capture = ShadingResources.GetScratch(0, width, height);
            graphicsDevice.SetRenderTarget(capture);
            graphicsDevice.Clear(Color.Transparent);
            var matrix = Matrix.Invert(GetLocalTransform(null))
                * Matrix.CreateScale(pixelScale, pixelScale, 1f)
                * Matrix.CreateTranslation(padding, padding, 0f);

            IsCapturing = true;
            CaptureHost = host.Control;
            CaptureOpacity = opacity / (float)OpacitySteps;
            try
            {
                controlManager.BeginOffscreen(batch, controlManager.BaseState.WithTransform(matrix).WithScissor(null).WithBlendState(BlendState.AlphaBlend));
                host.DrawShadingContent(batch);
                controlManager.EndOffscreen(batch);
            }
            finally
            {
                IsCapturing = false;
                CaptureHost = null;
                CaptureOpacity = 1f;
            }

            renderOpacity = opacity;
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (!effect.IsEnabled)
                    continue;

                var effectKey = ComputeEffectKey(effect);
                GetLevels(effect, out var low, out var high, out _);
                RenderLevel(graphicsDevice, batch, effect, effectKey, low, capture, width, height);
                if (high != low)
                    RenderLevel(graphicsDevice, batch, effect, effectKey, high, capture, width, height);
            }
        }

        /// <summary>Renders the shape of an effect at a gain level, unless it is already rendered for the current content.</summary>
        private void RenderLevel(GraphicsDevice graphicsDevice, SpriteBatch batch, ShadingEffect effect, int effectKey, int level, RenderTarget2D capture,
            int width, int height)
        {
            if (IsCurrent(effect, level, renderOpacity, effectKey))
                return;

            renderLevel = level;
            renderGain = GainOf(level);
            switch (effect.Shape)
            {
                case ShadingShape.Fill:
                    RenderFill(graphicsDevice, batch, effect, (IGradientShape)effect, capture, width, height);
                    break;
                case ShadingShape.Pattern:
                    RenderPattern(graphicsDevice, batch, (PatternOverlay)effect, capture, width, height);
                    break;
                case ShadingShape.Sparkles:
                    RenderSparkles(graphicsDevice, batch, (Sparkles)effect, capture, width, height);
                    break;
                case ShadingShape.Inner:
                    RenderInnerShape(graphicsDevice, batch, effect, capture, width, height);
                    break;
                default:
                    RenderShape(graphicsDevice, batch, effect, capture, width, height);
                    break;
            }

            var result = results[(effect, level, renderOpacity)];
            result.Key = effectKey;
            result.Generation = generation;
            result.Size = host.SizeWithoutScale;
            Premultiply(graphicsDevice, batch, result.Target);
        }

        /// <summary>
        /// Renders a gradient fill or overlay: the silhouette as alpha, then the colors of the gradient written into the
        /// red, green and blue channels of the whole target (the alpha is kept); an overlay then multiplies the alpha by
        /// the alpha of the gradient.
        /// </summary>
        private void RenderFill(GraphicsDevice graphicsDevice, SpriteBatch batch, ShadingEffect effect, IGradientShape fill, RenderTarget2D capture, int width, int height)
        {
            var result = GetResult(graphicsDevice, effect, width, height, 1);
            var gradientKey = fill.GetGradientKey();
            if (result.Gradient == null || result.Gradient.IsDisposed || result.GradientKey != gradientKey)
            {
                result.Gradient?.Dispose();
                result.Gradient = TextureFactory.CreateGradient(graphicsDevice, GradientWidth, fill.Colors, fill.Positions, fill.Hardness, fill.KeepsAlpha);
                result.GradientKey = gradientKey;
            }

            BeginTarget(graphicsDevice, batch, result.Target, ShadingResources.AlphaUnionBlendState);
            DrawTap(batch, capture, Vector2.Zero, new Rectangle(0, 0, width, height), 1f, 255);
            batch.End();

            // The gradient runs along its direction, from the start to the end of the bounds projected on it.
            var bounds = fill.Bounds == GradientBounds.Content ? host.GetShadingContentBounds() : new RectangleF(Vector2.Zero, host.SizeWithoutScale);
            if (bounds.IsEmpty)
                bounds = new RectangleF(Vector2.Zero, host.SizeWithoutScale);
            var angle = MathHelper.ToRadians(fill.Angle);
            var axis = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            var min = float.MaxValue;
            var max = float.MinValue;
            foreach (var corner in new[] { bounds.Position, new Vector2(bounds.Right, bounds.Top), new Vector2(bounds.Left, bounds.Bottom), new Vector2(bounds.Right, bounds.Bottom) })
            {
                var projection = Vector2.Dot(corner, axis);
                min = Math.Min(min, projection);
                max = Math.Max(max, projection);
            }

            var start = min + (max - min) * fill.RangeStart;
            var end = min + (max - min) * fill.RangeEnd;
            var length = Math.Max(0.01f, Math.Abs(end - start)) * pixelScale;
            if (end < start)
            {
                // A reversed range: the gradient runs the other way.
                axis = -axis;
                angle += MathHelper.Pi;
                start = -start;
            }

            var center = bounds.Center;
            var startPoint = center + axis * (start - Vector2.Dot(center, axis));
            var position = startPoint * pixelScale + new Vector2(padding);

            // The texture is drawn much longer than the gradient, so it covers the whole target: outside the gradient the
            // sampler extends its end colors (clamp) or repeats it (wrap, for a scrolling fill).
            var diagonal = (float)Math.Sqrt(width * width + height * height);
            var copies = Math.Min(256, (int)Math.Ceiling(diagonal / length) + 1);
            var phase = fill.GetPhase(time);
            var source = new Rectangle((int)Math.Round(-(copies + phase) * GradientWidth), 0, GradientWidth * (copies * 2 + 1), 1);
            var sampler = fill.ScrollSpeed != 0f || fill.Repeat ? SamplerState.LinearWrap : SamplerState.LinearClamp;
            var origin = new Vector2(copies * GradientWidth, 0.5f);
            var scale = new Vector2(length / GradientWidth, diagonal * 2f + 4f);

            graphicsDevice.SetRenderTarget(result.Target);
            batch.Begin(SpriteSortMode.Deferred, ShadingResources.ColorReplaceBlendState, sampler, DepthStencilState.None, RasterizerState.CullNone);
            batch.Draw(result.Gradient, position, source, Color.White, angle, origin, scale, SpriteEffects.None, 0f);
            batch.End();

            if (fill.KeepsAlpha)
            {
                batch.Begin(SpriteSortMode.Deferred, ShadingResources.AlphaMultiplyBlendState, sampler, DepthStencilState.None, RasterizerState.CullNone);
                batch.Draw(result.Gradient, position, source, Color.White, angle, origin, scale, SpriteEffects.None, 0f);
                batch.End();
            }
        }

        /// <summary>Renders a pattern: the silhouette as alpha, multiplied by the tiles of the pattern.</summary>
        private void RenderPattern(GraphicsDevice graphicsDevice, SpriteBatch batch, PatternOverlay pattern, RenderTarget2D capture, int width, int height)
        {
            var result = GetResult(graphicsDevice, pattern, width, height, 1);
            var texture = ShadingResources.GetPattern(pattern.Pattern);

            BeginTarget(graphicsDevice, batch, result.Target, ShadingResources.AlphaUnionBlendState);
            DrawTap(batch, capture, Vector2.Zero, new Rectangle(0, 0, width, height), 1f, 255);
            batch.End();

            // The tiles are in local space: local (0, 0) is at the padding of the target.
            var size = ShadingResources.PatternSize;
            var drawScale = pattern.TileSize * pixelScale / size;
            var scroll = pattern.GetScrollOffset(time) * size;
            var startX = -padding / drawScale - scroll.X;
            var startY = -padding / drawScale - scroll.Y;
            var sourceX = (int)Math.Floor(startX);
            var sourceY = (int)Math.Floor(startY);
            var source = new Rectangle(sourceX, sourceY, (int)Math.Ceiling(width / drawScale) + 2, (int)Math.Ceiling(height / drawScale) + 2);
            var position = new Vector2(sourceX - startX, sourceY - startY) * drawScale;

            graphicsDevice.SetRenderTarget(result.Target);
            batch.Begin(SpriteSortMode.Deferred, ShadingResources.AlphaMultiplyBlendState, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone);
            batch.Draw(texture, position, source, Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
            batch.End();
        }

        /// <summary>Renders sparkles: stars stamped over the box of the glyphs, then multiplied by the silhouette.</summary>
        private void RenderSparkles(GraphicsDevice graphicsDevice, SpriteBatch batch, Sparkles sparkles, RenderTarget2D capture, int width, int height)
        {
            var result = GetResult(graphicsDevice, sparkles, width, height, 1);
            var texture = ShadingResources.SparkleTexture;
            var bounds = host.GetShadingContentBounds();
            if (bounds.IsEmpty)
                bounds = new RectangleF(Vector2.Zero, host.SizeWithoutScale);

            BeginTarget(graphicsDevice, batch, result.Target, ShadingResources.AlphaAddBlendState);
            var random = new Random(sparkles.Seed);
            var phase = sparkles.GetTwinklePhase(time);
            var half = new Vector2(ShadingResources.PatternSize / 2f);
            for (var i = 0; i < sparkles.Count; i++)
            {
                var local = new Vector2(bounds.X + (float)random.NextDouble() * bounds.Width, bounds.Y + (float)random.NextDouble() * bounds.Height);
                var size = sparkles.Size * (0.45f + 0.55f * (float)random.NextDouble());
                var starPhase = (float)random.NextDouble();
                var rotation = ((float)random.NextDouble() - 0.5f) * 0.5f;
                var light = sparkles.TwinkleSpeed > 0f
                    ? (float)Math.Pow(0.5f - 0.5f * Math.Cos((phase + starPhase) * MathHelper.TwoPi), 2.0)
                    : 0.55f + 0.45f * starPhase;
                if (light < 0.02f)
                    continue;

                var pixels = size * pixelScale * (0.6f + 0.4f * light);
                batch.Draw(texture, local * pixelScale + new Vector2(padding), null, new Color(255, 255, 255, (int)(light * 255f)), rotation, half,
                    pixels / ShadingResources.PatternSize, SpriteEffects.None, 0f);
            }

            batch.End();

            // Only over the silhouette, grown by the spill (the rays can go a little outside the glyphs).
            Texture2D mask = capture;
            var spill = sparkles.Spill * pixelScale;
            if (spill > 0.5f)
            {
                mask = BeginScratch(graphicsDevice, batch, 1, width, height, ShadingResources.AlphaUnionBlendState, out _);
                var area = new Rectangle(0, 0, width, height);
                DrawTap(batch, capture, Vector2.Zero, area, 1f, 255);
                for (var radius = Math.Min(spill, MaxDilationPixels); radius > 0.01f; radius -= 1f)
                {
                    for (var i = 0; i < 16; i++)
                    {
                        var angle = i * MathHelper.TwoPi / 16f;
                        DrawTap(batch, capture, new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius, area, 1f, 255);
                    }
                }

                batch.End();
                graphicsDevice.SetRenderTarget(result.Target);
            }

            batch.Begin(SpriteSortMode.Deferred, ShadingResources.AlphaMultiplyBlendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            DrawTap(batch, mask, Vector2.Zero, new Rectangle(0, 0, width, height), 1f, 255);
            batch.End();
        }

        /// <summary>
        /// Renders an inner shape: the silhouette multiplied by one minus a blurred copy of it, moved by the baked offset
        /// (the edges the copy moves away from stay lit).
        /// </summary>
        private void RenderInnerShape(GraphicsDevice graphicsDevice, SpriteBatch batch, ShadingEffect effect, RenderTarget2D capture, int width, int height)
        {
            var sigma = effect.SoftEdge * pixelScale / 3f;
            var downsample = 1;
            while (sigma / downsample > MaxSigmaPixels && downsample < MaxDownsample)
                downsample *= 2;
            sigma /= downsample;
            var shapeWidth = (int)Math.Ceiling(width / (float)downsample);
            var shapeHeight = (int)Math.Ceiling(height / (float)downsample);
            var shift = effect.GetBakedOffset(time) * pixelScale / downsample;

            Texture2D silhouette = capture;
            var area = new Rectangle(0, 0, width, height);
            var sourceScale = 1f / downsample;
            if (downsample > 1)
            {
                silhouette = Downsample(graphicsDevice, batch, capture, area, downsample, shapeWidth, shapeHeight, 1);
                area = new Rectangle(0, 0, shapeWidth, shapeHeight);
                sourceScale = 1f;
            }

            var result = GetResult(graphicsDevice, effect, shapeWidth, shapeHeight, downsample);
            var gain = renderGain;
            if (sigma > 0.25f)
            {
                // The moved silhouette blurred horizontally into a scratch target, then vertically subtracted from white.
                var count = ComputeTaps(sigma, 1f, out var step);
                BeginScratch(graphicsDevice, batch, 2, shapeWidth, shapeHeight, ShadingResources.AlphaAddBlendState, out _);
                for (var i = -count; i <= count; i++)
                    DrawTap(batch, silhouette, shift + new Vector2(i * step, 0f), area, sourceScale, taps[Math.Abs(i)]);
                batch.End();
                var blurred = ShadingResources.GetScratch(2, shapeWidth, shapeHeight);
                var blurredArea = new Rectangle(0, 0, shapeWidth, shapeHeight);

                count = ComputeTaps(sigma, gain, out step);
                BeginTarget(graphicsDevice, batch, result.Target, ShadingResources.AlphaSubtractBlendState, ShadingResources.OpaqueWhite);
                for (var i = -count; i <= count; i++)
                    DrawTap(batch, blurred, new Vector2(0f, i * step), blurredArea, 1f, taps[Math.Abs(i)]);
                batch.End();
            }
            else
            {
                BeginTarget(graphicsDevice, batch, result.Target, ShadingResources.AlphaSubtractBlendState, ShadingResources.OpaqueWhite);
                DrawTap(batch, silhouette, shift, area, sourceScale, (int)Math.Round(255f * gain));
                batch.End();
            }

            // Only inside the silhouette.
            graphicsDevice.SetRenderTarget(result.Target);
            batch.Begin(SpriteSortMode.Deferred, ShadingResources.AlphaMultiplyBlendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            DrawTap(batch, silhouette, Vector2.Zero, area, sourceScale, 255);
            batch.End();
        }

        /// <summary>
        /// Multiplies the color of a shape by its alpha (the shapes are drawn premultiplied, in the batch of the controls).
        /// The target stays set.
        /// </summary>
        private static void Premultiply(GraphicsDevice graphicsDevice, SpriteBatch batch, RenderTarget2D target)
        {
            graphicsDevice.SetRenderTarget(target);
            // Linear like the controls: a texture drawn with another sampler state than the last time costs GL calls.
            batch.Begin(SpriteSortMode.Deferred, ShadingResources.PremultiplyBlendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            batch.Draw(ShapeExtension.WhitePixelTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(target.Width, target.Height),
                SpriteEffects.None, 0f);
            batch.End();
        }

        /// <summary>
        /// Downsamples a silhouette with a box filter (each bilinear sample averages 2x2 pixels, (downsample / 2)^2
        /// samples per pixel) into a scratch target, and returns it.
        /// </summary>
        private static RenderTarget2D Downsample(GraphicsDevice graphicsDevice, SpriteBatch batch, Texture2D source, Rectangle sourceArea, int downsample,
            int width, int height, int scratchIndex)
        {
            var target = BeginScratch(graphicsDevice, batch, scratchIndex, width, height, ShadingResources.AlphaAddBlendState, out _);
            var half = downsample / 2;
            var weight = (int)Math.Ceiling(255f / (half * half));
            for (var y = 0; y < half; y++)
            {
                for (var x = 0; x < half; x++)
                {
                    var position = new Vector2(-(1 + 2 * x - half), -(1 + 2 * y - half)) / downsample;
                    DrawTap(batch, source, position, sourceArea, 1f / downsample, weight);
                }
            }

            batch.End();
            return target;
        }

        /// <summary>Renders the shape of an effect from the silhouette: downsample, grow, blur.</summary>
        private void RenderShape(GraphicsDevice graphicsDevice, SpriteBatch batch, ShadingEffect effect, RenderTarget2D capture, int width, int height)
        {
            var dilation = effect.Dilation * pixelScale;
            var sigma = effect.SoftEdge * pixelScale / 3f;

            // Big blurs are made at a lower resolution: fewer taps, no banding, and the result is smooth when scaled up.
            // Thick outlines too: their edge is a bit softer, but they are grown with fewer stamps.
            var downsample = 1;
            while ((sigma / downsample > MaxSigmaPixels || dilation / downsample > MaxDilationPixels) && downsample < MaxDownsample)
                downsample *= 2;
            dilation /= downsample;
            sigma /= downsample;
            var shapeWidth = (int)Math.Ceiling(width / (float)downsample);
            var shapeHeight = (int)Math.Ceiling(height / (float)downsample);

            Texture2D source = capture;
            var sourceIndex = 0;
            var sourceArea = new Rectangle(0, 0, width, height);
            var sourceScale = 1f / downsample;

            if (downsample > 1)
            {
                var targetIndex = NextScratch(sourceIndex);
                source = Downsample(graphicsDevice, batch, source, sourceArea, downsample, shapeWidth, shapeHeight, targetIndex);
                sourceIndex = targetIndex;
                sourceArea = new Rectangle(0, 0, shapeWidth, shapeHeight);
                sourceScale = 1f;
            }

            if (dilation > 0.2f)
            {
                // Stamps the silhouette on rings, one pixel apart, up to the dilation: the union is the grown silhouette.
                var target = BeginScratch(graphicsDevice, batch, NextScratch(sourceIndex), shapeWidth, shapeHeight, ShadingResources.AlphaUnionBlendState, out var targetIndex);
                DrawTap(batch, source, Vector2.Zero, sourceArea, sourceScale, 255);
                var directions = dilation > 12f ? 32 : 16;
                var ring = 0;
                for (var radius = dilation; radius > 0.01f; radius -= 1f, ring++)
                {
                    // Every other ring is turned by half a step, so the edge is closer to a circle.
                    var turn = ring % 2 == 0 ? 0f : MathHelper.Pi / directions;
                    for (var i = 0; i < directions; i++)
                    {
                        var angle = turn + i * MathHelper.TwoPi / directions;
                        DrawTap(batch, source, new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius, sourceArea, sourceScale, 255);
                    }
                }

                batch.End();
                source = target;
                sourceIndex = targetIndex;
                sourceArea = new Rectangle(0, 0, shapeWidth, shapeHeight);
                sourceScale = 1f;
            }

            var result = GetResult(graphicsDevice, effect, shapeWidth, shapeHeight, downsample);
            var gain = renderGain;
            if (sigma > 0.25f)
            {
                // Horizontal pass into a scratch target, vertical pass into the result (with the gain of the effect).
                var count = ComputeTaps(sigma, 1f, out var step);
                BeginScratch(graphicsDevice, batch, NextScratch(sourceIndex), shapeWidth, shapeHeight, ShadingResources.AlphaAddBlendState, out var targetIndex);
                for (var i = -count; i <= count; i++)
                    DrawTap(batch, source, new Vector2(i * step, 0f), sourceArea, sourceScale, taps[Math.Abs(i)]);
                batch.End();
                source = ShadingResources.GetScratch(targetIndex, shapeWidth, shapeHeight);
                sourceArea = new Rectangle(0, 0, shapeWidth, shapeHeight);
                sourceScale = 1f;

                count = ComputeTaps(sigma, gain, out step);
                BeginTarget(graphicsDevice, batch, result.Target, ShadingResources.AlphaAddBlendState);
                for (var i = -count; i <= count; i++)
                    DrawTap(batch, source, new Vector2(0f, i * step), sourceArea, sourceScale, taps[Math.Abs(i)]);
                batch.End();
            }
            else
            {
                BeginTarget(graphicsDevice, batch, result.Target, ShadingResources.AlphaAddBlendState);
                DrawTap(batch, source, Vector2.Zero, sourceArea, sourceScale, (int)Math.Round(255f * gain));
                batch.End();
            }
        }

        /// <summary>
        /// Fills <see cref="taps"/> with the gaussian weights of a blur (in 1/255 units, summing to 255 times
        /// <paramref name="gain"/>) and returns the number of taps on each side.
        /// </summary>
        private int ComputeTaps(float sigma, float gain, out float step)
        {
            var count = Math.Min(MaxTapsPerSide, Math.Max(1, (int)Math.Ceiling(3f * sigma)));
            step = 3f * sigma / count;

            var total = 1f;
            Span<float> weights = stackalloc float[MaxTapsPerSide + 1];
            weights[0] = 1f;
            for (var i = 1; i <= count; i++)
            {
                var distance = i * step;
                weights[i] = (float)Math.Exp(-distance * distance / (2f * sigma * sigma));
                total += weights[i] * 2f;
            }

            var target = (int)Math.Round(255f * gain);
            var sides = 0;
            for (var i = 1; i <= count; i++)
            {
                taps[i] = (int)Math.Round(weights[i] / total * target);
                sides += taps[i] * 2;
            }

            // The center takes what the rounding lost, so the weights always sum to the target.
            taps[0] = Math.Max(0, target - sides);
            return count;
        }

        /// <summary>Draws a texture with an alpha weight in 1/255 units (several draws when it is more than 255).</summary>
        private static void DrawTap(SpriteBatch batch, Texture2D texture, Vector2 position, Rectangle sourceArea, float scale, int weight)
        {
            while (weight > 0)
            {
                var alpha = Math.Min(255, weight);
                batch.Draw(texture, position, sourceArea, new Color(255, 255, 255, alpha), 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                weight -= alpha;
            }
        }

        private static int NextScratch(int index) => index == 1 ? 2 : 1;

        private static RenderTarget2D BeginScratch(GraphicsDevice graphicsDevice, SpriteBatch batch, int index, int width, int height, BlendState blendState, out int targetIndex)
        {
            targetIndex = index;
            var target = ShadingResources.GetScratch(index, width, height);
            BeginTarget(graphicsDevice, batch, target, blendState);
            return target;
        }

        private static void BeginTarget(GraphicsDevice graphicsDevice, SpriteBatch batch, RenderTarget2D target, BlendState blendState, Color? clearColor = null)
        {
            graphicsDevice.SetRenderTarget(target);
            graphicsDevice.Clear(clearColor ?? ShadingResources.TransparentWhite);
            batch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
        }

        private ShapeResult GetResult(GraphicsDevice graphicsDevice, ShadingEffect effect, int width, int height, int downsample)
        {
            if (!results.TryGetValue((effect, renderLevel, renderOpacity), out var result))
                results[(effect, renderLevel, renderOpacity)] = result = new ShapeResult();

            if (result.Target == null || result.Target.IsDisposed || result.Target.Width != width || result.Target.Height != height)
            {
                result.Target?.Dispose();
                result.Target = new RenderTarget2D(graphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            }

            result.Downsample = downsample;
            return result;
        }

        /// <summary>The transformation from the local space of the control to the coordinate space of the sprite batch.</summary>
        private Matrix GetLocalTransform(Matrix? parentTransform)
        {
            var position = host.GetPosition();
            var origin = host.OriginWithoutScale;
            var scale = host.NestedScale;
            var matrix = Matrix.CreateTranslation(-origin.X, -origin.Y, 0f)
                * Matrix.CreateScale(scale.X, scale.Y, 1f);
            if (host.Rotation != 0f)
                matrix *= Matrix.CreateRotationZ(host.Rotation);
            matrix *= Matrix.CreateTranslation(position.X, position.Y, 0f);
            return parentTransform.HasValue ? matrix * parentTransform.Value : matrix;
        }

        private void ReleaseResults()
        {
            foreach (var result in results.Values)
                result.Dispose();
            results.Clear();
            hasKey = false;
            isReady = false;
        }

        /// <summary>
        /// The rendered shape of an effect: white with straight alpha (colored for fills), <see cref="Downsample"/> times
        /// smaller.
        /// </summary>
        private sealed class ShapeResult
        {
            public RenderTarget2D Target;
            public int Downsample = 1;

            /// <summary>The key of the effect when its shape was rendered.</summary>
            public int Key;

            /// <summary>The generation of the renderer (content and scale) when it was rendered.</summary>
            public int Generation;

            /// <summary>The size of the control when it was rendered (it is stretched to the size of the control when that stretches).</summary>
            public Vector2 Size;

            /// <summary>The gradient of a fill, and the key of its colors.</summary>
            public Texture2D Gradient;
            public int GradientKey;

            public bool IsValid => Target != null && !Target.IsDisposed;

            public void Dispose()
            {
                Target?.Dispose();
                Target = null;
                Gradient?.Dispose();
                Gradient = null;
            }
        }
    }
}
