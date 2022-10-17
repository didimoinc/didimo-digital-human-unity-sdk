using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using static Didimo.Core.Config.ShaderResources;

namespace  Didimo.Core.Utility
{

    public static class MeshUtils
    {

        public static void CopyMesh(Mesh source, Mesh target)
        {
            target.Clear();
            target.vertices = source.vertices;
            target.normals = source.normals;
            target.uv = source.uv;
            target.uv2 = source.uv2;
            target.uv3 = source.uv3;
            target.uv4 = source.uv4;
            target.uv5 = source.uv5;
            target.uv6 = source.uv6;
            target.uv7 = source.uv7;
            target.uv8 = source.uv8;
            target.tangents = source.tangents;
            target.colors = source.colors;
            target.boneWeights = source.boneWeights;
            target.bindposes = source.bindposes;
            target.triangles = source.triangles;
            
            List<SubMeshDescriptor> subMeshDescriptors = new List<SubMeshDescriptor>(source.subMeshCount);
            for (int submeshIndex = 0; submeshIndex < source.subMeshCount; submeshIndex++)
            {
                subMeshDescriptors.Add(source.GetSubMesh(submeshIndex));
            }
            target.SetSubMeshes(subMeshDescriptors);
        }
        
        /// <summary>
        /// Create a new mesh from an existing mesh with updated vertex positions,
        /// while keeping other properties intact. Also copies sub-meshes.
        /// If new vertices is null, they are taken from the source mesh instead, generating a copy.
        /// </summary>
        /// <param name="source">Source mesh to take the data from</param>
        /// <param name="newVertices">New vertex positions</param>
        /// <returns>Copy of the source mesh with the updated vertices</returns>
        public static Mesh ApplyMeshVertexDeformation(Mesh source, Vector3[] newVertices = null)
        {
            Mesh result = new();
            result.vertices = newVertices ?? source.vertices;
            result.normals = source.normals;
            result.uv = source.uv;
            result.uv2 = source.uv2;
            result.uv3 = source.uv3;
            result.uv4 = source.uv4;
            result.uv5 = source.uv5;
            result.uv6 = source.uv6;
            result.uv7 = source.uv7;
            result.uv8 = source.uv8;
            result.tangents = source.tangents;
            result.colors = source.colors;
            result.triangles = source.triangles;
            result.boneWeights = source.boneWeights;
            result.bindposes = source.bindposes;
            
            List<SubMeshDescriptor> subMeshDescriptors = new List<SubMeshDescriptor>(source.subMeshCount);
            for (int submeshIndex = 0; submeshIndex < source.subMeshCount; submeshIndex++)
            {
                subMeshDescriptors.Add(source.GetSubMesh(submeshIndex));
            }
            result.SetSubMeshes(subMeshDescriptors);


            return result;
        }

        public static Mesh TopologyFromAVerticesFromB(Mesh A, Mesh B)
        {
            Mesh result = new Mesh();
            result.vertices = B.vertices;
            result.normals = B.normals;
            result.uv = B.uv;
            result.uv2 = B.uv2;
            result.uv3 = B.uv3;
            result.uv4 = B.uv4;
            result.uv5 = B.uv5;
            result.uv6 = B.uv6;
            result.uv7 = B.uv7;
            result.uv8 = B.uv8;
            result.tangents = B.tangents;
            result.triangles = A.triangles;
            return result;
        }
        public static void SetMesh(GameObject ob, Mesh m)
        {
            MeshFilter mf = ob.GetComponentInChildren<MeshFilter>();
            if (mf)
            {
                mf.sharedMesh = m;
                mf.mesh = m;
                return;
            }
            SkinnedMeshRenderer mr = ob.GetComponentInChildren<SkinnedMeshRenderer>();
            if (mr)
                mr.sharedMesh = m;            
        }
        public static Mesh GetMesh(GameObject ob)
        {
            MeshFilter mf = ob.GetComponentInChildren<MeshFilter>();
            if (mf)
                return mf.sharedMesh;
            SkinnedMeshRenderer mr = ob.GetComponentInChildren<SkinnedMeshRenderer>();
            if (mr) 
                return mr.sharedMesh;
            return null;
        }

        public static void AddOrIncrement<T>(Dictionary<T,int> uniques, T value)
        {
            if (uniques.ContainsKey(value))
                uniques[value] += 1;
            else
                uniques.Add(value, 1);
        }
        public static void GetUniqueMeshes(GameObject ob, Dictionary<Mesh, int> uniqueMeshes)
        {            
            var mflist = ob.GetComponentsInChildren<MeshFilter>();
            foreach (var mf in mflist)
            {
                if (mf && mf.sharedMesh)
                    AddOrIncrement(uniqueMeshes, mf.sharedMesh);
            }
            var mrlist = ob.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var mr in mrlist)
            {
                if (mr && mr.sharedMesh)
                    AddOrIncrement(uniqueMeshes, mr.sharedMesh);
            }
        }
        public static Mesh GetMeshFromRenderer(Renderer render)
        {
            SkinnedMeshRenderer smr = render.GetComponent<SkinnedMeshRenderer>();
            if (smr)
                return smr.sharedMesh;
            MeshFilter mf = render.GetComponent<MeshFilter>();
            if (mf)
                return mf.sharedMesh;
            return null;
        }
        public static Mesh[] GetUniqueMeshes(GameObject ob)
        {
            Dictionary<Mesh, int> uniqueMeshes = new Dictionary<Mesh, int>();
            GetUniqueMeshes(ob, uniqueMeshes);           
            return uniqueMeshes.Select(e => e.Key).ToArray();
        }
        public static Mesh [] GetUniqueMeshes(GameObject [] obs)
        {
            Dictionary<Mesh, int> uniqueMeshes = new Dictionary<Mesh, int>();
            foreach (var ob in obs)
            {
                GetUniqueMeshes(ob,uniqueMeshes);
            }
            return uniqueMeshes.Select(e => e.Key).ToArray();
        }

        public static void GetUniqueMeshAssetPaths(GameObject ob, Dictionary<string, int> uniqueMeshPaths)
        {
            var mflist = ob.GetComponentsInChildren<MeshFilter>();
#if UNITY_EDITOR
            foreach (var mf in mflist)
            {
                if (mf && mf.sharedMesh)
                    AddOrIncrement(uniqueMeshPaths, AssetDatabase.GetAssetPath(mf.sharedMesh));
            }
            var mrlist = ob.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var mr in mrlist)
            {
                if (mr && mr.sharedMesh)
                    AddOrIncrement(uniqueMeshPaths, AssetDatabase.GetAssetPath(mr.sharedMesh));
            }
#endif
        }

        public static string[] GetUniqueMeshAssetPaths(GameObject ob)
        {
            Dictionary<string, int> uniqueMeshPaths = new Dictionary<string, int>();
            GetUniqueMeshAssetPaths(ob, uniqueMeshPaths);
            return uniqueMeshPaths.Select(e => e.Key).ToArray();
        }
        public static string[] GetUniqueMeshAssetPaths(GameObject[] obs)
        {
            Dictionary<string, int> uniqueMeshPaths = new Dictionary<string, int>();
            foreach (var ob in obs)
            {
                GetUniqueMeshAssetPaths(ob, uniqueMeshPaths);
            }
            return uniqueMeshPaths.Select(e => e.Key).ToArray();
        }
    }
}