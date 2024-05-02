using System;
using Object = UnityEngine.Object;

namespace Funky.AssetTool.Runtime
{
    public interface ILoader
    {
        public void Release();

        public void Tick(float deltaTime);
        
        public AssetHandle LoadAsset(string assetName, Action<bool, Object> onComplete);

        public AssetHandle LoadAssetAsync(string assetName, Action<bool, Object> onComplete);

        public void UnloadAsset(AssetHandle handle);
    }
}