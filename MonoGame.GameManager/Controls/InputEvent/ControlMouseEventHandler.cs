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
    public class ControlMouseEventHandler
    {
        private readonly MouseInputListener mouseInputListener;
        private readonly TouchInputListener touchInputListener;
        private readonly Panel panelContainer;

        public ControlMouseEventHandler(MouseInputListener mouseInputListener, TouchInputListener touchInputListener)
        {
            this.mouseInputListener = mouseInputListener;
            this.touchInputListener = touchInputListener;

            // create a container to use root panel events
            panelContainer = new Panel();

            mouseInputListener.OnMouseDown += OnMouseDown;
            mouseInputListener.OnMouseMove += OnMouseMove;
            mouseInputListener.OnMouseUp += OnMouseUp;
            touchInputListener.OnTouchStarted += OnTouchStarted;
            touchInputListener.OnTouchReleased += OnTouchReleased;
            touchInputListener.OnTouchMoved += OnTouchMoved;
            touchInputListener.OnMultipleTouch += OnMultipleTouchpoints;
        }

        public void AddRootPanel(Panel rootPanel)
        {
            panelContainer.AddChild(rootPanel);
        }

        public void Update(GameTime gameTime)
        {
            mouseInputListener.Update(gameTime);
            touchInputListener.Update(gameTime);
        }

        private void OnMouseDown(MouseEventArgs args)
            => CheckMouseEvent(args, panelContainer.Children, OnControlMouseDown);

        private void OnMouseMove(MouseEventArgs args)
        {
            // Get all controls that are marked as hover
            var mouseHoveredControls = panelContainer.Find(control => control.IsMouseHover).ToList();

            SetControlsAsNotHover();

            CheckMouseEvent(args, panelContainer.Children, OnControlMouseMove);

            // Check which controls have received mouse enter or mouse leave
            var mouseHoveredControlsUpdated = panelContainer.Find(control => control.IsMouseHover).ToList();
            var controlsMouseEnter = mouseHoveredControlsUpdated.Where(control => !mouseHoveredControls.Contains(control)).ToList();
            controlsMouseEnter.ForEach(control => control.FireOnMouseEnter(CreateControlEventArgs(control, args)));
            var controlsMouseLeave = mouseHoveredControls.Where(control => !mouseHoveredControlsUpdated.Contains(control)).ToList();
            controlsMouseLeave.ForEach(control => control.FireOnMouseLeave(CreateControlEventArgs(control, args)));
        }

        private void OnMouseUp(MouseEventArgs args)
        {
            if (args.IsTouchInput)
                SetControlsAsNotHover();

            // get and mark all items as not pressed
            // use the pressedControls to check click
            var pressedControls = panelContainer.Find(control => control.IsMousePressed).ToList();
            pressedControls.ForEach(control => control.SetMousePressed(false));

            CheckMouseEvent(args, panelContainer.Children, (control, args2) => OnControlMouseUp(control, args2, pressedControls));
        }

        private void SetControlsAsNotHover()
        {
            // Get all controls that are marked as hover
            var mouseHoveredControls = panelContainer.Find(control => control.IsMouseHover).ToList();

            // Set the controls as not pressed
            mouseHoveredControls.ForEach(control => control.SetMouseHover(false));
        }

        private bool OnControlMouseDown(IControl control, MouseEventArgs args)
        {
            var controlArgs = CreateControlEventArgs(control, args);
            control.SetMousePressed(true);
            control.FireOnPressed(controlArgs);
            return !controlArgs.ShouldStopPropagation;
        }

        private bool OnControlMouseMove(IControl control, MouseEventArgs args)
        {
            var controlArgs = CreateControlEventArgs(control, args);

            control.SetMouseHover(true);
            control.FireOnMoved(controlArgs);

            return !controlArgs.ShouldStopPropagation;
        }

        private bool OnControlMouseUp(IControl control, MouseEventArgs args, List<IControl> pressedControls)
        {
            var controlArgs = CreateControlEventArgs(control, args);

            // Check click action
            if (pressedControls.Contains(control))
                control.FireOnClick(controlArgs);

            control.FireOnReleased(controlArgs);
            return !controlArgs.ShouldStopPropagation;
        }

        private bool OnControlMultipleTouchpoints(IControl control, MultipleTouchpointsEventArgs args)
        {
            var controlArgs = new ControlMultipleTouchpointsEventArgs(control, args.Time, args.Touchpoints);

            control.FireOnMultipleTouchpoints(controlArgs);

            return !controlArgs.ShouldStopPropagation;
        }

        private bool CheckMouseEvent(MouseEventArgs args, IEnumerable<IControl> controls, Func<IControl, MouseEventArgs, bool> onInteractWithControl)
        {
            foreach (var control in controls.Reverse().ToList())
            {
                // First check children elements if the control is a container
                if (control is IContainer container && !CheckMouseEvent(args, container.Children, onInteractWithControl))
                    return false;

                // Check if the mouse events position is hitting this control
                if (!control.Intersects(args.Position))
                    continue;

                // call the control event and return if should continue propagation
                if (!onInteractWithControl(control, args))
                    return false;
            }

            return true;
        }

        private bool CheckMultipleTouchpointsEvent(MultipleTouchpointsEventArgs args, IEnumerable<IControl> controls)
        {
            foreach (var control in controls.Reverse().ToList())
            {
                // First check children elements if the control is a container
                if (control is IContainer container && !CheckMultipleTouchpointsEvent(args, container.Children))
                    return false;

                // Check if the mouse events position is hitting this control
                var allPointsIntersect = args.Touchpoints.Select(touchpoint => control.Intersects(touchpoint.Position.ToPoint()));
                if (!allPointsIntersect.All(intersects => intersects))
                    continue;

                // call the control event and return if should continue propagation
                if (!OnControlMultipleTouchpoints(control, args))
                    return false;
            }

            return true;

        }

        private ControlMouseEventArgs CreateControlEventArgs(IControl control, MouseEventArgs args)
            => new ControlMouseEventArgs(control, args.Time, args.CurrentState);

        private void OnTouchStarted(TouchEventArgs args)
            => OnMouseDown(ConvertTouchToMouseEventArgs(args, ButtonState.Pressed));

        private void OnTouchMoved(TouchEventArgs args)
            => OnMouseMove(ConvertTouchToMouseEventArgs(args, ButtonState.Pressed));

        private void OnTouchReleased(TouchEventArgs args)
            => OnMouseUp(ConvertTouchToMouseEventArgs(args, ButtonState.Released));

        private void OnMultipleTouchpoints(MultipleTouchpointsEventArgs args)
            => CheckMultipleTouchpointsEvent(args, panelContainer.Children);

        private static MouseEventArgs ConvertTouchToMouseEventArgs(TouchEventArgs touchEventArgs, ButtonState leftButton)
        {
            var mouseState = CreateMouseState(touchEventArgs.TouchLocation, leftButton);
            return new MouseEventArgs(touchEventArgs.Time, mouseState, true);
        }

        private static MouseState CreateMouseState(TouchLocation touchLocation, ButtonState leftButton)
            => new MouseState((int)touchLocation.Position.X, (int)touchLocation.Position.Y, 0, leftButton, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
    }
}
