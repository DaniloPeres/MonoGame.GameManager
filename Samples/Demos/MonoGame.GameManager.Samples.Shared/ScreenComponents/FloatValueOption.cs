using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using System;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    public static class FloatValueOption
    {
        public static void CreateFloatValueOption(Panel container, ref Vector2 position, float value, Action<float> onValueChanged, float step = 0.1f)
        {
            const int buttonSize = 35;
            const int marginLeft = 10;
            Label labelValue = null;

            Action<float> processValueChanged = addValue =>
            {
                value += addValue;
                onValueChanged(value);
                labelValue.Text = String.Format("{0:0.##}", value);
            };

            ButtonOption.CreateButtonOptionWithText(container, "-", position, new Vector2(buttonSize), () => processValueChanged(-step));

            position.X += buttonSize + marginLeft;

            var textPanel = new Panel(position, new Vector2(35, 35))
                .AddToScreen(container);

            labelValue = new Label(ContentHandler.Instance.SpriteFontArial, value.ToString(), Vector2.Zero, Color.White)
                .AddToScreen(textPanel)
                .SetScale(0.7f)
                .SetAnchor(Enums.Anchor.Center);

            position.X += textPanel.Size.X + marginLeft;

            ButtonOption.CreateButtonOptionWithText(container, "+", position, new Vector2(buttonSize), () => processValueChanged(step));

            position.X += buttonSize + marginLeft;
        }

    }
}
