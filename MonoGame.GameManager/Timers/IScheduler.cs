using MonoGame.GameManager.Core;
using System;

namespace MonoGame.GameManager.Timers
{
    /// <summary>
    /// Updates registered <see cref="IUpdatable"/> objects (animations, timers, state machines...) once per frame
    /// and schedules delayed or repeated actions.
    /// </summary>
    /// <remarks>
    /// Every <see cref="Screens.Screen"/> owns a scheduler that is cleared when the screen is closed, so anything
    /// registered on it stops automatically. <see cref="Services.ServiceProvider.GlobalScheduler"/> lives for the
    /// whole game.
    /// </remarks>
    public interface IScheduler : IUpdatable
    {
        /// <summary>Multiplier applied to the elapsed time of every update (1 = normal speed, 0 = frozen).</summary>
        float TimeScale { get; set; }

        /// <summary>True while the scheduler is paused. A paused scheduler does not update anything.</summary>
        bool IsPaused { get; }

        /// <summary>Number of registered objects.</summary>
        int Count { get; }

        /// <summary>Pauses every registered object.</summary>
        void Pause();

        /// <summary>Resumes the scheduler after <see cref="Pause"/>.</summary>
        void Resume();

        /// <summary>Runs <paramref name="action"/> once after <paramref name="seconds"/>.</summary>
        ScheduledAction Delay(float seconds, Action action);

        /// <summary>Runs <paramref name="action"/> every <paramref name="intervalSeconds"/>.</summary>
        /// <param name="intervalSeconds">The interval between two invocations, in seconds.</param>
        /// <param name="action">The action to run.</param>
        /// <param name="repeatCount">The total number of invocations, or -1 to repeat until cancelled.</param>
        ScheduledAction Every(float intervalSeconds, Action action, int repeatCount = -1);

        /// <summary>Runs <paramref name="action"/> once, on the next update of this scheduler.</summary>
        ScheduledAction NextFrame(Action action);

        /// <summary>Runs <paramref name="action"/> on every update with the elapsed time, until cancelled.</summary>
        ScheduledAction EveryFrame(Action<float> action);

        /// <summary>Registers an object to be updated every frame. Registering twice has no effect.</summary>
        void Add(IUpdatable updatable);

        /// <summary>Unregisters an object. Returns false if it was not registered.</summary>
        bool Remove(IUpdatable updatable);

        /// <summary>Returns true if the object is registered.</summary>
        bool Contains(IUpdatable updatable);

        /// <summary>Unregisters every object.</summary>
        void Clear();
    }
}
