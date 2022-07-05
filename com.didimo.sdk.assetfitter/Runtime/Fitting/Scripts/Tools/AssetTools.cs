using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.PathTools;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    public static class AssetTools
    {
        static AssetTools()
        {
            TrueColors = HTMLColors.Select(c => ColorUtility.TryParseHtmlString(c[0], out Color color) ? color : Color.magenta).ToArray();
        }

        public static string HashArray(int[] array) =>
            HashArray(array.SelectMany(v => new byte[] {
                (byte)((v >> 24) & 0xff),
                (byte)((v >> 16) & 0xff),
                (byte)((v >> 8) & 0xff),
                (byte)((v >> 0) & 0xff) }).ToArray());

        public static string HashArray(byte[] array)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
                return String.Join("", sha256.ComputeHash(array).Select(b => b.ToString("X2")));
        }

        static Material DefaultMaterial;
        public static Material GetDefaultMaterial()
        {
            if (!DefaultMaterial)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                DefaultMaterial = UnityEngine.Object.Instantiate(cube.GetComponent<MeshRenderer>().sharedMaterial);
                UnityEngine.Object.DestroyImmediate(cube);
            }
            return DefaultMaterial;
        }

        public static Material[] GetColorMaterials(int count)
        {
            Material create(Color color) => CloneAsset(GetDefaultMaterial(), (m) => { m.color = color; });
            return TrueColors.OrderBy(c => UnityEngine.Random.value).Take(count).Select(c => create(c)).ToArray();
        }

        public static T CloneAsset<T>(T obj, Action<T> initialize = null) where T : UnityEngine.Object
        {
            var result = UnityEngine.Object.Instantiate(obj);
            result.name = obj.name;
            initialize?.Invoke(result);
            return result;
        }

        public static Mesh[] GetAllMeshes(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(m => m.sharedMesh).
                Concat(gameObject.GetComponentsInChildren<MeshFilter>().Select(m => m.sharedMesh)).ToArray();
        }

        public static Material[] GetAllMaterials(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true).SelectMany(m => m.sharedMaterials).
                Concat(gameObject.GetComponentsInChildren<MeshRenderer>().SelectMany(m => m.sharedMaterials)).ToArray();
        }

#if UNITY_EDITOR
        public static UnityEngine.Object CreateAsset(UnityEngine.Object asset, string path, bool replace = true)
        {
            CreatePath(Path.GetDirectoryName(path));
            Debug.Log("Creating asset: " + "<a href=\"" + path + "\"" + ">" + path + "</a>");

            if (asset is GameObject)
            {
                PrefabUtility.SaveAsPrefabAsset(asset as GameObject, path);
            }
            else
            {
                UnityEngine.Object existingAsset = AssetDatabase.LoadMainAssetAtPath(path);
                if (existingAsset && replace)
                {
                    asset.name = existingAsset.name;
                    EditorUtility.CopySerialized(asset, existingAsset);
                    asset = existingAsset;
                    EditorUtility.SetDirty(asset);
                }
                else AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }
#endif



        public enum Gender
        {
            None = 0,
            Male = 1,
            Female = 2,
            Neutral = 3,
        }

        public static Color[] TrueColors;
        public static string[][] HTMLColors = new string[][]
        {
            new [] {"#556b2f","darkolivegreen"},
            new [] {"#800000","maroon"},
            new [] {"#483d8b","darkslateblue"},
            new [] {"#008000","green"},
            new [] {"#3cb371","mediumseagreen"},
            new [] {"#b8860b","darkgoldenrod"},
            new [] {"#008b8b","darkcyan"},
            new [] {"#000080","navy"},
            new [] {"#32cd32","limegreen"},
            new [] {"#8b008b","darkmagenta"},
            new [] {"#ff4500","orangered"},
            new [] {"#ff8c00","darkorange"},
            new [] {"#40e0d0","turquoise"},
            new [] {"#00ff00","lime"},
            new [] {"#9400d3","darkviolet"},
            new [] {"#00fa9a","mediumspringgreen"},
            new [] {"#dc143c","crimson"},
            new [] {"#00bfff","deepskyblue"},
            new [] {"#0000ff","blue"},
            new [] {"#adff2f","greenyellow"},
            new [] {"#ff00ff","fuchsia"},
            new [] {"#1e90ff","dodgerblue"},
            new [] {"#db7093","palevioletred"},
            new [] {"#f0e68c","khaki"},
            new [] {"#ffff54","laserlemon"},
            new [] {"#add8e6","lightblue"},
            new [] {"#ff1493","deeppink"},
            new [] {"#7b68ee","mediumslateblue"},
            new [] {"#ffa07a","lightsalmon"},
            new [] {"#ee82ee","violet"},
            new [] {"#ffc0cb","pink"},
            new [] {"#696969","dimgray"},
        };

    }

    public static partial class EnumerableUtility
    {
        /// <summary>Similar to List<>.ForEach</summary>
        public static void ForAll<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
                action(item);
        }

        /// <summary>Similar to List<>.ForEach with indexing as Linq.Select</summary>
        public static void ForAll<T>(this IEnumerable<T> enumeration, Action<T, int> action)
        {
            T[] items = enumeration.ToArray();
            for (int i = 0; i < items.Length; action(items[i], i), i++) ;
        }
    }

}
