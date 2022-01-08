using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;
using System;

namespace Ping_Pong.Paddles
{
    public abstract class Paddle
    {
        private RectangleControl rectanglePaddle;
        private const int width = 6;
        private const int height = 65;
        private const int margin = 6;
        public readonly PaddlePosition PaddlePosition;
        private readonly float speed = 1000f;
        private EaseAnimation paddleEaseAnimation;
        public Rectangle DestinationRectangle => rectanglePaddle.DestinationRectangle;
        private readonly Action onPaddleMoved;

        public Paddle(PaddlePosition paddlePosition, float speed, Action onPaddleMoved)
        {
            this.PaddlePosition = paddlePosition;
            this.speed = speed;
            this.onPaddleMoved = onPaddleMoved;
            CreatePaddle();
        }

        public void CreatePaddle()
        {
            rectanglePaddle = new RectangleControl(new Rectangle(margin, PositionCalculations.CenterVertical(height), width, height), Color.White)
                .SetAnchor(PaddlePosition == PaddlePosition.Left ? Anchor.TopLeft : Anchor.TopRight)
                .AddToScreen();
        }

        public void Move(int posMiddleY)
        {
            var posY = posMiddleY - (height / 2);
            // block the paddle inside of the screen
            posY = MathExtension.CapValue(posY, 0, ServiceProvider.ScreenManager.ScreenSize.Y - height);

            paddleEaseAnimation?.Stop();

            // block the paddle to move with max speed
            var duration = Math.Abs((posY - rectanglePaddle.PositionAnchor.Y) / speed);
            if (duration > 0)
            {
                paddleEaseAnimation = new EaseAnimation(rectanglePaddle, duration, new Vector2(rectanglePaddle.PositionAnchor.X, posY))
                    .AddOnAnimationEnd(onPaddleMoved)
                    .Play();
            }
        }
    }
}
