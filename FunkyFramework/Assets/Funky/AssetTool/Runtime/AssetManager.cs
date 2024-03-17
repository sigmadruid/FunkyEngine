using Funky.Utility;

namespace Funky.AssetTool.Runtime
{
    public class AssetManager : Singleton<AssetManager>
    {
        private ILoader _loader;

        public void Initialize(bool bundleMode)
        {
            if (bundleMode)
                _loader = new AssetLoader();
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
        
        public AssetHandle LoadAsset(string assetName)
        {
            return _loader.LoadAsset(assetName);
        }
        
        public AssetHandle LoadAssetAsync(string assetName)
        {
            return _loader.LoadAssetAsync(assetName);
        }

        public void UnloadAsset(AssetHandle handle)
        {
            _loader.UnloadAsset(handle);
        }
    }
}