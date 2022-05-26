using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Samples.Services;
using System;
using MonoGame.GameManager.Samples.Screens.Controls;
using MonoGame.GameManager.Samples.ScreenComponents;
using System.Collections.Generic;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Samples.Screens.Animations;
using MonoGame.GameManager.Samples.Animations;
using MonoGame.GameManager.Samples.Screens.ScreensInfo;

namespace MonoGame.GameManager.Samples.Screens
{
    public class MainScreen : Screen
    {
        private Texture2D
            textureButtonDefault,
            textureButtonHover,
            textureButtonPressed;

        private const int MarginOptions = 10;
        const int MarginButtonsFromTitle = 40;

        public override void LoadContent()
        {
            base.LoadContent();

            textureButtonDefault = CreateButtonTexture(new Color(150, 150, 150), new Color(200, 200, 200));
            textureButtonHover = CreateButtonTexture(new Color(100, 106, 149), new Color(160, 166, 210));
            textureButtonPressed = CreateButtonTexture(new Color(34, 52, 68), new Color(20, 30, 70));
        }

        public override void OnInit()
        {
            var sectionMargin = 20;

            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", OpenMainScreen)
            });

            //Controls section
            var posY = 58;
            AddTitle(posY, "Controls");
            AddControlsSection(ref posY);

            // Animations section
            posY += sectionMargin;
            AddTitle(posY, "Animations");
            AddAnimationsSection(ref posY);

            // Screen Sections
            posY += sectionMargin;
            AddTitle(posY, "Screen");
            AddScreenSection(ref posY);

