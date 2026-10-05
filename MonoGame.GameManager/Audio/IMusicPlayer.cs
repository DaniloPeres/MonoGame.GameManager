using Microsoft.Xna.Framework.Media;

namespace MonoGame.GameManager.Audio
{
    /// <summary>
    /// Abstraction over the static MonoGame <see cref="MediaPlayer"/>, so the <see cref="AudioManager"/> does not
    /// depend on a static class (and can be tested or replaced).
    /// </summary>
    public interface IMusicPlayer
    {
        float Volume { get; set; }

        bool IsRepeating { get; set; }

        bool IsPlaying { get; }

        bool IsPaused { get; }

        void Play(Song song);

        void Pause();

        void Resume();

        void Stop();
    }

    /// <summary>
    /// Default <see cref="IMusicPlayer"/>, backed by <see cref="MediaPlayer"/>.
    /// </summary>
    public class MediaPlayerMusicPlayer : IMusicPlayer
    {
        public float Volume
        {
            get => MediaPlayer.Volume;
            set => MediaPlayer.Volume = value;
        }

        public bool IsRepeating
        {
            get => MediaPlayer.IsRepeating;
            set => MediaPlayer.IsRepeating = value;
        }

        public bool IsPlaying => MediaPlayer.State == MediaState.Playing;

        public bool IsPaused => MediaPlayer.State == MediaState.Paused;

        public void Play(Song song) => MediaPlayer.Play(song);

        public void Pause() => MediaPlayer.Pause();

        public void Resume() => MediaPlayer.Resume();

        public void Stop() => MediaPlayer.Stop();
    }
}
