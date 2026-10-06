namespace MonoGame.GameManager.Controls
{
    /// <summary>The visual state of a <see cref="Button"/>.</summary>
    public enum ButtonVisualState
    {
        Normal,
        /// <summary>The pointer is over the button.</summary>
        Hover,
        /// <summary>The button is pressed and the pointer is over it.</summary>
        Pressed,
        /// <summary><see cref="Interfaces.IInputTarget.IsEnabled"/> is false.</summary>
        Disabled
    }
}
