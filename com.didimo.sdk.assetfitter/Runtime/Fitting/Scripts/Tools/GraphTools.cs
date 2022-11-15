using UnityEngine;
using System.Globalization;
using System;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Didimo.AssetFitter.Editor.Graph
{
    public static class GraphTools
    {
#if UNITY_EDITOR

        public static bool GetAssetFolder(ref string assetPath)
        {
            if (String.IsNullOrEmpty(assetPath)) assetPath = EditorUtility.SaveFolderPanel("Save Mesh(es) to file", "Assets", "");
            else assetPath = Path.Combine(Application.dataPath, assetPath);
            return !String.IsNullOrEmpty(assetPath) && GetAssetPath(ref assetPath);
        }
#endif


        static bool GetAssetPath(ref string path)
        {
            if (path.StartsWith("assets", false, CultureInfo.CurrentCulture))
                return true;

            if (path.StartsWith(Application.dataPath, false, CultureInfo.CurrentCulture))
            {
                path = path.Replace(Application.dataPath, "Assets");
                return true;
            }
            return false;
        }

    }

    public interface IExposable { }
}
