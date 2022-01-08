using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.Extensions;
using MonoGame.GameManager.GameMath;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using System.Collections.Generic;
using System.Linq;

namespace Snake.Screens
{
    public class MainMenuScreen : Screen
    {
        private Panel board;

        public override void OnInit()
        {
            board = UiHelper.CreatePanelBoard();

            AddTitle();
            AddChooseLevel();

            base.OnInit();
        }

        public void AddTitle()
        {
            new Label(ContentHandler.Instance.HoboStdSpriteFont, "SNAKE", new Vector2(0, 60), UiHelper.DarkBackgroundColor)
                .SetAnchor(Anchor.TopCenter)
                .SetScale(1.25f)
                .AddToScreen(board);

            CreateFood(Anchor.TopLeft);
            CreateFood(Anchor.TopRight);
        }

        private void CreateFood(Anchor anchor)
        {
            var food = new Image(ContentHandler.Instance.TextureFood)
                .SetPosition(80, 76)
                .SetOriginRate(0.5f)
                .SetColor(UiHelper.DarkBackgroundColor)
                .SetAnchor(anchor)
                .AddToScreen(board);
            new RotationAnimation(food, UiHelper.RotationAnimationTime, 360f)
                .SetIsLooping(true)
                .Play();
        }

        public void AddChooseLevel()
        {
            new Label(ContentHandler.Instance.HoboStdSpriteFont, "CHOOSE LEVEL:", new Vector2(0, 170), UiHelper.DarkBackgroundColor)
               .SetAnchor(Anchor.TopCenter)
               .SetScale(0.6f)
               .AddToScreen(board);
            AddPlayChooses();
        }

        private void AddPlayChooses()
        {
            var posY = 220;
            var scale = 0.7f;

            var options = new List<string>
            {
                "EASY",
                "MEDIUM",
                "HARD"
            };

            var labels = new List<Label>();
            for (var i = 0; i < options.Count; i++)
            {
                labels.Add(new Label(ContentHandler.Instance.HoboStdSpriteFont, options[i], Vector2.Zero, UiHelper.DarkBackgroundColor)
                    .SetInfo(i) // save the level difficulty in the info of label
                    .SetScale(scale));
            }

            var panelPadding = 10;
            var panelHeight = labels.Max(lbl => lbl.Size.Y) + (panelPadding * 2);

            var textureBackgrounds = new List<Texture2D>();
            for (var i = 0; i < options.Count; i++)
            {
                textureBackgrounds.Add(ShaderEffects.CreateRoudedRectangle(6f, new Vector2(labels[i].Size.X + (panelPadding * 2), panelHeight).ToPoint(), UiHelper.DarkBackgroundColor, ServiceProvider.GraphicsDevice));
            }

            var totalWidth = textureBackgrounds.Sum(texture => texture.Width);
            var posX = PositionCalculations.CenterHorizontal(totalWidth);

            for (var i = 0; i < options.Count; i++)
            {
                var optionPanel = new Panel(new Vector2(posX, posY), textureBackgrounds[i].Size().ToVector2())
                    .AddToScreen();
                var imgBackground = new Image(textureBackgrounds[i])
                    .AddToScreen(optionPanel)
                    .SetColor(Color.Transparent);
                var label = labels[i];
                label
                    .AddToScreen(optionPanel)
                    .SetPosition(0, 3)
                    .SetColor(UiHelper.DarkBackgroundColor)
                    .SetAnchor(Anchor.Center)
                    .SetZIndex(2);
                posX += textureBackgrounds[i].Width;

                optionPanel
                    .AddOnMouseEnter(args =>
                    {
                        imgBackground.Color = Color.White;
                        label.Color = UiHelper.LightBackgroundColor;
                    })
                    .AddOnMouseLeave(args =>
                    {
                        imgBackground.Color = Color.Transparent;
                        label.Color = UiHelper.DarkBackgroundColor;
                    })
                    .AddOnClick(args =>
                    {
                        ChangeScreen(new SnakeGameScreen((int)label.Info));
                    });
            }

        }
    }
}
