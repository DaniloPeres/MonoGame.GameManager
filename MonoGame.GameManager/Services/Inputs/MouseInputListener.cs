using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoGame.GameManager.Services.Inputs
{
    /// <summary>
    /// Polls the mouse once per frame and raises events for movement, buttons and the scroll wheel.
    /// Positions are converted from window coordinates to virtual screen coordinates.
    /// </summary>
    public class MouseInputListener
    {
        private static readonly MouseButtons[] Buttons = { MouseButtons.Left, MouseButtons.Right, MouseButtons.Middle, MouseButtons.XButton1, MouseButtons.XButton2 };

        private readonly Func<Vector2, Vector2> windowToScreen;
        private MouseState currentState;
        private MouseState previousState;
        private bool hasState;

        public event Action<MouseEventArgs> OnMouseDown;
        public event Action<MouseEventArgs> OnMouseUp;
        public event Action<MouseEventArgs> OnMouseMove;
        public event Action<MouseEventArgs> OnMouseWheelMoved;

        public MouseInputListener() : this(null) { }

        /// <param name="windowToScreen">Converts window coordinates to screen coordinates (null = no conversion).</param>
        public MouseInputListener(Func<Vector2, Vector2> windowToScreen)
        {
            this.windowToScreen = windowToScreen;
        }

        /// <summary>The state of the current frame (position in virtual screen coordinates).</summary>
        public MouseState CurrentState => currentState;

        /// <summary>The state of the previous frame (position in virtual screen coordinates).</summary>
        public MouseState PreviousState => previousState;

        /// <summary>The pointer position in virtual screen coordinates.</summary>
        public Point Position => currentState.Position;

        /// <summary>The scroll wheel change since the previous frame (positive = up).</summary>
        public int ScrollWheelDelta => currentState.ScrollWheelValue - previousState.ScrollWheelValue;

        public bool IsButtonDown(MouseButtons button) => GetButtonState(currentState, button) == ButtonState.Pressed;

        public bool IsButtonUp(MouseButtons button) => !IsButtonDown(button);

        /// <summary>True if the button was held down in the previous frame.</summary>
        public bool WasButtonDown(MouseButtons button) => GetButtonState(previousState, button) == ButtonState.Pressed;

        /// <summary>True only in the frame the button was pressed.</summary>
        public bool WasButtonPressed(MouseButtons button)
            => GetButtonState(currentState, button) == ButtonState.Pressed && GetButtonState(previousState, button) == ButtonState.Released;

        /// <summary>True only in the frame the button was released.</summary>
        public bool WasButtonReleased(MouseButtons button)
            => GetButtonState(currentState, button) == ButtonState.Released && GetButtonState(previousState, button) == ButtonState.Pressed;

        public void Update(GameTime gameTime) => Update(Mouse.GetState(), gameTime.TotalGameTime);

        /// <summary>
        /// Updates the listener with a given state (in window coordinates), for example to simulate input.
        /// </summary>
        public void Update(MouseState windowState, TimeSpan time)
        {
            var position = windowState.Position.ToVector2();
            if (windowToScreen != null)
                position = windowToScreen(position);

            var screenState = new MouseState((int)Math.Floor(position.X), (int)Math.Floor(position.Y), windowState.ScrollWheelValue,
                windowState.LeftButton, windowState.MiddleButton, windowState.RightButton, windowState.XButton1, windowState.XButton2,
                windowState.HorizontalScrollWheelValue);

            previousState = hasState ? currentState : screenState;
            currentState = screenState;
            hasState = true;

            if (currentState.Position != previousState.Position)
                OnMouseMove?.Invoke(new MouseEventArgs(time, currentState, false, MouseButtons.None));

            foreach (var button in Buttons)
            {
                if (WasButtonPressed(button))
                    OnMouseDown?.Invoke(new MouseEventArgs(time, currentState, false, button));
            }

            foreach (var button in Buttons)
            {
                if (WasButtonReleased(button))
                    OnMouseUp?.Invoke(new MouseEventArgs(time, currentState, false, button));
            }

            var wheelDelta = ScrollWheelDelta;
            if (wheelDelta != 0)
                OnMouseWheelMoved?.Invoke(new MouseEventArgs(time, currentState, false, MouseButtons.None, wheelDelta));
        }

        private static ButtonState GetButtonState(MouseState state, MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left: return state.LeftButton;
                case MouseButtons.Right: return state.RightButton;
                case MouseButtons.Middle: return state.MiddleButton;
                case MouseButtons.XButton1: return state.XButton1;
                case MouseButtons.XButton2: return state.XButton2;
                default: return ButtonState.Released;
            }
        }
    }
}
