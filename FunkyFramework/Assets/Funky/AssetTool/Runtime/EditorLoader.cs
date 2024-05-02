using System;
using Object = UnityEngine.Object;

namespace Funky.AssetTool.Runtime
{
    public class EditorLoader : ILoader
    {
        public void Release()
        {
            
        }

        public void Tick(float deltaTime)
        {
        }

        public AssetHandle LoadAsset(string assetName, Action<bool, Object> onComplete)
        {
            return null;
        }
        
        public AssetHandle LoadAssetAsync(string assetName, Action<bool, Object> onComplete)
        {
            return null;
        }

        public void UnloadAsset(AssetHandle handle)
        {
            
        }
    }
}