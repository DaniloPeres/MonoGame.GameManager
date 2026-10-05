using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Services.Inputs;
using System;

namespace MonoGame.GameManager.Controls.InputEvent
{
    /// <summary>
    /// Data of a pointer event received by a control.
    /// </summary>
    public class ControlMouseEventArgs : MouseEventArgs
    {
        /// <summary>The control that receives the event.</summary>
        public readonly IControl Control;

        /// <summary>
        /// When true (default) the event is not delivered to the controls below. Call
        /// <see cref="ContinuePropagation"/> to let them receive it too.
        /// </summary>
        public bool ShouldStopPropagation { get; set; } = true;

        public ControlMouseEventArgs(IControl control, TimeSpan time, MouseState currentState)
            : this(control, time, currentState, false, MouseButtons.Left, currentState.Position) { }

        public ControlMouseEventArgs(IControl control, TimeSpan time, MouseState currentState, bool isTouchInput, MouseButtons button, Point position, int scrollWheelDelta = 0)
            : base(time, currentState, isTouchInput, button, scrollWheelDelta, position)
        {
            Control = control;
        }

        /// <summary>
        /// Lets the controls below receive the same event.
        /// Eg: a button inside a panel receives OnMouseMoved and the panel receives it as well.
        /// </summary>
        public void ContinuePropagation()
        {
            ShouldStopPropagation = false;
        }
    }
}
