using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Screens.Transitions;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Timers;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Samples.Screens.ScreensInfo
{
    public class ScreenTransitionScreen : Screen
    {
        int sectionTop = Config.ScreenContentMargin + 60;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Screen Transition", OpenScreenManagerScreen)
            });

            AddOptions();

            base.OnInit();
        }


        public static void OpenScreenManagerScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new ScreenTransitionScreen());
        }

        private void AddOptions()
        {
            var pos = new Vector2(Config.ScreenContentMargin, sectionTop);

            AddNoTransitionOption(pos);
            pos.X += 220;
            AddFadeTransitionOption(pos);
        }

        private void AddNoTransitionOption(Vector2 pos)
        {
            var noTransitionButton = new Button(ContentHandler.Instance.TextureButtonBackground, pos)
               .AddToScreen()
               .SetScale(new Vector2(1.25f, 1f))
               .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
               .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed)
               .AddOnClick(args =>
               {
                   ServiceProvider.ScreenManager.DefaultTransition = null;
                   MainScreen.OpenMainScreen();
               });

            new Label(ContentHandler.Instance.SpriteFontArial, "No transaction", Vector2.Zero, Color.White)
                .AddToScreen(noTransitionButton)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);

            pos = pos + new Vector2(0, 55);

            var transitionContainer = new Panel(pos, new Vector2(100))
                .AddToScreen();



            List<(Color background, string text)> screensExamples = new List<(Color background, string text)>
            {
                {( Color.LightYellow, "A") },
                {( Color.LightBlue, "B") }
            };

            var screenBackground = new RectangleControl(Vector2.Zero, transitionContainer.Size, Color.White)
                .AddToScreen(transitionContainer);
            var screenText = new Label(ContentHandler.Instance.SpriteFontArial, "", Vector2.Zero, Color.Black)
                .AddToScreen(transitionContainer)
                .SetScale(1.6f)
                .SetAnchor(Enums.Anchor.Center);

            var counter = 0;
            Action changeScreen = null;
            changeScreen = () =>
            {
                new DelayTime(counter == 0 ? 0 : 1.5f, () =>
                {
                    var index = counter % 2;
                    counter++;
                    screenBackground.SetColor(screensExamples[index].background);
                    screenText.Text = screensExamples[index].text;
                    changeScreen();
                }).Play();
            };
            changeScreen();
        }

        private void AddFadeTransitionOption(Vector2 pos)
        {
            var noTransitionButton = new Button(ContentHandler.Instance.TextureButtonBackground, pos)
               .AddToScreen()
               .SetScale(new Vector2(1.25f, 1f))
               .SetHoverTexture(ContentHandler.Instance.TextureButtonBackgroundHover)
               .SetMousePressedTexture(ContentHandler.Instance.TextureButtonBackgroundPressed)
               .AddOnClick(args =>
               {
                   ServiceProvider.ScreenManager.DefaultTransition = new FadeTransition();
                   MainScreen.OpenMainScreen();
               });

            new Label(ContentHandler.Instance.SpriteFontArial, "Fade transaction", Vector2.Zero, Color.White)
                .AddToScreen(noTransitionButton)
                .SetScale(0.75f)
                .SetAnchor(Enums.Anchor.Center);

            pos = pos + new Vector2(0, 55);

            var transitionContainer = new Panel(pos, new Vector2(100))
                .AddToScreen();



            List<(Color background, string text)> screensExamples = new List<(Color background, string text)>
            {
                {( Color.LightYellow, "A") },
                {( Color.LightBlue, "B") }
            };

            var screenBackground = new RectangleControl(Vector2.Zero, transitionContainer.Size, Color.White)
                .AddToScreen(transitionContainer);
            var screenText = new Label(ContentHandler.Instance.SpriteFontArial, "", Vector2.Zero, Color.Black)
                .AddToScreen(transitionContainer)
                .SetScale(1.6f)
                .SetAnchor(Enums.Anchor.Center);

            var counter = 0;
            Action changeScreen = null;
            changeScreen = () =>
            {
                new DelayTime(counter == 0 ? 0 : 1.5f, () =>
                {
                    var index = counter % 2;
                    counter++;

                    var fadeAnimation = 0.4f;

                    var fadeBackground = new RectangleControl(Vector2.Zero, transitionContainer.Size, Color.Black)
                        .AddToScreen(transitionContainer);

                    new FadeAnimation(fadeBackground, fadeAnimation, 1f)
                        .Play()
                        .AddOnAnimationEnd(() =>
                        {
                            screenBackground.SetColor(screensExamples[index].background);
                            screenText.Text = screensExamples[index].text;

                            new FadeAnimation(fadeBackground, fadeAnimation, 0f)
                                .SetBaseColor(Color.Black)
                                .Play()
                                .AddOnAnimationEnd(changeScreen);
                        });

                    
                }).Play();
            };
            changeScreen();
        }
    }
}
