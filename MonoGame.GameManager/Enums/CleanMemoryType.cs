using System;

namespace MonoGame.GameManager.Enums
{
    [Obsolete("Assets are released with the ContentManager that loaded them (see Screen.Content).")]
    public enum CleanMemoryType
    {
        OnChangeScreen,
        Manually
    }
}
