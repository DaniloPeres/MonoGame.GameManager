namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>Where a <see cref="ParticleFieldEffect"/> scatters its particles.</summary>
    public enum ParticleFieldArea
    {
        /// <summary>Anywhere inside the button (clipped to its shape).</summary>
        Inside,

        /// <summary>The upper half inside the button (clipped to its shape).</summary>
        Top,

        /// <summary>The lower half inside the button (clipped to its shape), like light gathering at the bottom.</summary>
        Bottom,

        /// <summary>Above the top edge, outside the button.</summary>
        Above,

        /// <summary>Under the bottom edge, outside the button.</summary>
        Below,

        /// <summary>All around the button, just outside its edge.</summary>
        Around,

        /// <summary>On the edge of the button, half inside and half outside.</summary>
        Edge
    }
}
