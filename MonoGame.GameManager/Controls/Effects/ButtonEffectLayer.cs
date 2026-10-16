namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>Where a <see cref="ButtonEffect"/> is drawn, relative to the button.</summary>
    public enum ButtonEffectLayer
    {
        /// <summary>Before the background: lights around the button (outer glow, rays).</summary>
        Behind,

        /// <summary>
        /// Over the background and under the children, clipped to the shape of the button (see
        /// <see cref="ButtonEffectMask"/>): gloss, shine sweeps, inner glow, fills, ripples.
        /// </summary>
        Inside,

        /// <summary>After the children (the text and icons): rims, gems, running lights, sparkles.</summary>
        Front
    }
}
