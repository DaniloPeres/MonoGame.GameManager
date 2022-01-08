using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    public static class ColorOption
    {
        public static void CreateColorOption(Panel container, float posY, Action<Color> OnColorSelected, string label = "Color", List<Color> colors = null)
        {
            var marginLeft = 10;
            var posX = 0f;
            var squareSize = 35;

            var colorLabel = new Label(ContentHandler.Instance.SpriteFontArial, $"{label}: ", new Vector2(posX, posY), Color.Yellow)
                .AddToScreen(container);

            posX += colorLabel.Size.X + marginLeft;

            colors = colors ?? new List<Color>()
            {
                Color.White,
                Color.Yellow,
                Color.GreenYellow,
                Color.LightBlue,
                Color.Red,
                Color.Blue,
                Color.Gray,
                Color.DarkGray
            };

            colors.ForEach(color =>
            {
                new RectangleControl(new Rectangle((int)posX, (int)posY, squareSize, squareSize), color)
                    .AddToScreen(container)
                    .AddOnClick(args => OnColorSelected(color));
                posX += squareSize + marginLeft;
            });
        }
    }
}
