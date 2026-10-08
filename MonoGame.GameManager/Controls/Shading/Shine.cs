using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// A slanted band of light that sweeps across the control every few seconds, lighting only its silhouette (the
    /// glyphs of a text, the opaque pixels of an image): the shine of gold titles, coins and gems. Added over the control
    /// (<see cref="ShadingLayer.Front"/>, <see cref="ShadingBlend.Light"/>).
    /// </summary>
    /// <example>
    /// <code>
    /// title.AddShading(new Shine().SetColor(new Color(255, 245, 200)).SetTiming(0.9f, 3f));
    /// </code>
    /// </example>
    public class Shine : ShadingEffect<Shine>
    {
        private float width = 28f;
        private float duration = 0.8f;
        private float interval = 2.5f;
        private float softness = 1f;

        public Shine()
        {
            Blend = ShadingBlend.Light;
            Layer = ShadingLayer.Front;
            Intensity = 1f;
        }

        /// <param name="color">The color of the light.</param>
        public Shine(Color color) : this()
        {
            Color = color;
        }

        /// <summary>The width of the band of light, in local units.</summary>
        public float Width
        {
            get => width;
            set => width = Math.Max(1f, value);
        }

        /// <summary>How long a sweep takes, in seconds.</summary>
        public float Duration
        {
            get => duration;
            set => duration = Math.Max(0.05f, value);
        }

        /// <summary>The time from the start of a sweep to the start of the next one, in seconds.</summary>
        public float Interval
        {
            get => interval;
            set => interval = Math.Max(0f, value);
        }

        /// <summary>The slant of the band, in degrees (0 = vertical).</summary>
        public float Angle { get; set; } = 25f;

        /// <summary>When true, the band sweeps from the right to the left.</summary>
        public bool Reverse { get; set; }

        /// <summary>How far the light spills outside the silhouette, in local units (0 = only inside it).</summary>
        public float Softness
        {
            get => softness;
            set => softness = Math.Max(0f, value);
        }

        internal override float Dilation => 0f;

        internal override float SoftEdge => softness;

        /// <summary>
        /// The position of the band at a time, from 0 (entering) to 1 (gone), or a negative value between two sweeps.
        /// </summary>
        internal float GetProgress(float time)
        {
            var cycle = Math.Max(duration, interval);
            var local = Wrap(time + TimeOffset, cycle);
            return local <= duration ? local / duration : -1f;
        }

        private static float Wrap(float value, float period)
        {
            var result = value % period;
            return result < 0f ? result + period : result;
        }

        public Shine SetWidth(float width)
        {
            Width = width;
            return this;
        }

        /// <summary>Sets how long a sweep takes and the time between the starts of two sweeps, in seconds.</summary>
        public Shine SetTiming(float duration, float interval)
        {
            Duration = duration;
            Interval = interval;
            return this;
        }

        public Shine SetAngle(float degrees)
        {
            Angle = degrees;
            return this;
        }

        public Shine SetReverse(bool reverse)
        {
            Reverse = reverse;
            return this;
        }

        public Shine SetSoftness(float softness)
        {
            Softness = softness;
            return this;
        }
    }
}
