using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Services.Inputs
{
    /// <summary>
    /// Polls the touch panel once per frame. The first finger is reported as a "primary" touch (used like a mouse
    /// by the controls) and two or more fingers raise <see cref="OnMultipleTouch"/>. Positions are converted to
    /// virtual screen coordinates.
    /// </summary>
    public class TouchInputListener
    {
        private readonly Func<Vector2, Vector2> windowToScreen;
        private readonly List<TouchLocation> touches = new List<TouchLocation>();
        private int? primaryTouchId;
        private TouchLocation lastPrimaryTouch;

        public event Action<TouchEventArgs> OnTouchStarted;
        public event Action<TouchEventArgs> OnTouchMoved;
        public event Action<TouchEventArgs> OnTouchReleased;
        public event Action<TouchEventArgs> OnTouchCancelled;
        public event Action<MultipleTouchpointsEventArgs> OnMultipleTouch;

        /// <summary>Raised for every gesture when gestures are enabled with <see cref="EnableGestures"/>.</summary>
        public event Action<GestureSample> OnGesture;

        public TouchInputListener() : this(null) { }

        /// <param name="windowToScreen">Converts window coordinates to screen coordinates (null = no conversion).</param>
        public TouchInputListener(Func<Vector2, Vector2> windowToScreen)
        {
            this.windowToScreen = windowToScreen;
        }

        /// <summary>The touches of the current frame, in virtual screen coordinates.</summary>
        public IReadOnlyList<TouchLocation> Touches => touches;

        /// <summary>True while at least one finger touches the screen.</summary>
        public bool IsTouching => primaryTouchId.HasValue;

        /// <summary>
        /// Enables the gestures of the touch panel (tap, hold, flick, pinch...). They are raised by <see cref="OnGesture"/>.
        /// </summary>
        public void EnableGestures(GestureType gestures) => TouchPanel.EnabledGestures = gestures;

        public void Update(GameTime gameTime)
        {
            Update(TouchPanel.GetState(), gameTime.TotalGameTime);

            if (TouchPanel.EnabledGestures == GestureType.None)
                return;

            while (TouchPanel.IsGestureAvailable)
            {
                var gesture = TouchPanel.ReadGesture();
                OnGesture?.Invoke(new GestureSample(gesture.GestureType, gesture.Timestamp,
                    ToScreen(gesture.Position), ToScreen(gesture.Position2), ToScreenDelta(gesture.Delta), ToScreenDelta(gesture.Delta2)));
            }
        }

        /// <summary>
        /// Updates the listener with a given touch collection (in window coordinates), for example to simulate input.
        /// </summary>
        public void Update(TouchCollection touchCollection, TimeSpan time)
        {
            touches.Clear();
            foreach (var touch in touchCollection)
                touches.Add(new TouchLocation(touch.Id, touch.State, ToScreen(touch.Position)));

            UpdatePrimaryTouch(time);

            if (touches.Count >= 2)
                OnMultipleTouch?.Invoke(new MultipleTouchpointsEventArgs(time, new List<TouchLocation>(touches)));
        }

        private void UpdatePrimaryTouch(TimeSpan time)
        {
            if (primaryTouchId.HasValue)
            {
                var index = touches.FindIndex(touch => touch.Id == primaryTouchId.Value);
                if (index < 0)
                {
                    // The primary finger disappeared without a release: cancel it.
                    primaryTouchId = null;
                    OnTouchCancelled?.Invoke(new TouchEventArgs(time, lastPrimaryTouch));
                }
                else
                {
                    var touch = touches[index];
                    lastPrimaryTouch = touch;
                    switch (touch.State)
                    {
                        case TouchLocationState.Moved:
                            OnTouchMoved?.Invoke(new TouchEventArgs(time, touch));
                            break;
                        case TouchLocationState.Released:
                            primaryTouchId = null;
                            OnTouchReleased?.Invoke(new TouchEventArgs(time, touch));
                            break;
                        case TouchLocationState.Invalid:
                            primaryTouchId = null;
                            OnTouchCancelled?.Invoke(new TouchEventArgs(time, touch));
                            break;
                    }
                }
                return;
            }

            foreach (var touch in touches)
            {
                if (touch.State != TouchLocationState.Pressed)
                    continue;
                primaryTouchId = touch.Id;
                lastPrimaryTouch = touch;
                OnTouchStarted?.Invoke(new TouchEventArgs(time, touch));
                return;
            }
        }

        private Vector2 ToScreen(Vector2 windowPosition) => windowToScreen?.Invoke(windowPosition) ?? windowPosition;

        private Vector2 ToScreenDelta(Vector2 windowDelta) => ToScreen(windowDelta) - ToScreen(Vector2.Zero);
    }
}
