using Android.App;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Video;
using System;

namespace MonoGame.GameManager.Droid.Video
{
    /// <summary>
    /// Creates <see cref="AndroidVideoPlayer"/>s. The videos are Android assets (the game packs its content folder as
    /// <c>assets/Content/**</c>) and must be stored uncompressed (<c>AndroidStoreUncompressedFileExtensions</c> with
    /// the extension of the video, eg: .mp4) so the media player can read them.
    /// </summary>
    public sealed class AndroidVideoPlayerFactory : IVideoPlayerFactory
    {
        private readonly Activity activity;

        /// <param name="activity">The activity of the game: the video is shown over its content.</param>
        public AndroidVideoPlayerFactory(Activity activity)
        {
            this.activity = activity ?? throw new ArgumentNullException(nameof(activity));
        }

        public bool IsSupported => true;

        public IVideoPlayer Create(string contentPath, VideoPlaybackOptions options = null)
        {
            if (string.IsNullOrEmpty(contentPath))
                throw new ArgumentException("The video path cannot be empty.", nameof(contentPath));

            var root = ServiceProvider.ContentLoader.ContentManager.RootDirectory ?? string.Empty;
            var assetPath = string.IsNullOrEmpty(root) ? contentPath : root + "/" + contentPath;
            return new AndroidVideoPlayer(activity, assetPath.Replace('\\', '/'), options ?? new VideoPlaybackOptions());
        }
    }
}
