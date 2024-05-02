using System;
using Funky.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Funky.AssetTool.Runtime
{
    public class AssetManager : Singleton<AssetManager>
    {
        private ILoader _loader;

        private PathProvider _pathProvider;
        private BundleDependency _dependency;

        public void Initialize(bool bundleMode)
        {
            _pathProvider = new PathProvider(Application.streamingAssetsPath);
            _dependency = new BundleDependency();
            
            if (bundleMode)
                _loader = new AssetLoader(_dependency, _pathProvider);
            else
                _loader = new EditorLoader();
        }
        
        public void Release()
        {
            _loader.Release();
        }

        public void Tick(float deltaTime)
        {
            _loader.Tick(deltaTime);
        }
        
        public AssetHandle LoadAsset(string assetName, Action<bool, Object> onComplete)
        {
            return _loader.LoadAsset(assetName, onComplete);
        }
        
        public AssetHandle LoadAssetAsync(string assetName, Action<bool, Object> onComplete)
        {
            return _loader.LoadAssetAsync(assetName, onComplete);
        }

        public void UnloadAsset(AssetHandle handle)
        {
            _loader.UnloadAsset(handle);
        }
    }
}