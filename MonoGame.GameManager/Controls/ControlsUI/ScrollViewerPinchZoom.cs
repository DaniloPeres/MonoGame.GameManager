using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.GameManager.Controls.InputEvent;
using System;

namespace MonoGame.GameManager.Controls.ControlsUI
{
    /// <summary>
    /// Zooms and moves the content of a <see cref="ScrollViewer"/> with two fingers (created by the viewer). The
    /// point of the content between the fingers stays between the fingers.
    /// </summary>
    public class ScrollViewerPinchZoom
    {
        private const float MinimumFingerDistance = 1f;
        private const double EventTimeoutSeconds = 0.25;
        private readonly ScrollViewer scrollViewer;
        private float startDistance;
        private Vector2 startZoom;
        private Vector2 contentAnchor;
        private TimeSpan lastEventTime;

        public ScrollViewerPinchZoom(ScrollViewer scrollViewer)
        {
            this.scrollViewer = scrollViewer ?? throw new ArgumentNullException(nameof(scrollViewer));
            scrollViewer.AddOnMultipleTouchpoints(OnMultipleTouchpointsUpdate);
        }

        public bool IsPinchActive { get; private set; }

        /// <summary>When false, two fingers do not zoom the content.</summary>
        public bool IsEnabled { get; set; } = true;

        public void SetPinchAsInactive() => IsPinchActive = false;

        /// <summary>Ends a pinch whose fingers disappeared without a release event.</summary>
        public void Update(TimeSpan totalTime)
        {
            if (IsPinchActive && (totalTime - lastEventTime).TotalSeconds > EventTimeoutSeconds)
                OnPinchReleased();
        }

        private void OnMultipleTouchpointsUpdate(ControlMultipleTouchpointsEventArgs args)
        {
            if (!IsEnabled || args.Touchpoints == null || args.Touchpoints.Count < 2)
            {
                args.ContinuePropagation();
                return;
            }

            lastEventTime = args.Time;
            var first = args.Touchpoints[0];
            var second = args.Touchpoints[1];
            var isReleased = IsEnded(first.State) || IsEnded(second.State);
            if (isReleased)
            {
                if (IsPinchActive)
                    OnPinchReleased();
                return;
            }

            var a = scrollViewer.ToViewportPosition(first.Position);
            var b = scrollViewer.ToViewportPosition(second.Position);
            var distance = Vector2.Distance(a, b);
            var center = (a + b) / 2f;

            if (!IsPinchActive)
            {
                if (distance < MinimumFingerDistance)
                    return;
                OnPinchStart(distance, center);
                return;
            }

            scrollViewer.ZoomFromPinch(startZoom * (distance / startDistance), center, contentAnchor);
        }

        private void OnPinchStart(float distance, Vector2 center)
        {
            IsPinchActive = true;
            startDistance = distance;
            startZoom = scrollViewer.Zoom;
            var zoom = new Vector2(Math.Max(startZoom.X, 0.0001f), Math.Max(startZoom.Y, 0.0001f));
            contentAnchor = (center - scrollViewer.ScrollPosition) / zoom;
        }

        private void OnPinchReleased()
        {
            SetPinchAsInactive();
            scrollViewer.HideBars();
        }

        private static bool IsEnded(TouchLocationState state) => state == TouchLocationState.Released || state == TouchLocationState.Invalid;
    }
}
