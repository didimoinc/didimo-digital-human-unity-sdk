using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.GraphData;
using static Didimo.AssetFitter.Editor.Graph.GeomTools;
using static Didimo.AssetFitter.Editor.Graph.MeshTools;
using static UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Skinned Mesh/Skinned Mesh Bind")]
    [DisplayName("Skinned Mesh Bind")]
    [HeaderColor(TypeColors.SkinnedMeshRenderer)]
    public class CommandSkinnedMeshBind : GraphNode
    {
        [Input("Prefab", true)] public GameObject prefabInput;
        [Input("Skin")] public SkinnedMeshRenderer skinInput;
        [Input("Mesh")] public Mesh meshInput;
        [Output("Prefab")] public GameObject prefabOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            GameObject prefab = GetInputValues<GameObject>(nameof(prefabInput)).FirstOrDefault();
            List<SkinnedMeshRenderer> skins = GetInputValues<SkinnedMeshRenderer>(nameof(skinInput));
            List<Mesh> meshes = GetInputValues<Mesh>(nameof(meshInput));

            if (Bind(prefab, skins, meshes, out GameObject newPrefab))
            {
                values = new List<object>() { newPrefab };
                return true;
            }
            return base.GetOutputValues(info, out values);
        }

        bool Bind(GameObject prefab, IEnumerable<SkinnedMeshRenderer> skins, IEnumerable<Mesh> meshes, out GameObject gameObject)
        {
            gameObject = Instantiate(prefab);
            SkinnedMeshRenderer primarySkin = skins.First();
            int primaryIndex = GetDescendants(prefab.transform).IndexOf(primarySkin.transform);

            primarySkin = GetDescendants(gameObject.transform)[primaryIndex].GetComponent<SkinnedMeshRenderer>();

            MeshVolume volume = GetMeshVolume(skins);

            foreach (var mesh in meshes)
            {
                SkinnedMeshRenderer skin = new GameObject("Mesh") { transform = { parent = primarySkin.transform.parent } }.
                    AddComponent<SkinnedMeshRenderer>();

                skin.rootBone = primarySkin.rootBone;
                skin.bones = primarySkin.bones;
                skin.sharedMesh = GetBoneWeights(mesh, volume, skin.bones);
            }
            return true;
        }

        MeshVolume GetMeshVolume(IEnumerable<SkinnedMeshRenderer> skins)
        {
            string key = State.CKEY(this, "skins:" + String.Join("_", skins.Select(s => s.GetInstanceID())));
            if (!State.Has(key)) State.AddValues(key, new MeshVolume(CombineMeshesIntoSubMeshes(skins.Select(s => s.sharedMesh))));
            return State.GetValue<MeshVolume>(key);
        }

        // ONLY SUPPORTS 4 BONES PER VERTEX
        Mesh GetBoneWeights(Mesh mesh, MeshVolume volume, Transform[] bones)
        {
            mesh = Instantiate(mesh);
            mesh.boneWeights = mesh.vertices.Select(p => volume.GetBoneWeight(p)).ToArray();
            mesh.bindposes = GetBindPoses(bones);
            return mesh;
        }

        class MeshVolume
        {
            public readonly Mesh mesh;
            public readonly Vector3[] vertices;
            public readonly int[] indices;
            public readonly BoneWeight[] weights;
            public readonly Partition3D<int> partition;

            public MeshVolume(Mesh mesh)
            {
                this.mesh = mesh;
                vertices = mesh.vertices;
                indices = mesh.triangles;
                weights = mesh.boneWeights;
                partition = new Partition3D<int>(new Bounds(mesh.bounds.center, mesh.bounds.size * 1.01f), 0.1f);

                for (int i = 0, n = indices.Length; i < n; i += 3)
                {
                    Bounds v = Partition3D<int>.GetVolume(vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]]);
                    partition.Add(i, v);
                }



                // foreach (var v in vertices)
                //     Debug.Log(partition.GetIndex(v) + " " + v);
            }


            PointHit GetClosest(Vector3 point, IEnumerable<int> faceIndices)
            {
                PointHit closest = new PointHit { distance = 1000, triangleIndex = -1 };
                foreach (int i in faceIndices)
                {
                    if (PointToTriangle(point, vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]], out PointHit hit))
                    {
                        if (hit.distance < closest.distance)
                        {
                            hit.triangleIndex = i;
                            closest = hit;
                        }
                    }
                }
                return closest;
            }
            public PointHit GetClosest(Vector3 point) => GetClosest(point, partition.Get(point));
            public PointHit GetClosest(Bounds area) => GetClosest(area.center, partition.Get(area));
            // {
            //     var closest = new PointHit { distance = 1000, triangleIndex = -1 };
            //     foreach (int i in partition.Get(point))
            //     {
            //         if (PointToTriangle(point, vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]], out PointHit hit))
            //         {
            //             if (hit.distance < closest.distance)
            //             {
            //                 hit.triangleIndex = i;
            //                 closest = hit;
            //             }
            //         }
            //     }
            //     return closest;
            // }

            // public PointHit GetClosest(Bounds area)
            // {
            //     var closest = new PointHit { distance = 1000, triangleIndex = -1 };
            //     foreach (int i in partition.Get(area))
            //     {
            //         if (PointToTriangle(area.center, vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]], out PointHit hit))
            //         {
            //             if (hit.distance < closest.distance)
            //             {
            //                 hit.triangleIndex = i;
            //                 closest = hit;
            //             }
            //         }
            //     }
            //     return closest;
            // }


            public BoneWeight GetBoneWeight(Vector3 point)
            {
                PointHit closest = GetClosest(new Bounds(point, Vector3.one * 0.1f));

                if (closest.triangleIndex == -1)
                {
                    Debug.Log("NOT FOUND " + point);
                }

                return closest.triangleIndex > -1 ? weights[indices[closest.triangleIndex]] : new BoneWeight();
            }
        }
    }
}