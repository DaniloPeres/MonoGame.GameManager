using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// A shadow of the silhouette of the control, moved by an offset: soft (<see cref="Blur"/>), or hard and bold with a
    /// <see cref="Spread"/> and no blur (the deep shadow of cartoon titles). Painted under the control by default.
    /// </summary>
    /// <example>
    /// <code>
    /// title.AddShading(new Shadow(new Vector2(0, 4), 6, Color.Black * 0.6f));
    /// title.AddShading(new Shadow().SetOffset(4, 5).SetBlur(0).SetSpread(2).SetIntensity(1f)); // a hard, bold shadow
    /// </code>
    /// </example>
    public class Shadow : ShadingEffect<Shadow>
    {
        private float blur = 6f;
        private float spread;

        public Shadow()
        {
            Color = Color.Black;
            Intensity = 0.5f;
            Offset = new Vector2(0f, 4f);
        }

        /// <param name="offset">How far the shadow is moved from the control.</param>
        /// <param name="blur">How far the soft edge spreads (0 = a sharp shadow).</param>
        /// <param name="color">The color of the shadow (eg: <c>Color.Black * 0.6f</c>).</param>
        public Shadow(Vector2 offset, float blur, Color color) : this()
        {
            Offset = offset;
            Blur = blur;
            Color = color;
            Intensity = 1f;
        }

        /// <summary>How far the soft edge of the shadow spreads, in local units (0 = a sharp shadow).</summary>
        public float Blur
        {
            get => blur;
            set => blur = Math.Max(0f, value);
        }

        /// <summary>Grows the shadow by this distance before it is blurred (bolder shadows).</summary>
        public float Spread
        {
            get => spread;
            set => spread = Math.Max(0f, value);
        }

        internal override float Dilation => spread;

        internal override float SoftEdge => blur;

        public Shadow SetBlur(float blur)
        {
            Blur = blur;
            return this;
        }

        public Shadow SetSpread(float spread)
        {
            Spread = spread;
            return this;
        }
    }
}
