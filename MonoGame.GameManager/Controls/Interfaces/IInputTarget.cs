using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Services.Inputs;

namespace MonoGame.GameManager.Controls.Interfaces
{
    /// <summary>
    /// Members used by the input system to deliver pointer events to a control.
    /// </summary>
    public interface IInputTarget
    {
        bool IsMouseHover { get; }

        bool IsMousePressed { get; }

        /// <summary>When false, the control and its children do not receive input.</summary>
        bool IsEnabled { get; set; }

        /// <summary>When true, the controls below never receive pointer events that hit this control.</summary>
        bool BlocksMouseEvents { get; set; }

        /// <summary>The mouse buttons this control reacts to (default: <see cref="MouseButtons.Left"/>).</summary>
        MouseButtons AcceptedMouseButtons { get; set; }

        /// <summary>Returns true if the control has handlers for at least one of the given kinds of event.</summary>
        bool HasMouseHandlers(MouseEventKinds kinds);

        /// <summary>Returns true if the point (in the coordinate space of the parent) hits the control.</summary>
        bool Intersects(Point pointToCompare);

        void SetMouseHover(bool isMouseHover);

        void SetMousePressed(bool isMousePressed);

        void FireOnMouseEnter(ControlMouseEventArgs args);

        void FireOnMouseLeave(ControlMouseEventArgs args);

        void FireOnPressed(ControlMouseEventArgs args);

        void FireOnMoved(ControlMouseEventArgs args);

        void FireOnReleased(ControlMouseEventArgs args);

        void FireOnClick(ControlMouseEventArgs args);

        void FireOnMouseWheel(ControlMouseEventArgs args);

        void FireOnMultipleTouchpoints(ControlMultipleTouchpointsEventArgs args);
    }
}
