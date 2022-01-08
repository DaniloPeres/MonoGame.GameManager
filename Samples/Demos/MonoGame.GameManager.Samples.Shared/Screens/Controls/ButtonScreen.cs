using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Screens;
using System.Collections.Generic;
using System;
using MonoGame.GameManager.Services;
using Microsoft.Xna.Framework;
using MonoGame.GameManager.Samples.Services;
using System.ComponentModel;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    public class ButtonScreen : Screen
    {
        int sectionTop = Config.ScreenContentMargin + 60;
        int sectionDivisionLeft = 550;
        int sectionHeight = 690;
        int sectionDivisionBottom = 300;
        int sectionWidth = 1170;
        Button buttonPreview;
        Image buttonIconPreview;
        Label buttonTextPreview;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Button", OpenButtonScreen)
            });

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionTop, 2, sectionDivisionBottom - sectionTop), Color.White)
                .AddToScreen();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionDivisionBottom, sectionWidth - sectionDivisionLeft, 2), Color.White)
                .AddToScreen();

            base.OnInit();
        }

        public static void OpenButtonScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new ButtonScreen());
        }

        private void CreatePreviewSection()
        {
            var container = new Panel(new Rectangle(sectionDivisionLeft + Config.ScreenContentMargin, sectionTop, sectionWidth - sectionDivisionLeft - Config.ScreenContentMargin, sectionDivisionBottom - sectionTop - 15))
                .AddToScreen();

            new Label(ContentHandler.Instance.SpriteFontArial, "Preview", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            const int buttonContainerMarginTop = 50;
            var buttonContainer = new Panel(new Rectangle(0, buttonContainerMarginTop, (int)container.Size.X, (int)(container.Size.Y - buttonContainerMarginTop)))
                .AddToScreen(container);

            new RectangleControl(Vector2.Zero, buttonContainer.Size, new Color(15, 15, 15))
                .AddToScreen(buttonContainer);

            buttonPreview = new Button(ContentHandler.Instance.TextureButtonBackground, Vector2.Zero)
                .AddToScreen(buttonContainer)
                .SetScale(new Vector2(1.3f, 1.2f))
                .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
                .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed)
                .SetAnchor(Enums.Anchor.Center);

            buttonIconPreview = new Image(ContentHandler.Instance.TextureCamera)
                .AddToScreen(buttonPreview)
                .SetScale(0.5f)
                .SetAnchor(Enums.Anchor.CenterLeft)
                .SetPosition(new Vector2(15, 0));

            buttonTextPreview = new Label(ContentHandler.Instance.SpriteFontArial, "My button", new Vector2(25, 0), Color.White)
                .AddToScreen(buttonPreview)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);
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

            AnchorOption.CreateAnchorOption(container, posY, anchor => buttonPreview.SetAnchor(anchor));
            posY += 155;
            Vector2Option.CreateVector2Option(container, "Scale", posY, buttonPreview.Scale, scale => buttonPreview.SetScale(scale), 0.1f);
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Position", posY, new Vector2(0), scale => buttonPreview.SetPosition(scale), 1);
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Rotation", posY, 0, newRotation => buttonPreview.SetRotation(newRotation), 0.05f);
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Origin Rate", posY, new Vector2(0), newOrigin => buttonPreview.SetOriginRate(newOrigin), 0.1f);
            posY += 55;
            CheckboxOption.CreateCheckboxOption(container, "Display Icon", posY, true, displayIcon =>
            {
                if (displayIcon)
                    buttonIconPreview.AddToScreen(buttonPreview);
                else
                    buttonIconPreview.RemoveFromScreen();
            });
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Icon Position", posY, buttonIconPreview.PositionAnchor, newOrigin => buttonIconPreview.SetPosition(newOrigin));
            posY += 55;
            AnchorOption.CreateAnchorOption(container, posY, anchor => buttonIconPreview.SetAnchor(anchor), "Icon\nAnchor");

            container = new Panel(new Rectangle(sectionDivisionLeft + Config.ScreenContentMargin *  2, sectionDivisionBottom + Config.ScreenContentMargin, sectionWidth - sectionDivisionLeft, sectionHeight - sectionDivisionLeft - Config.ScreenContentMargin))
                .AddToScreen();
            posY = 0;
            CheckboxOption.CreateCheckboxOption(container, "Display Text", posY, true, displayIcon =>
            {
                if (displayIcon)
                    buttonTextPreview.AddToScreen(buttonPreview);
                else
                    buttonTextPreview.RemoveFromScreen();
            });
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Text Position", posY, buttonTextPreview.PositionAnchor, newOrigin => buttonTextPreview.SetPosition(newOrigin));
            posY += 55;
            AnchorOption.CreateAnchorOption(container, posY, anchor => buttonTextPreview.SetAnchor(anchor), "Text\nAnchor");
            posY += 155;
            LastEventsInfo.AddLastEventsInfo(container, posY, buttonPreview);
        }
    }
}
