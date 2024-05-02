using System;
using Funky.Utility;
using Object = UnityEngine.Object;

namespace Funky.AssetTool.Runtime
{
    public class AssetHandle : IReset
    {
        public string Name;
        
        public Object Asset;

        public Action<bool, Object> OnComplete;
        
        public bool Cancelled;
        
        public void Reset()
        {
            Name = null;
            Asset = null;
            OnComplete = null;
            Cancelled = false;
        }
    }
}