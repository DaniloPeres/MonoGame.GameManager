using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// A shadow inside the silhouette of the control, along the edges its offset moves away from: with the default
    /// offset (down), the top of the letters is shaded, as if they were carved in. With a light color and
    /// <see cref="ShadingBlend.Light"/> it is an inner highlight: two of them make a bevel
    /// (<see cref="ShadingPresets.Bevel"/>). Drawn over the control (<see cref="ShadingLayer.Front"/>).
    /// </summary>
    /// <remarks>
    /// The offset is in the local space of the control and is baked into the shape: it turns with a rotated control,
    /// and an orbiting inner shadow is rendered again in small steps while it moves.
    /// </remarks>
    /// <example>
    /// <code>
    /// // A light top edge and a dark bottom edge (a bevel) over a gradient fill.
    /// title.AddShadings(
    ///     new GradientFill(Color.LightYellow, Color.Orange),
    ///     new InnerShadow(new Vector2(0, 3), 2, Color.White * 0.8f).SetBlend(ShadingBlend.Light),
    ///     new InnerShadow(new Vector2(0, -3), 2, new Color(90, 40, 0) * 0.6f));
    /// </code>
    /// </example>
    public class InnerShadow : ShadingEffect<InnerShadow>
    {
        private float blur = 2f;

        public InnerShadow()
        {
            Color = Color.Black;
            Intensity = 0.6f;
            Offset = new Vector2(0f, 2f);
            Layer = ShadingLayer.Front;
        }

        /// <param name="offset">How far the inner copy of the silhouette is moved: the shadow is on the opposite edges.</param>
        /// <param name="blur">How soft the shadow is, in local units.</param>
        /// <param name="color">The color of the shadow (its alpha is its strength).</param>
        public InnerShadow(Vector2 offset, float blur, Color color) : this()
        {
            Offset = offset;
            Blur = blur;
            Color = color;
            Intensity = 1f;
        }

        /// <summary>How soft the shadow is, in local units (0 = a sharp edge).</summary>
        public float Blur
        {
            get => blur;
            set => blur = Math.Max(0f, value);
        }

        internal override float Dilation => 0f;

        internal override float SoftEdge => blur;

        internal override float Extent => 0f;

        internal override ShadingShape Shape => ShadingShape.Inner;

        internal override bool IsAttached => true;

        internal override int GetDynamicShapeKey(float time) => GetBakedOffset(time).GetHashCode();

        public InnerShadow SetBlur(float blur)
        {
            Blur = blur;
            return this;
        }
    }
}
