using System;
using Object = UnityEngine.Object;

namespace Funky.AssetTool.Runtime
{
    public class AssetHandle
    {
        public Object Asset;

        public Action<bool, Object> OnComplete;
    }
}