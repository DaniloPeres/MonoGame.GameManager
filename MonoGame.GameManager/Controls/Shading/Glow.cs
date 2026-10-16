using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// A soft halo of light around the silhouette of the control. It is added to what is below it
    /// (<see cref="ShadingBlend.Light"/>), so it shines best on dark backgrounds; <see cref="ShadingLayer.Front"/> draws
    /// it over the control too (a bloom), and <see cref="ShadingBlend.Normal"/> paints it (eg: a dark aura on a light
    /// background).
    /// </summary>
    /// <example>
    /// <code>
    /// title.AddShading(new Glow(Color.Gold, 16).SetPulse(1f, 0.4f));
    /// neonSign.AddShading(new Glow(Color.Magenta, 6).SetSpread(1).SetIntensity(2.5f))
    ///         .AddShading(new Glow(Color.Magenta, 26).SetIntensity(1.2f));
    /// </code>
    /// </example>
    public class Glow : ShadingEffect<Glow>
    {
        private float radius = 12f;
        private float spread = 1f;

        public Glow()
        {
            Blend = ShadingBlend.Light;
            Intensity = 1.5f;
        }

        /// <param name="color">The color of the light.</param>
        /// <param name="radius">How far the light spreads around the silhouette.</param>
        public Glow(Color color, float radius) : this()
        {
            Color = color;
            Radius = radius;
        }

        /// <summary>How far the light spreads around the silhouette, in local units.</summary>
        public float Radius
        {
            get => radius;
            set => radius = Math.Max(0f, value);
        }

        /// <summary>Grows the silhouette by this distance before the light fades (a brighter, thicker core).</summary>
        public float Spread
        {
            get => spread;
            set => spread = Math.Max(0f, value);
        }

        internal override float Dilation => spread;

        internal override float SoftEdge => radius;

        public Glow SetRadius(float radius)
        {
            Radius = radius;
            return this;
        }

        public Glow SetSpread(float spread)
        {
            Spread = spread;
            return this;
        }
    }
}
