using Microsoft.Xna.Framework;
using MonoGame.GameManager.Animations;

namespace MonoGame.GameManager.Controls.ControlsUI
{
    public class ScrollViewerBar
    {
        public readonly ScrollViewerBarType BarType;
        public readonly RectangleControl RectangleBar;
        private readonly ScrollViewer scrollViewer;
        private readonly Color barColor = Color.White * 0.8f;
        private const int BarWidth = 5;
        private const int BarMargin = 0;
        private FadeAnimation hideBarFadeAimation;
        private bool activeBar = false;

        public ScrollViewerBar(ScrollViewer scrollViewer, ScrollViewerBarType barType)
        {
            this.scrollViewer = scrollViewer;
            BarType = barType;
            RectangleBar = new RectangleControl(Vector2.Zero, Vector2.Zero, Color.Transparent)
                .SetAnchor(barType == ScrollViewerBarType.Horizontal ? Enums.Anchor.BottomLeft : Enums.Anchor.TopRight);
        }

        public void UpdateBar()
        {
            hideBarFadeAimation?.Stop();
            hideBarFadeAimation = null;

            if (BarType == ScrollViewerBarType.Horizontal && (!scrollViewer.HorizontalScrollEnabled || !scrollViewer.ShowHorizontalScrollBar)
                || BarType == ScrollViewerBarType.Vertical && (!scrollViewer.VerticalScrollEnabled || !scrollViewer.ShowVerticalScrollBar))
                return;

            var barSize = GetBarSize();
            if (barSize == 0)
            {
                if (RectangleBar.Color != Color.Transparent)
                    RectangleBar.SetColor(Color.Transparent);
                return;
            }

            var barPosition = GetBarPosition(barSize);

            var size = BarType == ScrollViewerBarType.Vertical
                ? new Vector2(BarWidth, barSize)
                : new Vector2(barSize, BarWidth);
            var position = BarType == ScrollViewerBarType.Vertical
                ? new Vector2(BarMargin, barPosition)
                : new Vector2(barPosition, BarMargin);

            activeBar = true;

            RectangleBar
                .SetColor(barColor)
                .SetSize(size)
                .SetPosition(position);
        }

        public void FadeOutBar()
        {
            if (!activeBar || GetBarSize() == 0)
                return;

            activeBar = false;
            var fadeDuration = 0.3f;
            hideBarFadeAimation = new FadeAnimation(RectangleBar, fadeDuration, 0.0f)
                .Play();
        }

        private float GetBarSize()
        {
            var containerSize = GetContainerSize();
            var scrollViewerSize = GetScrollViewerSize();

            if (containerSize <= scrollViewerSize)
                return 0;

            return (float)scrollViewerSize / containerSize * scrollViewerSize / GetScrollViewerNestedScale();
        }

        private float GetBarPosition(float barSize)
        {
            var containerSize = GetContainerSize();
            var scrollViewerSize = GetScrollViewerSize();

            var emptyScrollViewerSize = scrollViewerSize - containerSize;
            var positionRate = GetScrollPosition() / emptyScrollViewerSize;
            return positionRate * (scrollViewerSize - barSize * GetScrollViewerNestedScale());
        }

        private float GetScrollPosition() => BarType == ScrollViewerBarType.Horizontal
            ? scrollViewer.ScrollPosition.X
            : scrollViewer.ScrollPosition.Y;

        private int GetContainerSize() => BarType == ScrollViewerBarType.Horizontal
            ? (int)scrollViewer.ContainerSize.X
            : (int)scrollViewer.ContainerSize.Y;

        private int GetScrollViewerSize() => BarType == ScrollViewerBarType.Horizontal
            ? scrollViewer.DestinationRectangle.Width
            : scrollViewer.DestinationRectangle.Height;

        private float GetScrollViewerNestedScale() => BarType == ScrollViewerBarType.Horizontal
            ? scrollViewer.NestedScale.X
            : scrollViewer.NestedScale.Y;
    }
}
