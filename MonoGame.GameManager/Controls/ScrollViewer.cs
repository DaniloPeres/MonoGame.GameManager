using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;
using MonoGame.GameManager.Controls.Abstracts;
using MonoGame.GameManager.Controls.ControlsUI;
using MonoGame.GameManager.Controls.InputEvent;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Services;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Controls
{
    /// <summary>
    /// A container that shows a part of its content and scrolls it by dragging (mouse or touch), with the mouse
    /// wheel or from code, with inertia, scroll bars and pinch zoom.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The children are added to an inner panel (<see cref="ContentPanel"/>) that moves inside the viewer.
    /// <see cref="ScrollPosition"/> is the position of that panel, in the units of the viewer: (0, 0) shows the
    /// top-left of the content and the values become negative when scrolling down or to the right.
    /// </para>
    /// <para>
    /// The overflow is hidden by default. A drag starts after the pointer moves a few pixels, so the content still
    /// receives clicks; once the content scrolls, the release is not delivered to it.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var list = new ScrollViewer(new Vector2(20, 80), new Vector2(300, 400)).AddToScreen();
    /// for (var i = 0; i &lt; 50; i++)
    ///     new Label(font, "Item " + i, new Vector2(10, i * 40), Color.White).AddToScreen(list);
    /// list.ScrollToBottom(duration: 0.5f);
    /// </code>
    /// </example>
    public class ScrollViewer : ContainerAbstract<ScrollViewer>
    {
        private const float DragThreshold = 5f;
        private const float VelocitySampleSeconds = 0.1f;
        private const float MaxInertiaSpeed = 3000f;
        private const float MinInertiaSpeed = 15f;
        private const float FlingStopSpeed = 100f;
        private const float WheelDeltaPerNotch = 120f;
        private const float WheelScrollDuration = 0.12f;

        private readonly Panel content;
        private readonly Panel inputPanel;
        private readonly ScrollViewerBar[] bars;
        private readonly ScrollViewerPinchZoom pinchZoom;
        private readonly List<DragSample> dragSamples = new List<DragSample>();
        private Action onScrollPositionChanged;
        private Action onZoomChanged;
        private ScrollViewerStyle style = new ScrollViewerStyle();
        private Vector2 minZoom = new Vector2(0.25f);
        private Vector2 maxZoom = new Vector2(4f);
        private Vector2 contentSize;
        private bool isDragging;
        private bool isDragDeclined;
        private Vector2 pressPosition;
        private Vector2 lastPointerPosition;
        private Vector2 inertiaVelocity;
        private bool isAnimating;
        private Vector2 animationFrom;
        private Vector2 animationTo;
        private float animationTime;
        private float animationDuration;
        private EasingFunction animationEasing;

        /// <summary>Creates a viewer with the size of the virtual screen.</summary>
        public ScrollViewer() : this(Vector2.Zero, (ServiceProvider.ScreenManager?.ScreenSize ?? Point.Zero).ToVector2()) { }

        public ScrollViewer(Rectangle destinationRectangle) : this(destinationRectangle.Location.ToVector2(), destinationRectangle.Size.ToVector2()) { }

        public ScrollViewer(Vector2 position, Vector2 size)
            : base(position, size)
        {
            HideOverflow = true;

            content = new Panel(Vector2.Zero, size);
            base.AddChild(content);

            // A transparent panel over the content sees the presses first, so a drag can start on any child.
            inputPanel = new Panel(Vector2.Zero, size)
                .AddOnMousePressed(OnInputPressed)
                .AddOnMouseMoved(OnInputMoved)
                .AddOnMouseReleased(OnInputReleased);
            base.AddChild(inputPanel);

            bars = new[]
            {
                new ScrollViewerBar(this, ScrollViewerBarType.Horizontal),
                new ScrollViewerBar(this, ScrollViewerBarType.Vertical)
            };
            foreach (var bar in bars)
                base.AddChild(bar.RectangleBar);

            pinchZoom = new ScrollViewerPinchZoom(this);
            AddOnMouseWheel(OnMouseWheel);
            AddOnUpdateEvent(OnUpdate);
        }

        /// <summary>When true (default), the content scrolls vertically.</summary>
        public bool VerticalScrollEnabled { get; set; } = true;

        /// <summary>When true, the content scrolls horizontally (false by default).</summary>
        public bool HorizontalScrollEnabled { get; set; }

        public bool ShowVerticalScrollBar { get; set; } = true;

        public bool ShowHorizontalScrollBar { get; set; } = true;

        /// <summary>When true (default), the content keeps moving and slows down after a drag.</summary>
        public bool IsInertiaEnabled { get; set; } = true;

        /// <summary>How fast the inertia slows down (higher stops sooner).</summary>
        public float InertiaDamping { get; set; } = 3.5f;

        /// <summary>The distance scrolled by one notch of the mouse wheel, in the units of the viewer (0 disables the wheel).</summary>
        public float WheelScrollSpeed { get; set; } = 60f;

        /// <summary>When true (default), two fingers zoom the content.</summary>
        public bool IsPinchZoomEnabled
        {
            get => pinchZoom.IsEnabled;
            set => pinchZoom.IsEnabled = value;
        }

        /// <summary>The look of the scroll bars.</summary>
        public ScrollViewerStyle Style
        {
            get => style;
            set => style = value ?? new ScrollViewerStyle();
        }

        /// <summary>The inner panel that holds the content.</summary>
        public Panel ContentPanel => content;

        /// <summary>The size of the content, scaled (in the units of the parent).</summary>
        public Vector2 ContainerSize { get; private set; }

        /// <summary>The size of the content, with the zoom, in the units of the viewer.</summary>
        public Vector2 ContentSize => contentSize;

        /// <summary>The position of the content (0 or negative values).</summary>
        public Vector2 ScrollPosition => content.PositionAnchor;

        /// <summary>The scroll position at the end of the content.</summary>
        public Vector2 MinScrollPosition => Vector2.Min(Vector2.Zero, SizeWithoutScale - contentSize);

        /// <summary>True while the user drags the content.</summary>
        public bool IsDragging => isDragging;

        /// <summary>True while the content moves (drag, inertia, animation or pinch).</summary>
        public bool IsScrolling => isDragging || isAnimating || inertiaVelocity != Vector2.Zero || pinchZoom.IsPinchActive;

        public Vector2 MinZoom
        {
            get => minZoom;
            set
            {
                minZoom = value;
                Zoom = Zoom;
            }
        }

        public Vector2 MaxZoom
        {
            get => maxZoom;
            set
            {
                maxZoom = value;
                Zoom = Zoom;
            }
        }

        /// <summary>The scale of the content, clamped between <see cref="MinZoom"/> and <see cref="MaxZoom"/>.</summary>
        public Vector2 Zoom
        {
            get => content.Scale;
            set
            {
                var zoom = new Vector2(
                    MathHelper.Clamp(value.X, minZoom.X, Math.Max(minZoom.X, maxZoom.X)),
                    MathHelper.Clamp(value.Y, minZoom.Y, Math.Max(minZoom.Y, maxZoom.Y)));
                if (zoom == content.Scale)
                    return;

                content.Scale = zoom;
                RefreshContentSize();
                ApplyScrollPosition(ScrollPosition, false);
                onZoomChanged?.Invoke();
            }
        }

        public override ScrollViewer AddChild(IControl child)
        {
            content.AddChild(child);
            return this;
        }

        public override void RemoveChild(IControl child) => content.RemoveChild(child);

        public override void ClearChildren() => content.ClearChildren();

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

        public ScrollViewer SetStyle(ScrollViewerStyle style)
        {
            Style = style;
            return this;
        }

        public ScrollViewer SetZoom(Vector2 zoom)
        {
            Zoom = zoom;
            return this;
        }

        /// <summary>Changes the zoom keeping the content under <paramref name="focusPoint"/> (in the units of the viewer) in place.</summary>
        public ScrollViewer SetZoom(Vector2 zoom, Vector2 focusPoint)
        {
            var previousZoom = SafeZoom();
            var contentPoint = (focusPoint - ScrollPosition) / previousZoom;
            Zoom = zoom;
            ApplyScrollPosition(focusPoint - contentPoint * Zoom, false);
            return this;
        }

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

        public Vector2 GetScrollPosition() => ScrollPosition;

        /// <summary>Moves the content to a position immediately (see <see cref="ScrollPosition"/>).</summary>
        public ScrollViewer SetScrollPosition(Vector2 position)
        {
            StopScrolling();
            RefreshContentSize();
            ApplyScrollPosition(position, false);
            return this;
        }

        /// <summary>
        /// Moves the content to a position (see <see cref="ScrollPosition"/>), animated when
        /// <paramref name="duration"/> is greater than zero.
        /// </summary>
        public ScrollViewer ScrollTo(Vector2 position, float duration = 0f, EasingFunction easing = null)
        {
            inertiaVelocity = Vector2.Zero;
            RefreshContentSize();
            if (duration <= 0f)
            {
                isAnimating = false;
                ApplyScrollPosition(position, true);
                return this;
            }

            animationFrom = ScrollPosition;
            animationTo = ClampScrollPosition(position);
            animationTime = 0f;
            animationDuration = duration;
            animationEasing = easing ?? Easing.CubicOut;
            isAnimating = true;
            return this;
        }

        public ScrollViewer ScrollToTop(float duration = 0f) => ScrollTo(new Vector2(ScrollPosition.X, 0f), duration);

        public ScrollViewer ScrollToBottom(float duration = 0f)
        {
            RefreshContentSize();
            return ScrollTo(new Vector2(ScrollPosition.X, MinScrollPosition.Y), duration);
        }

        /// <summary>Scrolls the least possible so that a control of the content is completely visible.</summary>
        public ScrollViewer ScrollIntoView(IControl control, float duration = 0.25f)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            var scale = SafeNestedScale();
            var bounds = control.DestinationRectangle;
            var origin = control.Origin;
            var viewerLocation = DestinationRectangle.Location.ToVector2();
            var topLeft = (new Vector2(bounds.X - origin.X, bounds.Y - origin.Y) - viewerLocation) / scale;
            var bottomRight = topLeft + new Vector2(bounds.Width, bounds.Height) / scale;
            var viewport = SizeWithoutScale;

            var shift = Vector2.Zero;
            if (topLeft.X < 0f)
                shift.X = -topLeft.X;
            else if (bottomRight.X > viewport.X)
                shift.X = Math.Max(viewport.X - bottomRight.X, -topLeft.X);
            if (topLeft.Y < 0f)
                shift.Y = -topLeft.Y;
            else if (bottomRight.Y > viewport.Y)
                shift.Y = Math.Max(viewport.Y - bottomRight.Y, -topLeft.Y);

            return ScrollTo(ScrollPosition + shift, duration);
        }

        /// <summary>Moves the content by a distance in the units of the parent (disabled axes do not move).</summary>
        public void MoveContainer(Vector2 distance, bool updateBars = true)
        {
            if (!VerticalScrollEnabled)
                distance.Y = 0f;
            if (!HorizontalScrollEnabled)
                distance.X = 0f;
            ApplyScrollPosition(ScrollPosition + distance / SafeNestedScale(), updateBars);
        }

        /// <summary>Hides the scroll bars with a fade.</summary>
        public void HideBars()
        {
            foreach (var bar in bars)
                bar.FadeOutBar();
        }

        /// <summary>Stops the inertia and the scroll animation.</summary>
        public void StopScrolling()
        {
            inertiaVelocity = Vector2.Zero;
            isAnimating = false;
        }

        public ScrollViewer AddOnScrollPositionChanged(Action onScrollPositionChanged)
        {
            this.onScrollPositionChanged += onScrollPositionChanged;
            return this;
        }

        public ScrollViewer RemoveOnScrollPositionChanged(Action onScrollPositionChanged)
        {
            this.onScrollPositionChanged -= onScrollPositionChanged;
            return this;
        }

        public ScrollViewer AddOnZoomChanged(Action onZoomChanged)
        {
            this.onZoomChanged += onZoomChanged;
            return this;
        }

        public ScrollViewer RemoveOnZoomChanged(Action onZoomChanged)
        {
            this.onZoomChanged -= onZoomChanged;
            return this;
        }

        [Obsolete("Use RemoveOnZoomChanged instead.")]
        public void RemoveOnZoomChange(Action onZoomChanged) => RemoveOnZoomChanged(onZoomChanged);

        /// <summary>Converts a point of the parent's space to the space of the content position (the units of the viewer).</summary>
        internal Vector2 ToViewportPosition(Vector2 parentPoint) => ToLocalPosition(parentPoint) - OriginWithoutScale;

        /// <summary>Zoom and position of a pinch: <paramref name="contentAnchor"/> stays under <paramref name="center"/>.</summary>
        internal void ZoomFromPinch(Vector2 zoom, Vector2 center, Vector2 contentAnchor)
        {
            StopScrolling();
            Zoom = zoom;
            ApplyScrollPosition(center - contentAnchor * Zoom, true);
        }

        protected override void ArrangeChildren()
        {
            var size = SizeWithoutScale;
            if (content.SizeWithoutScale != size)
                content.SetSize(size);
            if (inputPanel.SizeWithoutScale != size)
                inputPanel.SetSize(size);

            // The content may have changed (children added, removed or resized): keep the position valid.
            RefreshContentSize();
            ApplyScrollPosition(ScrollPosition, false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                onScrollPositionChanged = null;
                onZoomChanged = null;
            }
            base.Dispose(disposing);
        }

        private void OnUpdate(GameTime gameTime)
        {
            // The interface does not follow the time scale of the game.
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            pinchZoom.Update(gameTime.TotalGameTime);

            if (isAnimating)
                UpdateAnimation(deltaSeconds);
            else if (inertiaVelocity != Vector2.Zero)
                UpdateInertia(deltaSeconds);

            var isScrolling = IsScrolling;
            foreach (var bar in bars)
                bar.Update(deltaSeconds, isScrolling);
        }

        private void UpdateAnimation(float deltaSeconds)
        {
            animationTime += deltaSeconds;
            var progress = animationDuration <= 0f ? 1f : MathHelper.Clamp(animationTime / animationDuration, 0f, 1f);
            ApplyScrollPosition(Vector2.Lerp(animationFrom, animationTo, animationEasing(progress)), true);
            if (progress >= 1f)
                isAnimating = false;
        }

        private void UpdateInertia(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
                return;

            var requested = ScrollPosition + inertiaVelocity * deltaSeconds;
            ApplyScrollPosition(requested, true);

            // An edge stops the movement on its axis.
            var reached = ScrollPosition;
            if (Math.Abs(reached.X - requested.X) > 0.01f)
                inertiaVelocity.X = 0f;
            if (Math.Abs(reached.Y - requested.Y) > 0.01f)
                inertiaVelocity.Y = 0f;

            inertiaVelocity *= (float)Math.Exp(-InertiaDamping * deltaSeconds);
            if (inertiaVelocity.Length() < MinInertiaSpeed)
                inertiaVelocity = Vector2.Zero;
        }

        private void OnInputPressed(ControlMouseEventArgs args)
        {
            var stopsMovement = inertiaVelocity.Length() > FlingStopSpeed || isAnimating;
            StopScrolling();
            isDragging = false;
            isDragDeclined = false;
            pressPosition = lastPointerPosition = args.Position.ToVector2();
            dragSamples.Clear();
            AddDragSample(pressPosition, args.Time);

            // A press that stops a moving list is not delivered to the content (it would click by accident).
            if (!stopsMovement)
                args.ContinuePropagation();
        }

        private void OnInputMoved(ControlMouseEventArgs args)
        {
            if (!inputPanel.IsMousePressed || pinchZoom.IsPinchActive)
            {
                // Not a drag of this viewer: hover and other controls receive the move.
                if (pinchZoom.IsPinchActive)
                    isDragging = false;
                args.ContinuePropagation();
                return;
            }

            var position = args.Position.ToVector2();
            if (!isDragging)
            {
                var delta = position - pressPosition;
                if (isDragDeclined || delta.Length() < DragThreshold)
                {
                    args.ContinuePropagation();
                    return;
                }

                if (!CanScrollAlong(delta / SafeNestedScale()))
                {
                    // Let an outer viewer (or the control below) handle this drag.
                    isDragDeclined = true;
                    args.ContinuePropagation();
                    return;
                }

                isDragging = true;
                lastPointerPosition = pressPosition;
                args.CapturePointer();
            }

            MoveContainer(position - lastPointerPosition);
            lastPointerPosition = position;
            AddDragSample(position, args.Time);
        }

        private void OnInputReleased(ControlMouseEventArgs args)
        {
            if (!isDragging)
            {
                args.ContinuePropagation(); // a simple click: the content receives it
                return;
            }

            isDragging = false;
            var position = args.Position.ToVector2();
            AddDragSample(position, args.Time);
            if (IsInertiaEnabled)
                inertiaVelocity = EstimateVelocity();
            dragSamples.Clear();
        }

        private void OnMouseWheel(ControlMouseEventArgs args)
        {
            if (WheelScrollSpeed <= 0f || args.ScrollWheelDelta == 0)
            {
                args.ContinuePropagation();
                return;
            }

            RefreshContentSize();
            var distance = args.ScrollWheelDelta / WheelDeltaPerNotch * WheelScrollSpeed;
            var viewport = SizeWithoutScale;
            var delta = VerticalScrollEnabled && contentSize.Y > viewport.Y ? new Vector2(0f, distance)
                : HorizontalScrollEnabled && contentSize.X > viewport.X ? new Vector2(distance, 0f)
                : Vector2.Zero;

            var start = isAnimating ? animationTo : ScrollPosition;
            var target = ClampScrollPosition(start + delta);
            if (target == start)
            {
                args.ContinuePropagation(); // nothing to scroll here: an outer viewer can scroll
                return;
            }

            ScrollTo(target, WheelScrollDuration, Easing.QuadOut);
        }

        private bool CanScrollAlong(Vector2 delta)
        {
            var viewport = SizeWithoutScale;
            var canScrollX = HorizontalScrollEnabled && contentSize.X > viewport.X;
            var canScrollY = VerticalScrollEnabled && contentSize.Y > viewport.Y;
            var x = Math.Abs(delta.X);
            var y = Math.Abs(delta.Y);
            return canScrollX && x >= y * 0.5f || canScrollY && y >= x * 0.5f;
        }

        private void AddDragSample(Vector2 position, TimeSpan time)
        {
            dragSamples.Add(new DragSample(position, time.TotalSeconds));
            if (dragSamples.Count > 32)
                dragSamples.RemoveAt(0);
        }

        /// <summary>The speed of the pointer at the end of the drag, in the units of the viewer per second.</summary>
        private Vector2 EstimateVelocity()
        {
            if (dragSamples.Count < 2)
                return Vector2.Zero;

            var last = dragSamples[dragSamples.Count - 1];
            var first = last;
            for (var i = dragSamples.Count - 2; i >= 0 && last.Time - dragSamples[i].Time <= VelocitySampleSeconds; i--)
                first = dragSamples[i];

            var elapsed = last.Time - first.Time;
            if (elapsed <= 0.001)
                return Vector2.Zero;

            var velocity = (last.Position - first.Position) / (float)elapsed / SafeNestedScale();
            if (!HorizontalScrollEnabled)
                velocity.X = 0f;
            if (!VerticalScrollEnabled)
                velocity.Y = 0f;
            if (velocity.Length() > MaxInertiaSpeed)
                velocity = Vector2.Normalize(velocity) * MaxInertiaSpeed;
            return velocity.Length() < MinInertiaSpeed ? Vector2.Zero : velocity;
        }

        private void ApplyScrollPosition(Vector2 position, bool showBars)
        {
            var clamped = ClampScrollPosition(position);
            if (clamped != content.PositionAnchor)
            {
                content.PositionAnchor = clamped;
                onScrollPositionChanged?.Invoke();
                if (showBars)
                {
                    foreach (var bar in bars)
                        bar.UpdateBar();
                }
            }
        }

        private Vector2 ClampScrollPosition(Vector2 position) => Vector2.Clamp(position, MinScrollPosition, Vector2.Zero);

        /// <summary>Measures the content from the area of the visible children of the content panel.</summary>
        private void RefreshContentSize()
        {
            var origin = content.DestinationRectangle.Location;
            var size = Vector2.Zero;
            foreach (var child in content.Children)
            {
                if (!child.IsVisible || !ReferenceEquals(child.Parent, content))
                    continue;

                var bounds = child.DestinationRectangle;
                var childOrigin = child.Origin;
                size.X = Math.Max(size.X, bounds.X - childOrigin.X + bounds.Width - origin.X);
                size.Y = Math.Max(size.Y, bounds.Y - childOrigin.Y + bounds.Height - origin.Y);
            }

            ContainerSize = size;
            contentSize = size / SafeNestedScale();
        }

        private Vector2 SafeNestedScale()
        {
            var scale = NestedScale;
            return new Vector2(Math.Abs(scale.X) < 0.0001f ? 1f : scale.X, Math.Abs(scale.Y) < 0.0001f ? 1f : scale.Y);
        }

        private Vector2 SafeZoom()
        {
            var zoom = Zoom;
            return new Vector2(Math.Abs(zoom.X) < 0.0001f ? 1f : zoom.X, Math.Abs(zoom.Y) < 0.0001f ? 1f : zoom.Y);
        }

        private struct DragSample
        {
            public DragSample(Vector2 position, double time)
            {
                Position = position;
                Time = time;
            }

            public Vector2 Position { get; }

            public double Time { get; }
        }
    }
}
