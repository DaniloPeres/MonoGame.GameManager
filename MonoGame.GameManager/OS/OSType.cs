using System;

namespace MonoGame.GameManager.OS
{
    [Flags]
    public enum OSType
    {
        WINDOWS_UAP = 1,
        ANDROID = 2,
        IOS = 4,
        WINDOWS_DESKTOP = 8
    }
}
