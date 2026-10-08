using Microsoft.Xna.Framework;

namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>
    /// Paints the silhouette of the control (the glyphs of a text, the opaque pixels of an image) with a gradient of two
    /// or more colors, instead of its own colors: the cream to gold letters of casual game titles, chrome, fire, candy.
    /// It is drawn in the <see cref="ShadingLayer.Content"/> layer, so the control itself is not drawn; the shadows,
    /// glows and outlines behind it and the bevels and shines in front of it still are.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The fill is rendered into a render target once, with the other shading shapes, and kept until the text, the
    /// size, the scale or the options of the fill change: changing its colors renders it again, but its
    /// <see cref="ShadingEffect.Intensity"/>, pulse and flicker are free (they fade it). A scrolling fill
    /// (<see cref="GradientEffect{TEffect}.ScrollSpeed"/>) is rendered again in small steps while it moves.
    /// </para>
    /// <para>
    /// The angle is in the local space of the control: the gradient turns with a rotated control. The alpha of the
    /// colors is ignored (fade the fill with its intensity or with <see cref="ShadingEffect.Color"/>, a tint that
    /// multiplies the gradient). The built-in outline of a <see cref="Label"/> is part of its silhouette and would be
    /// filled too: use an <see cref="Outline"/> effect with a fill. Paint details over the fill with
    /// <see cref="GradientOverlay"/>, <see cref="PatternOverlay"/> and <see cref="Sparkles"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// title.AddShadings(
    ///     new Shadow(new Vector2(0, 6), 6, Color.Black * 0.5f),
    ///     new Outline(new Color(80, 35, 5), 6),
    ///     new GradientFill(new Color(255, 250, 215), new Color(255, 210, 70), new Color(240, 140, 20)));
    /// </code>
    /// </example>
    public class GradientFill : GradientEffect<GradientFill>
    {
        public GradientFill()
        {
            Layer = ShadingLayer.Content;
            Blend = ShadingBlend.Normal;
            Color = Color.White;
        }

        /// <param name="colors">The colors of the gradient, from the start (the top, by default) to the end; at least two.</param>
        public GradientFill(params Color[] colors) : this()
        {
            SetColors(colors);
        }

        internal override bool KeepsAlpha => false;
    }
}
