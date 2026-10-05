namespace MonoGame.GameManager.Core
{
    /// <summary>
    /// Something that is advanced once per frame, usually by an <see cref="Timers.IScheduler"/>.
    /// </summary>
    public interface IUpdatable
    {
        /// <summary>
        /// Advances the object by the given amount of time.
        /// </summary>
        /// <param name="deltaSeconds">The elapsed time since the previous update, in seconds.</param>
        void Update(float deltaSeconds);
    }
}
