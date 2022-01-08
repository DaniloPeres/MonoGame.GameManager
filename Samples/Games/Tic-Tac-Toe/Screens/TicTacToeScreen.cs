using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Timers;
using System.Collections.Generic;
using System.Linq;

namespace Tic_Tac_Toe.Screens
{
    public class TicTacToeScreen : Screen
    {
        private const int FieldSquareSize = 115;
        private const int GamePosY = 125;
        public const int TotalGameSquares = 3;
        private int gamePosX;

        private SpriteFont fontArial;

        private Texture2D
            imgTitle,
            imgO,
            imgX;

        private Label
            labelPlayerScore,
            labelTieScore,
            labelComputerScore;

        private int
            scorePlayer,
            scoreTie,
            scoreComputer;

        private const float finalLabelScoreScale = 1.25f;

        private bool blockAction = true;
        private bool clickToReset;

        private FieldValue playerValue = FieldValue.X;
        private FieldValue botValue = FieldValue.O;

        private Image[,] imageFields = new Image[3, 3];
        private FieldValue[,] fieldValues = new FieldValue[3, 3];
        private List<RectangleControl> fieldLines = new List<RectangleControl>();
        private readonly Bot bot;
        private FieldValue startedPlaying = FieldValue.X;

        public TicTacToeScreen()
        {
            bot = new Bot(botValue, playerValue);
        }

        public override void LoadContent()
        {
            imgTitle = ContentLoader.LoadTexture2D("Title");
            imgO = ContentLoader.LoadTexture2D("O");
            imgX = ContentLoader.LoadTexture2D("X");

            fontArial = ContentLoader.LoadSpriteFont("Arial");

            base.LoadContent();
        }

        public override void OnInit()
        {
            gamePosX = PositionCalculations.CenterHorizontal(FieldSquareSize * TotalGameSquares, ScreenManager.ScreenSize.X);

            new Image(imgTitle)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.TopCenter)
                .SetPosition(0, 35)
                .AddToScreen();

            CreateBackgroundLines();
            CreateGameScore();
            CleanFields();
            NextTurn(startedPlaying);

            base.OnInit();
        }

        private void CreateGameScore()
        {
            const int posY = 500;
            var posX = 10;
            float scale = 0.65f;
            const int panelHeight = 80;
            const int scorePosY = 10;

            // Player score
            var playerScorePanel = new Panel(new Rectangle(posX, posY, 120, panelHeight))
                .AddToScreen();
            posX += (int)playerScorePanel.Size.X;

            new Label(fontArial, "PLAYER (X)", Vector2.Zero, Color.White)
                .SetScale(scale)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.TopCenter)
                .AddToScreen(playerScorePanel);

