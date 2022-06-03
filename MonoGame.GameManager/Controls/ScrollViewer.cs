using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.ControlsUI;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Timers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoGame.GameManager.Controls
{
    public class ScrollViewer : ContainerAbstract<ScrollViewer>
    {
        private readonly Panel touchPanelCheckScroll;
        // touchPanelFullScreen is used when the user is scrolling, use this touchPanel as mouse move and mouse release (to use all screen for scrolling events)
        private readonly Panel touchPanelFullScreen;
        private readonly Panel container;
        private const int historyScrollingMoveTotal = 6;
        private readonly List<Point> historyScrollingMove = new Point[historyScrollingMoveTotal].ToList();
        private Vector2 scrollingMoveVelocity;
        private readonly DelayTime scrollingMoveDelayAnimation;
        private double scrollingMoveAnimationTime;
        private readonly DelayTime scrollingActiveDelayTime;
        private bool isScrollingActive = false;
        private readonly List<ScrollViewerBar> bars;
        private ScrollViewerPinchZoom pinchZoom;

        public bool VerticalScrollEnabled = true;
        public bool HorizontalScrollEnabled = false;
        public bool ShowVerticalScrollBar = true;
        public bool ShowHorizontalScrollBar = true;
       
        private bool isPinchZoomActive => pinchZoom?.IsPinchActive ?? false;
        private Action onScrollPositionChanged;
        private Action onZoomChanged;

        public Vector2 ContainerSize { get; private set; }
        public Vector2 ScrollPosition => container.PositionAnchor;

        private Vector2 minZoom = new Vector2(0.25f);
        private Vector2 maxZoom = new Vector2(4f);
        public Vector2 MinZoom
        {
            get => minZoom;
            set
            {
                minZoom = value;
                // update zoom
                SetZoom(Zoom);
            }
        }
        public Vector2 MaxZoom
        {
            get => maxZoom;
            set
            {
                maxZoom = value;
                // update zoom
                SetZoom(Zoom);
            }
        }

        public Vector2 Zoom
        {
            get => container.Scale;
            set
            {
                container.Scale = value;
                CalculateContainerSize();
                CheckContainerPosition();
            }
        }

        public ScrollViewer() : this(Vector2.Zero, ServiceProvider.ScreenManager.ScreenSize.ToVector2()) { }

        public ScrollViewer(Rectangle destinationRectangle) : this(destinationRectangle.Location.ToVector2(), destinationRectangle.Size.ToVector2()) { }

        public ScrollViewer(Vector2 position, Vector2 size)
            : base(position, size)
        {
            container = new Panel()
                .AddOnUpddateDestinationRectangle(() =>
                {
                    CheckContainerPosition();
                })
                .AddOnChildRemoved(OnChildRemoved);
            base.AddChild(container);
            AddOnUpddateDestinationRectangle(UpdateContainerSize);

            touchPanelCheckScroll = new Panel()
                .AddOnMousePressed(OnMousePressed);
            base.AddChild(touchPanelCheckScroll);

            touchPanelFullScreen = new Panel()
                .AddOnMouseMoved(OnMouseMoved)
                .AddOnMouseReleased(OnMouseReleased)
                .SetZIndex(float.MaxValue - 1);

            scrollingMoveDelayAnimation = new DelayTime(0, ScrollMovingAnimation)
                .SetIsLoop(true);

            bars = new List<ScrollViewerBar>
            {
                new ScrollViewerBar(this, ScrollViewerBarType.Horizontal),
                new ScrollViewerBar(this, ScrollViewerBarType.Vertical)
            };

            bars.ForEach(bar => base.AddChild(bar.RectangleBar));

            pinchZoom = new ScrollViewerPinchZoom(this);
            scrollingActiveDelayTime = new DelayTime(0, OnScrollingActiveUpdate)
                    .SetIsLoop(true);
        }

        public override ScrollViewer AddChild(IControl child)
        {
            container.AddChild(child);
            MarkAsDirty();
            return this;
        }
        public override void RemoveChild(IControl child)
        {
            container.RemoveChild(child);
            MarkAsDirty();
        }
        public override void ClearChildren()
        {
            container.ClearChildren();
            MarkAsDirty();
        }

        public ScrollViewer SetVerticalScrollEnabled(bool verticalScrollEnabled)
        {
            VerticalScrollEnabled = verticalScrollEnabled;
            return this;
        }
        public ScrollViewer SetHorizontalScrollEnabled(bool horizontalScrollEnabled)
        {
            HorizontalScrollEnabled = horizontalScrollEnabled;
            return this;
        }
        public ScrollViewer SetShowVerticalScrollBar(bool showVerticalScrollBar)
        {
            ShowVerticalScrollBar = showVerticalScrollBar;
            return this;
        }
        public ScrollViewer SetShowHorizontalScrollBar(bool showHorizontalScrollBar)
        {
            ShowHorizontalScrollBar = showHorizontalScrollBar;
            return this;
        }

        public ScrollViewer SetZoom(Vector2 zoom)
        {
            Zoom = new Vector2(
                MathHelper.Clamp(zoom.X, MinZoom.X, MaxZoom.X),
                MathHelper.Clamp(zoom.Y, MinZoom.Y, MaxZoom.Y));
            onZoomChanged?.Invoke();
            return this;
        }

        public Vector2 GetScrollPosition() => container.PositionAnchor;

        public ScrollViewer SetMinZoom(Vector2 minZoom)
        {
            MinZoom = minZoom;
            return this;
        }

        public ScrollViewer SetMaxZoom(Vector2 maxZoom)
        {
            MaxZoom = maxZoom;
            return this;
        }

        public ScrollViewer SetScrollPosition(Vector2 position)
        {
            container.PositionAnchor = position;
            onScrollPositionChanged?.Invoke();
            return this;
        }

        protected override void UpdateDestinationRects()
        {
            base.UpdateDestinationRects();
            CalculateContainerSize();
        }

        private void CalculateContainerSize()
        {
            var containerChildren = container.Children;
            var containerSize = Vector2.Zero;
            foreach (var item in containerChildren)
            {
                var positionNested = item.PositionAnchor * NestedScale * Zoom;
                containerSize.X = Math.Max(containerSize.X, positionNested.X + item.Size.X);
                containerSize.Y = Math.Max(containerSize.Y, positionNested.Y + item.Size.Y);
            }

            ContainerSize = containerSize;
        }

        private void UpdateContainerSize()
        {
            container.SetSize(Size / NestedScale);
        }

        private void OnMousePressed(ControlMouseEventArgs args)
        {
            scrollingMoveDelayAnimation.Stop();
            touchPanelFullScreen.AddToScreen();

            // Clean and add the position to scrolling history
            for (var i = 0; i < historyScrollingMoveTotal; i++)
                historyScrollingMove[i] = Point.Zero;

            SetHistoryScrollingMove(args.Position);

            args.ShouldStopPropagation = false;
        }

        private void OnMouseMoved(ControlMouseEventArgs args)
        {
            if (isPinchZoomActive)
            {
                StopScrollingAction();
                return;
            }

            var diff = (args.Position - historyScrollingMove.First()).ToVector2();

            const int minPixelsToActiveScrolling = 5;
            if (!isScrollingActive && Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y)) < minPixelsToActiveScrolling)
            {
                args.ShouldStopPropagation = false;
                return;
            }
            MoveContainer(diff);
            SetHistoryScrollingMove(args.Position);
            isScrollingActive = true;
            scrollingActiveDelayTime.Play();
        }

        private void OnScrollingActiveUpdate()
        {
            // Add always the last position in case the user stops moving the mouse
            SetHistoryScrollingMove(historyScrollingMove[0]);
        }

        private void OnMouseReleased(ControlMouseEventArgs args)
        {
            try
            {
                pinchZoom?.SetPinchAsInactive();

                if (!isScrollingActive)
                {
                    args.ShouldStopPropagation = false;
                    return;
                }

                SetHistoryScrollingMove(args.Position);
                RunScrollMovingAnimation();
            }
            finally
            {
                StopScrollingAction();
            }
        }

        public void MoveContainer(Vector2 distance, bool updateBars = true)
        {
            if (!VerticalScrollEnabled)
                distance.Y = 0;
            if (!HorizontalScrollEnabled)
                distance.X = 0;

            var sizeScaled = Size;
            var containerSizeScaled = ContainerSize;
            var containerPositionScaled = container.PositionAnchor * NestedScale;

            // check to content do not be out of the scroll viewer
            if (containerPositionScaled.Y + distance.Y < sizeScaled.Y - containerSizeScaled.Y)
                distance.Y = sizeScaled.Y - containerSizeScaled.Y - containerPositionScaled.Y;
            if (containerPositionScaled.Y + distance.Y > 0)
                distance.Y = -containerPositionScaled.Y;
            if (containerPositionScaled.X + distance.X < sizeScaled.X - containerSizeScaled.X)
                distance.X = sizeScaled.X - containerSizeScaled.X - containerPositionScaled.X;
            if (containerPositionScaled.X + distance.X > 0)
                distance.X = -containerPositionScaled.X;

            SetScrollPosition(container.PositionAnchor + (distance / NestedScale));
            if (updateBars)
                bars.ForEach(x => x.UpdateBar());
        }

        private void StopScrollingAction()
        {
            touchPanelFullScreen.RemoveFromScreen();
            isScrollingActive = false;
            scrollingActiveDelayTime.Stop();
        }

        public void HideBars()
        {
            scrollingMoveVelocity = Vector2.Zero;
            scrollingMoveAnimationTime = 0;
            scrollingMoveDelayAnimation.Play();
        }

        public void AddOnScrollPositionChanged(Action onScrollPositionChanged)
            => this.onScrollPositionChanged += onScrollPositionChanged;

        public void RemoveOnScrollPositionChanged(Action onScrollPositionChanged)
            => this.onScrollPositionChanged -= onScrollPositionChanged;

        public void AddOnZoomChanged(Action onZoomChanged)
            => this.onZoomChanged += onZoomChanged;

        public void RemoveOnZoomChange(Action onZoomChanged)
            => this.onZoomChanged -= onZoomChanged;

        private void RunScrollMovingAnimation()
        {
            // take the history average to calculate some power to move the scroll
            var validPoints = historyScrollingMove.Where(x => x != Point.Zero).ToList();
            scrollingMoveVelocity = (validPoints.First() - validPoints.Last()).ToVector2();
            const int reduceDiffPower = 6;
            scrollingMoveVelocity /= reduceDiffPower;

            const float maxVelocity = 30;
            scrollingMoveVelocity.X = Math.Min(maxVelocity, Math.Abs(scrollingMoveVelocity.X)) * (scrollingMoveVelocity.X < 0 ? -1 : 1);
            scrollingMoveVelocity.Y = Math.Min(maxVelocity, Math.Abs(scrollingMoveVelocity.Y)) * (scrollingMoveVelocity.Y < 0 ? -1 : 1);
            scrollingMoveDelayAnimation.Play();
        }

        private void ScrollMovingAnimation()
        {
            var time = ServiceProvider.GameTime.ElapsedGameTime.TotalSeconds;

            var multiplier = (float)(time / 0.016d);

            MoveContainer(scrollingMoveVelocity * multiplier);

            // slow down the scrolling move animation
            if (scrollingMoveAnimationTime > 0.2)
                scrollingMoveVelocity *= 0.92f;
            else if (scrollingMoveAnimationTime > 0.4)
                scrollingMoveVelocity *= 0.99f;
            scrollingMoveAnimationTime += time;
            if (Math.Abs(scrollingMoveVelocity.X) <= 1 && Math.Abs(scrollingMoveVelocity.Y) <= 1)
            {
                scrollingMoveDelayAnimation.Stop();
                bars.ForEach(x => x.FadeOutBar());
            }
        }

        private void SetHistoryScrollingMove(Point pos)
        {
            // Move all items to the next item
            for (var i = historyScrollingMoveTotal - 1; i > 0; i--)
                historyScrollingMove[i] = historyScrollingMove[i - 1];
            historyScrollingMove[0] = pos;
        }

        private void CheckContainerPosition() => MoveContainer(Vector2.Zero, false);

        private void OnChildRemoved(IControl control) => MarkAsDirty();
    }
}
