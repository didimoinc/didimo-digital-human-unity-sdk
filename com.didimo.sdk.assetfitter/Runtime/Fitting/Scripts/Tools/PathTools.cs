using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    public static class PathTools
    {
        public static string GetFullPath(string assetPath)
        {
            if (!assetPath.StartsWith("Assets")) throw new Exception("Path must start with 'Assets'");
            return Application.dataPath + "/" + assetPath.Substring("Assets".Length);
        }

        public static void CreatePath(string path, bool isFile = false)
        {
            if (isFile)
            {
                var file = new FileInfo(path);
                path = file.DirectoryName;
            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
#if UNITY_EDITOR
        public static void CreateAssetPath(string assetPath) => AssetDatabase.CreateFolder(Path.GetDirectoryName(assetPath), Path.GetFileName(assetPath));
        public static void RemoveAssetPath(string assetPath) => AssetDatabase.DeleteAsset(assetPath);
#else
        public static void CreateAssetPath(string assetPath){}

        public static void RemoveAssetPath(string assetPath){}
#endif
    }
}
