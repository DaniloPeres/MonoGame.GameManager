using Microsoft.Xna.Framework;
using MonoGame.GameManager.Enums;
using System;

namespace MonoGame.GameManager.Controls.ControlsUI
{
    /// <summary>
    /// A scroll bar of a <see cref="ScrollViewer"/> (created by the viewer). It shows which part of the content is
    /// visible and fades out when the content stops moving (see <see cref="ScrollViewerStyle"/>).
    /// </summary>
    public class ScrollViewerBar
    {
        public readonly ScrollViewerBarType BarType;
        public readonly RectangleControl RectangleBar;
        private readonly ScrollViewer scrollViewer;
        private float idleTime;
        private bool isFading;

        public ScrollViewerBar(ScrollViewer scrollViewer, ScrollViewerBarType barType)
        {
            this.scrollViewer = scrollViewer ?? throw new ArgumentNullException(nameof(scrollViewer));
            BarType = barType;
            RectangleBar = new RectangleControl(Vector2.Zero, Vector2.Zero, Color.White)
                .SetAnchor(barType == ScrollViewerBarType.Horizontal ? Anchor.BottomLeft : Anchor.TopRight)
                .SetOpacity(0f);
        }

        /// <summary>True when the bar has something to show: its axis scrolls, it is enabled and the content is larger than the viewer.</summary>
        public bool IsNeeded
        {
            get
            {
                var isHorizontal = BarType == ScrollViewerBarType.Horizontal;
                var enabled = isHorizontal
                    ? scrollViewer.HorizontalScrollEnabled && scrollViewer.ShowHorizontalScrollBar
                    : scrollViewer.VerticalScrollEnabled && scrollViewer.ShowVerticalScrollBar;
                return enabled && GetContentLength() > GetViewportLength() + 0.5f;
            }
        }

        /// <summary>Updates the size and the position of the bar and shows it.</summary>
        public void UpdateBar()
        {
            RefreshGeometry();
            if (!IsNeeded)
            {
                RectangleBar.Opacity = 0f;
                return;
            }

            RectangleBar.Opacity = 1f;
            idleTime = 0f;
            isFading = false;
        }

        /// <summary>Starts hiding the bar.</summary>
        public void FadeOutBar()
        {
            if (RectangleBar.Opacity > 0f)
                isFading = true;
        }

        /// <summary>Advances the automatic hiding. Called every frame by the viewer.</summary>
        /// <param name="deltaSeconds">The duration of the frame.</param>
        /// <param name="isScrolling">True while the content is moving or being dragged (the bar stays visible).</param>
        public void Update(float deltaSeconds, bool isScrolling)
        {
            var style = scrollViewer.Style;
            if (!IsNeeded)
            {
                RectangleBar.Opacity = 0f;
                return;
            }

            if (!style.AutoHide)
            {
                RectangleBar.Opacity = 1f;
                RefreshGeometry();
                return;
            }

            if (RectangleBar.Opacity <= 0f)
                return;

            RefreshGeometry();
            if (isScrolling)
            {
                idleTime = 0f;
                isFading = false;
                RectangleBar.Opacity = 1f;
                return;
            }

            idleTime += deltaSeconds;
            if (!isFading && idleTime >= style.HideDelay)
                isFading = true;

            if (isFading)
                RectangleBar.Opacity = style.FadeDuration <= 0f ? 0f : RectangleBar.Opacity - deltaSeconds / style.FadeDuration;
        }

        /// <summary>Updates the size, the position and the color of the bar.</summary>
        public void RefreshGeometry()
        {
            var style = scrollViewer.Style;
            var viewport = GetViewportLength();
            var content = GetContentLength();
            if (content <= viewport || viewport <= 0f)
            {
                if (RectangleBar.SizeWithoutScale != Vector2.Zero)
                    RectangleBar.SetSize(Vector2.Zero);
                return;
            }

            var length = MathHelper.Clamp(viewport * viewport / content, Math.Min(style.MinBarLength, viewport), viewport);
            var scrollRange = content - viewport;
            var rate = MathHelper.Clamp(-GetScrollPosition() / scrollRange, 0f, 1f);
            var offset = rate * (viewport - length);

            var isHorizontal = BarType == ScrollViewerBarType.Horizontal;
            var size = isHorizontal ? new Vector2(length, style.BarWidth) : new Vector2(style.BarWidth, length);
            var position = isHorizontal ? new Vector2(offset, style.BarMargin) : new Vector2(style.BarMargin, offset);

            if (RectangleBar.SizeWithoutScale != size)
                RectangleBar.SetSize(size);
            if (RectangleBar.PositionAnchor != position)
                RectangleBar.SetPosition(position);
            RectangleBar.Color = style.BarColor;
        }

        private float GetScrollPosition() => BarType == ScrollViewerBarType.Horizontal ? scrollViewer.ScrollPosition.X : scrollViewer.ScrollPosition.Y;

        private float GetViewportLength() => BarType == ScrollViewerBarType.Horizontal ? scrollViewer.SizeWithoutScale.X : scrollViewer.SizeWithoutScale.Y;

        private float GetContentLength() => BarType == ScrollViewerBarType.Horizontal ? scrollViewer.ContentSize.X : scrollViewer.ContentSize.Y;
    }
}
