using System;
using Funky.Utility;
using UnityEngine;

namespace Funky.AssetTool.Runtime
{
    public class BundleHandle : IReset
    {
        public string Name;

        public AssetBundle Bundle;

        public Action<BundleHandle> OnComplete;

        public bool Cancelled;
        
        public void Reset()
        {
            Name = null;
            Bundle = null;
            OnComplete = null;
            Cancelled = false;
        }
    }
}