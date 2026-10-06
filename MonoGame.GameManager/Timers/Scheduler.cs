using MonoGame.GameManager.Core;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Timers
{
    /// <summary>
    /// Default <see cref="IScheduler"/> implementation.
    /// Objects can be added or removed safely while the scheduler is updating; objects added during an update
    /// start being updated on the next one.
    /// </summary>
    public class Scheduler : IScheduler
    {
        private readonly List<IUpdatable> items = new List<IUpdatable>();
        private readonly HashSet<IUpdatable> members = new HashSet<IUpdatable>();
        private readonly List<IUpdatable> pendingAdds = new List<IUpdatable>();
        private readonly HashSet<IUpdatable> pendingRemoves = new HashSet<IUpdatable>();
        private bool isUpdating;
        private float timeScale = 1f;

        /// <inheritdoc />
        public float TimeScale
        {
            get => timeScale;
            set => timeScale = Math.Max(0f, value);
        }

        /// <inheritdoc />
        public bool IsPaused { get; private set; }

        /// <inheritdoc />
        public int Count => members.Count;

        /// <inheritdoc />
        public void Pause() => IsPaused = true;

        /// <inheritdoc />
        public void Resume() => IsPaused = false;

        /// <inheritdoc />
        public ScheduledAction Delay(float seconds, Action action)
            => Schedule(new ScheduledAction(this, Math.Max(0f, seconds), 1, action ?? throw new ArgumentNullException(nameof(action)), null));

        /// <inheritdoc />
        public ScheduledAction Every(float intervalSeconds, Action action, int repeatCount = -1)
            => Schedule(new ScheduledAction(this, Math.Max(0f, intervalSeconds), repeatCount, action ?? throw new ArgumentNullException(nameof(action)), null));

        /// <inheritdoc />
        public ScheduledAction NextFrame(Action action)
            => Schedule(new ScheduledAction(this, 0f, 1, action ?? throw new ArgumentNullException(nameof(action)), null));

        /// <inheritdoc />
        public ScheduledAction EveryFrame(Action<float> action)
            => Schedule(new ScheduledAction(this, 0f, -1, null, action ?? throw new ArgumentNullException(nameof(action))));

        /// <inheritdoc />
        public void Add(IUpdatable updatable)
        {
            if (updatable == null)
                throw new ArgumentNullException(nameof(updatable));

            if (!isUpdating)
            {
                if (members.Add(updatable))
                    items.Add(updatable);
                return;
            }

            if (pendingRemoves.Remove(updatable))
            {
                // It was removed during this update and is still in the list: keep it.
                members.Add(updatable);
                return;
            }

            if (members.Add(updatable))
                pendingAdds.Add(updatable);
        }

        /// <inheritdoc />
        public bool Remove(IUpdatable updatable)
        {
            if (updatable == null || !members.Remove(updatable))
                return false;

            if (!isUpdating)
            {
                items.Remove(updatable);
                return true;
            }

            if (!pendingAdds.Remove(updatable))
                pendingRemoves.Add(updatable);

            return true;
        }

        /// <inheritdoc />
        public bool Contains(IUpdatable updatable) => updatable != null && members.Contains(updatable);

        /// <inheritdoc />
        public void Clear()
        {
            members.Clear();
            pendingAdds.Clear();

            if (isUpdating)
            {
                foreach (var item in items)
                    pendingRemoves.Add(item);
            }
            else
            {
                items.Clear();
                pendingRemoves.Clear();
            }
        }

        /// <inheritdoc />
        public void Update(float deltaSeconds)
        {
            if (IsPaused || isUpdating)
                return;

            var scaledDelta = Math.Max(0f, deltaSeconds) * timeScale;
            isUpdating = true;
            try
            {
                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (!pendingRemoves.Contains(item))
                        item.Update(scaledDelta);
                }
            }
            finally
            {
                isUpdating = false;
                ApplyPendingChanges();
            }
        }

        private ScheduledAction Schedule(ScheduledAction scheduledAction)
        {
            Add(scheduledAction);
            return scheduledAction;
        }

        private void ApplyPendingChanges()
        {
            if (pendingRemoves.Count > 0)
            {
                items.RemoveAll(pendingRemoves.Contains);
                pendingRemoves.Clear();
            }

            if (pendingAdds.Count > 0)
            {
                items.AddRange(pendingAdds);
                pendingAdds.Clear();
            }
        }
    }
}