            labelPlayerScore = new Label(fontArial, "0", new Vector2(0, scorePosY), Color.White)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.Center)
                .SetScale(finalLabelScoreScale)
                .AddToScreen(playerScorePanel);

            // Tie score
            var tieScorePanel = new Panel(new Rectangle(posX, posY, 110, panelHeight))
                .AddToScreen();
            posX += (int)tieScorePanel.Size.X;

            new Label(fontArial, "TIE", Vector2.Zero, Color.White)
                .SetScale(scale)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.TopCenter)
                .AddToScreen(tieScorePanel);

            labelTieScore = new Label(fontArial, "0", new Vector2(0, scorePosY), Color.White)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.Center)
                .SetScale(finalLabelScoreScale)
                .AddToScreen(tieScorePanel);

            // Computer score
            var computerScorePanel = new Panel(new Rectangle(posX, posY, 150, panelHeight))
                .AddToScreen();

            new Label(fontArial, "COMPUTER (O)", Vector2.Zero, Color.White)
                .SetScale(scale)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.TopCenter)
                .AddToScreen(computerScorePanel);

            labelComputerScore = new Label(fontArial, "0", new Vector2(0, scorePosY), Color.White)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.Center)
                .SetScale(finalLabelScoreScale)
                .AddToScreen(computerScorePanel);
        }

        private void CreateBackgroundLines()
        {
            const int lineWidth = 5;
            Color lineColor = Color.White;
            for (var i = 1; i < TotalGameSquares; i++)
            {
                // Create the horizontal line
                fieldLines.Add(new RectangleControl(new Rectangle(gamePosX, GamePosY + FieldSquareSize * i, FieldSquareSize * TotalGameSquares, lineWidth), lineColor)
                    .AddToScreen());

                // Create the vertical line
                fieldLines.Add(new RectangleControl(new Rectangle(gamePosX + FieldSquareSize * i, GamePosY, lineWidth, FieldSquareSize * TotalGameSquares), lineColor)
                    .AddToScreen());
            }

            FieldsCalculation.ForEachAllFields().ToList().ForEach(item =>
            {
                var x = item.x;
                var y = item.y;

                var panelsFields = new Panel(new Rectangle(gamePosX + FieldSquareSize * x, GamePosY + FieldSquareSize * y, FieldSquareSize, FieldSquareSize))
                    .AddOnClick(args => OnFieldClick(x, y))
                    .AddToScreen();

                imageFields[x, y] = new Image(imgO)
                    .SetAnchor(MonoGame.GameManager.Enums.Anchor.Center)
                    .AddToScreen(panelsFields);
            });
        }

        private void OnFieldClick(int x, int y)
        {
            if (clickToReset)
            {
                ResetGame();
                return;
            }

            if (blockAction || fieldValues[x, y] != FieldValue.None)
                return;

            blockAction = true;
            SetFieldValue(x, y, playerValue);
            NextTurnWithDelay(botValue);
        }

        private void ResetGame()
        {
            clickToReset = false;
            CleanFields();
            startedPlaying = startedPlaying == playerValue ? botValue : playerValue;
            NextTurn(startedPlaying);
        }

        private void RunBotWithDelay() => DelayTime.DelayAction(0.1f, RunBot);

        private void RunBot()
        {
            var field = bot.ChooseField(fieldValues);
            SetFieldValue(field.x, field.y, botValue);
            NextTurnWithDelay(playerValue);
        }

        private void NextTurnWithDelay(FieldValue nextToPlay)
        {
            DelayTime.DelayAction(0.3f, () => NextTurn(nextToPlay));
        }

        private void NextTurn(FieldValue nextToPlay)
        {
            if (CheckGameIsCompleted())
            {
                DelayTime.DelayAction(1f, () =>
                {
                    clickToReset = true;
                });
                return;
            }

            if (nextToPlay == playerValue)
                blockAction = false;
            else
                RunBotWithDelay();
        }

        private bool CheckGameIsCompleted()
        {
            var playerWinnerFields = FieldsCalculation.FieldsToWin.FirstOrDefault(items => items.All(item => fieldValues[item.x, item.y] == playerValue));
            var botWinnerFields = FieldsCalculation.FieldsToWin.FirstOrDefault(items => items.All(item => fieldValues[item.x, item.y] == botValue));
            if (playerWinnerFields != null) // check player winner
                OnPlayerWins(playerWinnerFields);
            else if (botWinnerFields != null) // check bot winner
                OnBotWins(botWinnerFields);
            else if (FieldsCalculation.ForEachAllFields().ToList().All(item => fieldValues[item.x, item.y] != FieldValue.None)) // check tie
                OnTie();
            else
                return false;

            return true;
        }

        private void OnPlayerWins(List<(int x, int y)> playerWinnerFields)
        {
            BlinkFields(playerWinnerFields);
            AddPlayerScore();
        }

        private void OnBotWins(List<(int x, int y)> botWinnerFields)
        {
            BlinkFields(botWinnerFields);
            AddBotScore();
        }

        private void OnTie()
        {
            // blink the lines
            fieldLines.ForEach(BlinkEffect);
            // darken all fields
            FieldsCalculation.ForEachAllFields().ToList().ForEach(item => DarkenControl(imageFields[item.x, item.y]));
            AddTieScore();
        }

        private void BlinkFields(List<(int x, int y)> fields)
        {
            // darken all fields (except in the fields parameter)
            FieldsCalculation.ForEachAllFields().ToList().ForEach(item =>
            {
                if (fieldValues[item.x, item.y] != FieldValue.None && !fields.Contains(item))
                    DarkenControl(imageFields[item.x, item.y]);
            });

            fields.ForEach(item => BlinkEffect(imageFields[item.x, item.y]));
        }

        private void AddPlayerScore()
        {
            UpdateScoreValue(labelPlayerScore, ++scorePlayer);
        }

        private void AddBotScore()
        {
            UpdateScoreValue(labelComputerScore, ++scoreComputer);
        }

        private void AddTieScore()
        {
            UpdateScoreValue(labelTieScore, ++scoreTie);
        }

        private void UpdateScoreValue(Label labelScore, int newValue)
        {
            new ScaleAnimation(labelScore, 0.15f, Vector2.Zero)
                .Play()
                .AddOnAnimationEnd(() =>
                {
                    labelScore.Text = newValue.ToString();
                    ScaleEffect(labelScore, finalLabelScoreScale);
                });
        }

        private void SetFieldValue(int x, int y, FieldValue value)
        {
            fieldValues[x, y] = value;
            var imageField = imageFields[x, y];
            imageField.Texture = value == FieldValue.X
                ? imgX
                : imgO;
            ScaleEffect(imageField, 1f);
        }

        private void ScaleEffect(IScalableControl control, float finalScale)
        {
            control.Color = Color.White;
            control.SetScale(0f);
            new ScaleAnimation(control, 0.15f, new Vector2(finalScale * 1.5f))
                .Play()
                .AddOnAnimationEnd(() =>
                {
                    new ScaleAnimation(control, 0.05f, new Vector2(finalScale))
                        .Play();
                });
        }

        private void DarkenControl(IControl control)
        {
            new FadeAnimation(control, 0.2f, 0.5f)
                .SetTransparencyStart(1f)
                .Play();
        }

        private void BlinkEffect(IControl control)
        {
            var blinkStepDelay = 0.1f;
            var steps = 6;
            for (var i = 0; i < steps; i++)
            {
                var stepIndex = i;
                DelayTime.DelayAction(blinkStepDelay * i, () =>
                {
                    control.Color = stepIndex % 2 == 0 ? Color.Transparent : Color.White;
                });
            }
        }

        private void CleanFields()
        {
            FieldsCalculation.ForEachAllFields().ToList().ForEach(item =>
            {
                fieldValues[item.x, item.y] = FieldValue.None;
                imageFields[item.x, item.y].SetColor(Color.Transparent);
            });
        }
    }
}
