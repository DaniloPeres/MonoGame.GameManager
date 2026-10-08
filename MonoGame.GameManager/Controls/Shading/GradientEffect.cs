using Microsoft.Xna.Framework;
using MonoGame.GameManager.GameMath;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>What the renderer needs from a gradient effect (<see cref="GradientFill"/>, <see cref="GradientOverlay"/>).</summary>
    internal interface IGradientShape
    {
        IList<Color> Colors { get; }
        IList<float> Positions { get; }
        float Angle { get; }
        float RangeStart { get; }
        float RangeEnd { get; }
        GradientBounds Bounds { get; }
        float Hardness { get; }
        float ScrollSpeed { get; }
        bool Repeat { get; }

        /// <summary>True when the alpha of the colors is kept (overlays); false paints the whole silhouette (fills).</summary>
        bool KeepsAlpha { get; }

        float GetPhase(float time);

        int GetGradientKey();
    }

    /// <summary>
    /// Base class of the gradients painted inside the silhouette of a control: <see cref="GradientFill"/> (the colors of
    /// the control) and <see cref="GradientOverlay"/> (a transparent gradient over them: gloss bands, shades, stripes).
    /// </summary>
    /// <remarks>
    /// The gradient is rendered into a render target with the other shading shapes and kept until the control or its
    /// options change: its intensity, pulse and flicker are free; a scrolling gradient is rendered again in small steps
    /// while it moves. The angle is in the local space of the control: the gradient turns with a rotated control.
    /// </remarks>
    public abstract class GradientEffect<TEffect> : ShadingEffect<TEffect>, IGradientShape where TEffect : GradientEffect<TEffect>
    {
        /// <summary>The steps of a scrolling gradient per length.</summary>
        internal const int PhaseSteps = 128;

        private static readonly Color[] DefaultColors = { new Color(255, 250, 215), new Color(240, 150, 20) };
        private IList<Color> colors = DefaultColors;
        private float hardness;

        /// <summary>The colors of the gradient, from the start to the end (one color = a solid color).</summary>
        public IList<Color> Colors
        {
            get => colors;
            set => colors = value == null || value.Count == 0 ? DefaultColors : value;
        }

        /// <summary>
        /// Where each color is, from 0 (the start) to 1 (the end), one per color in increasing order; null spreads the
        /// colors evenly.
        /// </summary>
        public IList<float> Positions { get; set; }

        /// <summary>
        /// The direction of the gradient, in degrees, in the local space of the control: 90 (the default) goes from the
        /// top to the bottom, 0 from the left to the right.
        /// </summary>
        public float Angle { get; set; } = 90f;

        /// <summary>Where the first color starts, as a fraction of the bounds along the direction (0 = their start).</summary>
        public float RangeStart { get; set; }

        /// <summary>Where the last color ends, as a fraction of the bounds along the direction (1 = their end).</summary>
        public float RangeEnd { get; set; } = 1f;

        /// <summary>The area the gradient is stretched over (by default the box of the glyphs).</summary>
        public GradientBounds Bounds { get; set; } = GradientBounds.Content;

        /// <summary>
        /// How sharp the passage from a color to the next is: 0 is smooth, 1 is a hard edge (two-tone or striped
        /// letters).
        /// </summary>
        public float Hardness
        {
            get => hardness;
            set => hardness = MathUtils.Clamp01(value);
        }

        /// <summary>
        /// When true, the gradient repeats outside its range (stripes: a short range repeated over the glyphs); when
        /// false, the end colors extend outside it.
        /// </summary>
        public bool Repeat { get; set; }

        /// <summary>
        /// How many times per second the gradient moves by its whole length along its direction (0 = still). While it
        /// moves, the gradient repeats: use colors that end like they start for a seamless loop.
        /// </summary>
        public float ScrollSpeed { get; set; }

        internal override float Dilation => 0f;

        internal override float SoftEdge => 0f;

        internal override float Extent => 0f;

        internal override ShadingShape Shape => ShadingShape.Fill;

        internal override bool IsAttached => true;

        bool IGradientShape.KeepsAlpha => KeepsAlpha;

        float IGradientShape.GetPhase(float time) => GetPhase(time);

        int IGradientShape.GetGradientKey() => GetGradientKey();

        /// <summary>True when the alpha of the colors is kept.</summary>
        internal abstract bool KeepsAlpha { get; }

        /// <summary>Sets the colors of the gradient, from the start to the end.</summary>
        public TEffect SetColors(params Color[] colors)
        {
            Colors = colors;
            return ThisAsT;
        }

        /// <summary>Sets where each color is, from 0 to 1 (null spreads them evenly).</summary>
        public TEffect SetPositions(params float[] positions)
        {
            Positions = positions;
            return ThisAsT;
        }

        /// <summary>Sets the direction of the gradient, in degrees (90 = from the top to the bottom).</summary>
        public TEffect SetAngle(float degrees)
        {
            Angle = degrees;
            return ThisAsT;
        }

        /// <summary>Sets where the gradient starts and ends, as fractions of the bounds along its direction.</summary>
        public TEffect SetRange(float start, float end)
        {
            RangeStart = start;
            RangeEnd = end;
            return ThisAsT;
        }

        /// <summary>Sets the area the gradient is stretched over.</summary>
        public TEffect SetBounds(GradientBounds bounds)
        {
            Bounds = bounds;
            return ThisAsT;
        }

        /// <summary>Sets how sharp the passage from a color to the next is (0 = smooth, 1 = a hard edge).</summary>
        public TEffect SetHardness(float hardness)
        {
            Hardness = hardness;
            return ThisAsT;
        }

        /// <summary>Makes the gradient repeat outside its range (stripes) or extend its end colors.</summary>
        public TEffect SetRepeat(bool repeat = true)
        {
            Repeat = repeat;
            return ThisAsT;
        }

        /// <summary>Makes the gradient move along its direction, in lengths per second (0 = still).</summary>
        public TEffect SetScroll(float speed)
        {
            ScrollSpeed = speed;
            return ThisAsT;
        }

        internal override int GetShapeKey()
            => HashCode.Combine(base.GetShapeKey(), GetGradientKey(), (int)Math.Round(Angle * 2f), (int)Math.Round(RangeStart * 256f),
                (int)Math.Round(RangeEnd * 256f), Bounds, ScrollSpeed != 0f, Repeat);

        internal override int GetDynamicShapeKey(float time) => ScrollSpeed != 0f ? (int)Math.Round(GetPhase(time) * PhaseSteps) : 0;

        /// <summary>How far a scrolling gradient moved, from 0 to 1 lengths, in steps.</summary>
        internal float GetPhase(float time)
        {
            if (ScrollSpeed == 0f)
                return 0f;
            var phase = MathUtils.Wrap((time + TimeOffset) * ScrollSpeed, 0f, 1f);
            return (float)Math.Floor(phase * PhaseSteps) / PhaseSteps;
        }

        /// <summary>A key of the options that change the texture of the gradient.</summary>
        internal int GetGradientKey()
        {
            var hash = new HashCode();
            hash.Add((int)Math.Round(hardness * 64f));
            hash.Add(KeepsAlpha);
            foreach (var color in colors)
                hash.Add(color.PackedValue);
            if (Positions != null)
            {
                foreach (var position in Positions)
                    hash.Add((int)Math.Round(position * 512f));
            }

            return hash.ToHashCode();
        }
    }
}
