namespace MonoGame.GameManager.Core
{
    /// <summary>
    /// Something that can be played, paused, resumed, stopped and reset, such as an animation or a timer.
    /// </summary>
    public interface IPlayable
    {
        /// <summary>True while the object is running (it can still be paused).</summary>
        bool IsPlaying { get; }

        /// <summary>True while the object is paused. A paused object keeps its progress.</summary>
        bool IsPaused { get; }

        /// <summary>Starts or resumes the object. A completed object starts again from the beginning.</summary>
        void Play();

        /// <summary>Pauses the object, keeping its progress.</summary>
        void Pause();

        /// <summary>Resumes a paused object.</summary>
        void Resume();

        /// <summary>Stops the object. Calling <see cref="Play"/> afterwards continues from the current progress.</summary>
        void Stop();

        /// <summary>Moves the object back to its initial state.</summary>
        void Reset();
    }
}
