using System;

namespace MonoGame.GameManager.Managers
{
    /// <summary>
    /// Stores assets by name (for the assets that the MonoGame <c>ContentManager</c> does not cache, such as
    /// parsed sprite animations) and disposable resources that must be released together.
    /// </summary>
    public interface IAssetCache : IDisposable
    {
        /// <summary>The number of cached assets.</summary>
        int Count { get; }

        bool TryGet<T>(string key, out T asset);

        bool Contains(string key);

        /// <summary>Adds or replaces an asset.</summary>
        /// <param name="key">The asset name.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="disposeOnClear">When true, the asset is disposed by <see cref="Clear"/>.</param>
        void Add(string key, object asset, bool disposeOnClear = false);

        bool Remove(string key);

        /// <summary>Registers a resource (render target, generated texture...) to dispose in <see cref="Clear"/>.</summary>
        void TrackDisposable(IDisposable disposable);

        /// <summary>Removes every asset and disposes the owned ones and the tracked resources.</summary>
        void Clear();
    }
}
