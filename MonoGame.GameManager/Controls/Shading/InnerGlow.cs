using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// A glow inside the silhouette of the control, along all its edges (light caught in ice, lit glass, a magic
    /// rim). Added over the control (<see cref="ShadingLayer.Front"/>, <see cref="ShadingBlend.Light"/>); with
    /// <see cref="ShadingBlend.Normal"/> and a dark color it darkens the edges instead (a vignette inside the letters).
    /// </summary>
    /// <example>
    /// <code>
    /// title.AddShading(new InnerGlow(new Color(150, 230, 255), 6));
    /// </code>
    /// </example>
    public class InnerGlow : ShadingEffect<InnerGlow>
    {
        private float radius = 5f;

        public InnerGlow()
        {
            Color = Color.White;
            Blend = ShadingBlend.Light;
            Layer = ShadingLayer.Front;
        }

        /// <param name="color">The color of the glow.</param>
        /// <param name="radius">How far the glow reaches inside the silhouette, in local units.</param>
        public InnerGlow(Color color, float radius) : this()
        {
            Color = color;
            Radius = radius;
        }

        /// <summary>How far the glow reaches inside the silhouette, in local units.</summary>
        public float Radius
        {
            get => radius;
            set => radius = Math.Max(0.5f, value);
        }

        internal override float Dilation => 0f;

        internal override float SoftEdge => radius;

        internal override float Extent => 0f;

        internal override ShadingShape Shape => ShadingShape.Inner;

        internal override bool IsAttached => true;

        internal override int GetDynamicShapeKey(float time) => GetBakedOffset(time).GetHashCode();

        public InnerGlow SetRadius(float radius)
        {
            Radius = radius;
            return this;
        }
    }
}
