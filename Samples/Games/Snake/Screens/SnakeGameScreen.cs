using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Controls.MouseEvent;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Timers;
using System.Collections.Generic;
using System.Linq;

namespace Snake.Screens
{
    public class SnakeGameScreen : Screen
    {
        private Panel board;
        private Texture2D textureSnakeBlock;
        private Queue<Image> snakeBlocksRectangles;
        private MovementDirection movementDirection;
        private MovementDirection? nextMovementDirection;
        private Point headPosition;
        private DelayTime moveSnakeDelayTime;
        private bool isPaused;
        private bool isGameOver;
        private int level;
        private Dictionary<int, float> snakeSpeedByLevel = new Dictionary<int, float>
        {
            { 0, 0.4f },
            { 1, 0.2f },
            { 2, 0.1f }
        };
        private ScaleAnimation foodScaleAnimation;
        private RotationAnimation foodRotationAnimation;
        private Image foodImage;
        private const float gamePausedTransparency = 0.5f;

        public SnakeGameScreen(int level)
        {
            this.level = level;
        }

        public override void LoadContent()
        {
            textureSnakeBlock = ShaderEffects.CreateRoudedRectangle(6f, new Point(UiHelper.BlockSize), UiHelper.DarkBackgroundColor, ServiceProvider.GraphicsDevice);

            base.LoadContent();
        }

        public override void OnInit()
        {
            board = UiHelper.CreatePanelBoard();

            DisplayCountDownInfo(3);

            base.OnInit();
        }

        public override void Dispose()
        {
            textureSnakeBlock.Dispose();
            base.Dispose();
        }

        private void InitiateSnake()
        {
            snakeBlocksRectangles = new Queue<Image>();
            AddSnakeBlock(new Point(3, 5));
            AddSnakeBlock(new Point(4, 5));
            AddSnakeBlock(new Point(5, 5));

            movementDirection = MovementDirection.Right;

            board.AddOnClick(OnBoardClick);
            board.AddOnUpdateEvent(CheckPressedKey);
            moveSnakeDelayTime = new DelayTime(snakeSpeedByLevel[level], MoveSnake)
                .SetIsLoop(true)
                .Play();

            CreateFood();
        }

        private void CreateFood()
        {
            var foodPosition = new Point(RandomGenerator.Random(0, UiHelper.BlocksX - 1), RandomGenerator.Random(0, UiHelper.BlocksY - 1));
            if (IsSnakeOnPosition(foodPosition, false))
            {
                // position is taken, try another position
                CreateFood();
                return;
            }

            // remove previous food
            foodImage?.RemoveFromScreen();
            foodRotationAnimation?.Stop();
            foodScaleAnimation?.Stop();

            foodImage = new Image(ContentHandler.Instance.TextureFood)
                .SetPosition(foodPosition.ToVector2() * new Vector2(UiHelper.BlockSize) + (ContentHandler.Instance.TextureFood.Size().ToVector2() / 2))
                .SetColor(UiHelper.DarkBackgroundColor)
                .SetOriginRate(0.5f)
                .SetInfo(foodPosition)
                .AddToScreen(board);

            foodRotationAnimation = new RotationAnimation(foodImage, UiHelper.RotationAnimationTime, 360f)
                .SetIsLooping(true)
                .Play();

            foodScaleAnimation = new ScaleAnimation(foodImage, 0.3f, new Vector2(1.5f))
                .SetScaleStart(new Vector2(0))
                .AddOnAnimationEnd(() =>
                {
                    foodScaleAnimation = new ScaleAnimation(foodImage, 0.1f, new Vector2(1f))
                        .AddOnAnimationEnd(() => foodScaleAnimation = null)
                        .Play();
                })
                .Play();
        }

        private void AddSnakeBlock(Point posOnMap)
        {
            var snakeBlockRectangle = new Image(textureSnakeBlock)
                .SetPosition(posOnMap.ToVector2() * UiHelper.BlockSize)
                .SetInfo(posOnMap)
                .AddToScreen(board);
            snakeBlocksRectangles.Enqueue(snakeBlockRectangle);
            headPosition = posOnMap;
        }

