namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>Which edges an inner <see cref="GlowEffect"/> lights.</summary>
    public enum GlowEdges
    {
        /// <summary>Every edge, following the corners.</summary>
        All,

        /// <summary>The top edge only: light falling from the top.</summary>
        Top,

        /// <summary>The bottom edge only: light coming from the bottom (a warm, lit-from-below look).</summary>
        Bottom
    }
}
