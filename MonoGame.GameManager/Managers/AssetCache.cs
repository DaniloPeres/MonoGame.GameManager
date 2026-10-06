using MonoGame.GameManager.Enums;
using System;
using System.Collections.Generic;

namespace MonoGame.GameManager.Managers
{
    /// <summary>
    /// Default <see cref="IAssetCache"/> implementation.
    /// </summary>
    public class AssetCache : IAssetCache
    {
        private readonly Dictionary<string, Entry> assets = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly List<IDisposable> trackedDisposables = new List<IDisposable>();

        public int Count => assets.Count;

        public bool TryGet<T>(string key, out T asset)
        {
            if (key != null && assets.TryGetValue(key, out var entry) && entry.Asset is T typedAsset)
            {
                asset = typedAsset;
                return true;
            }

            asset = default;
            return false;
        }

        public bool Contains(string key) => key != null && assets.ContainsKey(key);

        public void Add(string key, object asset, bool disposeOnClear = false)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            assets[key] = new Entry(asset, disposeOnClear);
        }

        public bool Remove(string key) => key != null && assets.Remove(key);

        public void TrackDisposable(IDisposable disposable)
        {
            if (disposable != null && !trackedDisposables.Contains(disposable))
                trackedDisposables.Add(disposable);
        }

        public void Clear()
        {
            foreach (var entry in assets.Values)
            {
                if (entry.DisposeOnClear && entry.Asset is IDisposable disposable)
                    disposable.Dispose();
            }
            assets.Clear();

            foreach (var disposable in trackedDisposables)
                disposable.Dispose();
            trackedDisposables.Clear();
        }

        public void Dispose() => Clear();

        #region Version 1.x compatibility

        [Obsolete("Assets are released with the ContentManager that loaded them (see Screen.Content). This setting has no effect.")]
        public CleanMemoryType CleanMemoryType { get; set; } = CleanMemoryType.OnChangeScreen;

        [Obsolete("Assets are released with the ContentManager that loaded them (see Screen.Content). This method has no effect.")]
        public void CleanMemory() { }

        [Obsolete("Assets loaded with ServiceProvider.ContentLoader are kept for the whole game. This method has no effect.")]
        public void SetAllAssetsAsFixed() { }

        [Obsolete("Use TrackDisposable (or Screen.RegisterDisposable) instead.")]
        public void AddAssetToDispose(IDisposable asset) => TrackDisposable(asset);

        [Obsolete("Use TryGet instead.")]
        public bool TryGetAsset<T>(string assetName, out T asset) => TryGet(assetName, out asset);

        [Obsolete("Use Add instead.")]
        public void AddAsset(string assetName, object asset) => Add(assetName, asset);

        #endregion

        private readonly struct Entry
        {
            public Entry(object asset, bool disposeOnClear)
            {
                Asset = asset;
                DisposeOnClear = disposeOnClear;
            }

            public object Asset { get; }

            public bool DisposeOnClear { get; }
        }
    }

    /// <summary>
    /// Kept for compatibility with version 1.x.
    /// </summary>
    [Obsolete("Use AssetCache (IAssetCache) instead.")]
    public class MemoryManager : AssetCache { }
}
