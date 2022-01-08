using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Controls.MouseEvent;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Screens;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Samples.Screens.Animations
{
    public class RotationAnimationScreen : Screen
    {
        int sectionTop = Config.ScreenContentMargin + 60;
        int sectionHeight = 690;
        int sectionDivisionLeft = 550;
        RectangleControl rectangleControlPreview;
        RotationAnimation rotationAnimationPreview;
        Label playStopLabel;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Rotation Animation", OpenRotationAnimationScreen)
            });

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionTop, 2, sectionHeight), Color.White)
                .AddToScreen();

            base.OnInit();
        }

        public static void OpenRotationAnimationScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new RotationAnimationScreen());
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

            Vector2Option.CreateVector2Option(container, "Size", posY, rectangleControlPreview.Size, size => rectangleControlPreview.SetSize(size));
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Rotation Start", posY, rotationAnimationPreview.RoationInDegreeStart, value => rotationAnimationPreview.SetRotationInDegreeStart(value), 5);
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Rotation End", posY, rotationAnimationPreview.RotationInDegreeEnd, value => rotationAnimationPreview.SetRotationInDegreeEnd(value), 5);
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Duration", posY, rotationAnimationPreview.Duration, value => rotationAnimationPreview.SetDuration(value));
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Loop Delay", posY, rotationAnimationPreview.LoopingDelayTimeDuration, value => rotationAnimationPreview.SetLoopingDelayTimeDuration(value));
            posY += 55;
            CheckboxOption.CreateCheckboxOption(container, "Loop", posY, rotationAnimationPreview.IsLooping, value => rotationAnimationPreview.SetIsLooping(value));
            posY += 55;
            CheckboxOption.CreateCheckboxOption(container, "Reverse", posY, rotationAnimationPreview.IsReverse, value => rotationAnimationPreview.SetIsReverse(value));
            posY += 55;
            CheckboxOption.CreateCheckboxOption(container, "Ping-Pong", posY, rotationAnimationPreview.IsPingPong, value => rotationAnimationPreview.SetIsPingPong(value));
            posY += 55;

            var playStopButton = new Button(ContentHandler.Instance.TextureButtonBackground, new Vector2(0, posY))
                .AddToScreen(container)
                .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
                .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed)
                .AddOnClick(PlayStopButtonClick);

            playStopLabel = new Label(ContentHandler.Instance.SpriteFontArial, "Stop", Vector2.Zero, Color.White)
                .AddToScreen(playStopButton)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);

            var resetAnimationButton = new Button(ContentHandler.Instance.TextureButtonBackground, new Vector2(playStopButton.Size.X + 10, posY))
               .AddToScreen(container)
               .SetScale(new Vector2(1.25f, 1f))
               .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
               .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed)
               .AddOnClick(ResetAnimationButtonClick);

            new Label(ContentHandler.Instance.SpriteFontArial, "Reset Animation", Vector2.Zero, Color.White)
                .AddToScreen(resetAnimationButton)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);
        }

        private void PlayStopButtonClick(ControlEventArgs args)
        {
            if (playStopLabel.Text == "Stop")
                rotationAnimationPreview.Stop();
            else
            {
                if (rotationAnimationPreview.IsCompleted)
                    rotationAnimationPreview.ResetAnimation();
                rotationAnimationPreview.Play();
            }

            UpdatePlayStopButtonLabel();
        }

        private void UpdatePlayStopButtonLabel()
        {
            if (playStopLabel == null)
                return;

            playStopLabel.Text = rotationAnimationPreview.IsPlaying
                ? "Stop"
                : "Play";
        }

        private void ResetAnimationButtonClick(ControlEventArgs args)
        {
            rotationAnimationPreview.ResetAnimation();
        }

        private void CreatePreviewSection()
        {
            var container = new Panel(new Rectangle(sectionDivisionLeft + Config.ScreenContentMargin, sectionTop, 600, sectionHeight))
                .AddToScreen();

            new Label(ContentHandler.Instance.SpriteFontArial, "Preview", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            const int labelContainerMarginTop = 50;
            var rotationAnimationContainer = new Panel(new Rectangle(0, labelContainerMarginTop, (int)container.Size.X, (int)(container.Size.Y - labelContainerMarginTop)))
                .AddToScreen(container);

            new RectangleControl(Vector2.Zero, rotationAnimationContainer.Size, new Color(15, 15, 15))
                .AddToScreen(rotationAnimationContainer);

            rectangleControlPreview = new RectangleControl(new Rectangle(75, 75, 150, 150), Color.White)
                .AddToScreen(rotationAnimationContainer)
                .SetAnchor(Enums.Anchor.Center)
                .SetOriginRate(0.5f);

            rotationAnimationPreview = new RotationAnimation(rectangleControlPreview, 1f, 360f)
                .SetIsPingPong(true)
                .SetIsLooping(true)
                .AddOnAnimationEnd(UpdatePlayStopButtonLabel)
                .Play();
        }
    }
}
