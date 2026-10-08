namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>Where a <see cref="ShadingEffect"/> is drawn, relative to its control.</summary>
    public enum ShadingLayer
    {
        /// <summary>Under the control (shadows, outlines and halos).</summary>
        Behind,
        /// <summary>Over the control (a bloom of light over the text or the image, inner shadows and bevels).</summary>
        Front,
        /// <summary>
        /// Instead of the control: the control itself is not drawn, its silhouette is painted by the effect (the
        /// default layer of <see cref="GradientFill"/>). Drawn after <see cref="Behind"/> and before <see cref="Front"/>.
        /// </summary>
        Content
    }
}
