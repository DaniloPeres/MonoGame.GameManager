using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// Star glints scattered over the glyphs of a text (or the opaque pixels of an image) that twinkle: the light caught
    /// by gold and gems in fantasy logos. Only the parts over the silhouette (and a little around it, <see cref="Spill"/>) are drawn. Added as light over the control
    /// (<see cref="ShadingLayer.Front"/>, <see cref="ShadingBlend.Light"/>).
    /// </summary>
    /// <remarks>
    /// The stars are placed from <see cref="Seed"/> inside the box of the glyphs; a star over the space between two
    /// letters is not seen, so use a few more stars than wanted. Twinkling sparkles are rendered again in small steps.
    /// </remarks>
    /// <example>
    /// <code>
    /// title.AddShading(new Sparkles(Color.White, 10, 18).SetTwinkle(0.8f));
    /// </code>
    /// </example>
    public class Sparkles : ShadingEffect<Sparkles>
    {
        /// <summary>The steps of a twinkle per second, per unit of speed.</summary>
        internal const int TwinkleSteps = 16;

        private int count = 8;
        private float size = 16f;
        private float spill = 3f;

        public Sparkles()
        {
            Color = Color.White;
            Intensity = 1.4f;
            Layer = ShadingLayer.Front;
            Blend = ShadingBlend.Light;
        }

        /// <param name="color">The color of the stars.</param>
        /// <param name="count">How many stars are scattered over the glyphs.</param>
        /// <param name="size">The size (from tip to tip) of the biggest stars, in local units.</param>
        public Sparkles(Color color, int count, float size) : this()
        {
            Color = color;
            Count = count;
            Size = size;
        }

        /// <summary>How many stars are scattered over the glyphs (at most 64).</summary>
        public int Count
        {
            get => count;
            set => count = Math.Max(0, Math.Min(64, value));
        }

        /// <summary>The size (from tip to tip) of the biggest stars, in local units.</summary>
        public float Size
        {
            get => size;
            set => size = Math.Max(1f, value);
        }

        /// <summary>
        /// How far the rays of the stars can go outside the silhouette, in local units (0 = only over the silhouette).
        /// </summary>
        public float Spill
        {
            get => spill;
            set => spill = Math.Max(0f, Math.Min(32f, value));
        }

        /// <summary>The seed of the positions and sizes of the stars.</summary>
        public int Seed { get; set; } = 1;

        /// <summary>How many times per second each star twinkles (0 = still stars).</summary>
        public float TwinkleSpeed { get; set; }

        internal override float Dilation => 0f;

        internal override float SoftEdge => 0f;

        internal override float Extent => spill;

        internal override ShadingShape Shape => ShadingShape.Sparkles;

        internal override bool IsAttached => true;

        public Sparkles SetCount(int count)
        {
            Count = count;
            return this;
        }

        public Sparkles SetSize(float size)
        {
            Size = size;
            return this;
        }

        /// <summary>Sets how far the rays of the stars can go outside the silhouette, in local units.</summary>
        public Sparkles SetSpill(float spill)
        {
            Spill = spill;
            return this;
        }

        public Sparkles SetSeed(int seed)
        {
            Seed = seed;
            return this;
        }

        /// <summary>Makes the stars twinkle, in twinkles per second (0 = still).</summary>
        public Sparkles SetTwinkle(float speed)
        {
            TwinkleSpeed = Math.Max(0f, speed);
            return this;
        }

        internal override int GetShapeKey() => HashCode.Combine(base.GetShapeKey(), count, (int)Math.Round(size * 8f), (int)Math.Round(spill * 8f), Seed, TwinkleSpeed > 0f);

        internal override int GetDynamicShapeKey(float time)
            => TwinkleSpeed > 0f ? (int)Math.Floor((time + TimeOffset) * TwinkleSpeed * TwinkleSteps) : 0;

        /// <summary>The time used for the twinkle, in the steps of the dynamic key (in twinkles).</summary>
        internal float GetTwinklePhase(float time)
            => TwinkleSpeed > 0f ? (float)Math.Floor((time + TimeOffset) * TwinkleSpeed * TwinkleSteps) / TwinkleSteps : 0f;
    }
}
