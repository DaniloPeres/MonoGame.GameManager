using Microsoft.Xna.Framework;
using MonoGame.GameManager.Controls.Interfaces;
using MonoGame.GameManager.Core;
using MonoGame.GameManager.Services;
using MonoGame.GameManager.Timers;
using System;

namespace MonoGame.GameManager.Animations
{
    /// <summary>
    /// Base class of every animation (Template Method pattern). It handles timing, start delay, repetitions,
    /// ping-pong, easing, callbacks and the registration in a scheduler, and calls
    /// <see cref="OnUpdateAnimation"/> with the eased progress so subclasses only apply the value.
    /// </summary>
    /// <remarks>
    /// <see cref="Play"/> registers the animation on <see cref="SetScheduler">its scheduler</see> or, by default,
    /// on the scheduler of the current screen, so it stops automatically when the screen is closed.
    /// The animation also stops when its control (or the owner set with <see cref="SetParent"/>) is disposed.
    /// </remarks>
    /// <typeparam name="TAnimation">The concrete animation type, returned by the fluent methods.</typeparam>
    public abstract class AnimationAbstract<TAnimation> : IAnimation where TAnimation : AnimationAbstract<TAnimation>
    {
        private const int MaxCyclesPerUpdate = 1000;

        private Action onStarted;
        private Action onAnimationUpdated;
        private Action onAnimationEnd;
        private Action onCompleted;
        private Action onStopped;
        private IScheduler scheduler;
        private IScheduler registeredScheduler;
        private IControl owner;
        private bool isSubscribedToDisposal;
        private float elapsed;
        private float delayRemaining;
        private float loopDelayRemaining;
        private int completedCycles;
        private bool hasStarted;
        private bool startValuesCaptured;
        private int stateVersion;
        private EasingFunction easingFunction = Easing.Linear;

        /// <summary>
        /// Creates an animation.
        /// </summary>
        /// <param name="control">The animated control. It can be null for animations that do not target a control.</param>
        /// <param name="duration">The duration of one cycle, in seconds.</param>
        protected AnimationAbstract(IControl control, float duration)
        {
            Control = control;
            Duration = duration;
        }

        /// <summary>The animated control (can be null).</summary>
        public IControl Control { get; }

        /// <summary>The duration of one cycle, in seconds. A duration of 0 completes on the first update.</summary>
        public float Duration { get; private set; }

        /// <summary>Time to wait, in seconds, after <see cref="Play"/> before the first cycle starts.</summary>
        public float Delay { get; private set; }

        /// <summary>Time to wait, in seconds, between two cycles.</summary>
        public float LoopingDelayTimeDuration { get; private set; }

        /// <summary>
        /// Number of extra cycles after the first one. 0 plays once, 2 plays three times, -1 repeats forever.
        /// </summary>
        public int RepeatCount { get; set; }

        /// <summary>True when the animation repeats forever (same as <see cref="RepeatCount"/> = -1).</summary>
        public bool IsLooping
        {
            get => RepeatCount < 0;
            set => RepeatCount = value ? -1 : 0;
        }

        /// <summary>When true, every other cycle plays backwards.</summary>
        public bool IsPingPong { get; set; }

        /// <summary>When true, the animation plays from the end value to the start value.</summary>
        public bool IsReverse { get; set; }

        /// <summary>The easing curve applied to the progress. Defaults to <see cref="Easing.Linear"/>.</summary>
        public EasingFunction EasingFunction
        {
            get => easingFunction;
            set => easingFunction = value ?? Easing.Linear;
        }

        /// <inheritdoc />
        public bool IsPlaying { get; private set; }

        /// <inheritdoc />
        public bool IsPaused { get; private set; }

        /// <inheritdoc />
        public bool IsCompleted { get; private set; }

        /// <summary>Index of the current cycle (0 for the first one).</summary>
        public int CurrentCycle => completedCycles;

