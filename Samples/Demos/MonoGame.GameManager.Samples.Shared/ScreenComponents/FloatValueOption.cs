using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using System;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    public static class FloatValueOption
    {
        public static Action<float> CreateFloatValueOption(Panel container, ref Vector2 position, float value, Action<float> onValueChanged, float step = 0.1f, string format = "{0:0.##}")
        {
            const int buttonSize = 35;
            const int marginLeft = 10;
            Label labelValue = null;

            Action<float, bool> processValueChanged = (addValue, callOnValueChanged) =>
            {
                value += addValue;
                if (callOnValueChanged)
                    onValueChanged(value);
                labelValue.Text = string.Format(format, value);
            };

            ButtonOption.CreateButtonOptionWithText(container, "-", position, new Vector2(buttonSize), () => processValueChanged(-step, true));

            position.X += buttonSize + marginLeft;

            var textPanel = new Panel(position, new Vector2(35, 35))
                .AddToScreen(container);

            labelValue = new Label(ContentHandler.Instance.SpriteFontArial, value.ToString(), Vector2.Zero, Color.White)
                .AddToScreen(textPanel)
                .SetScale(0.7f)
                .SetAnchor(Enums.Anchor.Center);

            position.X += textPanel.Size.X + marginLeft;

            ButtonOption.CreateButtonOptionWithText(container, "+", position, new Vector2(buttonSize), () => processValueChanged(step, true));

            position.X += buttonSize + marginLeft;

            return newValue =>
            {
                processValueChanged(newValue - value, false);
            };
        }

    }
}
