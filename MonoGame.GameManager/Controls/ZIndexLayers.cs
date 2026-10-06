namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// Well-known <see cref="Interfaces.IRenderable.ZIndex"/> values.
    /// </summary>
    public static class ZIndexLayers
    {
        /// <summary>The default layer of every control.</summary>
        public const float Default = 0f;

        /// <summary>Layer for overlays such as tooltips and popups.</summary>
        public const float Overlay = 1_000_000f;

        /// <summary>Layer of the screen transitions, above everything else.</summary>
        public const float Transition = float.MaxValue;
    }
}
