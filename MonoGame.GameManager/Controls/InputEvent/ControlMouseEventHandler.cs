using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Services.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Controls.InputEvent
{
    /// <summary>
    /// Delivers pointer events (mouse and touch) to the controls of a control tree.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tree is visited from the top-most control to the bottom-most one (children before their container,
    /// higher <see cref="IRenderable.ZIndex"/> first). Each control under the pointer receives the event until a
    /// control stops the propagation. A control stops it when it handles the event without calling
    /// <see cref="ControlMouseEventArgs.ContinuePropagation"/>, or when <see cref="IInputTarget.BlocksMouseEvents"/>
    /// is true. A control that handles clicks or releases also owns the press, so the controls below are not
    /// clicked at the same time.
    /// </para>
    /// <para>
    /// Hidden, disposed and disabled controls do not receive events, and the children of a container whose
    /// <see cref="IContainer.ClipsInput"/> is true only receive events inside the container.
    /// </para>
    /// </remarks>
    public class ControlMouseEventHandler
    {
        private const MouseEventKinds PointerReactionKinds = MouseEventKinds.Pressed | MouseEventKinds.Released | MouseEventKinds.Click | MouseEventKinds.Wheel;

        private readonly IControl root;
        private readonly List<IControl> hoveredControls = new List<IControl>();
        private readonly Dictionary<MouseButtons, List<IControl>> pressedControls = new Dictionary<MouseButtons, List<IControl>>();
        private MouseInputListener attachedMouse;
        private TouchInputListener attachedTouch;
        private IControl capturedControl;

        /// <param name="root">The root of the control tree that receives the events.</param>
        public ControlMouseEventHandler(IControl root)
        {
            this.root = root ?? throw new ArgumentNullException(nameof(root));
        }

        /// <summary>The control that currently captures the pointer, or null.</summary>
        public IControl CapturedControl => capturedControl;

        /// <summary>The controls currently under the pointer.</summary>
        public IReadOnlyList<IControl> HoveredControls => hoveredControls;

        /// <summary>
        /// Subscribes to the events of the input listeners.
        /// </summary>
        public void Attach(MouseInputListener mouse, TouchInputListener touch)
        {
            Detach();

            attachedMouse = mouse;
            if (mouse != null)
            {
                mouse.OnMouseDown += HandleMouseDown;
                mouse.OnMouseMove += HandleMouseMove;
                mouse.OnMouseUp += HandleMouseUp;
                mouse.OnMouseWheelMoved += HandleMouseWheel;
            }

            attachedTouch = touch;
            if (touch != null)
            {
                touch.OnTouchStarted += OnTouchStarted;
                touch.OnTouchMoved += OnTouchMoved;
                touch.OnTouchReleased += OnTouchReleased;
                touch.OnTouchCancelled += OnTouchCancelled;
                touch.OnMultipleTouch += HandleMultipleTouchpoints;
            }
        }

        /// <summary>
        /// Unsubscribes from the input listeners.
        /// </summary>
        public void Detach()
        {
            if (attachedMouse != null)
            {
                attachedMouse.OnMouseDown -= HandleMouseDown;
                attachedMouse.OnMouseMove -= HandleMouseMove;
                attachedMouse.OnMouseUp -= HandleMouseUp;
                attachedMouse.OnMouseWheelMoved -= HandleMouseWheel;
                attachedMouse = null;
            }

            if (attachedTouch != null)
            {
                attachedTouch.OnTouchStarted -= OnTouchStarted;
                attachedTouch.OnTouchMoved -= OnTouchMoved;
                attachedTouch.OnTouchReleased -= OnTouchReleased;
                attachedTouch.OnTouchCancelled -= OnTouchCancelled;
                attachedTouch.OnMultipleTouch -= HandleMultipleTouchpoints;
                attachedTouch = null;
            }
        }

        /// <summary>
        /// Makes <paramref name="control"/> receive the move and release events first, even when the pointer is
        /// outside of it, until the button is released or <see cref="ReleasePointer"/> is called. It is used to
        /// implement dragging (scroll viewers, sliders...).
        /// </summary>
        public void CapturePointer(IControl control) => capturedControl = control;

        /// <summary>
        /// Releases the pointer capture (only if <paramref name="control"/> holds it, when provided).
        /// </summary>
        public void ReleasePointer(IControl control = null)
        {
            if (control == null || ReferenceEquals(capturedControl, control))
                capturedControl = null;
        }

        /// <summary>
        /// Forgets the hovered and pressed controls and the pointer capture, firing the leave events.
        /// </summary>
        public void Reset()
        {
            capturedControl = null;
            foreach (var list in pressedControls.Values)
            {
                foreach (var control in list)
                    control.SetMousePressed(false);
                list.Clear();
            }
            ClearHover(null);
        }

        public void HandleMouseDown(MouseEventArgs args)
        {
            if (args.IsTouchInput)
                HandleMouseMove(args); // touch has no hover: the finger arrives where it presses

            var button = args.Button == MouseButtons.None ? MouseButtons.Left : args.Button;
            var pressed = GetPressedControls(button);
            pressed.Clear();

            Traverse(root, args.Position, (control, position) =>
            {
                if ((control.AcceptedMouseButtons & button) == 0)
                    return control.BlocksMouseEvents;

                var controlArgs = CreateArgs(control, args, position, button);
                control.SetMousePressed(true);
                pressed.Add(control);
                control.FireOnPressed(controlArgs);

                if (control.BlocksMouseEvents || controlArgs.ShouldStopPropagation)
                    return true;

                // A control that reacts to clicks or releases owns the press: the controls below must not be clicked too.
                return !control.HasMouseHandlers(MouseEventKinds.Pressed)
                    && control.HasMouseHandlers(MouseEventKinds.Click | MouseEventKinds.Released);
            });
        }

        public void HandleMouseMove(MouseEventArgs args)
        {
            var newHoveredControls = new List<IControl>();
            var captured = GetValidCapturedControl();
            var stop = false;

            if (captured != null)
            {
                var controlArgs = CreateArgs(captured, args, ToLocal(captured, args.Position), MouseButtons.None);
                captured.FireOnMoved(controlArgs);
                if (captured.BlocksMouseEvents || controlArgs.ShouldStopPropagation)
                {
                    stop = true;
                    captured.SetMouseHover(true);
                    newHoveredControls.Add(captured);
                }
            }

            if (!stop)
            {
                Traverse(root, args.Position, (control, position) =>
                {
                    control.SetMouseHover(true);
                    newHoveredControls.Add(control);
                    var controlArgs = CreateArgs(control, args, position, MouseButtons.None);
                    control.FireOnMoved(controlArgs);
                    return control.BlocksMouseEvents || controlArgs.ShouldStopPropagation;
                }, captured);
            }

            UpdateHover(newHoveredControls, args);
        }

        public void HandleMouseUp(MouseEventArgs args)
        {
            var button = args.Button == MouseButtons.None ? MouseButtons.Left : args.Button;
            var pressedList = GetPressedControls(button);
            var pressed = pressedList.ToArray();
            pressedList.Clear();
            foreach (var control in pressed)
                control.SetMousePressed(false);

            var captured = GetValidCapturedControl();
            var stop = false;
            if (captured != null)
                stop = DispatchRelease(captured, args, ToLocal(captured, args.Position), pressed, button, true);

            if (!stop)
                Traverse(root, args.Position, (control, position) => DispatchRelease(control, args, position, pressed, button, false), captured);

            if (button == MouseButtons.Left || args.IsTouchInput)
                capturedControl = null; // the capture ends with the release of the pointer

            if (args.IsTouchInput)
                ClearHover(args); // the finger left the screen
        }

        public void HandleMouseWheel(MouseEventArgs args)
        {
            Traverse(root, args.Position, (control, position) =>
            {
                if (!control.HasMouseHandlers(MouseEventKinds.Wheel))
                    return control.BlocksMouseEvents;

                var controlArgs = CreateArgs(control, args, position, MouseButtons.None);
                control.FireOnMouseWheel(controlArgs);
                return control.BlocksMouseEvents || controlArgs.ShouldStopPropagation;
            });
        }

        public void HandleMultipleTouchpoints(MultipleTouchpointsEventArgs args)
        {
            if (args.Touchpoints == null || args.Touchpoints.Count == 0)
                return;
            TraverseMultipleTouchpoints(root, args.Touchpoints, args);
        }

        /// <summary>
        /// Cancels the current touch: the pressed controls are released without a click and the hover is cleared.
        /// </summary>
        public void HandleTouchCancelled(MouseEventArgs args)
        {
            var pressedList = GetPressedControls(MouseButtons.Left);
            foreach (var control in pressedList)
                control.SetMousePressed(false);
            pressedList.Clear();
            capturedControl = null;
            ClearHover(args);
        }

        private bool DispatchRelease(IControl control, MouseEventArgs args, Point position, IControl[] pressed, MouseButtons button, bool isCaptured)
        {
            if ((control.AcceptedMouseButtons & button) == 0)
                return control.BlocksMouseEvents;

            var stop = control.BlocksMouseEvents;

            // A click is a press and a release on the same control; a captured control is clicked only when the
            // pointer is released over it.
            var isClick = Array.IndexOf(pressed, control) >= 0 && (!isCaptured || control.Intersects(position));
            if (isClick && control.HasMouseHandlers(MouseEventKinds.Click))
            {
                var clickArgs = CreateArgs(control, args, position, button);
                control.FireOnClick(clickArgs);
                stop |= clickArgs.ShouldStopPropagation;
            }

            if (control.HasMouseHandlers(MouseEventKinds.Released))
            {
                var releasedArgs = CreateArgs(control, args, position, button);
                control.FireOnReleased(releasedArgs);
                stop |= releasedArgs.ShouldStopPropagation;
            }

            return stop;
        }

        private void UpdateHover(List<IControl> newHoveredControls, MouseEventArgs args)
        {
            var previous = hoveredControls.ToArray();
            hoveredControls.Clear();
            foreach (var control in newHoveredControls)
            {
                if (!hoveredControls.Contains(control))
                    hoveredControls.Add(control);
            }

            foreach (var control in previous)
            {
                if (hoveredControls.Contains(control))
                    continue;

                control.SetMouseHover(false);
                if (!control.IsDisposed)
                    control.FireOnMouseLeave(CreateArgs(control, args, ToLocal(control, args.Position), MouseButtons.None));
            }

            foreach (var control in hoveredControls)
            {
                if (Array.IndexOf(previous, control) >= 0)
                    continue;

                control.SetMouseHover(true);
                control.FireOnMouseEnter(CreateArgs(control, args, ToLocal(control, args.Position), MouseButtons.None));
            }
        }

        private void ClearHover(MouseEventArgs args)
        {
            var previous = hoveredControls.ToArray();
            hoveredControls.Clear();
            foreach (var control in previous)
            {
                control.SetMouseHover(false);
                if (args != null && !control.IsDisposed)
                    control.FireOnMouseLeave(CreateArgs(control, args, ToLocal(control, args.Position), MouseButtons.None));
            }
        }

        private List<IControl> GetPressedControls(MouseButtons button)
        {
            if (!pressedControls.TryGetValue(button, out var list))
            {
                list = new List<IControl>();
                pressedControls[button] = list;
            }
            return list;
        }

        private IControl GetValidCapturedControl()
        {
            if (capturedControl != null && (capturedControl.IsDisposed || !IsInTree(capturedControl)))
                capturedControl = null;
            return capturedControl;
        }

        private bool IsInTree(IControl control)
        {
            for (IControl current = control; current != null; current = current.Parent)
            {
                if (ReferenceEquals(current, root))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Visits the controls under <paramref name="point"/>, top-most first. Returns true when a visitor stopped
        /// the propagation.
        /// </summary>
        private bool Traverse(IControl control, Point point, Func<IControl, Point, bool> visit, IControl skip = null)
        {
            if (!control.IsVisible || control.IsDisposed)
                return false;

            if (!control.IsEnabled)
            {
                // A disabled control absorbs the input it would react to, without receiving it.
                return control.Intersects(point)
                    && (control.BlocksMouseEvents || control.HasMouseHandlers(PointerReactionKinds));
            }

            if (control is IContainer container)
            {
                if (container.ClipsInput && !control.Intersects(point))
                    return false;

                var localPoint = container.TransformPointToLocal(point);
                var children = container.Children as IList<IControl> ?? container.Children.ToList();
                for (var i = children.Count - 1; i >= 0; i--)
                {
                    var child = children[i];
                    if (!ReferenceEquals(child.Parent, container))
                        continue;
                    if (Traverse(child, localPoint, visit, skip))
                        return true;
                }
            }

            if (ReferenceEquals(control, skip) || !control.Intersects(point))
                return false;

            return visit(control, point);
        }

        private bool TraverseMultipleTouchpoints(IControl control, List<TouchLocation> touchpoints, MultipleTouchpointsEventArgs args)
        {
            if (!control.IsVisible || control.IsDisposed || !control.IsEnabled)
                return false;

            var allPointsIntersect = touchpoints.All(touchpoint => control.Intersects(touchpoint.Position.ToPoint()));

            if (control is IContainer container)
            {
                if (container.ClipsInput && !allPointsIntersect)
                    return false;

                var localTouchpoints = touchpoints
                    .Select(touchpoint => new TouchLocation(touchpoint.Id, touchpoint.State, container.TransformPointToLocal(touchpoint.Position.ToPoint()).ToVector2()))
                    .ToList();

                var children = container.Children as IList<IControl> ?? container.Children.ToList();
                for (var i = children.Count - 1; i >= 0; i--)
                {
                    var child = children[i];
                    if (!ReferenceEquals(child.Parent, container))
                        continue;
                    if (TraverseMultipleTouchpoints(child, localTouchpoints, args))
                        return true;
                }
            }

            if (!allPointsIntersect)
                return false;

            var controlArgs = new ControlMultipleTouchpointsEventArgs(control, args.Time, touchpoints);
            control.FireOnMultipleTouchpoints(controlArgs);
            return control.BlocksMouseEvents || controlArgs.ShouldStopPropagation;
        }

        /// <summary>Converts a point from the space of the root to the space of the control's parent.</summary>
        private static Point ToLocal(IControl control, Point point)
        {
            var ancestors = new Stack<IContainer>();
            for (var parent = control.Parent; parent != null; parent = parent.Parent)
                ancestors.Push(parent);

            while (ancestors.Count > 0)
                point = ancestors.Pop().TransformPointToLocal(point);

            return point;
        }

        private ControlMouseEventArgs CreateArgs(IControl control, MouseEventArgs args, Point position, MouseButtons button)
            => new ControlMouseEventArgs(control, args.Time, args.CurrentState, args.IsTouchInput, button, position, args.ScrollWheelDelta) { Handler = this };

        private void OnTouchStarted(TouchEventArgs args) => HandleMouseDown(ToMouseEventArgs(args, ButtonState.Pressed));

        private void OnTouchMoved(TouchEventArgs args) => HandleMouseMove(ToMouseEventArgs(args, ButtonState.Pressed));

        private void OnTouchReleased(TouchEventArgs args) => HandleMouseUp(ToMouseEventArgs(args, ButtonState.Released));

        private void OnTouchCancelled(TouchEventArgs args) => HandleTouchCancelled(ToMouseEventArgs(args, ButtonState.Released));

        private static MouseEventArgs ToMouseEventArgs(TouchEventArgs touchEventArgs, ButtonState leftButton)
        {
            var position = touchEventArgs.TouchLocation.Position;
            var mouseState = new MouseState((int)position.X, (int)position.Y, 0, leftButton, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
            return new MouseEventArgs(touchEventArgs.Time, mouseState, true, MouseButtons.Left);
        }
    }
}