            base.OnInit();
        }

        public override void Dispose()
        {
            textureButtonDefault.Dispose();
            textureButtonHover.Dispose();
            textureButtonPressed.Dispose();
            base.Dispose();
        }

        public static void OpenMainScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new MainScreen());
        }

        private void AddControlsSection(ref int posY)
        {
            posY += MarginButtonsFromTitle;

            var contentHandler = ContentHandler.Instance;

            var posX = Config.ScreenContentMargin;
            AddOptionButtonWithIcon(new Vector2(posX, posY), "Image", contentHandler.TextureImage, ImageScreen.OpenImageScreen);
            posX += textureButtonDefault.Width + MarginOptions;
            AddOptionButtonWithIcon(new Vector2(posX, posY), "Button", contentHandler.TextureButton, ButtonScreen.OpenButtonScreen);
            posX += textureButtonDefault.Width + MarginOptions;
            AddOptionButtonWithIcon(new Vector2(posX, posY), "Label", contentHandler.TextureTextLabel, LabelScreen.OpenLabelScreen);
            posX += textureButtonDefault.Width + MarginOptions;
            AddOptionButtonWithIcon(new Vector2(posX, posY), "Multi-line Labels", contentHandler.TextureMultiLineText, MultiLineLabelScreen.OpenMultiLineLabelScreen, -24);
            posX += textureButtonDefault.Width + MarginOptions;
            AddOptionButtonWithIcon(new Vector2(posX, posY), "Panel", contentHandler.TexturePanel, PanelScreen.OpenPanelScreen);
            posX += textureButtonDefault.Width + MarginOptions;
            AddSpriteAnimationButton(new Vector2(posX, posY));
            posX += textureButtonDefault.Width + MarginOptions;
            AddRectangleButton(new Vector2(posX, posY));
            posY += textureButtonDefault.Height + MarginOptions;
            posX = Config.ScreenContentMargin;
            AddScrollViewerButton(new Vector2(posX, posY));

            posY += textureButtonDefault.Height;
        }

        private void AddRectangleButton(Vector2 pos)
        {
            var button = AddOptionButton(pos, "Rectangle", RectangleScreen.OpenRectangleControlScreen);
            var scaleRectangle = new RectangleControl(new Rectangle(0, -10, 80, 50), Color.DarkBlue)
                .AddToScreen(button)
                .SetAnchor(Enums.Anchor.Center);
        }

        private void AddSpriteAnimationButton(Vector2 pos)
        {
            var button = AddOptionButton(pos, "Sprite Animations", SpriteAnimationScreen.OpenSpriteAnimation);
            var dinoAnimation = ServiceProvider.ContentLoaderManager.LoadSpriteAnimationInfo("Images/Sprites/Dino/Dino.sa")
                .CreateSpriteAnimation()
                .AddToScreen(button)
                .Play(2)
                .SetScale(0.7f)
                .SetAnchor(Enums.Anchor.Center)
                .SetPosition(22, -22);
        }

        private void AddScrollViewerButton(Vector2 pos)
        {
            var button = AddOptionButton(pos, "Scroll Viewer", ScrollViewerScreen.OpenScrollViewerScreen);
            var dinoAnimation = ServiceProvider.ContentLoaderManager.LoadSpriteAnimationInfo("Images/Sprites/Scrolling.sa")
                .CreateSpriteAnimation()
                .AddToScreen(button)
                .Play()
                .SetAnchor(Enums.Anchor.Center)
                .SetPosition(0, -12);
        }

        private void AddAnimationsSection(ref int posY)
        {
            posY += MarginButtonsFromTitle;
            var posX = Config.ScreenContentMargin;
            AddFadeSection(new Vector2(posX, posY));
            posX += textureButtonDefault.Width + MarginOptions;
            AddEaseSection(new Vector2(posX, posY));
            posX += textureButtonDefault.Width + MarginOptions;
            AddScaleSection(new Vector2(posX, posY));
            posX += textureButtonDefault.Width + MarginOptions;
            AddRotationSection(new Vector2(posX, posY));

            posY += textureButtonDefault.Height;
        }

        private void AddFadeSection(Vector2 pos)
        {
            var fadeButton = AddOptionButton(pos, "Fade", FadeAnimationScreen.OpenFadeAnimationScreen);
            var fadeRectangle = new RectangleControl(new Rectangle(0, -10, 100, 70), Color.DarkGreen)
                .AddToScreen(fadeButton)
                .SetAnchor(Enums.Anchor.Center);

            new FadeAnimation(fadeRectangle, 1f, 0f)
                .SetBaseColor(fadeRectangle.Color)
                .SetParent(fadeButton)
                .SetIsLooping(true)
                .SetIsPingPong(true)
                .SetLoopingDelayTimeDuration(0.5f)
                .Play();
        }

        private void AddEaseSection(Vector2 pos)
        {
            var easeButton = AddOptionButton(pos, "Ease", EaseAnimationScreen.OpenEaseAnimationScreen);
            var easeRectangle = new RectangleControl(new Rectangle(-45, -40, 30, 30), Color.DarkGreen)
                .AddToScreen(easeButton)
                .SetAnchor(Enums.Anchor.Center);

            new EaseAnimation(easeRectangle, 1f, new Vector2(-easeRectangle.PositionAnchor.X, 20))
                .SetParent(easeButton)
                .SetIsLooping(true)
                .SetIsPingPong(true)
                .SetLoopingDelayTimeDuration(0.5f)
                .Play();
        }

        private void AddScaleSection(Vector2 pos)
        {
            var scaleButton = AddOptionButton(pos, "Scale", ScaleAnimationScreen.OpenScaleAnimationScreen);
            var scaleRectangle = new RectangleControl(new Rectangle(0, -10, 30, 30), Color.DarkGreen)
                .AddToScreen(scaleButton)
                .SetAnchor(Enums.Anchor.Center);

            new ScaleAnimation(scaleRectangle, 1f, new Vector2(3f))
                .SetParent(scaleButton)
                .SetIsLooping(true)
                .SetIsPingPong(true)
                .SetLoopingDelayTimeDuration(0.5f)
                .Play();
        }

        private void AddRotationSection(Vector2 pos)
        {
            var rotationButton = AddOptionButton(pos, "Rotation", RotationAnimationScreen.OpenRotationAnimationScreen);
            var rotationRectangle = new RectangleControl(new Rectangle(0, -15, 80, 60), Color.DarkGreen)
                .AddToScreen(rotationButton)
                .SetOriginRate(new Vector2(0.5f))
                .SetAnchor(Enums.Anchor.Center);

            new RotationAnimation(rotationRectangle, 1f, 360f)
                .SetParent(rotationButton)
                .SetIsLooping(true)
                .SetIsPingPong(true)
                .SetLoopingDelayTimeDuration(0.5f)
                .Play();
        }

        private void AddScreenSection(ref int posY)
        {
            posY += MarginButtonsFromTitle;

            var posX = Config.ScreenContentMargin;
            AddOptionButtonWithIcon(new Vector2(posX, posY), "Screen Transition",ContentHandler.Instance.TextureTransition, ScreenTransitionScreen.OpenScreenManagerScreen, -25);
            posX += textureButtonDefault.Width + MarginOptions;
            AddOptionButtonWithIcon(new Vector2(posX, posY), "Screen Manager", ContentHandler.Instance.TextureWindowApplication, ScreenManagerScreen.OpenScreenManagerScreen, -25);

            posY += textureButtonDefault.Height;
        }

        private void AddTitle(int posY, string title)
        {
            new Label(ContentHandler.Instance.SpriteFontArial, title, new Vector2(Config.ScreenContentMargin, posY), Color.Yellow)
                .SetScale(1.1f)
                .AddToScreen();
        }

        private Button AddOptionButtonWithIcon(Vector2 position, string title, Texture2D icon, Action onClick, int iconMarginTop = -12)
        {
            var optionButton = AddOptionButton(position, title, onClick);

            new Image(icon)
                .SetAnchor(Enums.Anchor.Center)
                .SetPosition(new Vector2(0, iconMarginTop))
                .AddToScreen(optionButton);

            return optionButton;
        }

        private Button AddOptionButton(Vector2 position, string title, Action onClick)
        {
            var optionButton = new Button(textureButtonDefault, position)
                .SetHoverTexture(textureButtonHover)
                .SetMousePressedTexture(textureButtonPressed)
                .AddOnClick(env => onClick())
                .AddToScreen();

            new MultiLineLabel(ContentHandler.Instance.SpriteFontArial, title, new Vector2(0, 5), Color.DarkBlue,(int)optionButton.Size.X)
                .SetTextAlign(Enums.TextAlign.Center)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.BottomCenter)
                .AddToScreen(optionButton);

            return optionButton;
        }

        private Texture2D CreateButtonTexture(Color backgroundColor, Color borderColor)
        {
            var radius = 15;
            var size = new Point(150, 134);
            using (var textureTemp = ShaderEffects.CreateRoudedRectangle(radius, size, backgroundColor, ServiceProvider.GraphicsDevice))
            {
                return StrokeEffect.CreateStroke(textureTemp, 3, borderColor, ServiceProvider.GraphicsDevice);
            }
        }
    }
}
