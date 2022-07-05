using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.GeomTools;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Skinned Mesh/Skinned Mesh Remap Bind")]
    [DisplayName("Skin Remap Bind")]
    [HeaderColor(TypeColors.SkinnedMeshRenderer)]
    public class CommandSkinnedMeshRemapBind : GraphNode
    {
        [Input("Prefab", true)] public GameObject prefabInput;
        [Input("Bone Map", true)] public BoneIndexRemap boneMapInput;
        [Input("Bones", true)] public Transform[] skinBonesInput;
        [Input("Materials")] public Material[] materialsInput;
        [Input("Mesh")] public Mesh meshInput;
        [Output("Prefab")] public GameObject prefabOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            var prefab = GetInputValues<GameObject>(nameof(prefabInput)).FirstOrDefault();
            var remap = GetInputValues<BoneIndexRemap>(nameof(boneMapInput)).FirstOrDefault();
            var bones = GetInputValues<Transform[]>(nameof(skinBonesInput));
            var materials = GetInputValues<Material[]>(nameof(materialsInput));
            var meshes = GetInputValues<Mesh>(nameof(meshInput));

            if (Bind(prefab, remap, bones, materials, meshes, out GameObject newPrefab))
            {
                values = new List<object>() { newPrefab };
                return true;
            }
            return base.GetOutputValues(info, out values);
        }

        bool Bind(GameObject prefab, BoneIndexRemap remap, List<Transform[]> boneGroups, List<Material[]> materials, List<Mesh> meshes, out GameObject gameObject)
        {
            if (boneGroups.Count != meshes.Count || materials.Count != meshes.Count)
                throw new Exception("Bones Count doesn't equal Meshes Count: '" + boneGroups.Count + "' != '" + meshes.Count + "' != '" + materials.Count);

            gameObject = CloneAsset(prefab);

            var primarySkin = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

            for (int i = 0; i < boneGroups.Count; i++)
            {
                var bones = boneGroups[i];
                var mesh = meshes[i];
                var map = remap?.GetRemapTable(bones, primarySkin.bones);

                var nskin = new GameObject(mesh.name) { transform = { parent = primarySkin.transform.parent } }.
                    AddComponent<SkinnedMeshRenderer>();

                nskin.sharedMaterials = materials[i]; //something wrong here
                Debug.Log(string.Join(",", materials[i].Select(v => v.name + " " + v.shader.name)));
                nskin.sharedMesh = CloneAsset(mesh);

                string[] boneNames = bones.Select(b => b.name).ToArray();

                nskin.sharedMesh.boneWeights = mesh.boneWeights.Select((bw, i) => RemapBone(boneNames, i, bw, map)).ToArray();
                nskin.sharedMesh.bindposes = GetBindPoses(primarySkin.bones);
                nskin.rootBone = primarySkin.rootBone;
                nskin.bones = primarySkin.bones;
            }
            return true;
        }

        BoneWeight RemapBone(string[] boneNames, int i, BoneWeight weight, Dictionary<int, int> iRemap)
        {
            // if (iRemap == null) return new BoneWeight();
            int change = 0;
            int hasBone = 0;
            var originalWeights = weight;

            if (!(weight.boneIndex0 == 0 && weight.weight0 == 0))
            {
                hasBone |= 1 << 0;

                if (iRemap.ContainsKey(weight.boneIndex0))
                {
                    weight.boneIndex0 = iRemap[weight.boneIndex0];
                    change |= 1 << 0;
                }
            }

            if (!(weight.boneIndex1 == 0 && weight.weight1 == 0))
            {
                hasBone |= 1 << 1;

                if (iRemap.ContainsKey(weight.boneIndex1))
                {
                    weight.boneIndex1 = iRemap[weight.boneIndex1];
                    change |= 1 << 1;
                }
            }

            if (!(weight.boneIndex2 == 0 && weight.weight2 == 0))
            {
                hasBone |= 1 << 2;

                if (iRemap.ContainsKey(weight.boneIndex2))
                {
                    weight.boneIndex2 = iRemap[weight.boneIndex2];
                    change |= 1 << 2;
                }
            }

            if (!(weight.boneIndex3 == 0 && weight.weight3 == 0))
            {
                hasBone |= 1 << 3;

                if (iRemap.ContainsKey(weight.boneIndex3))
                {
                    weight.boneIndex3 = iRemap[weight.boneIndex3];
                    change |= 1 << 3;
                }
            }


            if (change != hasBone)
            {
                //Debug.Log(boneNames[originalWeights.boneIndex0] + " " + boneNames[originalWeights.boneIndex1] + " " + boneNames[originalWeights.boneIndex2] + " " + boneNames[originalWeights.boneIndex3]);
                //Debug.Log(i + " change: " + Convert.ToString(change, 2) + " hasBone: " + Convert.ToString(hasBone, 2));
                return weight;
            }

            if (weight.boneIndex0 == weight.boneIndex1)
            {
                weight.weight0 += weight.weight1;
                weight.weight1 = weight.weight2;
                weight.weight2 = weight.weight3;
                weight.weight3 = 0;

                weight.boneIndex1 = weight.boneIndex2;
                weight.boneIndex2 = weight.boneIndex3;
                weight.boneIndex3 = 0;
            }

            if (weight.boneIndex0 == weight.boneIndex2)
            {
                weight.weight0 += weight.weight2;
                weight.weight2 = weight.weight3;
                weight.weight3 = 0;

                weight.boneIndex2 = weight.boneIndex3;
                weight.boneIndex3 = 0;
            }

            if (weight.boneIndex0 == weight.boneIndex3)
            {
                weight.weight0 += weight.weight3;
                weight.weight3 = 0;

                weight.boneIndex3 = 0;
            }

            if (weight.boneIndex1 == weight.boneIndex2)
            {
                weight.weight1 += weight.weight2;
                weight.weight2 = weight.weight3;
                weight.weight3 = 0;

                weight.boneIndex2 = weight.boneIndex3;
                weight.boneIndex3 = 0;
            }

            if (weight.boneIndex1 == weight.boneIndex3)
            {
                weight.weight1 += weight.weight3;
                weight.weight3 = 0;

                weight.boneIndex3 = 0;
            }

            if (weight.boneIndex2 == weight.boneIndex3)
            {
                weight.weight2 += weight.weight3;
                weight.weight3 = 0;

                weight.boneIndex3 = 0;
            }
            return weight;
        }
    }
}