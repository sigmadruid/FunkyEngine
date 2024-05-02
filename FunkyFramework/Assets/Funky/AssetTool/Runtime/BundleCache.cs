using Funky.Utility;
using UnityEngine;
using Logger = Funky.Log.Logger;

namespace Funky.AssetTool.Runtime
{
    public class BundleCache : IReset
    {
        public string Name  { get; private set; }
        
        public AssetBundle Bundle { get; private set; }
        
        public int RefCount  { get; private set; }

        public bool IsValid => Bundle;
        
        public void Reset()
        {
            Name = null;
            Bundle = null;
            RefCount = 0;
        }
        
        public void Fill(string name, AssetBundle bundle)
        {
            if (IsValid)
            {
                Logger.Error($"refill a bundle cache! name={Name}");
                return;
            }

            if (!bundle)
            {
                Logger.Error($"fill an empty bundle! name={Name}");
                return;
            }

            Name = name;
            Bundle = bundle;
        }
        
        public void Release(bool unloadObjects)
        {
            if (IsValid)
            {
                Bundle.Unload(unloadObjects);
                Bundle = null;
            }
            RefCount = 0;
        }

        public void IncreaseRef()
        {
            RefCount++;
        }
        
        public void DecreaseRef()
        {
            RefCount--;
        }

    }
}