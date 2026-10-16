namespace MonoGame.GameManager.Video
{
    /// <summary>
    /// Creates the video players of one platform. The game registers the one of its platform with
    /// <c>ServiceProvider.Register&lt;IVideoPlayerFactory&gt;(...)</c> before creating the <see cref="Screens.ScreenManager"/>.
    /// </summary>
    public interface IVideoPlayerFactory
    {
        /// <summary>False when this platform cannot play videos (the players created end at once).</summary>
        bool IsSupported { get; }

        /// <summary>Creates a player for a video file of the content folder.</summary>
        /// <param name="contentPath">The path relative to the content root directory, with the extension (eg: <c>Videos/Intro.mp4</c>).</param>
        /// <param name="options">The playback options (null = the defaults).</param>
        IVideoPlayer Create(string contentPath, VideoPlaybackOptions options = null);
    }
}
