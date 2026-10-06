using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using System;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    /// <summary>
    /// An option row with a label, a <see cref="Slider"/> and the current value.
    /// </summary>
    public static class SliderOption
    {
        public const int SliderLeft = 200;
        public const int SliderWidth = 245;

        /// <summary>Creates the row and returns an action that updates the displayed value without raising the callback.</summary>
        public static Action<float> CreateSliderOption(Panel container, string text, float posY, float minimum, float maximum, float value,
            Action<float> onValueChanged, float step = 0f, string format = "{0:0.##}")
        {
            new Label(ContentHandler.Instance.SpriteFontArial, text, new Vector2(0, posY + 3), Color.Yellow)
                .SetScale(0.75f)
                .AddToScreen(container);

            var valueLabel = new Label(ContentHandler.Instance.SpriteFontArial, string.Format(format, value), new Vector2(SliderLeft + SliderWidth + 24, posY + 3), Color.White)
                .SetScale(0.7f)
                .AddToScreen(container);

            var slider = new Slider(new Vector2(SliderLeft, posY), new Vector2(SliderWidth, 26), minimum, maximum)
                .SetStep(step)
                .SetValue(value, notify: false)
                .AddOnValueChanged(newValue =>
                {
                    valueLabel.Text = string.Format(format, newValue);
                    onValueChanged(newValue);
                })
                .AddToScreen(container);

            return newValue =>
            {
                slider.SetValue(newValue, notify: false);
                valueLabel.Text = string.Format(format, slider.Value);
            };
        }
    }
}
