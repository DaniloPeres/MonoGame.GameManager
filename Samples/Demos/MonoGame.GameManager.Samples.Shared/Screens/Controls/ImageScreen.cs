using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using System.Collections.Generic;
using System;
using MonoGame.GameManager.Services;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    public class ImageScreen : Screen
    {
        int sectionTop = Config.ScreenContentMargin + 60;
        int sectionHeight = 690;
        int sectionDivisionLeft = 550;
        Image imagePreview;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Images", OpenImageScreen)
            });

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionTop, 2, sectionHeight), Color.White)
                .AddToScreen();

            base.OnInit();
        }

        public static void OpenImageScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new ImageScreen());
        }

        private void CreateOptionsSection()
        {
            var container = new Panel(new Rectangle(Config.ScreenContentMargin, sectionTop, sectionDivisionLeft - (Config.ScreenContentMargin * 2), sectionHeight))
                .AddToScreen();

            var labelOptions = new Label(ContentHandler.Instance.SpriteFontArial, "Options", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            var optionMarginTop = 10;
            var spaceBetweenRows = 50;

            var posY = labelOptions.Size.Y + optionMarginTop;

            ColorOption.CreateColorOption(container, posY, color => imagePreview.SetColor(color));
            posY += spaceBetweenRows;
            AnchorOption.CreateAnchorOption(container, posY, anchor => imagePreview.SetAnchor(anchor));
            posY += 100 + spaceBetweenRows;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Scale", posY, 1f, scale => imagePreview.SetScale(scale));
            posY += spaceBetweenRows;
            Vector2Option.CreateVector2Option(container, "Position", posY, new Vector2(0), newPosition => imagePreview.SetPosition(newPosition));
            posY += spaceBetweenRows;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Rotation", posY, 0, newRotation => imagePreview.SetRotation(newRotation), 0.05f);
            posY += spaceBetweenRows;
            Vector2Option.CreateVector2Option(container, "Origin Rate", posY, new Vector2(0), newOrigin => imagePreview.SetOriginRate(newOrigin), 0.1f);
            posY += spaceBetweenRows;
            AddImagesOption(container, posY, newTexture => imagePreview.Texture = newTexture);
            posY += spaceBetweenRows;
            CheckboxOption.CreateCheckboxOption(container, "Ignore transparent pixels", posY, false, ignoreTransparentPixel => imagePreview.IgnoreIntersectionTransparentPixels = ignoreTransparentPixel);
            posY += spaceBetweenRows;
            LastEventsInfo.AddLastEventsInfo(container, posY, imagePreview);
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

            imagePreview = new Image(ContentHandler.Instance.TextureImage)
                .AddToScreen(imageContainer)
                .SetAnchor(Enums.Anchor.Center);
        }
    }
}
