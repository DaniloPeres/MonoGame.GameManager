using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Services.Inputs;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls.InputEvent
{
    public class ControlMultipleTouchpointsEventArgs : MultipleTouchpointsEventArgs
    {
        public readonly IControl Control;
        public bool ShouldStopPropagation { get; set; } = true;

        public ControlMultipleTouchpointsEventArgs(IControl control, TimeSpan time, List<TouchLocation> touchpoints)
            : base(time, touchpoints)
        {
            Control = control;
        }

        /// <summary>
        /// Inform to the caller to continue processing the next event.
        /// It is used when you would like to invoke the same event in some another control.
        /// Eg: Event OnMouseMove callback a button in the panel and also callback the panel under this button.
        /// </summary>
        public void ContinuePropagation()
        {
            ShouldStopPropagation = false;
        }
    }
}
