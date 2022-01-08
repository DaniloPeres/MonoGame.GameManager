using MonoGame.GameManager.Enums;
using MonoGame.GameManager.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace MonoGame.GameManager.Managers
{
    /// <summary>
    /// Memory Manager
    /// Keep the content in memory, and unload only when we need some memory space
    /// </summary>
    public class MemoryManager
    {
        private readonly ConcurrentDictionary<string, ContentAsset> assets = new ConcurrentDictionary<string, ContentAsset>();
        private readonly ConcurrentBag<object> assetsToDispose = new ConcurrentBag<object>();
        public CleanMemoryType CleanMemoryType = CleanMemoryType.OnChangeScreen;

        /// <summary>
        /// Add the asset to be disposed when the screen is changed
        /// </summary>
        /// <param name="asset">The asset to be disposed</param>
        public void AddAssetToDispose(IDisposable asset)
        {
            assetsToDispose.Add(asset);
        }

        public bool TryGetAsset<T>(string assetName, out T asset)
        {
            if (!assets.TryGetValue(assetName, out var contentAsset))
            {
                asset = default;
                return false;
            }

            SetAsUsed(contentAsset);
            asset = (T)contentAsset.Asset;
            return true;
        }

        public void AddAsset(string assetName, object asset)
        {
            var contentAsset = new ContentAsset
            {
                Asset = asset
            };
            SetAsUsed(contentAsset);
            assets.TryAdd(assetName, contentAsset);
        }

        public void SetAllAssetsAsFixed()
        {
            assets.Values.ToList().ForEach(asset =>
            {
                asset.State = ContentAssetState.Fixed;
            });
        }

        private void SetAsUsed(ContentAsset contentAsset)
        {
            contentAsset.TimeUsed = DateTime.Now;
            contentAsset.State = ContentAssetState.InUse;
        }

        public void CleanMemory()
        {
            var assetsToUnload = assets.Where(asset => asset.Value.State == ContentAssetState.NotInUse).OrderByDescending(asset => asset.Value.TimeUsed).ToList();
            assetsToUnload.ForEach(asset =>
            {
                if (asset.Value.Asset is IDisposable assetAsDisposable)
                    assetAsDisposable.Dispose();
                assets.TryRemove(asset.Key, out _);
            });
        }
    }
}
