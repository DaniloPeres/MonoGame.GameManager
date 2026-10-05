using System;

namespace MonoGame.GameManager.Services.Inputs
{
    /// <summary>
    /// Mouse buttons. Touch input is reported as <see cref="Left"/>.
    /// </summary>
    [Flags]
    public enum MouseButtons
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 4,
        XButton1 = 8,
        XButton2 = 16,
        All = Left | Right | Middle | XButton1 | XButton2
    }
}