        /// <summary>Linear progress of the current cycle, from 0 to 1.</summary>
        public float Progress => Duration > 0f
            ? MathHelper.Clamp(elapsed / Duration, 0f, 1f)
            : (IsCompleted ? 1f : 0f);

        /// <summary>When true, the control is removed from the screen when the animation completes.</summary>
        public bool ShouldRemoveControlOnAnimationEnd { get; set; }

        /// <inheritdoc />
        public event Action Completed
        {
            add => onCompleted += value;
            remove => onCompleted -= value;
        }

        /// <summary>This instance typed as <typeparamref name="TAnimation"/>, for fluent methods.</summary>
        protected TAnimation ThisAsT => (TAnimation)this;

        public TAnimation SetDuration(float duration)
        {
            Duration = duration;
            return ThisAsT;
        }

        public TAnimation SetDelay(float delay)
        {
            Delay = Math.Max(0f, delay);
            if (!hasStarted)
                delayRemaining = Delay;
            return ThisAsT;
        }

        public TAnimation SetEasing(EasingFunction easing)
        {
            EasingFunction = easing;
            return ThisAsT;
        }

        public TAnimation SetEasing(EasingType easingType) => SetEasing(Easing.Get(easingType));

        public TAnimation SetRepeatCount(int repeatCount)
        {
            RepeatCount = repeatCount < 0 ? -1 : repeatCount;
            return ThisAsT;
        }

        public TAnimation SetIsLooping(bool isLooping)
        {
            IsLooping = isLooping;
            return ThisAsT;
        }

        public TAnimation SetIsPingPong(bool isPingPong)
        {
            IsPingPong = isPingPong;
            return ThisAsT;
        }

        public TAnimation SetIsReverse(bool isReverse)
        {
            IsReverse = isReverse;
            return ThisAsT;
        }

        public TAnimation SetLoopingDelayTimeDuration(float delayTimeDuration)
        {
            LoopingDelayTimeDuration = Math.Max(0f, delayTimeDuration);
            return ThisAsT;
        }

        public TAnimation SetShouldRemoveControlOnAnimationEnd(bool shouldRemoveControlOnAnimationEnd)
        {
            ShouldRemoveControlOnAnimationEnd = shouldRemoveControlOnAnimationEnd;
            return ThisAsT;
        }

        /// <summary>
        /// Binds the lifetime of the animation to a control: the animation stops when that control is disposed.
        /// </summary>
        public TAnimation SetParent(IControl parent)
        {
            var wasSubscribed = isSubscribedToDisposal;
            UnsubscribeFromDisposal();
            owner = parent;
            if (wasSubscribed)
                SubscribeToDisposal();
            return ThisAsT;
        }

        /// <summary>
        /// Sets the scheduler that updates this animation. By default the scheduler of the current screen is used.
        /// </summary>
        public TAnimation SetScheduler(IScheduler scheduler)
        {
            this.scheduler = scheduler;
            if (registeredScheduler != null && IsPlaying)
                Register();
            return ThisAsT;
        }

        /// <summary>Adds a callback invoked when the animation starts from the beginning.</summary>
        public TAnimation AddOnStarted(Action onStarted)
        {
            this.onStarted += onStarted;
            return ThisAsT;
        }

        /// <summary>Adds a callback invoked after every update of the animated value.</summary>
        public TAnimation AddOnAnimationUpdated(Action onAnimationUpdated)
        {
            this.onAnimationUpdated += onAnimationUpdated;
            return ThisAsT;
        }

        /// <summary>Adds a callback invoked at the end of every cycle, including the last one.</summary>
        public TAnimation AddOnAnimationEnd(Action onAnimationEnd)
        {
            this.onAnimationEnd += onAnimationEnd;
            return ThisAsT;
        }

        /// <summary>Adds a callback invoked once, when the last cycle ends.</summary>
        public TAnimation AddOnCompleted(Action onCompleted)
        {
            this.onCompleted += onCompleted;
            return ThisAsT;
        }

