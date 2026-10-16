namespace MonoGame.GameManager.Controls.Shading
{
    /// <summary>How a <see cref="ShadingEffect"/> is mixed with what is below it.</summary>
    public enum ShadingBlend
    {
        /// <summary>Painted over what is below (shadows and outlines): it can darken.</summary>
        Normal,

        /// <summary>Added to what is below (glows): it can only brighten, and shines best on dark backgrounds.</summary>
        Light
    }
}
