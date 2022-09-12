using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.GraphData;
using static Didimo.AssetFitter.Editor.Graph.GeomTools;
using static UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Skinned Mesh/Skinned Mesh Combine")]
    [DisplayName("Skinned Mesh Combine")]
    [HeaderColor(TypeColors.SkinnedMeshRenderer)]
    public class CommandSkinnedMeshCombine : GraphNode
    {
        [Input("Prefab")] public GameObject prefabInput;
        [Input("Skin")] public SkinnedMeshRenderer skinInput;

        [Output("Prefab")] public GameObject prefabOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(prefabOutput))
            {
                List<GameObject> prefabs = GetInputValues<GameObject>(nameof(prefabInput));
                List<SkinnedMeshRenderer> skins = GetInputValues<SkinnedMeshRenderer>(nameof(skinInput));

                GameObject prefab = prefabs[0] as GameObject;

                if (Prerequisite(prefabs, skins) && Combine(prefabs, skins, out GameObject newPrefab))
                {
                    values = new List<object>() { newPrefab };
                    return true;
                }
            }
            return base.GetOutputValues(info, out values);
        }

        bool Combine(List<GameObject> prefabs, IEnumerable<SkinnedMeshRenderer> skins, out GameObject gameObject)
        {
            gameObject = null;
            GameObject prefab = prefabs.First();
            // PrefabUtility.SaveAsPrefabAsset(prefab, State.TempPath);

            // Create instance of the prefab
            State.Add(gameObject = UnityEngine.Object.Instantiate(prefab));
            gameObject.name = prefab.name;

            // Get the first skin and use as combined
            SkinnedMeshRenderer skin = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().First();
            skin.name = "Combined";

            // create the combined mesh from skins
            skin.sharedMesh = MeshTools.CombineMeshesIntoSubMeshes(skins.Select(s => s.sharedMesh).ToArray());
            skin.sharedMesh.bindposes = GetBindPoses(skin.bones);
            skin.sharedMesh.name = gameObject.name + "_" + skin.name;
            skin.sharedMesh.RecalculateBounds();

            // assign materials
            skin.sharedMaterials = skins.SelectMany(s => s.sharedMaterials).ToArray();

            // clean out the new prefab
            gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().Where(s => s != skin).ForAll(s => DestroyImmediate(s.gameObject));
            gameObject.GetComponentsInChildren<MeshRenderer>().ForAll(r => DestroyImmediate(r.gameObject));

            return true;
        }

        bool Prerequisite(List<GameObject> prefabs, IEnumerable<SkinnedMeshRenderer> skins)
        {
            if (prefabs.Count != 1)
            { Error("Too many Prefab inputs: " + prefabs.Count + "."); return true; }

            GameObject prefab = prefabs.First();
            SkinnedMeshRenderer[] pskins = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();

            if (!skins.All(s => pskins.Contains(s)))
            { Error("Skins do not belong to prefab!"); return false; }

            IEnumerable<Transform> rootBones = pskins.Select(s => s.rootBone);
            if (!rootBones.All(b => b == prefab.transform))
            { Error("Root bone must equal the prefab!"); return false; }

            Mesh[] meshes = skins.Select(s => s.sharedMesh).ToArray();
            Material[] materials = skins.SelectMany(s => s.sharedMaterials).ToArray();

            if (meshes.Length != materials.Length)
            { Error("Doesn't support submeshes, must be single mesh per skin!"); return false; }

            return true;
        }
    }
}