using System.Collections.Generic;
using Funky.Utility;
using Newtonsoft.Json;

namespace Funky.AssetTool.Runtime
{
    public class AssetToBundles
    {
        private Dictionary<string, string> _assetToBundle;
        
        public AssetToBundles(PathProvider pathProvider)
        {
            var json = IOUtility.ReadAllText(pathProvider.BundleDependencyPath);
            _assetToBundle = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public void Release()
        {
            _assetToBundle = null;
        }

        public string GetBundleName(string assetName)
        {
            if (_assetToBundle.TryGetValue(assetName, out var bundleName))
            {
                return bundleName;
            }
            return string.Empty;
        }
    }
}