        /// <summary>Adds a callback invoked when <see cref="Stop"/> stops a running animation.</summary>
        public TAnimation AddOnStopped(Action onStopped)
        {
            this.onStopped += onStopped;
            return ThisAsT;
        }

        public TAnimation RemoveOnStarted(Action onStarted)
        {
            this.onStarted -= onStarted;
            return ThisAsT;
        }

        public TAnimation RemoveOnAnimationUpdated(Action onAnimationUpdated)
        {
            this.onAnimationUpdated -= onAnimationUpdated;
            return ThisAsT;
        }

        public TAnimation RemoveOnAnimationEnd(Action onAnimationEnd)
        {
            this.onAnimationEnd -= onAnimationEnd;
            return ThisAsT;
        }

        public TAnimation RemoveOnCompleted(Action onCompleted)
        {
            this.onCompleted -= onCompleted;
            return ThisAsT;
        }

        public TAnimation RemoveOnStopped(Action onStopped)
        {
            this.onStopped -= onStopped;
            return ThisAsT;
        }

        /// <summary>
        /// Plays the animation. A stopped animation continues from where it stopped; a completed animation starts
        /// again from the beginning. The current value is applied immediately.
        /// </summary>
        public TAnimation Play()
        {
            if (IsCompleted || !hasStarted)
                Begin();

            IsPlaying = true;
            IsPaused = false;
            stateVersion++;
            Register();
            ApplyCurrentValue();
            return ThisAsT;
        }

        /// <inheritdoc />
        public void Start()
        {
            Unregister();
            Begin();
            IsPlaying = true;
            IsPaused = false;
            stateVersion++;
            ApplyCurrentValue();
        }

        /// <summary>Stops the animation, keeping its progress.</summary>
        public TAnimation Stop()
        {
            Unregister();
            var wasPlaying = IsPlaying;
            IsPlaying = false;
            IsPaused = false;
            stateVersion++;
            if (wasPlaying)
                onStopped?.Invoke();
            return ThisAsT;
        }

        /// <summary>Pauses a playing animation.</summary>
        public TAnimation Pause()
        {
            if (IsPlaying)
                IsPaused = true;
            return ThisAsT;
        }

        /// <summary>Resumes a paused animation.</summary>
        public TAnimation Resume()
        {
            IsPaused = false;
            return ThisAsT;
        }

        /// <summary>Moves the animation back to its start and applies the start value.</summary>
        public TAnimation Reset()
        {
            ResetProgress();
            hasStarted = IsPlaying;
            stateVersion++;
            ApplyCurrentValue();
            return ThisAsT;
        }

        [Obsolete("Use Reset() instead.")]
        public TAnimation ResetAnimation() => Reset();

        /// <inheritdoc />
        public void Update(float deltaSeconds)
        {
            if (!IsPlaying || IsPaused || IsCompleted)
                return;

            var version = stateVersion;
            var deltaTime = Math.Max(0f, deltaSeconds);

            if (delayRemaining > 0f)
            {
                delayRemaining -= deltaTime;
                if (delayRemaining > 0f)
                    return;
                deltaTime = -delayRemaining;
                delayRemaining = 0f;
            }

            for (var iteration = 0; iteration < MaxCyclesPerUpdate; iteration++)
            {
                if (loopDelayRemaining > 0f)
                {
                    loopDelayRemaining -= deltaTime;
                    if (loopDelayRemaining > 0f)
                        return; // keep the end value of the previous cycle while waiting
                    deltaTime = -loopDelayRemaining;
                    loopDelayRemaining = 0f;
                }

                elapsed += deltaTime;
                if (Duration > 0f && elapsed < Duration)
                {
                    Apply(elapsed / Duration, completedCycles);
                    onAnimationUpdated?.Invoke();
                    return;
                }

                // The current cycle is finished.
                var leftover = Duration > 0f ? elapsed - Duration : 0f;
                elapsed = Math.Max(0f, Duration);
                Apply(1f, completedCycles);
                onAnimationUpdated?.Invoke();
                if (version != stateVersion)
                    return;

                if (RepeatCount >= 0 && completedCycles >= RepeatCount)
                {
                    Complete();
                    return;
                }

                onAnimationEnd?.Invoke();
                if (version != stateVersion)
                    return;

                completedCycles++;
                elapsed = 0f;
                loopDelayRemaining = LoopingDelayTimeDuration;
                deltaTime = leftover;

                // A zero duration animation advances at most one cycle per update.
                if (Duration <= 0f)
                    return;
            }
        }

