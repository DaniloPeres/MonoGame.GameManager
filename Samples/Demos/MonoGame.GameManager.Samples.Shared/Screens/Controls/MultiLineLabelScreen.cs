using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Screens;
using System.Collections.Generic;
using System;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Services;
using Microsoft.Xna.Framework;
using MonoGame.GameManager.Enums;
using static System.Net.Mime.MediaTypeNames;
using MonoGame.GameManager.Controls.Interfaces;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    public class MultiLineLabelScreen : Screen
    {
        int sectionTop = Config.ScreenContentMargin + 60;
        int sectionHeight = 690;
        int sectionDivisionLeft = 550;
        RectangleControl rectangleTextBoxPreview;
        MultiLineLabel multiLineLabelPreview;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Multi-line Label", OpenMultiLineLabelScreen)
            });

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionTop, 2, sectionHeight), Color.White)
                .AddToScreen();

            base.OnInit();
        }

        public static void OpenMultiLineLabelScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new MultiLineLabelScreen());
        }

        private void CreatePreviewSection()
        {
            var container = new Panel(new Rectangle(sectionDivisionLeft + Config.ScreenContentMargin, sectionTop, 600, sectionHeight))
                .AddToScreen();

            new Label(ContentHandler.Instance.SpriteFontArial, "Preview", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            const int labelContainerMarginTop = 50;
            var labelContainer = new Panel(new Rectangle(0, labelContainerMarginTop, (int)container.Size.X, (int)(container.Size.Y - labelContainerMarginTop)))
                .AddToScreen(container);

            new RectangleControl(Vector2.Zero, labelContainer.Size, new Color(15, 15, 15))
                .AddToScreen(labelContainer);

            rectangleTextBoxPreview = new RectangleControl(Vector2.Zero, Vector2.Zero, Color.Green * 0.1f)
                .AddToScreen(labelContainer);

            var loremIpsumText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            multiLineLabelPreview = new MultiLineLabel(ContentHandler.Instance.SpriteFontArial, $"My long text example:\n{loremIpsumText}", Vector2.Zero, Color.White, 500)
                .AddToScreen(labelContainer)
                .SetAnchor(Enums.Anchor.Center)
                .AddOnUpddateDestinationRectangle(UpdateRectangleTextBoxPreview);
        }

        private void CreateOptionsSection()
        {
            var container = new Panel(new Rectangle(Config.ScreenContentMargin, sectionTop, sectionDivisionLeft - (Config.ScreenContentMargin * 2), sectionHeight))
                .AddToScreen();

            var labelOptions = new Label(ContentHandler.Instance.SpriteFontArial, "Options", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            var optionMarginTop = 10;

            var posY = labelOptions.Size.Y + optionMarginTop;

            ColorOption.CreateColorOption(container, posY, color => multiLineLabelPreview.SetColor(color));
            posY += 55;
            AnchorOption.CreateAnchorOption(container, posY, anchor => multiLineLabelPreview.SetAnchor(anchor));
            posY += 155;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Scale", posY, 1f, scale => multiLineLabelPreview.SetScale(scale));
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Position", posY, new Vector2(0), newPosition => multiLineLabelPreview.SetPosition(newPosition));
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Textbox Width", posY, multiLineLabelPreview.TextBoxWidth, value => multiLineLabelPreview.TextBoxWidth = (int)value, 5);
            posY += 55;
            AddTextAlignOption(container, posY, textAlign => multiLineLabelPreview.TextAlign = textAlign);
            posY += 55;
            LastEventsInfo.AddLastEventsInfo(container, posY, multiLineLabelPreview);
        }

        private void AddTextAlignOption(IContainer container, float posY, Action<TextAlign> onTextAlignSelected)
        {
            var marginLeft = 10;
            var posX = 0f;

            var textAlignLabel = new Label(ContentHandler.Instance.SpriteFontArial, $"Text align: ", new Vector2(posX, posY), Color.Yellow)
                .AddToScreen(container);
            posX += textAlignLabel.Size.X + marginLeft;

            TextAlign[] textAligns = { TextAlign.Left, TextAlign.Center, TextAlign.Right };

            var squareSize = new Vector2(118, 40);
            var squareMargin = 5;

            for (var i = 0; i < textAligns.Length; i++)
            {
                var textAlign = textAligns[i];
                var pos = new Vector2(posX, posY) + (squareSize + new Vector2(squareMargin)) * new Vector2(i, 0);
                var textAlignContainer = new Panel(pos, squareSize)
                    .AddToScreen(container);

                new RectangleControl(Vector2.Zero, textAlignContainer.Size, Color.DarkGray)
                    .AddToScreen(textAlignContainer)
                    .SetMouseEventsColor(Color.Gray, Color.DarkSlateGray)
                    .AddOnClick(args =>
                    {
                        onTextAlignSelected(textAlign);
                    });

                new Label(ContentHandler.Instance.SpriteFontArial, textAlign.ToString(), Vector2.Zero, Color.White)
                    .AddToScreen(textAlignContainer)
                    .SetScale(0.65f)
                    .SetAnchor(Anchor.Center);
            }
        }

        private void UpdateRectangleTextBoxPreview()
        {
            rectangleTextBoxPreview
                .SetSize(new Vector2(multiLineLabelPreview.TextBoxWidth, multiLineLabelPreview.DestinationRectangle.Height))
                .SetAnchor(multiLineLabelPreview.Anchor)
                .SetPosition(multiLineLabelPreview.PositionAnchor);
        }
    }
}
