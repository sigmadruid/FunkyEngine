using System.IO;

namespace Funky.AssetTool.Runtime
{
    public class PathProvider
    {
        public string AssetToPathPath { get; } 
        public string AssetToBundlePath { get; } 
        public string BundleDependencyPath { get; }
        public string BasePath { get; }
        
        public PathProvider(string basePath)
        {
            BasePath = basePath;
            
            AssetToPathPath = Path.Combine(basePath, "Asset2Bundles.json");
            AssetToBundlePath = Path.Combine(basePath, "Asset2Paths.json");
            BundleDependencyPath = Path.Combine(basePath, "BundleDependencies.json");
        }
        
        
    }
}