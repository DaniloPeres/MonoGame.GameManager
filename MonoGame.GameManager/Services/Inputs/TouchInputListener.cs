using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Services.Inputs
{
    public class TouchInputListener
    {
        public event Action<TouchEventArgs> OnTouchStarted;
        public event Action<TouchEventArgs> OnTouchMoved;
        public event Action<TouchEventArgs> OnTouchReleased;
        public event Action<TouchEventArgs> OnTouchCancelled;
        public event Action<MultipleTouchpointsEventArgs> OnMultipleTouch;

        public void Update(GameTime gameTime)
        {
            var touchCollection = TouchPanel.GetState();
            var touchesLocationOnScreen = new List<TouchLocation>();

            foreach (var touchLocation in touchCollection)
            {
                var touchLocationOnScreen = new TouchLocation(
                    touchLocation.Id,
                    touchLocation.State,
                    ServiceProvider.GameWindowManager.GetPositionOnScreen(touchLocation.Position));
                touchesLocationOnScreen.Add(touchLocationOnScreen);
            }

            if (touchesLocationOnScreen.Any())
            {
                var touchLocationOnScreen = touchesLocationOnScreen.First();
                var args = new TouchEventArgs(gameTime.TotalGameTime, touchLocationOnScreen);
                switch (touchLocationOnScreen.State)
                {
                    case TouchLocationState.Pressed:
                        OnTouchStarted?.Invoke(args);
                        break;
                    case TouchLocationState.Moved:
                        OnTouchMoved?.Invoke(args);
                        break;
                    case TouchLocationState.Released:
                        OnTouchReleased?.Invoke(args);
                        break;
                    case TouchLocationState.Invalid:
                        OnTouchCancelled?.Invoke(args);
                        break;
                }
            }

            if (touchesLocationOnScreen.Count >= 2)
            {
                var multipleTouchpointsArgs = new MultipleTouchpointsEventArgs(gameTime.TotalGameTime, touchesLocationOnScreen);
                OnMultipleTouch?.Invoke(multipleTouchpointsArgs);
            }
        }
    }
}
