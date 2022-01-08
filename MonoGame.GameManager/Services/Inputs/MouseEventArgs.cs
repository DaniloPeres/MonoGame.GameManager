using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Services.Inputs
{
    public class MouseEventArgs
    {
        public readonly TimeSpan Time;
        public readonly MouseState CurrentState;
        public readonly bool IsTouchInput;

        /// <summary>
        /// Inform the mouse position when this event has been called.
        /// </summary>
        public Point Position => CurrentState.Position;

        public MouseEventArgs(TimeSpan time, MouseState currentState, bool isTouchInput = false)
        {
            Time = time;
            CurrentState = currentState;
            IsTouchInput = isTouchInput;
        }
    }
}
