using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// A gradient with transparent colors painted inside the silhouette of the control, over it (or over a
    /// <see cref="GradientFill"/>): the glossy band on the top half of game titles, a darker bottom, light facets of a gem,
    /// stripes and scan lines (<see cref="GradientEffect{TEffect}.Repeat"/>). Drawn over the control
    /// (<see cref="ShadingLayer.Front"/>); with <see cref="ShadingBlend.Light"/> it only brightens.
    /// </summary>
    /// <remarks>
    /// The alpha of each color is its strength (XNA colors are premultiplied: <c>Color.White * 0.6f</c> is a white at 60
    /// percent). <see cref="Band"/> creates the usual gloss: a band between two fractions of the glyphs, with soft or hard
    /// edges.
    /// </remarks>
    /// <example>
    /// <code>
    /// // A glossy upper half, fading from 70% to 25% white, with a hard edge in the middle of the letters.
    /// title.AddShading(GradientOverlay.Band(Color.White, 0.7f, 0.25f, 0f, 0.5f).SetBlend(ShadingBlend.Light));
    ///
    /// // Diagonal stripes that move.
    /// title.AddShading(new GradientOverlay(Color.White * 0.3f, Color.Transparent).SetAngle(45).SetRange(0, 0.12f)
    ///     .SetHardness(1).SetRepeat().SetScroll(0.5f));
    /// </code>
    /// </example>
    public class GradientOverlay : GradientEffect<GradientOverlay>
    {
        public GradientOverlay()
        {
            Layer = ShadingLayer.Front;
            Blend = ShadingBlend.Normal;
            Color = Color.White;
        }

        /// <param name="colors">The colors of the gradient (their alpha is their strength), from the start to the end.</param>
        public GradientOverlay(params Color[] colors) : this()
        {
            SetColors(colors);
        }

        internal override bool KeepsAlpha => true;

        /// <summary>
        /// A band of a color between two fractions of the glyphs along the direction (90 degrees: from the top to the
        /// bottom), going from <paramref name="startStrength"/> to <paramref name="endStrength"/>, transparent outside it.
        /// </summary>
        /// <param name="color">The color of the band (opaque; the strengths make it transparent).</param>
        /// <param name="startStrength">The strength at the start of the band, from 0 to 1.</param>
        /// <param name="endStrength">The strength at the end of the band, from 0 to 1.</param>
        /// <param name="start">Where the band starts, as a fraction of the glyphs (0 = their top).</param>
        /// <param name="end">Where the band ends, as a fraction of the glyphs (1 = their bottom).</param>
        /// <param name="edgeSoftness">How soft the edges of the band are, as a fraction of its length (0 = hard edges).</param>
        public static GradientOverlay Band(Color color, float startStrength, float endStrength, float start, float end, float edgeSoftness = 0f)
        {
            var soft = Math.Max(0.002f, Math.Min(0.49f, edgeSoftness));
            return new GradientOverlay(Color.Transparent, color * startStrength, color * endStrength, Color.Transparent)
                .SetPositions(0f, soft, 1f - soft, 1f)
                .SetRange(start, end);
        }
    }
}
