﻿using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Screens;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using System.Collections.Generic;
using System;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Services;
using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls.InputEvent;

namespace MonoGame.GameManager.Samples.Screens.Animations
{
    public class FadeAnimationScreen : Screen
    {
        int sectionTop = Config.ScreenContentMargin + 60;
        int sectionHeight = 690;
        int sectionDivisionLeft = 550;
        RectangleControl rectangleControlPreview;
        FadeAnimation fadeAnimationPreview;
        Label playStopLabel;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Fade Animation", OpenFadeAnimationScreen)
            });

            CreatePreviewSection();
            CreateOptionsSection();
            new RectangleControl(new Rectangle(sectionDivisionLeft, sectionTop, 2, sectionHeight), Color.White)
                .AddToScreen();

            base.OnInit();
        }

        public static void OpenFadeAnimationScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new FadeAnimationScreen());
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

            ColorOption.CreateColorOption(container, posY, color => fadeAnimationPreview.SetBaseColor(color));
            posY += 55;
            Vector2Option.CreateVector2Option(container, "Size", posY, rectangleControlPreview.Size, size => rectangleControlPreview.SetSize(size));
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Transparency Start", posY, 1f, value => fadeAnimationPreview.SetTransparencyStart(value));
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Transparency End", posY, 0f, value => fadeAnimationPreview.SetTransparencyEnd(value));
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Duration", posY, fadeAnimationPreview.Duration, value => fadeAnimationPreview.SetDuration(value));
            posY += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Loop Delay", posY, fadeAnimationPreview.LoopingDelayTimeDuration, value => fadeAnimationPreview.SetLoopingDelayTimeDuration(value));
            posY += 55;
            CheckboxOption.CreateCheckboxOption(container, "Loop", posY, fadeAnimationPreview.IsLooping, value => fadeAnimationPreview.SetIsLooping(value));
            posY += 55;
            CheckboxOption.CreateCheckboxOption(container, "Reverse", posY, fadeAnimationPreview.IsReverse, value => fadeAnimationPreview.SetIsReverse(value));
            posY += 55;
            CheckboxOption.CreateCheckboxOption(container, "Ping-Pong", posY, fadeAnimationPreview.IsPingPong, value => fadeAnimationPreview.SetIsPingPong(value));
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
               .SetBackgroundScale(new Vector2(1.25f, 1f))
               .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
               .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed)
               .AddOnClick(ResetAnimationButtonClick);

            new Label(ContentHandler.Instance.SpriteFontArial, "Reset Animation", Vector2.Zero, Color.White)
                .AddToScreen(resetAnimationButton)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);
        }

        private void PlayStopButtonClick(ControlMouseEventArgs args)
        {
            if (playStopLabel.Text == "Stop")
                fadeAnimationPreview.Stop();
            else
            {
                if (fadeAnimationPreview.IsCompleted)
                    fadeAnimationPreview.ResetAnimation();
                fadeAnimationPreview.Play();
            }

            UpdatePlayStopButtonLabel();
        }

        private void UpdatePlayStopButtonLabel()
        {
            if (playStopLabel == null)
                return;

            playStopLabel.Text = fadeAnimationPreview.IsPlaying
                ? "Stop"
                : "Play";
        }

        private void ResetAnimationButtonClick(ControlMouseEventArgs args)
        {
            fadeAnimationPreview.ResetAnimation();
        }

        private void CreatePreviewSection()
        {
            var container = new Panel(new Rectangle(sectionDivisionLeft + Config.ScreenContentMargin, sectionTop, 600, sectionHeight))
                .AddToScreen();

            new Label(ContentHandler.Instance.SpriteFontArial, "Preview", Vector2.Zero, Color.Yellow)
                .SetAnchor(Enums.Anchor.TopCenter)
                .AddToScreen(container);

            const int labelContainerMarginTop = 50;
            var fadeAnimationContainer = new Panel(new Rectangle(0, labelContainerMarginTop, (int)container.Size.X, (int)(container.Size.Y - labelContainerMarginTop)))
                .AddToScreen(container);

            new RectangleControl(Vector2.Zero, fadeAnimationContainer.Size, new Color(15, 15, 15))
                .AddToScreen(fadeAnimationContainer);

            rectangleControlPreview = new RectangleControl(new Rectangle(0, 0, 150, 150), Color.White)
                .AddToScreen(fadeAnimationContainer)
                .SetAnchor(Enums.Anchor.Center);

            fadeAnimationPreview = new FadeAnimation(rectangleControlPreview, 1f, 0f)
                .SetIsPingPong(true)
                .SetIsLooping(true)
                .AddOnAnimationEnd(UpdatePlayStopButtonLabel)
                .Play();
        }
    }
}
