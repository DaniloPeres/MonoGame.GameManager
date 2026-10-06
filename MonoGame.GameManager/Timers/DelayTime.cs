using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Core;
using MonoGame.GameManager.Services;
using System;

namespace MonoGame.GameManager.Timers
{
    /// <summary>
    /// A timer that invokes an action after a delay, once or repeatedly.
    /// </summary>
    /// <remarks>
    /// <see cref="Play"/> registers the timer on <see cref="SetScheduler">its scheduler</see> or, by default, on the
    /// scheduler of the current screen, so it stops automatically when the screen is closed.
    /// For simple cases, <see cref="IScheduler.Delay"/> and <see cref="IScheduler.Every"/> are shorter.
    /// </remarks>
    public class DelayTime : IUpdatable, IPlayable
    {
        private Action onTimeEnd;
        private IScheduler scheduler;
        private IScheduler registeredScheduler;
        private IControl owner;
        private bool isSubscribedToOwner;
        private float elapsed;
        private int completedCycles;

        /// <param name="delayDuration">The delay in seconds.</param>
        /// <param name="onTimeEnd">The action invoked when the delay ends.</param>
        public DelayTime(float delayDuration, Action onTimeEnd)
        {
            DelayDuration = Math.Max(0f, delayDuration);
            this.onTimeEnd = onTimeEnd;
        }

        /// <summary>Creates a timer and plays it immediately.</summary>
        public static DelayTime DelayAction(float delayDuration, Action onTimeEnd)
            => new DelayTime(delayDuration, onTimeEnd).Play();

        /// <summary>The delay in seconds.</summary>
        public float DelayDuration { get; private set; }

        /// <summary>Seconds elapsed in the current cycle.</summary>
        public float Elapsed => elapsed;

        /// <summary>Progress of the current cycle, from 0 to 1.</summary>
        public float Progress => DelayDuration <= 0f ? (IsCompleted ? 1f : 0f) : Math.Min(1f, elapsed / DelayDuration);

        /// <summary>Seconds left in the current cycle.</summary>
        public float RemainingTime => Math.Max(0f, DelayDuration - elapsed);

        /// <inheritdoc />
        public bool IsPlaying { get; private set; }

        /// <inheritdoc />
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Number of extra cycles after the first one. 0 runs once, 2 runs three times, -1 repeats forever.
        /// </summary>
        public int RepeatCount { get; set; }

        /// <summary>True when the timer repeats forever (same as <see cref="RepeatCount"/> = -1).</summary>
        public bool IsLooping
        {
            get => RepeatCount < 0;
            set => RepeatCount = value ? -1 : 0;
        }

        /// <summary>True when the last cycle ended.</summary>
        public bool IsCompleted { get; private set; }

        public DelayTime SetDelayDuration(float delayDuration)
        {
            DelayDuration = Math.Max(0f, delayDuration);
            return this;
        }

        /// <summary>Replaces the action invoked when the delay ends.</summary>
        public DelayTime SetOnTimeEnd(Action onTimeEnd)
        {
            this.onTimeEnd = onTimeEnd;
            return this;
        }

        /// <summary>
        /// Binds the lifetime of the timer to a control: the timer stops when that control is disposed.
        /// </summary>
        public DelayTime SetParent(IControl parent)
        {
            var wasSubscribed = isSubscribedToOwner;
            UnsubscribeFromOwner();
            owner = parent;
            if (wasSubscribed)
                SubscribeToOwner();
            return this;
        }

        /// <summary>
        /// Sets the scheduler that updates this timer. By default the scheduler of the current screen is used.
        /// </summary>
        public DelayTime SetScheduler(IScheduler scheduler)
        {
            this.scheduler = scheduler;
            if (registeredScheduler != null && IsPlaying)
                Register();
            return this;
        }

        public DelayTime SetIsLooping(bool isLooping)
        {
            IsLooping = isLooping;
            return this;
        }

        [Obsolete("Use SetIsLooping(bool) instead.")]
        public DelayTime SetIsLoop(bool loop) => SetIsLooping(loop);

        public DelayTime SetRepeatCount(int repeatCount)
        {
            RepeatCount = repeatCount < 0 ? -1 : repeatCount;
            return this;
        }

        /// <summary>
        /// Plays the timer. A stopped timer continues from where it stopped; a completed timer starts again.
        /// </summary>
        public DelayTime Play()
        {
            if (IsCompleted)
                Reset();
            IsPlaying = true;
            IsPaused = false;
            Register();
            return this;
        }

        /// <summary>Stops the timer, keeping its progress.</summary>
        public DelayTime Stop()
        {
            Unregister();
            IsPlaying = false;
            IsPaused = false;
            return this;
        }

        /// <summary>Pauses a playing timer.</summary>
        public DelayTime Pause()
        {
            if (IsPlaying)
                IsPaused = true;
            return this;
        }

        /// <summary>Resumes a paused timer.</summary>
        public DelayTime Resume()
        {
            IsPaused = false;
            return this;
        }

        /// <summary>Moves the timer back to the start of its first cycle.</summary>
        public DelayTime Reset()
        {
            elapsed = 0f;
            completedCycles = 0;
            IsCompleted = false;
            return this;
        }

        /// <inheritdoc />
        public void Update(float deltaSeconds)
        {
            if (!IsPlaying || IsPaused || IsCompleted)
                return;

            elapsed += Math.Max(0f, deltaSeconds);
            if (elapsed < DelayDuration)
                return;

            // Keep the remainder to avoid drift, but never invoke more than once per update.
            elapsed = DelayDuration > 0f ? (elapsed - DelayDuration) % DelayDuration : 0f;

            if (RepeatCount >= 0 && completedCycles >= RepeatCount)
            {
                IsCompleted = true;
                Stop();
            }
            else
            {
                completedCycles++;
            }

            onTimeEnd?.Invoke();
        }

        void IPlayable.Play() => Play();

        void IPlayable.Pause() => Pause();

        void IPlayable.Resume() => Resume();

        void IPlayable.Stop() => Stop();

        void IPlayable.Reset() => Reset();

        private void Register()
        {
            var target = scheduler ?? ServiceProvider.Scheduler;
            if (registeredScheduler != null && registeredScheduler != target)
                registeredScheduler.Remove(this);
            registeredScheduler = target;
            target.Add(this);
            SubscribeToOwner();
        }

        private void Unregister()
        {
            registeredScheduler?.Remove(this);
            registeredScheduler = null;
            UnsubscribeFromOwner();
        }

        private void SubscribeToOwner()
        {
            if (isSubscribedToOwner || owner == null)
                return;
            owner.Disposed += OnOwnerDisposed;
            isSubscribedToOwner = true;
        }

        private void UnsubscribeFromOwner()
        {
            if (!isSubscribedToOwner)
                return;
            owner.Disposed -= OnOwnerDisposed;
            isSubscribedToOwner = false;
        }

        private void OnOwnerDisposed(IControl control) => Stop();
    }
}
