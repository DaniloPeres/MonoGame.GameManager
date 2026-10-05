namespace MonoGame.GameManager.Pooling
{
    /// <summary>
    /// Optional callbacks for objects managed by an <see cref="ObjectPool{T}"/>.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called when the object is taken from the pool.</summary>
        void OnSpawn();

        /// <summary>Called when the object is returned to the pool.</summary>
        void OnDespawn();
    }
}
