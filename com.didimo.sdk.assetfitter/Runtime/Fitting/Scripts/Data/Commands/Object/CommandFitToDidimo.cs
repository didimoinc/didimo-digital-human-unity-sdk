using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using static Didimo.AssetFitter.Editor.Graph.GeomTools;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using static Didimo.AssetFitter.Editor.Graph.AssetFitterHelper;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Object/Fit To Didimo")]
    [DisplayName("Fit To Didimo")]
    [HeaderColor("#50009FCC")]
    public class CommandFitToDidimo : GraphNode
    {
        [Input("Source Skin", true)] public SkinnedMeshRenderer sourceSkinInput;
        [Input("Accessories Skin")] public SkinnedMeshRenderer accessoriesInput;
        [Input("Accessories Filter")] public MeshFilter accessoriesFilterInput;
        [Input("Bone Map")] public BoneIndexRemap boneMapInput;
        [Input("Prefab", true)] public GameObject prefabInput;
        [Input("Wrap", true)] public Mesh wrapMesh;
        [Input("TPS", true)] public TPSWeights tpsWeightsInput;
        [Output("Prefab")] public GameObject prefabOutput;
        [Expose] public bool excludeDidimoMeshes;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            IEnumerable<SkinnedMeshRenderer> accessories = GetInputValues<SkinnedMeshRenderer>(nameof(accessoriesInput)).
                Where(a => !(a.GetComponent<AssetFitterHelper>()?.accessoryType == AssetFitterHelper.AccessoryType.Ignore));
            IEnumerable<MeshFilter> accessoriesFilter = GetInputValues<MeshFilter>(nameof(accessoriesFilterInput)).
                Where(a => !(a.GetComponent<AssetFitterHelper>()?.accessoryType == AssetFitterHelper.AccessoryType.Ignore));

            SkinnedMeshRenderer shapeSkin = GetInputValue<SkinnedMeshRenderer>(nameof(sourceSkinInput));

            if (accessories.Count() > 0 || accessoriesFilter.Count() > 0)
            {
                GameObject didimo = CloneAsset(GetInputValue<GameObject>(nameof(prefabInput)));
                values = new List<object> { didimo };

                //excludeDidimoMeshes
                //exclude didimo.GetComponentsInChildren<Renderer>(true);

                SkinnedMeshRenderer primarySkin = didimo.GetComponentInChildren<SkinnedMeshRenderer>();

                Transform[] sourceBones = GetInputValue<SkinnedMeshRenderer>(nameof(sourceSkinInput)).bones;
                BoneIndexRemap boneStructure = GetBoneIndexRemap();
                Dictionary<int, int> remapIndex = boneStructure?.GetRemapTable(sourceBones, primarySkin.bones);

                foreach (var accessory in accessories)
                {
                    AccessoryType type = GetAccessoryType(accessory);
                    if (type == AccessoryType.Ignore) continue;

                    Mesh bmesh = CloneAsset(accessory.sharedMesh);
                    //Debug.Log("Accessory: " + mesh.boneWeights.Count());
                    accessory.BakeMesh(bmesh);
                    //Debug.Log("After Bake: " + mesh.boneWeights.Count());
                    Mesh mesh = CloneAsset(accessory.sharedMesh);
                    mesh.vertices = bmesh.vertices;
                    mesh.normals = bmesh.normals;
                    mesh.tangents = bmesh.tangents;
                    mesh.RecalculateBounds();

                    if (shapeSkin.bones.Intersect(accessory.bones).Count() == 0)
                    {
                        Transform parent = boneStructure.FindParentBone(accessory.transform, sourceBones, primarySkin.bones);
                        if (!parent)
                        {
                            Debug.LogWarning(accessory.name + " can't find parent to remap to.");
                            continue;
                        }

                        GameObject obj = new GameObject(accessory.name) { transform = { parent = parent } };

                        obj.AddComponent<MeshFilter>().sharedMesh = ApplyTransform(mesh, accessory.transform);
                        obj.AddComponent<MeshRenderer>().sharedMaterials = accessory.sharedMaterials;
                        continue;
                    }

                    BoneIndexRemap remap = GetBoneIndexRemap(type);
                    SkinnedMeshRenderer nskin = new GameObject(mesh.name) { transform = { parent = primarySkin.transform.parent } }.
                        AddComponent<SkinnedMeshRenderer>();

                    nskin.sharedMaterials = accessory.sharedMaterials;
                    nskin.sharedMesh = ApplyTransform(mesh);

                    string[] boneNames = accessory.bones.Select(b => b.name).ToArray();

                    nskin.sharedMesh.boneWeights = mesh.boneWeights.Select((bw, i) => CommandSkinnedMeshRemapBind.RemapBone(boneNames, i, bw,
                                                                                remap?.GetRemapTable(accessory.bones, primarySkin.bones))).ToArray();

                    nskin.sharedMesh.bindposes = GetBindPoses(primarySkin.bones);
                    nskin.rootBone = primarySkin.rootBone;
                    nskin.bones = primarySkin.bones;
                }

                foreach (var filter in accessoriesFilter)
                {
                    Transform parent = boneStructure.FindParentBone(filter.transform, sourceBones, primarySkin.bones);
                    if (!parent)
                    {
                        Debug.LogWarning(filter.name + " couldn't find a parent bone to remap to: Mesh Filters should be in the rig hierarchy so they can be properly animated.");
                        continue;
                    }

                    GameObject obj = new GameObject(filter.name) { transform = { parent = parent } };

                    obj.AddComponent<MeshFilter>().sharedMesh = ApplyTransform(filter.sharedMesh, filter.transform);
                    obj.AddComponent<MeshRenderer>().sharedMaterials = filter.GetComponent<MeshRenderer>().sharedMaterials;
                }

                //exclude didimo meshes here:`
                // renderers.ForEach(r=>DestroyImmediate(r.gameObject))
                return true;
            }
            return base.GetOutputValues(info, out values);
        }

        Mesh ApplyTransform(Mesh mesh, Transform transform = null)
        {
            if (transform)
            {
                mesh = CloneAsset(mesh);
                mesh.vertices = mesh.vertices.Select(v => transform.TransformPoint(v)).ToArray();
            }

            if (CommandMeshDeformTPS.Transform(
                    new List<Mesh> { mesh },
                    new List<TPSWeights> { GetInputValue<TPSWeights>(nameof(tpsWeightsInput)) },
                    out List<object> values
                ))
            {
                return values.First() as Mesh;
            }
            return null;
        }

        private AccessoryType GetAccessoryType(SkinnedMeshRenderer skin)
        {
            AssetFitterHelper helper = skin.GetComponent<AssetFitterHelper>();
            return helper ? helper.accessoryType : AccessoryType.Auto;
        }

        BoneIndexRemap GetBoneIndexRemap(AccessoryType type = AccessoryType.Auto)
        {
            Dictionary<string, BoneIndexRemap> boneMapInput = GetInputValues<BoneIndexRemap>(nameof(this.boneMapInput)).ToDictionary(k => k.name.Split("_").Last(), v => v);

            if (!boneMapInput.TryGetValue(System.Enum.GetName(typeof(AccessoryType), type), out BoneIndexRemap remap))
            {
                if (!boneMapInput.TryGetValue(System.Enum.GetName(typeof(AccessoryType), AccessoryType.Auto), out remap))
                    throw new System.Exception("Missing Bone Index Remap 'Auto'");
            }
            return remap;
        }
    }
}