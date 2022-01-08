using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using System;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    public static class Vector2Option
    {
        public static void CreateVector2Option(Panel container, string text, float posY, Vector2 value, Action<Vector2> onValueChanged, float step = 5f)
        {
            var marginLeft = 10;
            var pos = new Vector2(0, posY);

            var vector2Label = new Label(ContentHandler.Instance.SpriteFontArial, $"{text}: X:", pos, Color.Yellow)
                .AddToScreen(container);

            pos.X += vector2Label.Size.X + marginLeft;

            FloatValueOption.CreateFloatValueOption(container, ref pos, value.X, newValue =>
            {
                value.X = newValue;
                onValueChanged(value);
            }, step);

            pos.X += 10;
            vector2Label = new Label(ContentHandler.Instance.SpriteFontArial, $"Y:", pos, Color.Yellow)
                .AddToScreen(container);

            pos.X += vector2Label.Size.X + marginLeft;

            FloatValueOption.CreateFloatValueOption(container, ref pos, value.Y, newValue =>
            {
                value.Y = newValue;
                onValueChanged(value);
            }, step);
        }
    }
}
