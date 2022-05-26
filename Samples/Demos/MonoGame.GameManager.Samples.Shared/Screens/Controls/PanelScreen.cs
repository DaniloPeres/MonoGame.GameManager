using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    public class PanelScreen : Screen
    {
        int sectionTop = Config.ScreenContentMargin + 60;
        int sectionHeight = 690;
        int sectionDivisionLeft = 550;
        RectangleControl backgroundPanelRectangle;
        Panel panelPreview;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Panel", OpenPanelScreen)
            });

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionTop, 2, sectionHeight), Color.White)
                .AddToScreen();

            base.OnInit();
        }

        public static void OpenPanelScreen()
            => ServiceProvider.ScreenManager.ChangeScreen(new PanelScreen());

        private void CreateOptionsSection()
        {
            var container = new Panel(new Rectangle(Config.ScreenContentMargin, sectionTop, sectionDivisionLeft - (Config.ScreenContentMargin * 2), sectionHeight))
                .AddToScreen();

            var labelOptions = new Label(ContentHandler.Instance.SpriteFontArial, "Options", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            var optionMarginTop = 10;

            var posY = labelOptions.Size.Y + optionMarginTop;

            AnchorOption.CreateAnchorOption(container, posY, anchor => panelPreview.SetAnchor(anchor));
            posY += 155;
            Vector2Option.CreateVector2Option(container, "Position", posY, new Vector2(0), newPosition => panelPreview.SetPosition(newPosition));
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Size", posY, panelPreview.Size, newSize => panelPreview.SetSize(newSize));
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Scale", posY, panelPreview.Scale, scale => panelPreview.SetScale(scale), 0.1f);
            posY += 55;
            CheckboxOption.CreateCheckboxOption(container, "Hide Overflow (Parent)", posY, false, hideOverflow => panelPreview.Parent.HideOverflow = hideOverflow);
            posY += 55;
            LastEventsInfo.AddLastEventsInfo(container, posY, panelPreview);
        }

        private void CreatePreviewSection()
        {
            var container = new Panel(new Rectangle(sectionDivisionLeft + Config.ScreenContentMargin, sectionTop, 600, sectionHeight))
                .AddToScreen();

            new Label(ContentHandler.Instance.SpriteFontArial, "Preview", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            const int labelContainerMarginTop = 50;
            var panelContainer = new Panel(new Rectangle(0, labelContainerMarginTop, (int)container.Size.X, (int)(container.Size.Y - labelContainerMarginTop)))
                .AddToScreen(container);

            new RectangleControl(Vector2.Zero, panelContainer.Size, new Color(15, 15, 15))
                .AddToScreen(panelContainer);

            panelPreview = new Panel(new Rectangle(0, 0, 300, 200))
                .AddToScreen(panelContainer)
                .SetAnchor(Enums.Anchor.Center)
                .AddOnUpddateDestinationRectangle(UpdatePanelBackgroundRectangle);

            new Label(ContentHandler.Instance.SpriteFontArial, "My panel text example", Vector2.Zero, Color.LightCyan)
                .AddToScreen(panelPreview)
                .SetScale(0.75f)
                .SetMouseEventsColor(Color.Blue, Color.Red)
                .SetPosition(new Vector2(15));

            new Image(ContentHandler.Instance.TextureImage)
                .AddToScreen(panelPreview)
                .SetPosition(new Vector2(15, 15))
                .SetAnchor(Enums.Anchor.BottomRight);

            var myButton = new Button(ContentHandler.Instance.TextureButtonBackground, Vector2.Zero)
                .AddToScreen(panelPreview)
                .SetBackgroundScale(new Vector2(1f, 1.2f))
                .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
                .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed)
                .SetPosition(new Vector2(15, 15))
                .SetAnchor(Enums.Anchor.BottomLeft);

            new Label(ContentHandler.Instance.SpriteFontArial, "MY BUTTON", Vector2.Zero, Color.White)
                .AddToScreen(myButton)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);

            backgroundPanelRectangle = new RectangleControl(Vector2.Zero, Vector2.Zero, Color.Green * 0.1f)
                .AddToScreen(panelPreview);

        }

        private void UpdatePanelBackgroundRectangle()
            => backgroundPanelRectangle.SetSize(panelPreview.Size / panelPreview.NestedScale);
    }
}
