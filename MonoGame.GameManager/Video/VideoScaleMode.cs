namespace MonoGame.GameManager.Video
{
    /// <summary>How a video is fitted into the area where it is shown.</summary>
    public enum VideoScaleMode
    {
        /// <summary>The whole video is visible and keeps its aspect ratio (bars appear when the ratios differ).</summary>
        Fit,

        /// <summary>The video covers the whole area and keeps its aspect ratio (the parts outside are cropped).</summary>
        Fill,

        /// <summary>The video is stretched to the area, ignoring its aspect ratio.</summary>
        Stretch
    }
}
