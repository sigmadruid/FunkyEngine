using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Funky.AssetTool.Editor
{
    public static class AssetBundleTool
    {
        [MenuItem("Funky Framework/Asset Tool/Build AssetBundles", false, 1)]
        private static void BuildAllAssetBundles()
        {
            var outputPath = Path.Combine(Application.streamingAssetsPath, "AssetBundles");
            AssetToolUtility.CreateFolder(outputPath);
            
            var bundleBuilds = CollectAssetBundleBuilds();

            var options = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle;

            var target = BuildTarget.StandaloneWindows64;
            
            BuildPipeline.BuildAssetBundles(outputPath, bundleBuilds, options, target);
        }
        
        private static AssetBundleBuild[] CollectAssetBundleBuilds()
        {
            var allAssetPaths = AssetDatabase.GetAllAssetPaths();
            var allTagPath = new List<string>();
            foreach (var path in allAssetPaths)
            {
                if (AssetToolUtility.IsAssetTag(path))
                {
                    allTagPath.Add(path);
                }
            }

            var assetPathList = new List<string>();
            var buildList = new List<AssetBundleBuild>();
            foreach (var tagPath in allTagPath)
            {
                var fileInfo = new FileInfo(tagPath);
                var folderPath = fileInfo.DirectoryName;
                folderPath = AssetToolUtility.ToAssetPath(folderPath);

                if (string.IsNullOrEmpty(folderPath)) continue;
                
                assetPathList.Clear();
                var assetsInFolder = Directory.GetFiles(folderPath);
                foreach (var path in assetsInFolder)
                {
                    if (AssetToolUtility.IsMetaFile(path) || AssetToolUtility.IsAssetTag(path)) continue;

                    var assetPath = AssetToolUtility.ToAssetPath(path);
                    assetPathList.Add(assetPath);
                }

                if (assetPathList.Count == 0) continue;

                var bundleName = GetAssetBundleName(folderPath);
                var build = new AssetBundleBuild()
                {
                    assetBundleName = bundleName,
                    assetNames = assetPathList.ToArray(),
                };
                buildList.Add(build);
            }
            return buildList.ToArray();
        }
        
        [MenuItem("Funky Framework/Asset Tool/Generate Asset Tags", false, 2)]
        private static void GenerateAssetTags()
        {
            var allAssetFolders = Directory.GetDirectories(AssetConstant.AssetRootFolder, "*.*", SearchOption.AllDirectories);
            foreach (var folder in allAssetFolders)
            {
                var assetsInFolder = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly);
                var hasValidAsset = false;
                foreach (var assetPath in assetsInFolder)
                {
                    if (!AssetToolUtility.IsMetaFile(assetPath) && !AssetToolUtility.IsAssetTag(assetPath))
                    {
                        hasValidAsset = true;
                        break;
                    }
                }
                var underIgnoredFolder = AssetToolUtility.UnderIgnoredFolder(folder);
                
                var assetFolder = AssetToolUtility.ToAssetPath(folder);
                var tagPath = Path.Combine(assetFolder, AssetConstant.AssetTagName + ".asset");
                var assetTag = AssetDatabase.LoadAssetAtPath<AssetTag>(tagPath);

                if (hasValidAsset && !underIgnoredFolder)
                {
                    if (!assetTag)
                    {
                        assetTag = ScriptableObject.CreateInstance<AssetTag>();
                        AssetDatabase.CreateAsset(assetTag, tagPath);
                    }
                }
                else
                {
                    if (assetTag)
                    {
                        AssetDatabase.DeleteAsset(tagPath);
                    }
                }
            }
        }

        [MenuItem("Funky Framework/Asset Tool/Remove Asset Tags", false, 3)]
        private static void ClearAllAssetTags()
        {
            var allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (var path in allAssetPaths)
            {
                if (AssetToolUtility.IsAssetTag(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        private static string GetAssetBundleName(string folder)
        {
            return folder.ToLower().Replace('/', '@').Replace('\\', '@');
        }



    }
}