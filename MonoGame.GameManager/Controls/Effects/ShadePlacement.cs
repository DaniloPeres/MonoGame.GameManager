namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>Where an <see cref="InnerShadeEffect"/> darkens the button.</summary>
    public enum ShadePlacement
    {
        /// <summary>The bottom of the button, fading upwards (volume, a rounded body).</summary>
        Bottom,

        /// <summary>The top of the button, fading downwards (a pressed or hollow look).</summary>
        Top,

        /// <summary>Along the inside of the edge, fading towards the center (a vignette, an inner shadow).</summary>
        Edges,

        /// <summary>The whole button (a darker tint, eg: on hover or when pressed).</summary>
        Full
    }
}
