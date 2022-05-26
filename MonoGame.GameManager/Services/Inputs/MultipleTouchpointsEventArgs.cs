using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Services.Inputs
{
    public class MultipleTouchpointsEventArgs
    {
        public readonly TimeSpan Time;
        public readonly List<TouchLocation> Touchpoints;

        public MultipleTouchpointsEventArgs(TimeSpan time, List<TouchLocation> touchpoints)
        {
            Time = time;
            Touchpoints = touchpoints;
        }
    }
}
