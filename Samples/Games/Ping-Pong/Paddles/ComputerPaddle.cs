using Microsoft.Xna.Framework;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;
using System;

namespace Ping_Pong.Paddles
{
    public class ComputerPaddle : Paddle
    {
        private readonly Ball ball;

        private const float HitPositionTimeChange = 3f;
        private double timeCountChooseHitPosition;
        private int variationMiddlePositionY;

        public ComputerPaddle(Ball ball, Action onPaddleMoved) : base(PaddlePosition.Right, 600, onPaddleMoved)
        {
            this.ball = ball;

            ServiceProvider.RootPanel.AddOnUpdateEvent(UpdatePaddlePosition);
        }

        public void UpdatePaddlePosition(GameTime gameTime)
        {
            // update the paddle position following the ball position

            // To avoid the paddle always hit the middle and have no Y variation, let's calculate a random position between 5% and 95% of paddle position Y
            timeCountChooseHitPosition += gameTime.ElapsedGameTime.TotalSeconds;
            if (timeCountChooseHitPosition > HitPositionTimeChange)
            {
                const float variationRateMiddle = 0.9f;
                var middleAreaToHit = (int)((DestinationRectangle.Height * variationRateMiddle) - (DestinationRectangle.Height / 2));
                variationMiddlePositionY = RandomGenerator.Random(-middleAreaToHit, middleAreaToHit);
                timeCountChooseHitPosition = 0;
            }

            Move(ball.DestinationRectangle.Top + variationMiddlePositionY);
        }
    }
}
