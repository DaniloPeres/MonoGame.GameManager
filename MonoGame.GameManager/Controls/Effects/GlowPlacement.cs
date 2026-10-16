namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>Where a <see cref="GlowEffect"/> shines, relative to the edge of the button.</summary>
    public enum GlowPlacement
    {
        /// <summary>A halo around the button, drawn behind it.</summary>
        Outer,

        /// <summary>Light along the inside of the edge, fading towards the center.</summary>
        Inner,

        /// <summary>A line of light following the edge (a lit border), drawn in front of the button.</summary>
        Rim
    }
}
