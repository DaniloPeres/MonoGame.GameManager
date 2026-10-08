namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>The area a <see cref="GradientFill"/> is stretched over.</summary>
    public enum GradientBounds
    {
        /// <summary>
        /// The box of what the control draws: the glyphs of a text (without the space above and under them), the whole
        /// control for the other controls. The first color starts at the top of the letters.
        /// </summary>
        Content,

        /// <summary>The whole area of the control (for a label: the line height, including the space above and under the glyphs).</summary>
        Control
    }
}
