using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Pooling
{
    /// <summary>
    /// Reuses objects (bullets, enemies, effects...) instead of creating new ones, to avoid garbage collection
    /// pauses during the game (Object Pool pattern).
    /// </summary>
    /// <example>
    /// <code>
    /// var bullets = new ObjectPool&lt;Bullet&gt;(() => new Bullet(), initialSize: 20);
    /// var bullet = bullets.Get();
    /// // ...
    /// bullets.Return(bullet);
    /// </code>
    /// </example>
    public class ObjectPool<T> where T : class
    {
        private readonly Func<T> factory;
        private readonly Action<T> onGet;
        private readonly Action<T> onReturn;
        private readonly Stack<T> inactiveItems = new Stack<T>();
        private readonly HashSet<T> inactiveSet = new HashSet<T>();

        /// <param name="factory">Creates a new object when the pool is empty.</param>
        /// <param name="onGet">Called when an object is taken from the pool.</param>
        /// <param name="onReturn">Called when an object is returned to the pool.</param>
        /// <param name="initialSize">The number of objects created immediately.</param>
        /// <param name="maxSize">The maximum number of inactive objects kept (the others are discarded).</param>
        public ObjectPool(Func<T> factory, Action<T> onGet = null, Action<T> onReturn = null, int initialSize = 0, int maxSize = int.MaxValue)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.onGet = onGet;
            this.onReturn = onReturn;
            MaxSize = Math.Max(1, maxSize);
            Prewarm(initialSize);
        }

        /// <summary>The maximum number of inactive objects kept.</summary>
        public int MaxSize { get; }

        /// <summary>The number of objects taken and not returned yet.</summary>
        public int CountActive { get; private set; }

        /// <summary>The number of objects waiting in the pool.</summary>
        public int CountInactive => inactiveItems.Count;

        /// <summary>Takes an object from the pool (or creates one).</summary>
        public T Get()
        {
            T item;
            if (inactiveItems.Count > 0)
            {
                item = inactiveItems.Pop();
                inactiveSet.Remove(item);
            }
            else
            {
                item = factory();
            }

            CountActive++;
            (item as IPoolable)?.OnSpawn();
            onGet?.Invoke(item);
            return item;
        }

        /// <summary>Returns an object to the pool.</summary>
        public void Return(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (inactiveSet.Contains(item))
                throw new InvalidOperationException("The object was already returned to the pool.");

            CountActive = Math.Max(0, CountActive - 1);
            onReturn?.Invoke(item);
            (item as IPoolable)?.OnDespawn();

            if (inactiveItems.Count >= MaxSize)
            {
                (item as IDisposable)?.Dispose();
                return;
            }

            inactiveItems.Push(item);
            inactiveSet.Add(item);
        }

        /// <summary>Creates objects in advance (up to <see cref="MaxSize"/>).</summary>
        public void Prewarm(int count)
        {
            for (var i = 0; i < count && inactiveItems.Count < MaxSize; i++)
            {
                var item = factory();
                inactiveItems.Push(item);
                inactiveSet.Add(item);
            }
        }

        /// <summary>Discards the inactive objects (disposable objects are disposed).</summary>
        public void Clear()
        {
            while (inactiveItems.Count > 0)
                (inactiveItems.Pop() as IDisposable)?.Dispose();
            inactiveSet.Clear();
        }
    }
}
