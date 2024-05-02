using System;
using System.Collections.Generic;
using System.IO;
using Funky.Utility;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Logger = Funky.Log.Logger;

namespace Funky.AssetTool.Runtime
{
    public class BundleLoader
    {
        private const int MaxAsyncProcessingCount = 10;
        
        private readonly LinkedList<BundleHandle> _processingHandles = new();
        private readonly Dictionary<string, BundleCache> _caches = new();

        private readonly ObjectPool<BundleHandle> _handlePool = new();
        private readonly ObjectPool<BundleCache> _cachePool = new();
        
        private readonly BundleDependency _dependency;
        private readonly PathProvider _pathProvider;

        private readonly List<BundleHandle> _toRemoveHandles = new();

        public BundleLoader(BundleDependency dependency, PathProvider pathProvider)
        {
            _dependency = dependency;
            _pathProvider = pathProvider;
        }

        public void Release()
        {
            foreach (var handle in _processingHandles)
            {
                _handlePool.Recycle(handle);
            }
            _processingHandles.Clear();
            
            foreach (var cache in _caches.Values)
            {
                cache.Release(true);
                _cachePool.Recycle(cache);
            }
            _caches.Clear();
            
            _handlePool.Clear();
            _cachePool.Clear();
            _toRemoveHandles.Clear();

            _dependency.Dispose();
        }

        public void Tick()
        {
            //processing
            _toRemoveHandles.Clear();
            
            var counter = 0;
            foreach (var handle in _processingHandles)
            {
                if (handle.Cancelled)
                {
                    _toRemoveHandles.Add(handle);
                    continue;
                }

                if (!ExistBundle(handle.Name))
                {
                    continue;
                }
                
                DoLoadSync(handle);
                if (IsAllDependenciesReady(handle.Name))
                {
                    handle.OnComplete?.Invoke(handle);
                    _toRemoveHandles.Add(handle);
                }
                
                if (++counter == MaxAsyncProcessingCount)
                    break;
            }

            foreach (var handle in _toRemoveHandles)
            {
                _processingHandles.Remove(handle);
                _handlePool.Recycle(handle);
            }
        }

        public BundleHandle LoadBundle(string bundleName, Action<BundleHandle> onComplete)
        {
            var handle = _handlePool.Allocate();
            handle.Name = bundleName;
            handle.OnComplete = onComplete;
            DoLoadSync(handle);

            var dependentBundles = _dependency.GetDependentBundles(bundleName);
            foreach (var depBundleName in dependentBundles)
            {
                var depHandle = _handlePool.Allocate();
                handle.Name = depBundleName;
                DoLoadSync(depHandle);
                _handlePool.Recycle(depHandle);
                //todo: simplify bundle deps, to avoid recursive invoking.
            }
            
            handle.OnComplete?.Invoke(handle);
            return handle;
        }
        
        public BundleHandle LoadBundleAsync(string bundleName, Action<BundleHandle> onComplete)
        {
            var processingHandle = GetProcessingHandle(bundleName);
            if (processingHandle != null)
            {
                if (processingHandle.Cancelled)
                    processingHandle.Cancelled = false;
                processingHandle.OnComplete += onComplete;
                return processingHandle;
            }
            
            var handle = _handlePool.Allocate();
            handle.Name = bundleName;
            handle.OnComplete = onComplete;
            DoLoadAsync(handle);
            
            var dependentBundles = _dependency.GetDependentBundles(bundleName);
            foreach (var depBundleName in dependentBundles)
            {
                var depHandle = _handlePool.Allocate();
                handle.Name = depBundleName;
                DoLoadAsync(depHandle);
                _handlePool.Recycle(depHandle);
                //todo: simplify bundle deps, to avoid recursive invoking.
            }
            
            return handle;
        }

        public void UnloadBundle(BundleHandle handle, bool unloadObjects = true)
        {
            var dependentBundles = _dependency.GetDependentBundles(handle.Name);
            foreach (var depBundleName in dependentBundles)
            {
                var depHandle = _handlePool.Allocate();
                depHandle.Name = depBundleName;
                DoUnloadBundle(depHandle);
            }
            DoUnloadBundle(handle);
        }

        private void DoLoadSync(BundleHandle handle)
        {
            var name = handle.Name;
            if (_caches.TryGetValue(name, out var cache))
            {
                cache.IncreaseRef();
                handle.Bundle = cache.Bundle;
                return;
            }
            
            if (!ExistBundle(name))
            {
                Logger.Error($"load a non-exist bundle! name={name}");
                return;
            }
            
            var existedHandle = GetProcessingHandle(name);
            if (existedHandle != null)
            {
                handle.OnComplete += existedHandle.OnComplete;
                existedHandle.Cancelled = true;
            }

            var bundle = DoLoadBundle(name);
            cache = _cachePool.Allocate();
            cache.Fill(name, bundle);
            cache.IncreaseRef();
            _caches.Add(name, cache);
        }
        
        private void DoLoadAsync(BundleHandle handle)
        {
            var name = handle.Name;
            
            if (_caches.TryGetValue(name, out var cache))
            {
                cache.IncreaseRef();
                handle.Bundle = cache.Bundle;
                handle.OnComplete?.Invoke(handle);
                return;
            }

            _processingHandles.AddLast(handle);
        }
        
        private void DoUnloadBundle(BundleHandle handle, bool unloadObjects = true)
        {
            var processingHandle = GetProcessingHandle(handle.Name);
            if (processingHandle != null)
            {
                processingHandle.Cancelled = true;
                return;
            }

            var name = handle.Name;
            if (_caches.TryGetValue(name, out var cache))
            {
                cache.DecreaseRef();
                if (cache.RefCount <= 0)
                {
                    cache.Release(unloadObjects);
                    _caches.Remove(name);
                }
            }
            _handlePool.Recycle(handle);
        }

        private BundleHandle GetProcessingHandle(string name)
        {
            foreach (var existedHandle in _processingHandles)
            {
                if (existedHandle.Name == name)
                {
                    return existedHandle;
                }
            }
            return null;
        }

        private bool ExistBundle(string name)
        {
            var path = Path.Combine(_pathProvider.BasePath, name);
            return IOUtility.Exists(path);
        }

        private AssetBundle DoLoadBundle(string name)
        {
            //todo: consider when the file is not downloaded
            var path = Path.Combine(_pathProvider.BasePath, name);
            var bundle = AssetBundle.LoadFromFile(path);
            return bundle;

        }

        private bool IsAllDependenciesReady(string name)
        {
            var depBundles = _dependency.GetDependentBundles(name);
            if (depBundles != null)
            {
                foreach (var depBundle in depBundles)
                {
                    if (!_caches.TryGetValue(depBundle, out var cache) || !cache.IsValid)
                        return false;
                }

                return true;
            }

            return false;
        }
    }
}