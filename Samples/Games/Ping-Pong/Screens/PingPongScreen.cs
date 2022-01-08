using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using Ping_Pong.Paddles;
using System.Collections.Generic;

namespace Ping_Pong.Screens
{
    public class PingPongScreen : Screen
    {
        private SpriteFont fontHoboStb;
        private Score score;
        private Ball ball;

        private List<Paddle> paddles;

        public override void LoadContent()
        {
            fontHoboStb = ContentLoader.LoadSpriteFont("HoboStd");

            base.LoadContent();
        }

        public override void OnInit()
        {
            score = new Score(fontHoboStb);
            ball = new Ball(CheckBallCollisionWithPaddles);

            AddMiddleStrippedLine();

            paddles = new List<Paddle>
            {
                new PlayerPaddle(ProcessBallCollisionWithPaddles),
                new ComputerPaddle(ball, ProcessBallCollisionWithPaddles)
            };

            ServiceProvider.RootPanel.AddOnUpdateEvent(Update);

            base.OnInit();
        }

        private void Update(GameTime gameTime)
        {
            if (ball.IsPlayerGoal())
            {
                score.AddScoreToPlayer();
                ball.ResetBall();
                return;
            }
            else if (ball.IsComputerGoal())
            {
                score.AddScoreToComputer();
                ball.ResetBall();
                return;
            }
        }

        private void ProcessBallCollisionWithPaddles() => CheckBallCollisionWithPaddles();

        private bool CheckBallCollisionWithPaddles()
        {
            foreach (var paddle in paddles)
            {
                if (ball.CheckCollisionWithPaddle(paddle))
                    return true;
            }

            return false;
        }

        private void AddMiddleStrippedLine()
        {
            var lineWidth = 3;
            var lineHeight = 12;
            var lineSpace = 8;

            var posX = (ServiceProvider.ScreenManager.ScreenSize.X - lineWidth) / 2;
            var posY = 0;

            while (posY < ServiceProvider.ScreenManager.ScreenSize.Y)
            {
                new RectangleControl(new Rectangle(posX, posY, lineWidth, lineHeight), Color.White * 0.5f)
                    .AddToScreen();

                posY += lineSpace + lineHeight;
            }
        }
    }
}
