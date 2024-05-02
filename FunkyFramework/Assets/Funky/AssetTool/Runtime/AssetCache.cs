using Funky.Utility;
using UnityEngine;
using Logger = Funky.Log.Logger;

namespace Funky.AssetTool.Runtime
{
    public class AssetCache : IReset
    {
        public string Name { get; private set; }

        public Object Asset { get; private set; }
        
        public bool IsValid => Asset;
        
        public int RefCount  { get; private set; }
        
        public void Release()
        {
            if (IsValid)
            {
                Object.Destroy(Asset);
                Asset = null;
            }
            RefCount = 0;
        }
        
        public void Reset()
        {
            RefCount = 0;
        }

        public void Fill(string name, Object asset)
        {
            if (IsValid)
            {
                Logger.Error($"refill an asset cache! name={name}");
                return;
            }

            if (!asset)
            {
                Logger.Error($"fill an empty asset! name={name}");
                return;
            }

            Name = name;
            Asset = asset;
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