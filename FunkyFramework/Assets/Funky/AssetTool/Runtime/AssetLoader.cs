namespace Funky.AssetTool.Runtime
{
    public class AssetLoader : ILoader
    {
        private BundleLoader _bundleLoader;
        
        public void Release()
        {
            _bundleLoader.Release();
        }

        public void Tick(float deltaTime)
        {
        }

        public AssetHandle LoadAsset(string assetName)
        {
            return null;
        }
        
        public AssetHandle LoadAssetAsync(string assetName)
        {
            return null;
        }

        public void UnloadAsset(AssetHandle handle)
        {
            
        }
    }
}