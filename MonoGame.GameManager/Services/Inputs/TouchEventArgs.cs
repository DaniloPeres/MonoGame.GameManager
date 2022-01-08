using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Input.Touch;

namespace MonoGame.GameManager.Services.Inputs
{
    public class TouchEventArgs
    {
        public readonly TimeSpan Time;
        public readonly TouchLocation TouchLocation;
        /// <summary>
        /// Inform the mouse position when this event has been called.
        /// </summary>
        public Point Position => TouchLocation.Position.ToPoint();

        public TouchEventArgs(TimeSpan time, TouchLocation touchLocation)
        {
            Time = time;
            TouchLocation = touchLocation;
        }
    }
}
