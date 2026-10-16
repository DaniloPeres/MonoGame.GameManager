using Microsoft.Xna.Framework;

namespace MonoGame.GameManager.Video
{
    /// <summary>The options of a video playback, given to <see cref="IVideoPlayerFactory.Create"/>.</summary>
    public class VideoPlaybackOptions
    {
        /// <summary>When true the video plays without sound.</summary>
        public bool IsMuted { get; set; }

        /// <summary>How the video is fitted into the screen (default: <see cref="VideoScaleMode.Fit"/>).</summary>
        public VideoScaleMode ScaleMode { get; set; } = VideoScaleMode.Fit;

        /// <summary>
        /// Moves the picture by this many pixels of the video (scaled with it): +X right, +Y down. Use it when the art is
        /// not centered in its video. With <see cref="VideoScaleMode.Fill"/> the cropped part fills the edge it uncovers;
        /// otherwise the background shows there.
        /// </summary>
        public Vector2 ContentOffset { get; set; }

        public VideoPlaybackOptions SetContentOffset(Vector2 contentOffset)
        {
            ContentOffset = contentOffset;
            return this;
        }

        public VideoPlaybackOptions SetIsMuted(bool isMuted)
        {
            IsMuted = isMuted;
            return this;
        }

        public VideoPlaybackOptions SetScaleMode(VideoScaleMode scaleMode)
        {
            ScaleMode = scaleMode;
            return this;
        }
    }
}
