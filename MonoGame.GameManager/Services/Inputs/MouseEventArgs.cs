using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoGame.GameManager.Services.Inputs
{
    /// <summary>
    /// Data of a mouse (or touch) event. Positions are in virtual screen coordinates.
    /// </summary>
    public class MouseEventArgs
    {
        /// <summary>The total game time when the event happened.</summary>
        public readonly TimeSpan Time;

        /// <summary>The mouse state, with the position converted to virtual screen coordinates.</summary>
        public readonly MouseState CurrentState;

        /// <summary>True when the event comes from a touch screen.</summary>
        public readonly bool IsTouchInput;

        /// <summary>The button that was pressed or released (None for move and wheel events).</summary>
        public readonly MouseButtons Button;

        /// <summary>The change of the scroll wheel value for wheel events (positive = up).</summary>
        public readonly int ScrollWheelDelta;

        public MouseEventArgs(TimeSpan time, MouseState currentState, bool isTouchInput = false)
            : this(time, currentState, isTouchInput, MouseButtons.Left, 0) { }

        public MouseEventArgs(TimeSpan time, MouseState currentState, bool isTouchInput, MouseButtons button, int scrollWheelDelta = 0)
            : this(time, currentState, isTouchInput, button, scrollWheelDelta, currentState.Position) { }

        protected MouseEventArgs(TimeSpan time, MouseState currentState, bool isTouchInput, MouseButtons button, int scrollWheelDelta, Point position)
        {
            Time = time;
            CurrentState = currentState;
            IsTouchInput = isTouchInput;
            Button = button;
            ScrollWheelDelta = scrollWheelDelta;
            Position = position;
        }

        /// <summary>
        /// The pointer position. For control events it is in the coordinate space of the control's parent, which is
        /// the virtual screen space unless the control is inside a <see cref="Controls.CameraPanel"/>.
        /// </summary>
        public Point Position { get; }

        /// <summary>The pointer position in virtual screen coordinates.</summary>
        public Point ScreenPosition => CurrentState.Position;
    }
}
