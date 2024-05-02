using System;
using System.Collections.Generic;
using Funky.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Funky.AssetTool.Runtime
{
    public class BundleRequest : IReset
    {
        private readonly Dictionary<string, List<AssetHandle>> _assetHandles = new();
        private BundleHandle _bundleHandle;

        private BundleLoader _bundleLoader;
        private ObjectPool<AssetHandle> _assetHandlePool;
        
        public bool IsDone { get; private set; }
        public int AssetCount => _assetHandles.Count;
        public string BundleName => _bundleHandle != null ? _bundleHandle.Name : string.Empty;
        
        public void Reset()
        {
            IsDone = false;
            
            _bundleHandle = null;
            _assetHandles.Clear();
            
            _bundleLoader = null;
            _assetHandlePool = null;
        }

        public void Initialize(BundleLoader bundleLoader, ObjectPool<AssetHandle> assetHandlePool)
        {
            _bundleLoader = bundleLoader;
            _assetHandlePool = assetHandlePool;
        }
        
        public void AddAsset(AssetHandle handle)
        {
            if (IsDone)
            {
                handle.Asset = _bundleHandle.Bundle.LoadAsset(handle.Name);
                handle.OnComplete?.Invoke(true, handle.Asset);
            }
            
            if (!_assetHandles.TryGetValue(handle.Name, out var list))
            {
                list = new List<AssetHandle>();
                _assetHandles.Add(handle.Name, list);
            }
            list.Add(handle);
        }

        public void RemoveAsset(AssetHandle handle)
        {
            if (_assetHandles.TryGetValue(handle.Name, out var list))
            {
                list.Remove(handle);
            }
            _assetHandles.Remove(handle.Name);
        }

        public void ClearAssets()
        {
            foreach (var list in _assetHandles.Values)
            {
                foreach (var handle in list)
                {
                    _assetHandlePool.Recycle(handle);
                }
            }
            _assetHandles.Clear();
            _bundleHandle.Bundle.Unload(true);
        }
        
        public void Start(string bundleName)
        {
            _bundleHandle = _bundleLoader.LoadBundle(bundleName, OnBundleLoaded);
        }

        public void StartAsync(string bundleName)
        {
            _bundleHandle = _bundleLoader.LoadBundleAsync(bundleName, OnBundleLoaded);
        }

        public void End()
        {
            if (_bundleHandle != null)
            {
                _bundleHandle.Cancelled = true;
                _bundleLoader.UnloadBundle(_bundleHandle);
            }
        }

        public void Complete()
        {
            if (!IsDone)
            {
                _bundleLoader.LoadBundle(_bundleHandle.Name, OnBundleLoaded);
            }
        }

        private void OnBundleLoaded(BundleHandle bundleHandle)
        {
            if (!bundleHandle.Bundle) 
                return;

            var bundle = bundleHandle.Bundle;
            foreach (var list in _assetHandles.Values)
            {
                foreach (var handle in list)
                {
                    handle.Asset = bundle.LoadAsset(handle.Name);
                    handle.OnComplete?.Invoke(true, handle.Asset);
                }
            }
            IsDone = true;
        }

    }
}