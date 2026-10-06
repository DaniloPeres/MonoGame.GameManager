using MonoGame.GameManager.Core;
using System;

namespace MonoGame.GameManager.Timers
{
    /// <summary>
    /// An action scheduled on an <see cref="IScheduler"/> (see <see cref="IScheduler.Delay"/>,
    /// <see cref="IScheduler.Every"/>, <see cref="IScheduler.NextFrame"/> and <see cref="IScheduler.EveryFrame"/>).
    /// </summary>
    public sealed class ScheduledAction : IUpdatable
    {
        private readonly IScheduler scheduler;
        private readonly Action action;
        private readonly Action<float> frameAction;
        private float elapsed;

        internal ScheduledAction(IScheduler scheduler, float interval, int repeatCount, Action action, Action<float> frameAction)
        {
            this.scheduler = scheduler;
            Interval = interval;
            RepeatCount = repeatCount;
            this.action = action;
            this.frameAction = frameAction;
        }

        /// <summary>The time between two invocations, in seconds.</summary>
        public float Interval { get; }

        /// <summary>The total number of invocations, or -1 when it repeats until cancelled.</summary>
        public int RepeatCount { get; }

        /// <summary>How many times the action has been invoked.</summary>
        public int InvocationCount { get; private set; }

        /// <summary>True after <see cref="Cancel"/> was called.</summary>
        public bool IsCancelled { get; private set; }

        /// <summary>True when every invocation was done.</summary>
        public bool IsCompleted { get; private set; }

        /// <summary>True while the action is paused.</summary>
        public bool IsPaused { get; private set; }

        /// <summary>Progress towards the next invocation, from 0 to 1.</summary>
        public float Progress => Interval <= 0f ? 1f : Math.Min(1f, elapsed / Interval);

        /// <summary>Seconds left until the next invocation.</summary>
        public float RemainingTime => Math.Max(0f, Interval - elapsed);

        /// <summary>Invocations left, or -1 when it repeats until cancelled.</summary>
        public int RemainingRepeats => RepeatCount < 0 ? -1 : Math.Max(0, RepeatCount - InvocationCount);

        /// <summary>Cancels the action. It will not be invoked anymore.</summary>
        public void Cancel()
        {
            IsCancelled = true;
            scheduler.Remove(this);
        }

        /// <summary>Pauses the countdown of this action.</summary>
        public void Pause() => IsPaused = true;

        /// <summary>Resumes the countdown of this action.</summary>
        public void Resume() => IsPaused = false;

        void IUpdatable.Update(float deltaSeconds)
        {
            if (IsCancelled || IsCompleted || IsPaused)
                return;

            if (frameAction != null)
            {
                InvocationCount++;
                frameAction(deltaSeconds);
                return;
            }

            elapsed += deltaSeconds;
            if (elapsed < Interval)
                return;

            // Keep the remainder to avoid drift, but never fire more than once per update.
            elapsed = Interval > 0f ? (elapsed - Interval) % Interval : 0f;
            InvocationCount++;

            if (RepeatCount >= 0 && InvocationCount >= RepeatCount)
            {
                IsCompleted = true;
                scheduler.Remove(this);
            }

            action();
        }
    }
}
