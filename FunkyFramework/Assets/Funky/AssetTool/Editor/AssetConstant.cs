using System.Collections.Generic;

namespace Funky.AssetTool.Editor
{
    public static class AssetConstant
    {
        public const string AssetBundleSuffix = ".ab";
        
        public const string AssetTagName = "__AssetTag";
        
        public const string AssetRootFolder = "Assets/__Art";

        public static readonly string[] TagIgnoredFolders = new[]
        {
            "_StaticScene",
        };

    }
}