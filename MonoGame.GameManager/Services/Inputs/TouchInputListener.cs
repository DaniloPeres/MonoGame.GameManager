using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace MonoGame.GameManager.Services.Inputs
{
    public class TouchInputListener
    {
        public event Action<TouchEventArgs> OnTouchStarted;
        public event Action<TouchEventArgs> OnTouchMoved;
        public event Action<TouchEventArgs> OnTouchReleased;
        public event Action<TouchEventArgs> OnTouchCancelled;

        public void Update(GameTime gameTime)
        {
            var touchCollection = TouchPanel.GetState();

            foreach (var touchLocation in touchCollection)
            {
                var touchLocationOnScreen = new TouchLocation(
                    touchLocation.Id,
                    touchLocation.State,
                    ServiceProvider.GameWindowManager.GetPositionOnScreen(touchLocation.Position));
                
                var args = new TouchEventArgs(gameTime.TotalGameTime, touchLocationOnScreen);
                switch (touchLocation.State)
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
        }
    }
}
