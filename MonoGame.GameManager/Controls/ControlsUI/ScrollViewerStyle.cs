using Microsoft.Xna.Framework;

namespace MonoGame.GameManager.Controls.ControlsUI
{
    /// <summary>The look and the behavior of the scroll bars of a <see cref="ScrollViewer"/>.</summary>
    public class ScrollViewerStyle
    {
        public Color BarColor { get; set; } = Color.White * 0.8f;

        /// <summary>The thickness of the bars, in the units of the viewer.</summary>
        public float BarWidth { get; set; } = 5f;

        /// <summary>The distance between a bar and the edge of the viewer.</summary>
        public float BarMargin { get; set; }

        /// <summary>The minimum length of a bar.</summary>
        public float MinBarLength { get; set; } = 20f;

        /// <summary>When true (default), the bars are shown while scrolling and hidden afterwards.</summary>
        public bool AutoHide { get; set; } = true;

        /// <summary>Seconds the bars stay visible after the scrolling stops.</summary>
        public float HideDelay { get; set; } = 0.4f;

        /// <summary>The duration of the fade out of the bars, in seconds.</summary>
        public float FadeDuration { get; set; } = 0.3f;
    }
}
