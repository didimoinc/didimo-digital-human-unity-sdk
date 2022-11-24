using System.ComponentModel;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using static Didimo.AssetFitter.Editor.Graph.GraphTools;
using static Didimo.AssetFitter.Editor.Graph.GraphData;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Prefab/Prefab Save")]
    [DisplayName("Prefab Save")]
    [Width(200)]
    [HeaderColor(64 / 255f, 64 / 255f, 192 / 255f, 0.8f)]
    public class CommandPrefabSave : GraphNode
    {
        [Input("Prefab")] public GameObject prefabInput;
        [Input("Path")] [Expose] public string path;

        [Expose] public Options options = Options.IncludeMaterials | Options.IncludeTextures;

        [Expose] public SaveType saveType;// = SaveType.UnityAsset;
#if UNITY_EDITOR
        internal override void EndPoint(bool Build = false)
        {
            List<object> prefabs = GetInputValues(GetType().GetField(nameof(prefabInput)));
            if (prefabs.Count > 0)
            {
                string path = this.path;
                if (GetAssetFolder(ref path))
                {
                    for (int i = 0; i < prefabs.Count; i++)
                    {
                        GameObject prefab = prefabs[i] as GameObject;

                        if (PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.NotAPrefab)
                            State.Add(prefab = UnityEngine.Object.Instantiate(prefab));// ????

                        if (saveType == SaveType.UnityAsset)
                        {
                            string assetPath = path + "/asset-" + DateTime.Now.ToString("yyMMdd-HHmmss" + DateTime.Now.Millisecond); //"yyyy'-'MM'-'dd'T'HH':'mm':'ss");
                            PathTools.CreatePath(assetPath);
                            WriteAllAssets(prefab, assetPath);
                            CreateAsset(prefab, assetPath + "/" + prefab.name + ".prefab");
                            UnityEngine.Object.DestroyImmediate(prefab);
                        }
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else Debug.LogError("No valid asset path '" + path + "'");
            }
        }


        void WriteAllAssets(GameObject gameObject, string assetPath)
        {
            Dictionary<UnityEngine.Object, UnityEngine.Object> objectCache = new Dictionary<UnityEngine.Object, UnityEngine.Object>();

            IEnumerable<string> GetTextureProperties(Material material)
            {
                for (int i = 0; i < ShaderUtil.GetPropertyCount(material.shader); i++)
                    if (ShaderUtil.GetPropertyType(material.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                        yield return ShaderUtil.GetPropertyName(material.shader, i);
            }

            T CreateAsset<T>(T asset, string assetPath, Action<T, string> create, bool copy = false) where T : UnityEngine.Object
            {
                if (!objectCache.ContainsKey(asset))
                {
                    // if exists then get a new name
                    if (File.Exists(assetPath))
                        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath); // _1

                    string originalAssetPath = AssetDatabase.GetAssetPath(asset);

                    PathTools.CreatePath(Path.GetDirectoryName(assetPath));
                    // Debug.Log("Create Asset: Asset " + asset.name + " originalAssetPath: " + originalAssetPath + " assetPath: " + assetPath);

                    if (copy && !string.IsNullOrEmpty(originalAssetPath))
                    {
                        //throw new Exception("Not implemented!");
                        AssetDatabase.CopyAsset(originalAssetPath, assetPath); // does this create the path? Does the database require a refresh?
                    }
                    else
                        create(asset, assetPath);

                    AssetDatabase.Refresh();
                    objectCache.Add(asset, AssetDatabase.LoadAssetAtPath<T>(assetPath));
                    // Debug.Log("Create Asset: Dict return" + objectCache[asset].name);

                }
                return (T)objectCache[asset];
            }

            // renderer1
            //      material1
            //  	    texture1
            // renderer2
            //      material1
            //          texture1

            Texture2D ExportTexture(Texture2D texture, string path)
            {
                void Create(Texture2D asset, string p)
                {
                    // Debug.Log("ExportTexture(Create):asset " + asset + " path " + p);
                    File.WriteAllBytes(p, asset.CreateReadable().EncodeToPNG());
                }
                // Debug.Log("ExportTexture: " + assetPath + " Texture: " + texture);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return CreateAsset(texture, path + "/" + texture.name + ".png", Create, true);
            }

            Material ExportMaterial(Material material, string path)
            {
                if ((options & Options.IncludeMaterials) > 0)
                {
                    void Create(Material asset, string p) => AssetDatabase.CreateAsset(CloneAsset(asset), p);

                    material = CreateAsset(material, path + "/" + material.name + ".mat", Create);
                    // Debug.Log("ExportMaterial: Path " + path + material.name + ".mat" + " and mat -> " + material.name);

                    if ((options & Options.IncludeTextures) > 0)
                    {
                        foreach (string name in GetTextureProperties(material).Where(p => material.GetTexture(p)))
                        {
                            // Debug.Log("ExportMaterial(SetText): texture path " + path + " and text used -> " + name);
                            material.SetTexture(name, ExportTexture(material.GetTexture(name) as Texture2D, path));
                        }

                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                return material;
            }

            Mesh ExportMesh(Mesh mesh, string path)
            {
                void Create(Mesh asset, string p) => AssetDatabase.CreateAsset(CloneAsset(asset), p);
                mesh = CreateAsset(mesh, path + "/" + mesh.name + ".mesh", Create);
                // Debug.Log("ExportMesh: Path " + path + mesh.name + ".asset" + " and mesh -> " + mesh.name);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return mesh;
            }

            void ExportRenderer(Renderer renderer, string path)
            {
                renderer.SetMesh(ExportMesh(renderer.GetMesh(), path));
                renderer.sharedMaterials = renderer.sharedMaterials.Select(m => ExportMaterial(m, path)).ToArray();
                Debug.Log("ExportRenderer: Path " + path + " and rend -> " + renderer.name);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            foreach (var renderer in GetAllRenderers(gameObject))
            {
                Debug.Log("Renderers: Rname " + renderer.name + " gName: " + renderer.gameObject.name);
                ExportRenderer(renderer, assetPath + "/" + renderer.name + "/");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
        public enum SaveType
        {
            Simulate = 0,
            Hierarchy = 1,
            UnityAsset = 2,
        }

        [Flags]
        public enum Options
        {
            // Default = IncludeMaterials | IncludeTextures,
            IncludeMaterials = 1 << 0,
            IncludeTextures = 1 << 1,
        }
    }
}