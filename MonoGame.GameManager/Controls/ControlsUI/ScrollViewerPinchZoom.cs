using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.GameManager.Controls.InputEvent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Controls.ControlsUI
{
    public class ScrollViewerPinchZoom
    {
        public bool IsPinchActive { get; private set; }
        private readonly ScrollViewer scrollViewer;
        private float lastScale;
        private float nthZoom;
        private Vector2 lastZoomCenter;
        private Vector2[] startTouchPositions;
        private float zoomFactor = 1;
        private Vector2 offset = Vector2.Zero;

        public ScrollViewerPinchZoom(ScrollViewer scrollViewer)
        {
            this.scrollViewer = scrollViewer;

            scrollViewer.AddOnMultipleTouchpoints(OnMultipleTouchpointsUpdate);
        }

        private void OnMultipleTouchpointsUpdate(ControlMultipleTouchpointsEventArgs multipleTouchpointsArgs)
        {
            var touchpoints = multipleTouchpointsArgs.Touchpoints.ToArray().Take(2).ToList();

            var isReleased = touchpoints.Any(x => x.State == TouchLocationState.Released);

            if (touchpoints.Count < 2 || !IsPinchActive && isReleased)
                return;

            var touchPositions = GetTouchpointsPositions(touchpoints);

            if (!IsPinchActive)
                OnPinchStart(touchPositions);
            else if (!isReleased)
                OnPinchMoved(touchPositions);
            else
                OnPinchReleased();
        }

        private void OnPinchStart(Vector2[] touchPositions)
        {
            IsPinchActive = true;
            lastScale = 1;
            nthZoom = 0;
            startTouchPositions = touchPositions;
            offset = -scrollViewer.GetScrollPosition();
            zoomFactor = scrollViewer.Zoom.X / GetInitialZoomFactor();
        }

        private void OnPinchMoved(Vector2[] touchpositions)
        {
            var newScale = CalculateScale(startTouchPositions, touchpositions);

            // a relative scale factor is used
            var touchCenter = GetTouchCenter(touchpositions);
            var scale = newScale / lastScale;
            lastScale = newScale;

            // the first touch events are thrown away since they are not precise
            nthZoom += 1;
            if (nthZoom > 3)
            {
                Scale(scale, touchCenter);
                Drag(touchCenter, lastZoomCenter);
            }
            lastZoomCenter = touchCenter;

            var zoomFactor = GetInitialZoomFactor() * this.zoomFactor;

            scrollViewer.SetZoom(new Vector2(zoomFactor));
            scrollViewer.MoveContainer(-offset - scrollViewer.ScrollPosition);
        }

        private void OnPinchReleased()
        {
            SetPinchAsInactive();
            scrollViewer.HideBars();
        }

        public void SetPinchAsInactive()
        {
            IsPinchActive = false;
        }

        private void AddOffset(Vector2 offset)
        {
            this.offset += offset;
        }

        private void Scale(float scale, Vector2 center)
        {
            scale = ScaleZoomFactor(scale);
            AddOffset((scale - 1) * (center + offset));
        }

        private float ScaleZoomFactor(float scale)
        {
            var originalZoomFactor = zoomFactor;
            zoomFactor *= scale;
            var initialZoomFactor = GetInitialZoomFactor();
            zoomFactor = MathHelper.Clamp(zoomFactor, scrollViewer.MinZoom.X / initialZoomFactor, scrollViewer.MaxZoom.X / initialZoomFactor);
            return zoomFactor / originalZoomFactor;
        }

        private void Drag(Vector2 center, Vector2 lastCenter)
        {
            AddOffset(-(center - lastCenter));
        }

        private float CalculateScale(Vector2[] startTouchpoints, Vector2[] endTouchpoints)
        {
            var startDistance = GetDistance(startTouchpoints[0], startTouchpoints[1]);
            var endDistance = GetDistance(endTouchpoints[0], endTouchpoints[1]);
            return endDistance / startDistance;
        }

        private Vector2[] GetTouchpointsPositions(List<TouchLocation> touchpoints)
        {
            // Disconsidere the container position
            var containerPosition = scrollViewer.DestinationRectangle.Location.ToVector2();
            return touchpoints.Select(touchpoint => touchpoint.Position - containerPosition).ToArray();
        }

        private float GetInitialZoomFactor()
            => scrollViewer.Size.X / (scrollViewer.ContainerSize.X / scrollViewer.Zoom.X);
        private Vector2 GetTouchCenter(Vector2[] touchpoints)
            => new Vector2(touchpoints.Sum(x => x.X) / touchpoints.Count(), touchpoints.Sum(x => x.Y) / touchpoints.Count());
        private float GetDistance(Vector2 a, Vector2 b)
            => (float)Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
    }
}
