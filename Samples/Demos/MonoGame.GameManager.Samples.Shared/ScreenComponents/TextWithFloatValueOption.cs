using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using System;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    public static class TextWithFloatValueOption
    {
        public static void CreateTextWithFloatValueOption(Panel container, string text, float posY, float value, Action<float> onValueChanged, float step = 0.1f)
        {
            var marginLeft = 10;
            var posX = 0f;

            var optionLabel = new Label(ContentHandler.Instance.SpriteFontArial, $"{text}: ", new Vector2(posX, posY), Color.Yellow)
                .AddToScreen(container);

            posX += optionLabel.Size.X + marginLeft;

            var position = new Vector2(posX, posY);
            FloatValueOption.CreateFloatValueOption(container, ref position, value, newValue => onValueChanged(newValue), step);
        }
    }
}
