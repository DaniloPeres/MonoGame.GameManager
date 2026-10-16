namespace MonoGame.GameManager.Video
{
    /// <summary>The state of an <see cref="IVideoPlayer"/>.</summary>
    public enum VideoPlayerState
    {
        /// <summary>Created, <see cref="Core.IPlayable.Play"/> was not called yet.</summary>
        Created,

        /// <summary>Opening the file or playing it.</summary>
        Playing,

        /// <summary>Paused; the position is kept.</summary>
        Paused,

        /// <summary>Reached the end, or was stopped.</summary>
        Ended,

        /// <summary>The video could not be opened or played (see <see cref="IVideoPlayer.Error"/>).</summary>
        Failed
    }
}
