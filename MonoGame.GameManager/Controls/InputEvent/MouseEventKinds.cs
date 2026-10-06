using System;

namespace MonoGame.GameManager.Controls.InputEvent
{
    /// <summary>
    /// The kinds of pointer events a control can handle.
    /// </summary>
    [Flags]
    public enum MouseEventKinds
    {
        None = 0,
        Enter = 1,
        Leave = 2,
        Pressed = 4,
        Moved = 8,
        Released = 16,
        Click = 32,
        Wheel = 64,
        MultipleTouchpoints = 128,
        All = Enter | Leave | Pressed | Moved | Released | Click | Wheel | MultipleTouchpoints
    }
}
