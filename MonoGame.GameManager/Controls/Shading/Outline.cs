using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// A border of a given thickness around the silhouette of the control (the outlined text of games, the white border
    /// of stickers). It is painted under the control, so only the part outside the silhouette shows; stack two outlines
    /// of different thicknesses for a double border (add the thinner one last, so it is drawn over the thicker one).
    /// </summary>
    /// <example>
    /// <code>
    /// title.AddShading(new Outline(new Color(60, 20, 0), 3));
    /// icon.AddShading(new Outline(Color.Black, 7)).AddShading(new Outline(Color.White, 4));
    /// </code>
    /// </example>
    public class Outline : ShadingEffect<Outline>
    {
        private float thickness = 2f;
        private float softness;

        public Outline()
        {
            Color = Color.Black;
        }

        /// <param name="color">The color of the outline.</param>
        /// <param name="thickness">The thickness of the outline, in local units.</param>
        public Outline(Color color, float thickness) : this()
        {
            Color = color;
            Thickness = thickness;
        }

        /// <summary>The thickness of the outline, in local units.</summary>
        public float Thickness
        {
            get => thickness;
            set => thickness = Math.Max(0f, value);
        }

        /// <summary>How far the outer edge of the outline fades, in local units (0 = a crisp outline).</summary>
        public float Softness
        {
            get => softness;
            set => softness = Math.Max(0f, value);
        }

        internal override float Dilation => thickness;

        internal override float SoftEdge => softness;

        public Outline SetThickness(float thickness)
        {
            Thickness = thickness;
            return this;
        }

        public Outline SetSoftness(float softness)
        {
            Softness = softness;
            return this;
        }
    }
}
