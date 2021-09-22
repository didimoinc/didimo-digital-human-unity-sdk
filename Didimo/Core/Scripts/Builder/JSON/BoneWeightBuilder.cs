using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Didimo.Builder.JSON
{
    public class BoneWeightBuilder
    {
        public bool TryBuild(DidimoBuildContext context, DidimoModelDataObject dataObject)
        {
            foreach (DidimoModelDataObject.Mesh meshData in dataObject.Meshes)
            {
                if (!meshData.HasSkinning) continue;

                if (!meshData.HasValidWeights)
                {
                    Debug.LogWarning("Skin indices and skinWeights counts don't match!");
                    continue;
                }

                byte[] bonesPerVertex = new byte[meshData.skin_indices.Count];
                List<BoneWeight1> boneWeights = new List<BoneWeight1>();

                for (int vertId = 0; vertId < meshData.skin_indices.Count; vertId++)
                {
                    if (meshData.skin_weights[vertId].Count >= 255)
                    {
                        Debug.LogError("ERROR: exceeded maximum number of weights: " + meshData.skin_weights[vertId].Count);
                    }

                    bonesPerVertex[vertId] = (byte) meshData.skin_weights[vertId].Count;

                    for (int weightId = 0; weightId < meshData.skin_weights[vertId].Count; weightId++)
                    {
                        BoneWeight1 boneWeight = new BoneWeight1();
                        boneWeight.boneIndex = meshData.skin_indices[vertId][weightId];
                        boneWeight.weight = meshData.skin_weights[vertId][weightId];

                        boneWeights.Add(boneWeight);
                    }
                }

                NativeArray<byte> bonesPerVertexArray = new NativeArray<byte>(bonesPerVertex, Allocator.Temp);
                NativeArray<BoneWeight1> weightsArray = new NativeArray<BoneWeight1>(boneWeights.ToArray(), Allocator.Temp);

                if (!context.MeshHierarchyRoot.TryFindRecursive(meshData.Name, out SkinnedMeshRenderer smr))
                {
                    Debug.LogWarning($"Failed to locate hierarchy in hierarchy: {meshData.Name}");
                    continue;
                }

                // set boneWeights to null, otherwise we might get an error when calling SetBoneWeights
                smr.sharedMesh.boneWeights = null;
                smr.sharedMesh.SetBoneWeights(bonesPerVertexArray, weightsArray);

                Transform[] bones = new Transform[dataObject.Bones.Count];

                for (int i = 0; i < bones.Length; i++)
                {
                    if (context.MeshHierarchyRoot.TryFindRecursive(dataObject.Bones[i], out Transform target))
                    {
                        bones[i] = target;
                    }
                    else
                    {
                        bones[i] = null;
                    }
                }

                smr.bones = bones;

                smr.localBounds = smr.sharedMesh.bounds;

                Matrix4x4[] bindPoses = new Matrix4x4[bones.Length];
                for (int i = 0; i < bones.Length; i++)
                {
                    bindPoses[i] = bones[i].worldToLocalMatrix * smr.transform.localToWorldMatrix;
                }

                smr.sharedMesh.bindposes = bindPoses;
            }

            return true;
        }
    }
}