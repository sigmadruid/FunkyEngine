using System.Collections.Generic;
using Funky.Log;
using Funky.Utility;
using Newtonsoft.Json;

namespace Funky.AssetTool.Runtime
{
    public class BundleDependency
    {
        private static readonly string[] Empty = {};
        
        private Dictionary<string, string[]> _dependencies;
        public void Initialize(PathProvider pathProvider)
        {
            var dependencyJson = IOUtility.ReadAllText(pathProvider.BundleDependencyPath);
            _dependencies = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(dependencyJson);
        }

        public void Dispose()
        {
            _dependencies = null;
        }

        public string[] GetDependentBundles(string bundleName)
        {
            if (_dependencies.TryGetValue(bundleName, out var bundles))
            {
                return bundles;
            }
            Logger.Warn($"can't find dependency of {bundleName}");
            return Empty;
        }
    }
}