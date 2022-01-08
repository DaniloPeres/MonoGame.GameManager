using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using System.Collections.Generic;
using System;
using MonoGame.GameManager.Controls;
using Microsoft.Xna.Framework;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Samples.ScreenComponents;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    public class RectangleScreen : Screen
    {
        int sectionTop = Config.ScreenContentMargin + 60;
        int sectionHeight = 690;
        int sectionDivisionLeft = 550;
        RectangleControl rectangleControlPreview;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Rectangle", OpenRectangleControlScreen)
            });

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionTop, 2, sectionHeight), Color.White)
                .AddToScreen();

            base.OnInit();
        }

        public static void OpenRectangleControlScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new RectangleScreen());
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

            ColorOption.CreateColorOption(container, posY, color => rectangleControlPreview.SetColor(color));
            posY += 55;
            AnchorOption.CreateAnchorOption(container, posY, anchor => rectangleControlPreview.SetAnchor(anchor));
            posY += 155;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Scale", posY, 1f, scale => rectangleControlPreview.SetScale(scale));
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Position", posY, new Vector2(0), newPosition => rectangleControlPreview.SetPosition(newPosition));
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Size", posY, rectangleControlPreview.Size, newSize => rectangleControlPreview.SetSize(newSize));
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Rotation", posY, 0, newRotation => rectangleControlPreview.SetRotation(newRotation), 0.05f);
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Origin Rate", posY, new Vector2(0), newOrigin => rectangleControlPreview.SetOriginRate(newOrigin), 0.1f);
            posY += 55;
            LastEventsInfo.AddLastEventsInfo(container, posY, rectangleControlPreview);
        }

        private void AddImagesOption(Panel container, float posY, Action<Texture2D> onTextureSelected)
        {
            var marginLeft = 10;
            var posX = 0f;

            var imageLabel = new Label(ContentHandler.Instance.SpriteFontArial, "Texture: ", new Vector2(posX, posY), Color.Yellow)
                .AddToScreen(container);

            posX += imageLabel.Size.X + marginLeft;

            var imageOption = new Image(ContentHandler.Instance.TextureImage)
                .AddToScreen(container)
                .SetScale(0.55f)
                .SetPosition(new Vector2(posX, posY))
                .AddOnClick(args => onTextureSelected(ContentHandler.Instance.TextureImage));

            posX += imageOption.Size.X + marginLeft;

            imageOption = new Image(ContentHandler.Instance.TextureCalculator)
                .AddToScreen(container)
                .SetScale(0.38f)
                .SetPosition(new Vector2(posX, posY))
                .AddOnClick(args => onTextureSelected(ContentHandler.Instance.TextureCalculator));

            posX += imageOption.Size.X + marginLeft;

            imageOption = new Image(ContentHandler.Instance.TextureCamera)
                .AddToScreen(container)
                .SetScale(0.6f)
                .SetPosition(new Vector2(posX, posY))
                .AddOnClick(args => onTextureSelected(ContentHandler.Instance.TextureCamera));

            posX += imageOption.Size.X + marginLeft;
        }

        private void CreatePreviewSection()
        {
            var container = new Panel(new Rectangle(sectionDivisionLeft + Config.ScreenContentMargin, sectionTop, 600, sectionHeight))
                .AddToScreen();

            new Label(ContentHandler.Instance.SpriteFontArial, "Preview", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            const int imageContainerMarginTop = 50;
            var imageContainer = new Panel(new Rectangle(0, imageContainerMarginTop, (int)container.Size.X, (int)(container.Size.Y - imageContainerMarginTop)))
                .AddToScreen(container);

            new RectangleControl(Vector2.Zero, imageContainer.Size, new Color(15, 15, 15))
                .AddToScreen(imageContainer);

            rectangleControlPreview = new RectangleControl(new Rectangle(0, 0, 200, 150), Color.White)
                .AddToScreen(imageContainer)
                .SetAnchor(Enums.Anchor.Center);
        }
    }
}
