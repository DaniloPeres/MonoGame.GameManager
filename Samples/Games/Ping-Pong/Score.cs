using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Services;

namespace Ping_Pong
{
    public class Score
    {
        private readonly SpriteFont spriteFont;
        private int playerScore;
        private int computerScore;

        private Label
            labelPlayerScore,
            labelComputerScore;

        public Score(SpriteFont spriteFont)
        {
            this.spriteFont = spriteFont;
            CreateScoreUI();
        }


        private void CreateScoreUI()
        {
            var screenSize = ServiceProvider.ScreenManager.ScreenSize;
            var panelWidth = screenSize.X / 2;

            const int textMarginTop = 20;

            var panelPlayerScore = new Panel(new Rectangle(0, 0, panelWidth, screenSize.Y))
                .AddToScreen();
            labelPlayerScore = new Label(spriteFont, "0", new Vector2(0, textMarginTop), Color.White)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.TopCenter)
                .AddToScreen(panelPlayerScore);

            var panelComputerScore = new Panel(new Rectangle(panelWidth, 0, panelWidth, screenSize.Y))
                .AddToScreen();
            labelComputerScore = new Label(spriteFont, "0", new Vector2(0, textMarginTop), Color.White)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.TopCenter)
                .AddToScreen(panelComputerScore);
        }

        public void AddScoreToPlayer()
        {
            playerScore++;
            EffectToUpdateScore(labelPlayerScore, playerScore);
        }

        public void AddScoreToComputer()
        {
            computerScore++;
            EffectToUpdateScore(labelComputerScore, computerScore);
        }

        private void EffectToUpdateScore(Label labelScore, int newValue)
        {
            var effectDuration = 0.15f;
            new FadeAnimation(labelScore, effectDuration, 0f)
                .AddOnAnimationEnd(() =>
                {
                    labelScore.Text = newValue.ToString();
                    new FadeAnimation(labelScore, effectDuration, 1f)
                        .Play();
                })
                .Play();
        }
    }
}
