using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace MonoGame.GameManager.Services.Inputs
{
    /// <summary>
    /// Data of a single touch event. Positions are in virtual screen coordinates.
    /// </summary>
    public class TouchEventArgs
    {
        /// <summary>The total game time when the event happened.</summary>
        public readonly TimeSpan Time;

        /// <summary>The touch location, with the position converted to virtual screen coordinates.</summary>
        public readonly TouchLocation TouchLocation;

        /// <summary>The touch position.</summary>
        public Point Position => TouchLocation.Position.ToPoint();

        public TouchEventArgs(TimeSpan time, TouchLocation touchLocation)
        {
            Time = time;
            TouchLocation = touchLocation;
        }
    }
}
