using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;

namespace Snake
{
    public static class UiHelper
    {
        public const int BlocksX = 20;
        public const int BlocksY = 14;
        public const int BlockSize = 20;
        public static readonly Color DarkBackgroundColor = new Color(39, 47, 23);
        public static readonly Color LightBackgroundColor = new Color(155, 186, 90);
        public const float RotationAnimationTime = 2f;

        public static Panel CreatePanelBoard()
        {
            var board = new Panel(new Rectangle(0, 0, BlocksX * BlockSize, BlocksY * BlockSize))
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.Center)
                .AddToScreen();

            // Add rectangle around the board
            const int borderMargin = 2;
            const int borderWidth = 2;

            var panelBorder = new Panel(new Rectangle((board.GetPosition() - new Vector2(borderMargin + borderWidth)).ToPoint(), (board.DestinationRectangle.Size.ToVector2() + new Vector2((borderMargin + borderWidth) * 2)).ToPoint()))
                .AddToScreen();

            // add border lines the lines
            new RectangleControl(Vector2.Zero, new Vector2(panelBorder.DestinationRectangle.Width, borderWidth), DarkBackgroundColor)
                .AddToScreen(panelBorder);
            new RectangleControl(Vector2.Zero, new Vector2(panelBorder.DestinationRectangle.Width, borderWidth), DarkBackgroundColor)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.BottomLeft)
                .AddToScreen(panelBorder);
            new RectangleControl(Vector2.Zero, new Vector2(borderWidth, panelBorder.DestinationRectangle.Height), DarkBackgroundColor)
                .AddToScreen(panelBorder);
            new RectangleControl(Vector2.Zero, new Vector2(borderWidth, panelBorder.DestinationRectangle.Height), DarkBackgroundColor)
                .SetAnchor(MonoGame.GameManager.Enums.Anchor.TopRight)
               .AddToScreen(panelBorder);

            return board;
        }
    }
}
