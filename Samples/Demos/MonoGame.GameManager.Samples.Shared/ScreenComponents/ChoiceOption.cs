using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using MonoGame.GameManager.Services.Inputs;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    /// <summary>
    /// An option row with a label and a button that cycles through a list of choices (left click: next, right
    /// click: previous).
    /// </summary>
    public static class ChoiceOption
    {
        private static readonly Color ButtonColor = new Color(55, 55, 65);

        /// <summary>Creates the row and returns an action that selects a choice without raising the callback.</summary>
        public static Action<int> CreateChoiceOption(Panel container, string text, float posY, IList<string> choices, int selected, Action<int> onChanged)
        {
            new Label(ContentHandler.Instance.Font, text, new Vector2(0, posY + 3), Color.Yellow)
                .SetScale(0.75f)
                .AddToScreen(container);

            var index = Math.Max(0, Math.Min(choices.Count - 1, selected));
            var button = new Button(new Vector2(SliderOption.SliderLeft, posY), new Vector2(SliderOption.SliderWidth, 28), ButtonColor)
                .SetBorder(Color.Gray)
                .SetText(ContentHandler.Instance.Font, choices[index], Color.White)
                .SetAcceptedMouseButtons(MouseButtons.Left | MouseButtons.Right)
                .AddToScreen(container);
            button.TextLabel.SetScale(0.65f);

            button.AddOnClick(args =>
            {
                var direction = args.Button == MouseButtons.Right ? -1 : 1;
                index = (index + direction + choices.Count) % choices.Count;
                button.SetText(choices[index]);
                onChanged(index);
            });

            return newIndex =>
            {
                index = Math.Max(0, Math.Min(choices.Count - 1, newIndex));
                button.SetText(choices[index]);
            };
        }
    }
}
