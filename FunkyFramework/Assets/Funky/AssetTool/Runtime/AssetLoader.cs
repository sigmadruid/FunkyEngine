using System;
using System.Collections.Generic;
using Funky.Log;
using Funky.Utility;
using Object = UnityEngine.Object;

namespace Funky.AssetTool.Runtime
{
    public class AssetLoader : ILoader
    {
        private const int MaxAsyncProcessingCount = 10;

        private readonly Dictionary<string, AssetCache> _caches = new();
        private readonly Dictionary<string, BundleRequest> _bundleRequests = new();
        private readonly LinkedList<AssetHandle> _processingHandles = new();

        private readonly BundleLoader _bundleLoader;
        private readonly AssetToBundles _assetToBundles;

        private readonly ObjectPool<AssetCache> _cachePool = new();
        private readonly ObjectPool<AssetHandle> _handlePool = new();
        private readonly ObjectPool<BundleRequest> _requestPool = new();
        
        private readonly List<AssetHandle> _toRemoveHandles = new();


        public AssetLoader(BundleDependency dependency, PathProvider pathProvider)
        {
            _bundleLoader = new BundleLoader(dependency, pathProvider);
            _assetToBundles = new AssetToBundles(pathProvider);
        }
        
        public void Release()
        {
            foreach (var cache in _caches.Values)
            {
                cache.Release();
                _cachePool.Recycle(cache);
            }
            _caches.Clear();
            
            foreach (var request in _bundleRequests.Values)
            {
                request.End();
                _requestPool.Recycle(request);
            }
            _bundleRequests.Clear();

            foreach (var handle in _processingHandles)
            {
                _handlePool.Recycle(handle);
            }
            _processingHandles.Clear();
            
            _cachePool.Clear();
            _handlePool.Clear();
            _requestPool.Clear();
            
            _bundleLoader.Release();
            _assetToBundles.Release();
        }

        public void Tick(float deltaTime)
        {
            _toRemoveHandles.Clear();
            foreach (var handle in _processingHandles)
            {
                if (handle.Asset)
                {
                    CreateCache(handle.Name, handle.Asset);
                    _toRemoveHandles.Add(handle);
                }
            }

            foreach (var handle in _toRemoveHandles)
            {
                _processingHandles.Remove(handle);
            }
        }

        public AssetHandle LoadAsset(string assetName, Action<bool, Object> onComplete)
        {
            AssetHandle handle = null;
            if (_caches.TryGetValue(assetName, out var cache))
            {
                cache.IncreaseRef();
                handle = CreateAssetHandle(assetName, onComplete, cache.Asset);
                onComplete?.Invoke(true, handle.Asset);
                return handle;
            }
            
            var bundleName = _assetToBundles.GetBundleName(assetName);
            if (_bundleRequests.TryGetValue(bundleName, out var request))
            {
                request.Complete();
                handle = CreateAssetHandle(assetName, onComplete);
                request.AddAsset(handle);
            }
            else
            {
                handle = CreateAssetHandle(assetName, onComplete);
                CreateNewRequest(bundleName, handle, false);
            }

            CreateCache(assetName, handle.Asset);
            return handle;
        }

        public AssetHandle LoadAssetAsync(string assetName, Action<bool, Object> onComplete)
        {
            AssetHandle handle = null;
            if (_caches.TryGetValue(assetName, out var cache))
            {
                cache.IncreaseRef();
                handle = CreateAssetHandle(assetName, onComplete, cache.Asset);
                onComplete?.Invoke(true, handle.Asset);
                return handle;
            }
            
            handle = CreateAssetHandle(assetName, onComplete);

            var bundleName = _assetToBundles.GetBundleName(assetName);
            if (_bundleRequests.TryGetValue(bundleName, out var request))
            {
                if (!request.IsDone)
                    _processingHandles.AddLast(handle);
                request.AddAsset(handle);
                return handle;
            }
            
            _processingHandles.AddLast(handle);
            CreateNewRequest(bundleName, handle, true);
            return handle;
        }

        public void UnloadAsset(AssetHandle handle)
        {
            var name = handle.Name;
            if (_caches.TryGetValue(name, out var cache))
            {
                cache.DecreaseRef();
                if (cache.RefCount <= 0)
                {
                    cache.Release();
                    _caches.Remove(name);
                }
            }

            var bundleName = _assetToBundles.GetBundleName(name);
            if (_bundleRequests.TryGetValue(bundleName, out var request))
            {
                request.RemoveAsset(handle);
                if (request.AssetCount <= 0)
                {
                    request.End();
                }
            }
            
            _handlePool.Recycle(handle);
        }

        private AssetHandle CreateAssetHandle(string assetName, Action<bool, Object> onComplete, Object asset = null)
        {
            var handle = _handlePool.Allocate();
            handle.Name = assetName;
            handle.Asset = asset;
            handle.OnComplete = onComplete;
            handle.Asset = asset;
            return handle;
        }

        private void CreateNewRequest(string bundleName, AssetHandle handle, bool isAsync)
        {
            var request = _requestPool.Allocate();
            request.Initialize(_bundleLoader, _handlePool);
            request.AddAsset(handle);
            if (isAsync)
                request.StartAsync(bundleName);
            else
                request.Start(bundleName);
            _bundleRequests.Add(bundleName, request);
        }

        private void CreateCache(string assetName, Object asset)
        {
            var cache = _cachePool.Allocate();
            cache.Fill(assetName, asset);
            cache.IncreaseRef();
            _caches.Add(assetName, cache);
        }

    }
}