using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using System.Collections.Generic;
using System;
using MonoGame.GameManager.Controls;
using Microsoft.Xna.Framework;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Controls.Sprites;
using System.Linq;
using MonoGame.GameManager.Animations;

namespace MonoGame.GameManager.Samples.Screens.Controls
{
    public class ScrollViewerScreen : Screen
    {
        const int sectionTop = Config.ScreenContentMargin + 60;
        const int sectionHeight = 690;
        const int sectionDivisionLeft = 560;
        ScrollViewer scrollViewerPreview;
        RectangleControl backgroundPanelRectangle;
        readonly SpriteAnimationInfo coinSpriteAnimationInfo;
        readonly List<List<Panel>> scrollviewerColumnsContainers = new List<List<Panel>>();
        private readonly Vector2 containerSize = new Vector2(500, 390);
        private Action<Vector2> updateScrollPositionValues;
        private Action<Vector2> updateZoomValues;
        private Panel scrollViewerOptionsContainer;
        private Panel pinchZoomOptionsContainer;

        public ScrollViewerScreen()
        {
            coinSpriteAnimationInfo = ServiceProvider.ContentLoaderManager.LoadSpriteAnimationInfo("Images/Sprites/Coin.sa");
        }

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Scroll Viewer", OpenScrollViewerScreen)
            });

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionTop, 2, sectionHeight), Color.White)
                .AddToScreen();

            base.OnInit();
        }

        public static void OpenScrollViewerScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new ScrollViewerScreen());
        }

        private void CreateOptionsSection()
        {
            var container = new Panel(new Rectangle(Config.ScreenContentMargin, sectionTop, sectionDivisionLeft - (Config.ScreenContentMargin * 2), sectionHeight))
                .SetHideOverflow(true)
                .AddToScreen();

            var labelOptions = new Label(ContentHandler.Instance.SpriteFontArial, "Options", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);


            CreateScroolViewerOptionsSection(container);
            CreatePinchZoomOptionsSection(container);

            var posY = 600;

            AddButton(container, 0, posY, "Add vertical", AddVertical);
            AddButton(container, 220, posY, "Remove vertical", RemoveVertical);
            posY += 50;
            AddButton(container, 0, posY, "Add horizontal", AddHorizontal);
            AddButton(container, 220, posY, "Remove horizontal", RemoveHorizontal);
        }

        private int CreateScroolViewerOptionsSection(Panel container)
        {
            scrollViewerOptionsContainer = new Panel(new Rectangle(0, 43, (int)container.Size.X, 560))
                .AddToScreen(container);

            var posY = 0;
            var spaceBetweenOptions = 50;
            AnchorOption.CreateAnchorOption(scrollViewerOptionsContainer, posY, anchor => scrollViewerPreview.SetAnchor(anchor));
            posY += 155;
            Vector2Option.CreateVector2Option(scrollViewerOptionsContainer, "Position", posY, new Vector2(0), newPosition => scrollViewerPreview.SetPosition(newPosition));
            posY += spaceBetweenOptions;
            Vector2Option.CreateVector2Option(scrollViewerOptionsContainer, "Size", posY, scrollViewerPreview.Size, newSize => scrollViewerPreview.SetSize(newSize), format: "{0:0}");
            posY += spaceBetweenOptions;
            CheckboxOption.CreateCheckboxOption(scrollViewerOptionsContainer, "Hide Overflow", posY, scrollViewerPreview.HideOverflow, hideOverflow => scrollViewerPreview.HideOverflow = hideOverflow);
            posY += spaceBetweenOptions;
            CheckboxOption.CreateCheckboxOption(scrollViewerOptionsContainer, "Vertical Scroll", posY, scrollViewerPreview.VerticalScrollEnabled, newValue => scrollViewerPreview.VerticalScrollEnabled = newValue);
            posY += spaceBetweenOptions;
            CheckboxOption.CreateCheckboxOption(scrollViewerOptionsContainer, "Horizontal Scroll", posY, scrollViewerPreview.HorizontalScrollEnabled, newValue => scrollViewerPreview.HorizontalScrollEnabled = newValue);
            posY += spaceBetweenOptions;
            CheckboxOption.CreateCheckboxOption(scrollViewerOptionsContainer, "Horizontal Scroll Bar", posY, scrollViewerPreview.ShowHorizontalScrollBar, hideOverflow => scrollViewerPreview.ShowHorizontalScrollBar = hideOverflow);

            var btnShowPinchZoomOptions = new Button(ContentHandler.Instance.TextureButtonBackground, new Vector2(5, 475))
                .AddToScreen(scrollViewerOptionsContainer)
                .SetAnchor(Enums.Anchor.TopRight)
                .AddOnClick(args => MoveToPinchZoomOptions())
                .SetBackgroundScale(new Vector2(1.15f, 1.5f))
                .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
                .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed);

            new Label(ContentHandler.Instance.SpriteFontArial, "Pinch zoom\noptions >", Vector2.Zero, Color.White)
                .AddToScreen(btnShowPinchZoomOptions)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);

            posY += spaceBetweenOptions;
            CheckboxOption.CreateCheckboxOption(scrollViewerOptionsContainer, "Vertical Scroll Bar", posY, scrollViewerPreview.ShowVerticalScrollBar, hideOverflow => scrollViewerPreview.ShowVerticalScrollBar = hideOverflow);
            posY += spaceBetweenOptions;



            return posY;
        }

        private void CreatePinchZoomOptionsSection(Panel container)
        {
            pinchZoomOptionsContainer = new Panel(new Rectangle((int)container.Size.X, 43, (int)container.Size.X, 560))
                .AddToScreen(container);

            var posY = 0;
            var spaceBetweenOptions = 50;

            updateScrollPositionValues = Vector2Option.CreateVector2Option(pinchZoomOptionsContainer, "Scroll Pos", posY, scrollViewerPreview.ScrollPosition, pos => scrollViewerPreview.SetScrollPosition(pos), 5);
            posY += spaceBetweenOptions;
            updateZoomValues = Vector2Option.CreateVector2Option(pinchZoomOptionsContainer, "Zoom", posY, scrollViewerPreview.Zoom, zoom => scrollViewerPreview.SetZoom(zoom), 0.1f);
            posY += spaceBetweenOptions;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(pinchZoomOptionsContainer, "Min Zoom", posY, scrollViewerPreview.MinZoom.X, newScale => scrollViewerPreview.SetMinZoom(new Vector2(newScale)));
            posY += spaceBetweenOptions;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(pinchZoomOptionsContainer, "Max Zoom", posY, scrollViewerPreview.MaxZoom.X, newScale => scrollViewerPreview.SetMaxZoom(new Vector2(newScale)));
            posY += spaceBetweenOptions;

            scrollViewerPreview.AddOnZoomChanged(OnScrollViewerZoomChanged);
            scrollViewerPreview.AddOnScrollPositionChanged(OnScrollViewerScrollPositionChanged);

            var btnShowPinchZoomOptions = new Button(ContentHandler.Instance.TextureButtonBackground, new Vector2(5, 475))
                .AddToScreen(pinchZoomOptionsContainer)
                .SetAnchor(Enums.Anchor.TopRight)
                .AddOnClick(args => MoveToScrollViewerOptions())
                .SetBackgroundScale(new Vector2(1.15f, 1.5f))
                .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
                .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed);

            new Label(ContentHandler.Instance.SpriteFontArial, "Scroll viewer\noptions <", Vector2.Zero, Color.White)
                .AddToScreen(btnShowPinchZoomOptions)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);
        }

        private void OnScrollViewerScrollPositionChanged()
            // transform to point and after to vector to display only integer numbers
            => updateScrollPositionValues(scrollViewerPreview.ScrollPosition.ToPoint().ToVector2());

        private void OnScrollViewerZoomChanged()
            => updateZoomValues(scrollViewerPreview.Zoom);

        private void AddButton(Panel container, float posX, float posY, string text, Action onClick)
        {
            var myButton = new Button(ContentHandler.Instance.TextureButtonBackground, new Vector2(posX, posY))
                .AddToScreen(container)
                .AddOnClick(args => onClick())
                .SetBackgroundScale(new Vector2(1.45f, 1f))
                .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
                .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed);

            new Label(ContentHandler.Instance.SpriteFontArial, text, Vector2.Zero, Color.White)
                .AddToScreen(myButton)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);
        }

        private void MoveToScrollViewerOptions()
        {
            var duration = 0.15f;
            new EaseAnimation(scrollViewerOptionsContainer, duration, new Vector2(0, scrollViewerOptionsContainer.PositionAnchor.Y))
                .Play();
            new EaseAnimation(pinchZoomOptionsContainer, duration, new Vector2(scrollViewerOptionsContainer.Size.X, pinchZoomOptionsContainer.PositionAnchor.Y))
                .Play();
        }

        private void MoveToPinchZoomOptions()
        {
            var duration = 0.15f;
            new EaseAnimation(scrollViewerOptionsContainer, duration, new Vector2(-scrollViewerOptionsContainer.Size.X, scrollViewerOptionsContainer.PositionAnchor.Y))
                .Play();
            new EaseAnimation(pinchZoomOptionsContainer, duration, new Vector2(0, pinchZoomOptionsContainer.PositionAnchor.Y))
                .Play();
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

            backgroundPanelRectangle = new RectangleControl(Vector2.Zero, Vector2.Zero, Color.Green * 0.1f)
                .AddToScreen(panelContainer);

            scrollViewerPreview = new ScrollViewer(Vector2.Zero, panelContainer.Size)
                .AddToScreen(panelContainer)
                .SetHideOverflow(true)
                .AddOnUpddateDestinationRectangle(UpdatePanelBackgroundRectangle)
                .SetHorizontalScrollEnabled(true)
                .SetAnchor(Enums.Anchor.TopLeft);

            AddVertical();
            AddVertical();
            AddVertical();
            AddHorizontal();
        }

        private void UpdatePanelBackgroundRectangle()
        {
            backgroundPanelRectangle
                .SetSize(scrollViewerPreview.Size / scrollViewerPreview.NestedScale)
                .SetPosition(scrollViewerPreview.PositionAnchor)
                .SetScale(scrollViewerPreview.Scale)
                .SetAnchor(scrollViewerPreview.Anchor);
        }

        private void AddHorizontal()
        {
            var newColumn = new List<Panel>();

            for (var i = 0; i < scrollviewerColumnsContainers.First().Count; i++)
            {
                newColumn.Add(CreateScrollViewerSection(scrollviewerColumnsContainers.Count, i).AddToScreen(scrollViewerPreview));
            }

            scrollviewerColumnsContainers.Add(newColumn);
        }

        private void RemoveHorizontal()
        {
            if (scrollviewerColumnsContainers.Count <= 1)
                return;

            var lastRow = scrollviewerColumnsContainers.Last();
            scrollviewerColumnsContainers.Remove(lastRow);
            lastRow.ForEach(x => x.RemoveFromScreen());
        }

        private void AddVertical()
        {
            if (!scrollviewerColumnsContainers.Any())
                scrollviewerColumnsContainers.Add(new List<Panel>());

            for (var i = 0; i < scrollviewerColumnsContainers.Count; i++)
            {
                scrollviewerColumnsContainers[i].Add(CreateScrollViewerSection(i, scrollviewerColumnsContainers[i].Count).AddToScreen(scrollViewerPreview));
            }
        }

        private void RemoveVertical()
        {
            for (var i = 0; i < scrollviewerColumnsContainers.Count; i++)
            {
                var column = scrollviewerColumnsContainers[i];

                if (column.Count <= 1)
                    continue;

                var lastPanel = column.Last();
                column.Remove(lastPanel);
                lastPanel.RemoveFromScreen();
            }
        }

        private Panel CreateScrollViewerSection(int totalColumns, int totalRows)
        {
            var container = new Panel(new Vector2(totalColumns, totalRows) * (containerSize + new Vector2(10)), containerSize);

            new Label(ContentHandler.Instance.SpriteFontArial, "This is my scroll viewer content example", new Vector2(5, 0), Color.Yellow)
                .SetScale(0.9f)
                .AddToScreen(container);

            var myButton = new Button(ContentHandler.Instance.TextureButtonBackground, Vector2.Zero)
                .AddToScreen(container)
                .SetBackgroundScale(new Vector2(1f, 1.2f))
                .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
                .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed)
                .SetPosition(new Vector2(15, 60));

            new Label(ContentHandler.Instance.SpriteFontArial, "MY BUTTON", Vector2.Zero, Color.White)
                .AddToScreen(myButton)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);

            new Image(ContentHandler.Instance.TextureImage)
                .AddToScreen(container)
                .SetAnchor(Enums.Anchor.TopCenter)
                .SetPosition(new Vector2(25, 50));

            coinSpriteAnimationInfo
                .CreateSpriteAnimation()
                .SetScale(0.8f)
                .SetAnchor(Enums.Anchor.TopRight)
                .AddToScreen(container)
                .Play()
                .SetPosition(25, 45);

            var loremIpsumText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            new MultiLineLabel(ContentHandler.Instance.SpriteFontArial, loremIpsumText, new Vector2(5, 150), Color.White, (int)container.Size.X - 10)
                .AddToScreen(container)
                .SetScale(0.7f);

            return container;
        }
    }
}
