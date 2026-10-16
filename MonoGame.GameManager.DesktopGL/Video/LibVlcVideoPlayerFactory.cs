using LibVLCSharp.Shared;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Video;
using System;
using System.IO;

namespace MonoGame.GameManager.DesktopGL.Video
{
    /// <summary>
    /// Creates <see cref="LibVlcVideoPlayer"/>s. LibVLC is started on the first use; when its native files are
    /// missing <see cref="IsSupported"/> is false and the players created fail without breaking the game.
    /// </summary>
    /// <remarks>
    /// The game must reference the native package of its OS (eg: <c>VideoLAN.LibVLC.Windows</c>), which copies the
    /// <c>libvlc</c> folder next to the executable. On Linux and macOS, VLC has to be installed.
    /// </remarks>
    public sealed class LibVlcVideoPlayerFactory : IVideoPlayerFactory, IDisposable
    {
        private readonly object syncRoot = new object();
        private LibVLC libVlc;
        private string initializationError;
        private bool isInitialized;

        public bool IsSupported
        {
            get
            {
                EnsureInitialized();
                return libVlc != null;
            }
        }

        public IVideoPlayer Create(string contentPath, VideoPlaybackOptions options = null)
        {
            if (string.IsNullOrEmpty(contentPath))
                throw new ArgumentException("The video path cannot be empty.", nameof(contentPath));

            EnsureInitialized();
            if (libVlc == null)
                return new UnsupportedVideoPlayer(initializationError);

            var fullPath = ResolvePath(contentPath);
            if (!File.Exists(fullPath))
                return new UnsupportedVideoPlayer($"The video file was not found: {fullPath}");

            try
            {
                return new LibVlcVideoPlayer(libVlc, fullPath, options ?? new VideoPlaybackOptions());
            }
            catch (Exception exception)
            {
                return new UnsupportedVideoPlayer($"The video could not be opened: {exception.Message}");
            }
        }

        public void Dispose()
        {
            lock (syncRoot)
            {
                libVlc?.Dispose();
                libVlc = null;
            }
        }

        private static string ResolvePath(string contentPath)
        {
            var root = ServiceProvider.ContentLoader.ContentManager.RootDirectory ?? string.Empty;
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, root, contentPath.Replace('\\', '/')));
        }

        private void EnsureInitialized()
        {
            lock (syncRoot)
            {
                if (isInitialized)
                    return;

                isInitialized = true;
                try
                {
                    LibVLCSharp.Shared.Core.Initialize();
                    libVlc = new LibVLC();
                }
                catch (Exception exception)
                {
                    initializationError = $"LibVLC could not be started (is the native LibVLC package referenced?): {exception.Message}";
                }
            }
        }
    }
}
