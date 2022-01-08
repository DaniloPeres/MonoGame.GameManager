using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using System;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    public static class ButtonOption
    {
        static Color buttonColor = Color.LightBlue;
        static Color hoverButtonColor = new Color(123, 166, 170);
        static Color pressedButtonColor = new Color(100, 150, 150);

        public static void CreateButtonOptionWithText(Panel container, string text, Vector2 position, Vector2 size, Action onClick)
        {
            var panelButton = new Panel(position, size)
                .AddToScreen(container);

            new RectangleControl(Vector2.Zero, panelButton.Size, buttonColor)
                .AddToScreen(panelButton)
                .SetMouseEventsColor(hoverButtonColor, pressedButtonColor)
                .AddOnClick(args => onClick());

            new Label(ContentHandler.Instance.SpriteFontArial, text, Vector2.Zero, Color.Black)
                .AddToScreen(panelButton)
                .SetAnchor(Enums.Anchor.Center);
        }
    }
}
