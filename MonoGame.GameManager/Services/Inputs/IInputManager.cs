using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;

namespace MonoGame.GameManager.Services.Inputs
{
    /// <summary>
    /// Single access point to every input device (Facade) and to the named actions of an <see cref="InputMap"/>.
    /// </summary>
    public interface IInputManager
    {
        KeyboardInputListener Keyboard { get; }

        MouseInputListener Mouse { get; }

        TouchInputListener Touch { get; }

        GamePadInputListener GamePads { get; }

        /// <summary>The bindings of the named actions and axes.</summary>
        InputMap Map { get; }

        /// <summary>When false, the devices are not polled.</summary>
        bool IsEnabled { get; set; }

        /// <summary>The control that receives the keyboard (eg: a text box), or null.</summary>
        IControl FocusedControl { get; set; }

        /// <summary>True only in the frame the action started.</summary>
        bool IsActionPressed(string action);

        /// <summary>True while any binding of the action is held.</summary>
        bool IsActionDown(string action);

        /// <summary>True only in the frame the action ended.</summary>
        bool IsActionReleased(string action);

        /// <summary>The value of an axis from -1 to 1 (the binding with the largest magnitude wins).</summary>
        float GetAxis(string axis);

        /// <summary>A vector made of two axes, with a length of at most 1.</summary>
        Vector2 GetVector(string horizontalAxis, string verticalAxis);

        /// <summary>Polls every device. Called once per frame by the screen manager.</summary>
        void Update(GameTime gameTime);
    }
}
