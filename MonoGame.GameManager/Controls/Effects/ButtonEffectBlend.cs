namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>How a <see cref="ButtonEffect"/> is mixed with what is below it.</summary>
    public enum ButtonEffectBlend
    {
        /// <summary>The effect is a light: its color is added to what is below (it can only brighten).</summary>
        Light,

        /// <summary>The effect is painted over what is below (it can darken: shades, outlines, shadows, lips).</summary>
        Normal
    }
}
