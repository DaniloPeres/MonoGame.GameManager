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
        /// <param name="textScale">The scale of the label (compact pages use 0.75).</param>
        /// <param name="squareSize">The size of the color squares.</param>
        /// <param name="squaresLeft">Where the squares start (null = after the label), to align them with other rows.</param>
        public static void CreateColorOption(Panel container, float posY, Action<Color> OnColorSelected, string label = "Color", List<Color> colors = null, bool showBorder = false,
            float textScale = 1f, int squareSize = 35, float? squaresLeft = null)
        {
            var font = ContentHandler.Instance.SpriteFontArial;
            var marginLeft = squareSize < 35 ? 6 : 10;
            var posX = 0f;
            var labelY = textScale < 1f ? posY + (float)System.Math.Round((squareSize - font.LineSpacing * textScale) / 2f) : posY;

            var colorLabel = new Label(font, squaresLeft.HasValue ? label : $"{label}: ", new Vector2(posX, labelY), Color.Yellow)
                .SetScale(textScale)
                .AddToScreen(container);

            posX = squaresLeft ?? colorLabel.Size.X + marginLeft;

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
                var square = new RectangleControl(new Rectangle((int)posX, (int)posY, squareSize, squareSize), color)
                    .AddToScreen(container)
                    .AddOnClick(args => OnColorSelected(color));
                if (showBorder)
                    square.SetBorder(Color.Gray);
                posX += squareSize + marginLeft;
            });
        }
    }
}
