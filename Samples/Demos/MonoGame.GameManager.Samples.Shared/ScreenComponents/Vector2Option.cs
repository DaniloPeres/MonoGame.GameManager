using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using System;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    public static class Vector2Option
    {
        public static Action<Vector2> CreateVector2Option(Panel container, string text, float posY, Vector2 value, Action<Vector2> onValueChanged, float step = 5f, float textScale = 1f, string format = "{0:0.##}")
        {
            var marginLeft = 10 * textScale;
            var pos = new Vector2(0, posY);

            var vector2Label = new Label(ContentHandler.Instance.SpriteFontArial, $"{text}: X:", pos, Color.Yellow)
                .SetScale(textScale)
                .AddToScreen(container);

            pos.X += vector2Label.Size.X + marginLeft;

            var updateXValue = FloatValueOption.CreateFloatValueOption(container, ref pos, value.X, newValue =>
            {
                value.X = newValue;
                onValueChanged(value);
            }, step, format);

            pos.X += (10 * textScale);
            vector2Label = new Label(ContentHandler.Instance.SpriteFontArial, $"Y:", pos, Color.Yellow)
                .SetScale(textScale)
                .AddToScreen(container);

            pos.X += vector2Label.Size.X + marginLeft;

            var updateYValue = FloatValueOption.CreateFloatValueOption(container, ref pos, value.Y, newValue =>
            {
                value.Y = newValue;
                onValueChanged(value);
            }, step, format);

            return newValue =>
            {
                value = newValue;
                updateXValue(newValue.X);
                updateYValue(newValue.Y);
            };
        }
    }
}
