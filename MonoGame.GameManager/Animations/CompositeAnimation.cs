using MonoGame.GameManager.Core;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Timers;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Base class of the animations made of other animations (Composite pattern).
    /// The children are driven by the composite: they must not be played on their own.
    /// </summary>
    public abstract class CompositeAnimation<TComposite> : IAnimation where TComposite : CompositeAnimation<TComposite>
    {
        private Action onStarted;
        private Action onCompleted;
        private IScheduler scheduler;
        private IScheduler registeredScheduler;
        private bool hasStarted;

        /// <summary>The child animations, in the order they were added.</summary>
        protected readonly List<IAnimation> Children = new List<IAnimation>();

        /// <summary>The child animations, in the order they were added.</summary>
        public IReadOnlyList<IAnimation> Animations => Children;

        /// <inheritdoc />
        public abstract float Duration { get; }

        /// <inheritdoc />
        public bool IsPlaying { get; private set; }

        /// <inheritdoc />
        public bool IsPaused { get; private set; }

        /// <inheritdoc />
        public bool IsCompleted { get; private set; }

        /// <summary>When true, the composite starts again when it reaches the end.</summary>
        public bool IsLooping { get; set; }

        /// <inheritdoc />
        public event Action Completed
        {
            add => onCompleted += value;
            remove => onCompleted -= value;
        }

        /// <summary>This instance typed as <typeparamref name="TComposite"/>, for fluent methods.</summary>
        protected TComposite ThisAsT => (TComposite)this;

        /// <summary>Adds a child animation.</summary>
        public TComposite Add(IAnimation animation)
        {
            if (animation == null)
                throw new ArgumentNullException(nameof(animation));
            if (ReferenceEquals(animation, this))
                throw new ArgumentException("An animation cannot contain itself.", nameof(animation));
            Children.Add(animation);
            return ThisAsT;
        }

        /// <summary>Adds a pause, in seconds.</summary>
        public TComposite AddDelay(float seconds) => Add(new DelayStep(seconds));

        /// <summary>Adds an action that runs instantly when it is reached.</summary>
        public TComposite AddCallback(Action action) => Add(new CallbackStep(action));

        public TComposite SetIsLooping(bool isLooping)
        {
            IsLooping = isLooping;
            return ThisAsT;
        }

        /// <summary>
        /// Sets the scheduler that updates this composite. By default the scheduler of the current screen is used.
        /// </summary>
        public TComposite SetScheduler(IScheduler scheduler)
        {
            this.scheduler = scheduler;
            if (registeredScheduler != null && IsPlaying)
                Register();
            return ThisAsT;
        }

        public TComposite AddOnStarted(Action onStarted)
        {
            this.onStarted += onStarted;
            return ThisAsT;
        }

        public TComposite AddOnCompleted(Action onCompleted)
        {
            this.onCompleted += onCompleted;
            return ThisAsT;
        }

        /// <summary>Plays the composite. A completed composite starts again from the beginning.</summary>
        public TComposite Play()
        {
            if (IsCompleted || !hasStarted)
                Begin();
            IsPlaying = true;
            IsPaused = false;
            Register();
            return ThisAsT;
        }

        /// <inheritdoc />
        public void Start()
        {
            Unregister();
            Begin();
            IsPlaying = true;
            IsPaused = false;
        }

        public TComposite Stop()
        {
            Unregister();
            IsPlaying = false;
            IsPaused = false;
            return ThisAsT;
        }

        public TComposite Pause()
        {
            if (IsPlaying)
                IsPaused = true;
            return ThisAsT;
        }

        public TComposite Resume()
        {
            IsPaused = false;
            return ThisAsT;
        }

        public TComposite Reset()
        {
            Begin();
            hasStarted = IsPlaying;
            return ThisAsT;
        }

        /// <inheritdoc />
        public void Update(float deltaSeconds)
        {
            if (!IsPlaying || IsPaused || IsCompleted)
                return;

            if (UpdateChildren(Math.Max(0f, deltaSeconds)))
                Finish();
        }

        void IPlayable.Play() => Play();

        void IPlayable.Pause() => Pause();

        void IPlayable.Resume() => Resume();

        void IPlayable.Stop() => Stop();

        void IPlayable.Reset() => Reset();

        /// <summary>Starts the children from the beginning.</summary>
        protected abstract void StartChildren();

        /// <summary>Updates the children. Returns true when the composite reached its end.</summary>
        protected abstract bool UpdateChildren(float deltaSeconds);

        private void Begin()
        {
            IsCompleted = false;
            hasStarted = true;
            StartChildren();
            onStarted?.Invoke();
        }

        private void Finish()
        {
            if (IsLooping)
            {
                StartChildren();
                return;
            }

            IsCompleted = true;
            IsPlaying = false;
            Unregister();
            onCompleted?.Invoke();
        }

        private void Register()
        {
            var target = scheduler ?? ServiceProvider.Scheduler;
            if (registeredScheduler != null && registeredScheduler != target)
                registeredScheduler.Remove(this);
            registeredScheduler = target;
            target.Add(this);
        }

        private void Unregister()
        {
            registeredScheduler?.Remove(this);
            registeredScheduler = null;
        }

        private sealed class DelayStep : IAnimation
        {
            private float elapsed;

            public DelayStep(float duration) => Duration = Math.Max(0f, duration);

            public float Duration { get; }

            public bool IsCompleted { get; private set; }

            public bool IsPlaying { get; private set; }

            public bool IsPaused { get; private set; }

            public event Action Completed;

            public void Start()
            {
                elapsed = 0f;
                IsPaused = false;
                IsCompleted = Duration <= 0f;
                IsPlaying = !IsCompleted;
                if (IsCompleted)
                    Completed?.Invoke();
            }

            public void Update(float deltaSeconds)
            {
                if (!IsPlaying || IsPaused || IsCompleted)
                    return;

                elapsed += deltaSeconds;
                if (elapsed < Duration)
                    return;

                IsCompleted = true;
                IsPlaying = false;
                Completed?.Invoke();
            }

            public void Play() => Start();

            public void Pause() => IsPaused = true;

            public void Resume() => IsPaused = false;

            public void Stop() => IsPlaying = false;

            public void Reset() => elapsed = 0f;
        }

        private sealed class CallbackStep : IAnimation
        {
            private readonly Action action;

            public CallbackStep(Action action) => this.action = action ?? throw new ArgumentNullException(nameof(action));

            public float Duration => 0f;

            public bool IsCompleted { get; private set; }

            public bool IsPlaying => false;

            public bool IsPaused => false;

            public event Action Completed;

            public void Start()
            {
                IsCompleted = true;
                action();
                Completed?.Invoke();
            }

            public void Update(float deltaSeconds) { }

            public void Play() => Start();

            public void Pause() { }

            public void Resume() { }

            public void Stop() { }

            public void Reset() => IsCompleted = false;
        }
    }
}
