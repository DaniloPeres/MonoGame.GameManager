using Microsoft.Xna.Framework.Input;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Services.Inputs;
using System;

namespace MonoGame.GameManager.Controls.MouseEvent
{
    public class ControlEventArgs : MouseEventArgs
    {
        public readonly IControl Control;
        public bool ShouldStopPropagation { get; set; } = true;
        
        public ControlEventArgs(IControl control, TimeSpan time, MouseState currentState)
            : base(time, currentState)
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
