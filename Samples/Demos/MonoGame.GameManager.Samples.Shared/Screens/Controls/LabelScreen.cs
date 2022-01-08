using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Screens;
using System.Collections.Generic;
using System;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Samples.Services;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    public class LabelScreen : Screen
    {
        int sectionTop = Config.ScreenContentMargin + 60;
        int sectionHeight = 690;
        int sectionDivisionLeft = 550;
        Label labelPreview;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Label", OpenLabelScreen)
            });

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionTop, 2, sectionHeight), Color.White)
                .AddToScreen();

            base.OnInit();
        }

        public static void OpenLabelScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new LabelScreen());
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

            ColorOption.CreateColorOption(container, posY, color => labelPreview.SetColor(color));
            posY += 55;
            AnchorOption.CreateAnchorOption(container, posY, anchor => labelPreview.SetAnchor(anchor));
            posY += 155;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Scale", posY, 1f, scale => labelPreview.SetScale(scale));
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Position", posY, new Vector2(0), newPosition => labelPreview.SetPosition(newPosition));
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Rotation", posY, 0, newRotation => labelPreview.SetRotation(newRotation), 0.05f);
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Origin Rate", posY, new Vector2(0), newOrigin => labelPreview.SetOriginRate(newOrigin), 0.1f);
            posY += 55;
            LastEventsInfo.AddLastEventsInfo(container, posY, labelPreview);
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

            labelPreview = new Label(ContentHandler.Instance.SpriteFontArial, "My text example", Vector2.Zero, Color.White)
                .AddToScreen(labelContainer)
                .SetAnchor(Enums.Anchor.Center);
        }
    }
}
