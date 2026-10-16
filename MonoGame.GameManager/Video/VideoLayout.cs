using Microsoft.Xna.Framework;
using System;

namespace MonoGame.GameManager.Video
{
    /// <summary>The math that places a video inside an area (shared by every platform).</summary>
    public static class VideoLayout
    {
        /// <summary>
        /// The rectangle where a video of <paramref name="videoSize"/> is drawn inside an area of
        /// <paramref name="areaSize"/> (the area starts at 0,0). It can be larger than the area with
        /// <see cref="VideoScaleMode.Fill"/>.
        /// </summary>
        /// <param name="videoSize">The size of the video, in its pixels.</param>
        /// <param name="areaSize">The size of the area.</param>
        /// <param name="mode">How the video is fitted into the area.</param>
        /// <param name="contentOffset">
        /// Moves the picture by this many video pixels (scaled like the video), eg to center an art that is not centered
        /// in its video (see <see cref="VideoPlaybackOptions.ContentOffset"/>).
        /// </param>
        public static Rectangle Place(Point videoSize, Point areaSize, VideoScaleMode mode, Vector2 contentOffset = default)
        {
            if (videoSize.X <= 0 || videoSize.Y <= 0 || areaSize.X <= 0 || areaSize.Y <= 0)
                return new Rectangle(Point.Zero, areaSize);

            Rectangle rectangle;
            if (mode == VideoScaleMode.Stretch)
            {
                rectangle = new Rectangle(Point.Zero, areaSize);
            }
            else
            {
                var scaleX = areaSize.X / (double)videoSize.X;
                var scaleY = areaSize.Y / (double)videoSize.Y;
                var scale = mode == VideoScaleMode.Fill ? Math.Max(scaleX, scaleY) : Math.Min(scaleX, scaleY);

                var width = (int)Math.Round(videoSize.X * scale);
                var height = (int)Math.Round(videoSize.Y * scale);
                rectangle = new Rectangle((areaSize.X - width) / 2, (areaSize.Y - height) / 2, width, height);
            }

            rectangle.X += (int)Math.Round(contentOffset.X * rectangle.Width / (double)videoSize.X);
            rectangle.Y += (int)Math.Round(contentOffset.Y * rectangle.Height / (double)videoSize.Y);
            return rectangle;
        }
    }
}
