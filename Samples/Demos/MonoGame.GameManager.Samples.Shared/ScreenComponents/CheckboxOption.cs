using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using System;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    public static class CheckboxOption
    {
        public static void CreateCheckboxOption(Panel container, string text, float posY, bool value, Action<bool> onValueChanged)
            => CreateCheckboxOption(container, text, new Vector2(0, posY), value, onValueChanged);

        public static void CreateCheckboxOption(Panel container, string text, Vector2 pos, bool value, Action<bool> onValueChanged)
        {
            var marginLeft = 10;

            var vector2Label = new Label(ContentHandler.Instance.SpriteFontArial, $"{text}: ", pos, Color.Yellow)
                .AddToScreen(container);

            pos.X += vector2Label.Size.X + marginLeft;

            var checkboxSize = 30;
            var checkboxSizeInternal = 12;

            RectangleControl internalCheckRectangle = null;

            new RectangleControl(pos, new Vector2(checkboxSize), Color.White)
                .AddToScreen(container)
                .AddOnClick(args => 
                {
                    value = !value;
                    onValueChanged(value);
                    internalCheckRectangle.Color = value ? Color.Black : Color.Transparent;
                });

            internalCheckRectangle = new RectangleControl(pos + new Vector2((checkboxSize - checkboxSizeInternal) / 2), new Vector2(checkboxSizeInternal), value ? Color.Black : Color.Transparent)
                .AddToScreen(container);
        }
    }
}
