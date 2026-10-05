using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Services.Inputs;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.InputEvent
{
    /// <summary>
    /// Data of a multi-touch event (two or more fingers) received by a control.
    /// </summary>
    public class ControlMultipleTouchpointsEventArgs : MultipleTouchpointsEventArgs
    {
        /// <summary>The control that receives the event.</summary>
        public readonly IControl Control;

        /// <summary>
        /// When true (default) the event is not delivered to the controls below. Call
        /// <see cref="ContinuePropagation"/> to let them receive it too.
        /// </summary>
        public bool ShouldStopPropagation { get; set; } = true;

        public ControlMultipleTouchpointsEventArgs(IControl control, TimeSpan time, List<TouchLocation> touchpoints)
            : base(time, touchpoints)
        {
            Control = control;
        }

        /// <summary>
        /// Lets the controls below receive the same event.
        /// </summary>
        public void ContinuePropagation()
        {
            ShouldStopPropagation = false;
        }
    }
}
