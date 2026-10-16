using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoGame.GameManager.Video
{
    /// <summary>
    /// A player that plays nothing (Null Object): it ends on the first update, so the game goes on when a platform
    /// has no video support or a video cannot be opened.
    /// </summary>
    public sealed class UnsupportedVideoPlayer : IVideoPlayer
    {
        private bool finishedRaised;
        private bool isDisposed;

        /// <param name="error">Why the video cannot play (null = it is just not supported, state Ended).</param>
        public UnsupportedVideoPlayer(string error = null)
        {
            Error = error;
            State = string.IsNullOrEmpty(error) ? VideoPlayerState.Created : VideoPlayerState.Failed;
        }

        public VideoPlayerState State { get; private set; }

        public string Error { get; }

        public bool IsMuted { get; set; }

        public Point Size => Point.Zero;

        public Texture2D FrameTexture => null;

        public bool IsPlaying => false;

        public bool IsPaused => false;

        public event Action<IVideoPlayer> Finished;

        public void Play() { }

        public void Pause() { }

        public void Resume() { }

        public void Stop() { }

        public void Reset() { }

        public void Update(float deltaSeconds)
        {
            if (isDisposed || finishedRaised)
                return;

            finishedRaised = true;
            if (State != VideoPlayerState.Failed)
                State = VideoPlayerState.Ended;
            Finished?.Invoke(this);
        }

        public void Dispose() => isDisposed = true;
    }

    /// <summary>The factory registered by default: no video support.</summary>
    public sealed class UnsupportedVideoPlayerFactory : IVideoPlayerFactory
    {
        public bool IsSupported => false;

        public IVideoPlayer Create(string contentPath, VideoPlaybackOptions options = null)
            => new UnsupportedVideoPlayer("No video player is registered for this platform.");
    }
}
