namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// How a container clips its children when <see cref="Interfaces.IContainer.HideOverflow"/> is enabled.
    /// </summary>
    public enum ContainerClipMode
    {
        /// <summary>
        /// Clips with the scissor rectangle of the graphics device. Fast and without extra memory, but limited to
        /// axis-aligned areas (a rotated container uses <see cref="RenderTarget"/> automatically).
        /// </summary>
        Scissor,

        /// <summary>
        /// Draws the children into a render target the size of the container and draws it as a texture, so the
        /// children follow the rotation of the container.
        /// </summary>
        RenderTarget
    }
}
