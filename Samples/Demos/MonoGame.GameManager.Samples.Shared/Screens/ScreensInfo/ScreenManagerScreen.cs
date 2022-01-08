using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.ScreenComponents;
using MonoGame.GameManager.Samples.Screens;
using MonoGame.GameManager.Samples.Screens.Controls;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Screens;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGame.GameManager.Samples.Screens.ScreensInfo
{
    public class ScreenManagerScreen : Screen
    {
        int sectionTop = Config.ScreenContentMargin + 60;
        int sectionHeight = 690;

        public override void OnInit()
        {
            BreadcrumbNavigation.CreateBreadcrumbNavigation(new List<(string text, Action openScreen)>
            {
                ("Home", MainScreen.OpenMainScreen),
                ("Screen Manager", OpenScreenManagerScreen)
            });

            AddOptions();

            base.OnInit();
        }


        public static void OpenScreenManagerScreen()
        {
            ServiceProvider.ScreenManager.ChangeScreen(new ScreenManagerScreen());
        }


        private void AddOptions()
        {
            var container = new Panel(new Rectangle(Config.ScreenContentMargin, sectionTop, ServiceProvider.GameWindowManager.ScreenSize.X - (Config.ScreenContentMargin * 2), sectionHeight))
                .AddToScreen();

            var pos = new Vector2(0);

            new Label(ContentHandler.Instance.SpriteFontArial, "Screen Options", pos, Color.Yellow)
                .AddToScreen(container);

            var colors = new List<Color>()
            {
                Color.Black,
                Color.White,
                Color.DarkBlue,
                Color.DarkGreen,
                Color.Red,
                Color.Blue,
                Color.Gray,
                Color.DarkGray
            };

            pos.Y += 55;
            ColorOption.CreateColorOption(container, pos.Y, color => ServiceProvider.ScreenManager.ScreenBackgroundColor = color, "Screen background", colors);
            pos.Y += 55;
            ColorOption.CreateColorOption(container, pos.Y, color => ServiceProvider.ScreenManager.WindowBackgroundColor = color, "Window background", colors);
            pos.Y += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Matin Left", pos.Y, ServiceProvider.GameWindowManager.ScreenMargin.X, value => ServiceProvider.GameWindowManager.SetMarginLeft(value), 5f);
            pos.Y += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Matin Top", pos.Y, ServiceProvider.GameWindowManager.ScreenMargin.Y, value => ServiceProvider.GameWindowManager.SetMarginRight(value), 5f);
            pos.Y += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Matin Right", pos.Y, ServiceProvider.GameWindowManager.ScreenMargin.Z, value => ServiceProvider.GameWindowManager.SetMarginTop(value), 5f);
            pos.Y += 55;
            TextWithFloatValueOption.CreateTextWithFloatValueOption(container, "Matin Bottom", pos.Y, ServiceProvider.GameWindowManager.ScreenMargin.W, value => ServiceProvider.GameWindowManager.SetMarginBottom(value), 5f);
            pos.Y += 55;
            Vector2Option.CreateVector2Option(container, "Scale", pos.Y, ServiceProvider.GameWindowManager.ScreenScale, value => ServiceProvider.GameWindowManager.ScreenScale = value, 0.1f);


            pos.Y += 100;
            new Label(ContentHandler.Instance.SpriteFontArial, "Try resizing the window", pos, Color.White)
                .AddToScreen(container);
        }

    }
}
