using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Enums;
using MonoGame.GameManager.Samples.Services;
using System;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    public static class AnchorOption
    {
        public static void CreateAnchorOption(Panel container, float posY, Action<Anchor> OnAnchorSelected, string text = "Anchor")
        {
            var marginLeft = 10;
            var posX = 0f;

            var anchorLabel = new Label(ContentHandler.Instance.SpriteFontArial, $"{text}: ", new Vector2(posX, posY + 46), Color.Yellow)
                .AddToScreen(container);
            posX += anchorLabel.Size.X + marginLeft;

            Anchor[,] anchors =
            {
                { Anchor.TopLeft,       Anchor.TopCenter,    Anchor.TopRight },
                { Anchor.CenterLeft,    Anchor.Center,       Anchor.CenterRight },
                { Anchor.BottomLeft,    Anchor.BottomCenter, Anchor.BottomRight }
            };

            var squareSize = new Vector2(128, 40);
            var squareMargin = 5;

            for (var x = 0; x < anchors.GetLength(1); x++)
            {
                for (var y = 0; y < anchors.GetLength(0); y++)
                {
                    var anchor = anchors[y, x];
                    var pos = new Vector2(posX, posY) + (squareSize + new Vector2(squareMargin)) * new Vector2(x, y);
                    var anchorContainer = new Panel(pos, squareSize)
                        .AddToScreen(container);

                    new RectangleControl(Vector2.Zero, anchorContainer.Size, Color.DarkGray)
                        .AddToScreen(anchorContainer)
                        .SetMouseEventsColor(Color.Gray, Color.DarkSlateGray)
                        .AddOnClick(args =>
                        {
                            OnAnchorSelected(anchor);
                        });

                    new Label(ContentHandler.Instance.SpriteFontArial, anchor.ToString(), Vector2.Zero, Color.White)
                        .AddToScreen(anchorContainer)
                        .SetScale(0.65f)
                        .SetAnchor(Anchor.Center);
                }
            }
        }
    }
}
