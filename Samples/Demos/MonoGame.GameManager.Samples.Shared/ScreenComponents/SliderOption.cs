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
        public const int SliderHeight = 26;

        /// <summary>Creates the row and returns an action that updates the displayed value without raising the callback.</summary>
        /// <param name="textScale">The scale of the label: 0.75 for compact pages, 1 to match the other option rows.</param>
        public static Action<float> CreateSliderOption(Panel container, string text, float posY, float minimum, float maximum, float value,
            Action<float> onValueChanged, float step = 0f, string format = "{0:0.##}", float textScale = 0.75f)
        {
            var font = ContentHandler.Instance.Font;
            var valueScale = textScale >= 1f ? 0.8f : 0.7f;

            new Label(font, text, new Vector2(0, CenteredTop(posY, font.LineHeight * textScale)), Color.Yellow)
                .SetScale(textScale)
                .AddToScreen(container);

            var valueLabel = new Label(font, string.Format(format, value), new Vector2(SliderLeft + SliderWidth + 24, CenteredTop(posY, font.LineHeight * valueScale)), Color.White)
                .SetScale(valueScale)
                .AddToScreen(container);

            var slider = new Slider(new Vector2(SliderLeft, posY), new Vector2(SliderWidth, SliderHeight), minimum, maximum)
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

        /// <summary>The top of a text of the given height centered on the slider row.</summary>
        private static float CenteredTop(float posY, float textHeight) => posY + (float)Math.Round((SliderHeight - textHeight) / 2f);
    }
}
