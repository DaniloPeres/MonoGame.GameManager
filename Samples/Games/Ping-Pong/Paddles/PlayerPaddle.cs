using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Services;
using System;

namespace Ping_Pong.Paddles
{
    public class PlayerPaddle : Paddle
    {
        public PlayerPaddle(Action onPaddleMoved) : base(PaddlePosition.Left, 1200, onPaddleMoved)
        {
            ServiceProvider.RootPanel.AddOnMouseMoved(OnMouseMoved);
        }

        private void OnMouseMoved(ControlMouseEventArgs args)
        {
            args.ShouldStopPropagation = false;
            Move(args.Position.Y);
        }
    }
}
