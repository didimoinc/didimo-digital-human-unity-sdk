using System;
using System.ComponentModel;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using static Didimo.AssetFitter.Editor.Graph.GraphTools;
using static Didimo.AssetFitter.Editor.Graph.GraphData;
using System.Linq;

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
        [Expose] public SaveType saveType = SaveType.UnityAsset;
#if UNITY_EDITOR
        internal override void EndPoint(bool Build = false)
        {
            var prefabs = GetInputValues(GetType().GetField(nameof(prefabInput)));
            if (prefabs.Count > 0)
            {
                var path = this.path;
                if (GetAssetFolder(ref path))
                {
                    for (int i = 0; i < prefabs.Count; i++)
                    {
                        var assetPath = string.Format(path + "/prefab-{0:000}.prefab", i);
                        var prefab = prefabs[i] as GameObject;

                        if (PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.NotAPrefab)
                            State.Add(prefab = UnityEngine.Object.Instantiate(prefab)); // ????

                        if (saveType == SaveType.UnityAsset)
                        {
                            WriteAllAssets(prefab, assetPath);
                            CreateAsset(prefab, assetPath);
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
            assetPath = System.IO.Path.ChangeExtension(assetPath, null);

            Debug.Log("Count: " + GetAllMeshes(gameObject).Where(m => String.IsNullOrEmpty(AssetDatabase.GetAssetPath(m))).Count());

            GetAllMeshes(gameObject).Where(m => String.IsNullOrEmpty(AssetDatabase.GetAssetPath(m))).
                ForAll((m, i) => CreateAsset(m, string.Format(assetPath + "/mesh-{0:000}.asset", i)));

            GetAllMaterials(gameObject).Where(m => String.IsNullOrEmpty(AssetDatabase.GetAssetPath(m))).ToArray().
                  ForAll((m, i) => CreateAsset(m, string.Format(assetPath + "/material-{0:000}.mat", i)));
        }
#endif


        public enum SaveType
        {
            Simulate = 0,
            Hierarchy = 1,
            UnityAsset = 2,
        }
    }

}