        private void CheckPressedKey(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            if ((keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up)) && movementDirection != MovementDirection.Down)
            {
                nextMovementDirection = MovementDirection.Up;
            }
            else if ((keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down)) && movementDirection != MovementDirection.Up)
            {
                nextMovementDirection = MovementDirection.Down;
            }
            else if ((keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left)) && movementDirection != MovementDirection.Right)
            {
                nextMovementDirection = MovementDirection.Left;
            }
            else if ((keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right)) && movementDirection != MovementDirection.Left)
            {
                nextMovementDirection = MovementDirection.Right;
            }
        }

        private void MoveSnake()
        {
            movementDirection = nextMovementDirection ?? movementDirection;

            var newPosition = headPosition;
            switch (movementDirection)
            {
                case MovementDirection.Left:
                    newPosition.X--;
                    break;
                case MovementDirection.Right:
                    newPosition.X++;
                    break;
                case MovementDirection.Up:
                    newPosition.Y--;
                    break;
                case MovementDirection.Down:
                    newPosition.Y++;
                    break;
            }

            // check game over
            if (CheckIsGameOver(newPosition))
            {
                GameOver();
                return;
            }

            AddSnakeBlock(newPosition);

            if ((Point)foodImage.Info == newPosition)
            {
                // add points
                CreateFood();
            }
            else
            {
                // do not remove if the snake eat the food
                var removeBlock = snakeBlocksRectangles.Dequeue();
                removeBlock.RemoveFromScreen();
            }
        }

        private bool CheckIsGameOver(Point newPos)
        {
            // check hit with the walls
            if (newPos.X >= UiHelper.BlocksX || newPos.X < 0 || newPos.Y >= UiHelper.BlocksY || newPos.Y < 0)
                return true;

            // check hit with the body
            if (IsSnakeOnPosition(newPos, true))
                return true;

            return false;
        }

        private bool IsSnakeOnPosition(Point position, bool skipLastBlock)
        {
            return snakeBlocksRectangles.ToList().Skip(skipLastBlock ? 1 : 0).Any(x => (Point)x.Info == position);
        }

        private void GameOver()
        {
            isGameOver = true;
            Pause();

            new Label(ContentHandler.Instance.HoboStdSpriteFont, "GAME OVER!", Vector2.Zero, UiHelper.DarkBackgroundColor)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.Center)
                .SetScale(1.8f)
                .AddToScreen(board);
        }

        private void OnBoardClick(ControlEventArgs args)
        {
            if (isGameOver)
                ChangeScreen(new MainMenuScreen());
            else
                PauseOrUnpause();
        }

        private void PauseOrUnpause()
        {
            if (isPaused)
                UnPause();
            else
                Pause();
        }

        private void Pause()
        {
            isPaused = true;
            moveSnakeDelayTime.Stop();
            foodRotationAnimation?.Stop();
            foodScaleAnimation?.Stop();
            SetGameControlsTransparency(gamePausedTransparency);
        }

        private void UnPause()
        {
            isPaused = false;
            moveSnakeDelayTime.Play();
            foodRotationAnimation?.Play();
            foodScaleAnimation?.Play();
            SetGameControlsTransparency(1f);
        }

        private void SetGameControlsTransparency(float transparency)
        {
            snakeBlocksRectangles.ToList().ForEach(x => x.SetColor(Color.White * transparency));
            foodImage.SetColor(UiHelper.DarkBackgroundColor * transparency);
        }

        private void DisplayCountDownInfo(int number)
        {
            var text = number == 0 ? "GO!" : number.ToString();
            var labelTextInfo = new Label(ContentHandler.Instance.HoboStdSpriteFont, text, Vector2.Zero, UiHelper.DarkBackgroundColor)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.Center)
                .SetScale(1.8f)
                .AddToScreen(board);

            var effectTime = 0.6f;
            var scaleAnimation = new ScaleAnimation(labelTextInfo, effectTime, new Vector2(4f))
                .Play()
                .SetShouldRemoveControlOnAnimationEnd(true)
                .AddOnAnimationEnd(() =>
                {
                    DisplayCountDownInfo(number - 1);
                });


            var fadeAnimationTime = 0.15f;
            new DelayTime(effectTime - fadeAnimationTime, () =>
                {
                    if (number == 0)
                    {
                        scaleAnimation.Stop();
                        scaleAnimation = new ScaleAnimation(labelTextInfo, fadeAnimationTime, new Vector2(20f))
                            .Play()
                            .SetShouldRemoveControlOnAnimationEnd(true)
                            .AddOnAnimationEnd(InitiateSnake);
                    }

                    new FadeAnimation(labelTextInfo, fadeAnimationTime, 0f)
                        .Play();
                })
                .Play();
        }
    }
}
