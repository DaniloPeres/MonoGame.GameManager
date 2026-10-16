using Microsoft.Xna.Framework;
using MonoGame.GameManager.GameMath;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// A shadow, a glow or an outline that follows the silhouette of a control: the opaque pixels of an image, a sprite
    /// animation or a nine-slice image, the glyphs of a text, or anything else a control draws. Added with
    /// <see cref="Abstracts.ScalableControlAbstract{TControl}.AddShading"/>; several effects can be stacked on the same
    /// control (eg: an outline, a glow and two shadows on a title).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The silhouette of the control is drawn into a render target, grown (outline, spread) and blurred once, and the
    /// result is reused every frame until the control changes (its text, texture, frame, size, scale or alpha, or the
    /// shape options of its effects). The color, the pulse, the color cycle, the offset and the rotation of the control
    /// can change every frame for free.
    /// </para>
    /// <para>
    /// Sizes are in local units of the control, so they grow with its scale. The <see cref="Offset"/> keeps its
    /// direction on the screen when the control rotates (a shadow always falls down).
    /// </para>
    /// </remarks>
    public abstract class ShadingEffect
    {
        /// <summary>The highest <see cref="Intensity"/>.</summary>
        public const float MaxIntensity = 8f;

        private float intensity = 1f;

        /// <summary>When false, the effect is neither rendered nor drawn.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>The color of the effect (its alpha makes it more transparent).</summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// The strength of the effect: 1 = the color as it is, lower values are more transparent, higher values (up to
        /// <see cref="MaxIntensity"/>) make soft glows and shadows denser near the silhouette.
        /// </summary>
        public float Intensity
        {
            get => intensity;
            set => intensity = MathUtils.Clamp(value, 0f, MaxIntensity);
        }

        /// <summary>How the effect is mixed with what is below it.</summary>
        public ShadingBlend Blend { get; set; } = ShadingBlend.Normal;

        /// <summary>Whether the effect is drawn under the control or over it.</summary>
        public ShadingLayer Layer { get; set; } = ShadingLayer.Behind;

        /// <summary>How far the effect is moved from the control, in local units, in a fixed direction on the screen.</summary>
        public Vector2 Offset { get; set; }

        /// <summary>The number of pulses per second (0 = no pulse).</summary>
        public float PulseSpeed { get; set; }

        /// <summary>How much the effect dims during a pulse, from 0 (no pulse) to 1 (it goes off).</summary>
        public float PulseAmount { get; set; }

        /// <summary>Added to the time of the control, to shift the pulse and the color cycle of this effect.</summary>
        public float TimeOffset { get; set; }

        /// <summary>Colors the effect goes through, one after the other (null or empty = <see cref="Color"/>).</summary>
        public IList<Color> ColorCycle { get; set; }

        /// <summary>The duration of a whole <see cref="ColorCycle"/>, in seconds.</summary>
        public float ColorCyclePeriod { get; set; } = 3f;

        /// <summary>How much the effect flickers like a flame or an old neon tube, from 0 (steady) to 1 (it can go off).</summary>
        public float FlickerAmount { get; set; }

        /// <summary>How fast the effect flickers (about the number of flickers per second).</summary>
        public float FlickerSpeed { get; set; } = 8f;

        /// <summary>
        /// How much the effect grows and shrinks around the center of the control while it breathes, as a part of its
        /// size (eg: 0.06 = 6% bigger at the top of a breath). It costs nothing: the rendered shape is scaled.
        /// </summary>
        public float BreatheAmount { get; set; }

        /// <summary>The number of breaths per second (0 = no breathing).</summary>
        public float BreatheSpeed { get; set; }

        /// <summary>The radius of a circle the effect turns around (added to <see cref="Offset"/>), in local units.</summary>
        public float OrbitRadius { get; set; }

        /// <summary>The number of turns per second around the orbit (negative = counterclockwise).</summary>
        public float OrbitSpeed { get; set; }

        /// <summary>How far the silhouette is grown before it is blurred, in local units.</summary>
        internal abstract float Dilation { get; }

        /// <summary>How far the soft edge spreads, in local units (0 = a sharp edge).</summary>
        internal abstract float SoftEdge { get; }

        /// <summary>The control that owns the effect.</summary>
        internal object Owner { get; set; }

        /// <summary>The part of the intensity above 1, baked into the rendered shape (denser soft edges).</summary>
        internal float BakedGain => Math.Max(1f, intensity);

        /// <summary>How far the effect reaches outside the silhouette, in local units (offset not included).</summary>
        internal virtual float Extent => Dilation + SoftEdge;

        /// <summary>How the shape of the effect is rendered from the silhouette.</summary>
        internal virtual ShadingShape Shape => ShadingShape.Outer;

        /// <summary>
        /// True when the shape stays on the silhouette (fills, inner shadows): it is drawn without the offset, the orbit
        /// and the breathing of the effect (an inner shadow bakes its offset into its shape).
        /// </summary>
        internal virtual bool IsAttached => false;

        /// <summary>The pulse multiplier at a time, from 1 - <see cref="PulseAmount"/> to 1.</summary>
        internal float GetPulse(float time)
            => PulseSpeed > 0f && PulseAmount > 0f
                ? 1f - MathUtils.Clamp01(PulseAmount) * (0.5f - 0.5f * (float)Math.Cos((time + TimeOffset) * PulseSpeed * MathHelper.TwoPi))
                : 1f;

        /// <summary>The flicker multiplier at a time, from 1 - <see cref="FlickerAmount"/> to 1 (smooth noise).</summary>
        internal float GetFlicker(float time)
        {
            if (FlickerAmount <= 0f || FlickerSpeed <= 0f)
                return 1f;

            // Three waves of unrelated frequencies make a noise that never repeats visibly.
            var t = (time + TimeOffset) * FlickerSpeed;
            var noise = 0.5f * (float)Math.Sin(t * 1.13f) + 0.3f * (float)Math.Sin(t * 2.71f + 1.7f) + 0.2f * (float)Math.Sin(t * 5.37f + 0.4f);
            var amount = MathUtils.Clamp01(noise * 0.5f + 0.5f);
            return 1f - MathUtils.Clamp01(FlickerAmount) * amount * amount;
        }

        /// <summary>The intensity multiplier of the animations (pulse and flicker) at a time.</summary>
        internal float GetAnimatedIntensity(float time) => GetPulse(time) * GetFlicker(time);

        /// <summary>The scale of the breathing at a time (1 = no breathing).</summary>
        internal float GetBreathe(float time)
            => BreatheSpeed > 0f && BreatheAmount != 0f
                ? 1f + BreatheAmount * (0.5f - 0.5f * (float)Math.Cos((time + TimeOffset) * BreatheSpeed * MathHelper.TwoPi))
                : 1f;

        /// <summary>The <see cref="Offset"/> plus the position on the orbit at a time.</summary>
        internal Vector2 GetAnimatedOffset(float time)
        {
            if (OrbitRadius == 0f || OrbitSpeed == 0f)
                return Offset;
            var angle = (time + TimeOffset) * OrbitSpeed * MathHelper.TwoPi;
            return Offset + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * OrbitRadius;
        }

        /// <summary>The current color: the current color of the <see cref="ColorCycle"/>, or <see cref="Color"/>.</summary>
        internal Color GetColor(float time)
        {
            var cycle = ColorCycle;
            if (cycle == null || cycle.Count == 0)
                return Color;
            if (cycle.Count == 1 || ColorCyclePeriod <= 0f)
                return cycle[0];

            var position = MathUtils.Wrap((time + TimeOffset) / ColorCyclePeriod, 0f, 1f) * cycle.Count;
            var index = Math.Min(cycle.Count - 1, (int)position);
            return Color.Lerp(cycle[index], cycle[(index + 1) % cycle.Count], position - index);
        }

        /// <summary>A key of the options that change the rendered shape (quantized, so tiny changes do not render again).</summary>
        internal virtual int GetShapeKey()
            => HashCode.Combine(IsEnabled, (int)Math.Round(Dilation * 8f), (int)Math.Round(SoftEdge * 8f), (int)Math.Round(BakedGain * 32f));

        /// <summary>
        /// A key of the animations baked into the shape (a scrolling gradient, an orbiting inner shadow): when it changes,
        /// only the shape of this effect is rendered again. 0 when nothing is baked.
        /// </summary>
        internal virtual int GetDynamicShapeKey(float time) => 0;

        /// <summary>The offset baked into an attached shape, in local units: the animated offset, in steps of a quarter unit.</summary>
        internal Vector2 GetBakedOffset(float time)
        {
            var offset = GetAnimatedOffset(time);
            return new Vector2((float)Math.Round(offset.X * 4f) / 4f, (float)Math.Round(offset.Y * 4f) / 4f);
        }
    }

    /// <summary>
    /// Base class of the shading effects, with fluent setters that return the concrete effect type
    /// (eg: <c>new Glow(Color.Gold, 16).SetPulse(1f, 0.4f).SetLayer(ShadingLayer.Front)</c>).
    /// </summary>
    public abstract class ShadingEffect<TEffect> : ShadingEffect where TEffect : ShadingEffect<TEffect>
    {
        /// <summary>This instance typed as <typeparamref name="TEffect"/>, for fluent methods.</summary>
        protected TEffect ThisAsT => (TEffect)this;

        public TEffect SetIsEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            return ThisAsT;
        }

        public TEffect SetColor(Color color)
        {
            Color = color;
            return ThisAsT;
        }

        public TEffect SetIntensity(float intensity)
        {
            Intensity = intensity;
            return ThisAsT;
        }

        public TEffect SetBlend(ShadingBlend blend)
        {
            Blend = blend;
            return ThisAsT;
        }

        public TEffect SetLayer(ShadingLayer layer)
        {
            Layer = layer;
            return ThisAsT;
        }

        public TEffect SetOffset(Vector2 offset)
        {
            Offset = offset;
            return ThisAsT;
        }

        public TEffect SetOffset(float x, float y) => SetOffset(new Vector2(x, y));

        /// <summary>Makes the effect pulse: <paramref name="speed"/> pulses per second, dimming by <paramref name="amount"/> (0 to 1).</summary>
        public TEffect SetPulse(float speed, float amount)
        {
            PulseSpeed = speed;
            PulseAmount = amount;
            return ThisAsT;
        }

        /// <summary>Makes the effect flicker like a flame or an old neon tube: <paramref name="amount"/> from 0 to 1.</summary>
        public TEffect SetFlicker(float amount, float speed = 8f)
        {
            FlickerAmount = amount;
            FlickerSpeed = speed;
            return ThisAsT;
        }

        /// <summary>
        /// Makes the effect grow and shrink: <paramref name="speed"/> breaths per second, growing by
        /// <paramref name="amount"/> (a part of its size, eg: 0.06).
        /// </summary>
        public TEffect SetBreathe(float speed, float amount)
        {
            BreatheSpeed = speed;
            BreatheAmount = amount;
            return ThisAsT;
        }

        /// <summary>Makes the effect turn around a circle of <paramref name="radius"/>, <paramref name="speed"/> turns per second.</summary>
        public TEffect SetOrbit(float radius, float speed)
        {
            OrbitRadius = radius;
            OrbitSpeed = speed;
            return ThisAsT;
        }

        public TEffect SetTimeOffset(float timeOffset)
        {
            TimeOffset = timeOffset;
            return ThisAsT;
        }

        /// <summary>Makes the effect go through colors, one after the other, in <paramref name="period"/> seconds.</summary>
        public TEffect SetColorCycle(IList<Color> colors, float period = 3f)
        {
            ColorCycle = colors;
            ColorCyclePeriod = period;
            return ThisAsT;
        }
    }
}
