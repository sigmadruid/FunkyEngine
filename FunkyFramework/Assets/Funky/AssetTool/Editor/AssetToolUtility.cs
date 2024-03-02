using System;
using System.IO;
using UnityEngine;

namespace Funky.AssetTool.Editor
{
    public static class AssetToolUtility
    {
        public static string ToAssetPath(string path)
        {
            var startIndex = path.IndexOf("Assets/", StringComparison.Ordinal);
            if (startIndex < 0)
                startIndex = path.IndexOf("Assets\\", StringComparison.Ordinal);
            return path.Substring(startIndex);
        }

        public static bool IsMetaFile(string path)
        {
            return path.EndsWith(".meta");
        }
        
        public static bool IsAssetTag(string path)
        {
            return path.EndsWith($"{AssetConstant.AssetTagName}.asset");
        }

        public static void CreateFolder(string folder)
        {
            try
            {
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static bool UnderIgnoredFolder(string folder)
        {
            foreach (var ignored in AssetConstant.TagIgnoredFolders)
            {
                if (folder.Contains(ignored))
                    return true;
            }
            return false;
        }
    }
}