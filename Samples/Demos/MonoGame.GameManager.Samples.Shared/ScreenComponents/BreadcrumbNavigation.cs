using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    public static class BreadcrumbNavigation
    {
        private static readonly Color color = Color.Orange;
        private static readonly Color hoverColor = Color.GreenYellow;
        private static readonly Color pressedColor = Color.DarkGreen;

        public static void CreateBreadcrumbNavigation(List<(string text, Action openScreen)> paths)
        {
            var posX = Config.ScreenContentMargin;
            var marginLeft = 10;

            for (var i = 0; i < paths.Count; i++)
            {
                var path = paths[i];
                var isLastItem = i == paths.Count - 1;

                var pathLabel = CreatePathLabel(path.text, posX);

                posX += (int)pathLabel.Size.X + marginLeft;

                if (!isLastItem)
                {
                    SetPathItemClick(pathLabel, path.openScreen);
                    var pathLabelSeparator = CreatePathLabel(">", posX);
                    posX += (int)pathLabelSeparator.Size.X + marginLeft;
                }
            }
        }

        private static Label CreatePathLabel(string text, int posX)
        {
            const int posY = Config.ScreenContentMargin;
            return new Label(ContentHandler.Instance.SpriteFontArial, text, new Vector2(posX, posY), color)
                .AddToScreen();
        }

        private static void SetPathItemClick(Label pathLabel, Action openScreen)
        {
            pathLabel.SetMouseEventsColor(hoverColor, pressedColor);
            pathLabel.AddOnClick(args => openScreen());
        }
    }
}