        void IPlayable.Play() => Play();

        void IPlayable.Pause() => Pause();

        void IPlayable.Resume() => Resume();

        void IPlayable.Stop() => Stop();

        void IPlayable.Reset() => Reset();

        /// <summary>
        /// Applies the animated value.
        /// </summary>
        /// <param name="progress">
        /// The eased progress in the direction of the animation: 0 is the start value and 1 the end value.
        /// Overshooting easings (Back, Elastic) can produce values outside of [0, 1].
        /// </param>
        protected abstract void OnUpdateAnimation(float progress);

        /// <summary>
        /// Called once, the first time the animation starts, before the first value is applied.
        /// Override it to read the start value from the current state of the control (unless the user set it
        /// explicitly), so chained animations continue from where the previous one ended.
        /// </summary>
        protected virtual void CaptureStartValues() { }

        private void Begin()
        {
            ResetProgress();
            hasStarted = true;
            if (!startValuesCaptured)
            {
                startValuesCaptured = true;
                CaptureStartValues();
            }
            onStarted?.Invoke();
        }

        private void ResetProgress()
        {
            elapsed = 0f;
            completedCycles = 0;
            loopDelayRemaining = 0f;
            delayRemaining = Delay;
            IsCompleted = false;
        }

        private void Complete()
        {
            IsCompleted = true;
            IsPlaying = false;
            IsPaused = false;
            stateVersion++;
            Unregister();

            if (ShouldRemoveControlOnAnimationEnd)
                Control?.RemoveFromScreen();

            onAnimationEnd?.Invoke();
            onCompleted?.Invoke();
        }

        private void ApplyCurrentValue()
        {
            if (loopDelayRemaining > 0f && completedCycles > 0)
                Apply(1f, completedCycles - 1);
            else if (Duration > 0f)
                Apply(elapsed / Duration, completedCycles);
            else
                Apply(IsCompleted ? 1f : 0f, completedCycles);
        }

        private void Apply(float linearProgress, int cycle)
        {
            var isBackwards = IsReverse ^ (IsPingPong && (cycle & 1) == 1);
            var t = MathHelper.Clamp(linearProgress, 0f, 1f);
            if (isBackwards)
                t = 1f - t;
            OnUpdateAnimation(easingFunction(t));
        }

        private void Register()
        {
            var target = scheduler ?? ServiceProvider.Scheduler;
            if (registeredScheduler != null && registeredScheduler != target)
                registeredScheduler.Remove(this);
            registeredScheduler = target;
            target.Add(this);
            SubscribeToDisposal();
        }

        private void Unregister()
        {
            registeredScheduler?.Remove(this);
            registeredScheduler = null;
            UnsubscribeFromDisposal();
        }

        private void SubscribeToDisposal()
        {
            if (isSubscribedToDisposal)
                return;

            if (Control != null)
                Control.Disposed += OnBoundControlDisposed;
            if (owner != null && owner != Control)
                owner.Disposed += OnBoundControlDisposed;
            isSubscribedToDisposal = true;
        }

        private void UnsubscribeFromDisposal()
        {
            if (!isSubscribedToDisposal)
                return;

            if (Control != null)
                Control.Disposed -= OnBoundControlDisposed;
            if (owner != null && owner != Control)
                owner.Disposed -= OnBoundControlDisposed;
            isSubscribedToDisposal = false;
        }

        private void OnBoundControlDisposed(IControl control) => Stop();
    }
}
