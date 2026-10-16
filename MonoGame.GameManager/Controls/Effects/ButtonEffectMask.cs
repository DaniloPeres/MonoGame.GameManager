namespace MonoGame.GameManager.Controls.Effects
{
    /// <summary>The shape that clips the <see cref="ButtonEffectLayer.Inside"/> effects of a button.</summary>
    public enum ButtonEffectMask
    {
        /// <summary>
        /// <see cref="Texture"/> when the button is drawn with a texture, <see cref="RoundedRectangle"/> when it has a
        /// corner radius, otherwise <see cref="Rectangle"/>.
        /// </summary>
        Auto,

        /// <summary>The rectangle of the button (the cheapest: a scissor rectangle when the button is not rotated).</summary>
        Rectangle,

        /// <summary>The rectangle of the button with its corner radius.</summary>
        RoundedRectangle,

        /// <summary>The transparency of the texture of the current state (for buttons with transparent corners or shapes).</summary>
        Texture
    }
}
