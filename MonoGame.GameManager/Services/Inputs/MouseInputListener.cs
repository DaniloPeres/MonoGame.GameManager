using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Services.Inputs
{
    public class MouseInputListener
    {
        private MouseState currentState;
        private GameTime gameTime;
        private MouseEventArgs onMouseDownArgs;
        private MouseState previousState;

        public event Action<MouseEventArgs> OnMouseDown;
        public event Action<MouseEventArgs> OnMouseUp;
        public event Action<MouseEventArgs> OnMouseMove;
        public event Action<MouseEventArgs> OnMouseWheelMoved;

        public void Update(GameTime gameTime)
        {
            this.gameTime = gameTime;
            var mouseState = Mouse.GetState();
            var mousePositionOnScreen = ServiceProvider.GameWindowManager.GetPositionOnScreen(mouseState.Position.ToVector2()).ToPoint();
            currentState = new MouseState(mousePositionOnScreen.X, mousePositionOnScreen.Y, mouseState.ScrollWheelValue, mouseState.LeftButton, mouseState.MiddleButton, mouseState.RightButton, mouseState.XButton1, mouseState.XButton2, mouseState.HorizontalScrollWheelValue);

            CheckMouseMoved();
            CheckMousePressed();
            CheckMouseReleased();

            // Handle mouse wheel events.
            if (previousState.ScrollWheelValue != currentState.ScrollWheelValue)
            {
                var args = new MouseEventArgs(gameTime.ElapsedGameTime, currentState);
                OnMouseWheelMoved?.Invoke(args);
            }

            previousState = currentState;
        }

        private void CheckMouseMoved()
        {
            if (!currentState.Position.Equals(previousState.Position))
                OnMouseMove?.Invoke(new MouseEventArgs(gameTime.ElapsedGameTime, currentState));

        }

        private void CheckMousePressed()
        {
            if (currentState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released)
            {
                onMouseDownArgs = new MouseEventArgs(gameTime.ElapsedGameTime, currentState);
                OnMouseDown?.Invoke(onMouseDownArgs);
            }
        }

        private void CheckMouseReleased()
        {
            if (currentState.LeftButton == ButtonState.Released && previousState.LeftButton == ButtonState.Pressed)
            {
                var args = new MouseEventArgs(gameTime.ElapsedGameTime, currentState);
                OnMouseUp?.Invoke(args);
            }
        }
    }
}
