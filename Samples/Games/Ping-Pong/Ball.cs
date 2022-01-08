using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Services;
using Ping_Pong.Paddles;
using System;

namespace Ping_Pong
{
    public class Ball
    {
        private Image ball;
        private float initialSpeed = 320;
        private float speed;
        private float diameter = 12;
        private const float speedIncrease = 40;
        private Vector2 direction;
        private Point screenSize => ServiceProvider.ScreenManager.ScreenSize;
        public Rectangle DestinationRectangle => ball.DestinationRectangle;
        private readonly Func<bool> onBallMoved;

        public Ball(Func<bool> onBallMoved)
        {
            this.onBallMoved = onBallMoved;
            CreateBall();
            ServiceProvider.RootPanel.AddOnUpdateEvent(Update);
        }

        private void CreateBall()
        {
            var ballTexture2D = ShaderEffects.CreateCircle((int)diameter, Color.White, ServiceProvider.GraphicsDevice);
            ball = new Image(ballTexture2D)
                .AddToScreen();
            ResetBall();
        }

        public void ResetBall()
        {
            speed = initialSpeed;
            direction = new Vector2(0.92f, 0.38f);
            ball.SetPosition(PositionCalculations.CenterVerticalAndHorizontal(ball.Texture.Size().ToVector2()));
        }

        public void Update(GameTime gameTime)
        {
            var move = direction * (speed * (float)gameTime.ElapsedGameTime.TotalSeconds);

            // move pixel per pixel to ball not skip the screen
            while (Math.Abs(move.X) + Math.Abs(move.Y) > 1)
            {
                var movePixel = new Vector2(Math.Min(Math.Abs(move.X), 1) * (move.X < 0 ? -1 : 1), Math.Min(Math.Abs(move.Y), 1) * (move.Y < 0 ? -1 : 1));
                move -= movePixel;

                var pos = ball.PositionAnchor + movePixel;
                ball.SetPosition(pos);

                if (onBallMoved())
                    break;

                if (pos.Y + diameter >= screenSize.Y && direction.Y > 0  // Collision to the bottom
                    || pos.Y <= 0 && direction.Y < 0)                    // Collision to the top
                {
                    direction.Y *= -1;
                    break;
                }
            }
        }

        public bool IsPlayerGoal()
             => ball.PositionAnchor.X + diameter >= screenSize.X; // Collision to the right

        public bool IsComputerGoal()
             => ball.PositionAnchor.X <= 0; // Collision to the left

        public bool CheckCollisionWithPaddle(Paddle paddle)
        {
            if ((paddle.PaddlePosition == PaddlePosition.Left && direction.X < 0 || paddle.PaddlePosition == PaddlePosition.Right && direction.X > 0) // first check if the ball is going to correct direction for this paddle
                && paddle.DestinationRectangle.Intersects(ball.DestinationRectangle))
            {
                var collidePoint = ball.DestinationRectangle.Top - (paddle.DestinationRectangle.Top + paddle.DestinationRectangle.Height / 2f);
                collidePoint /= (paddle.DestinationRectangle.Height / 2f);

                // calculate angle in Radian
                var angleRad = collidePoint * Math.PI / 4;

                // change ball direction
                var xDirection = paddle.PaddlePosition == PaddlePosition.Left ? 1 : -1;
                direction = new Vector2((float)Math.Cos(angleRad) * xDirection, (float)Math.Sin(angleRad));

                speed += speedIncrease;
                return true;
            }

            return false;
        }
    }
}
