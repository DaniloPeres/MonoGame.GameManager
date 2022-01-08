using MonoGame.GameManager.Controls;
using MonoGame.GameManager.Samples.Services;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using System.Linq;
using MonoGame.GameManager.Controls.Interfaces;

namespace MonoGame.GameManager.Samples.ScreenComponents
{
    public static class LastEventsInfo
    {
        public static void AddLastEventsInfo(Panel container, float posY, IControl controlToWatch)
        {
            var marginLeft = 10;
            var posX = 0f;

            var lastEventsLabel = new Label(ContentHandler.Instance.SpriteFontArial, "Last events: ", new Vector2(posX, posY), Color.Yellow)
                .AddToScreen(container);

            posX += lastEventsLabel.Size.X + marginLeft;

            var lastEventsValue = new Label(ContentHandler.Instance.SpriteFontArial, "", new Vector2(posX, posY), Color.White)
                .AddToScreen(container)
                .SetScale(0.75f);

            var lastEventsFifo = new Queue<string>();
            const int maxLastEventsDisplay = 7;

            Action<string> onUpdateLastEvents = eventName =>
            {
                lastEventsFifo.Enqueue(eventName);

                while (lastEventsFifo.Count > maxLastEventsDisplay)
                    lastEventsFifo.Dequeue();

                lastEventsValue.Text = string.Join("\n", lastEventsFifo.Reverse());
            };

            controlToWatch
                .AddOnClick(args => onUpdateLastEvents("OnClick"))
                .AddOnMouseEnter(args => onUpdateLastEvents("OnMouseEnter"))
                .AddOnMouseLeave(args => onUpdateLastEvents("OnMouseLeave"))
                .AddOnMouseMoved(args => onUpdateLastEvents("OnMouseMoved"))
                .AddOnMousePressed(args => onUpdateLastEvents("OnMousePressed"))
                .AddOnMouseReleased(args => onUpdateLastEvents("OnMouseReleased"));
        }
    }
}
