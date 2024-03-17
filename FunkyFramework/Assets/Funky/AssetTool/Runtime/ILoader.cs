namespace Funky.AssetTool.Runtime
{
    public interface ILoader
    {
        public void Release();

        public void Tick(float deltaTime);
        
        public AssetHandle LoadAsset(string assetName);

        public AssetHandle LoadAssetAsync(string assetName);

        public void UnloadAsset(AssetHandle handle);
    }
}