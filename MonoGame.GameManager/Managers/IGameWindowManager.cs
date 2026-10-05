using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Managers
{
    /// <summary>
    /// Fits the virtual screen (the resolution the game is designed for) into the window, keeping the aspect ratio
    /// (letterboxing), and converts positions between the window and the virtual screen.
    /// </summary>
    public interface IGameWindowManager
    {
        /// <summary>The virtual resolution of the game.</summary>
        Point ScreenSize { get; set; }

        /// <summary>Extra scale applied to the virtual screen inside the window.</summary>
        Vector2 ScreenScale { get; set; }

        /// <summary>Margins around the virtual screen (X = left, Y = top, Z = right, W = bottom), in virtual units.</summary>
        Vector4 ScreenMargin { get; set; }

        /// <summary>Where the virtual screen is drawn in the window.</summary>
        Point DrawScreenPosition { get; }

        /// <summary>The scale used to fit the virtual screen in the window.</summary>
        Vector2 DrawScreenScale { get; }

        /// <summary>Raised when the position or the scale of the virtual screen in the window changes.</summary>
        event Action LayoutChanged;

        /// <summary>Raised when <see cref="ScreenSize"/> changes.</summary>
        event Action ScreenSizeChanged;

        Rectangle GetScreenRectangle();

        /// <summary>The area of the window where the virtual screen is drawn.</summary>
        Rectangle GetScreenDrawRectangle();

        Vector2 GetScreenDrawSize();

        /// <summary>Converts a window position (eg: from the mouse) to the virtual screen.</summary>
        Vector2 GetPositionOnScreen(Vector2 positionOnWindow);

        /// <summary>Converts a virtual screen position to the window.</summary>
        Vector2 GetPositionOnWindow(Vector2 positionOnScreen);

        void SetMarginLeft(float marginLeft);
        void SetMarginTop(float marginTop);
        void SetMarginRight(float marginRight);
        void SetMarginBottom(float marginBottom);

        /// <summary>Changes the size of the window (ignored on platforms with a fixed window).</summary>
        void SetWindowSize(Point size);

        void SetFullScreen(bool isFullScreen);

        void ToggleFullScreen();

        /// <summary>Recalculates where the virtual screen is drawn in the window.</summary>
        void UpdateLayout();
    }
}
