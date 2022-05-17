using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static Didimo.Core.Config.ShaderResources;

namespace  Didimo.Core.Utility
{

    public static class MeshUtils
    {

        public static class DidimoMeshPartNames
        {
            public static string[] LeftEyelidJoints =
            {
                "DemLuUpperEyelidLevelTwo01", "DemLuUpperEyelidLevelTwo02", "DemLuUpperEyelidLevelTwo03", "DemLuUpperEyelidLevelTwo04", "DemLuUpperEyelidLevelTwo05",
                "DemLuUpperEyelidLevelTwo06", "DemLuUpperEyelidLevelTwo07", "DemLuUpperEyelidLevelTwo08", "DemLdLowerEyelidLevelTwo08", "DemLdLowerEyelidLevelTwo07",
                "DemLdLowerEyelidLevelTwo06", "DemLdLowerEyelidLevelTwo05", "DemLdLowerEyelidLevelTwo04", "DemLdLowerEyelidLevelTwo02", "DemLdLowerEyelidLevelTwo01",
            };

            public static string[] RightEyelidJoints =
            {
                "DemRuUpperEyelidLevelTwo01", "DemRuUpperEyelidLevelTwo02", "DemRuUpperEyelidLevelTwo03", "DemRuUpperEyelidLevelTwo04", "DemRuUpperEyelidLevelTwo05",
                "DemRuUpperEyelidLevelTwo06", "DemRuUpperEyelidLevelTwo07", "DemRuUpperEyelidLevelTwo08", "DemRdLowerEyelidLevelTwo08", "DemRdLowerEyelidLevelTwo07",
                "DemRdLowerEyelidLevelTwo06", "DemRdLowerEyelidLevelTwo05", "DemRdLowerEyelidLevelTwo04", "DemRdLowerEyelidLevelTwo02", "DemRdLowerEyelidLevelTwo01"
            };

            
            public static string HeadJoint      = "Head";
            public static string HeadMesh       = "mesh_m_low_baseFace_001";
            public static string EyeLashes      = "mesh_m_low_eyeLashes_001";
            public static string Mouth          = "mesh_m_low_mouth_001";
            public static string Body           = "mesh_m_low_baseBody_001";
            public static string Clothing       = "mesh_m_low_baseClothing_001";
            public static string LeftEyeMesh    = "mesh_l_low_eye_001";
            public static string RightEyeMesh   = "mesh_r_low_eye_001";
            public static string Hat            = "BaseballHat_Hair_01";
            public static string Hair           = "hair_001";

            //these align with 'EBodyPartID'. There isn't a one to one matching - eyes are specified individually as meshes but treated as a class by materials
            public static string[] bodyPartDefaultNames = {LeftEyeMesh, HeadMesh, Body, Mouth, Hair, EyeLashes, Clothing, Hat, "unknown"};          
        }
        public static Renderer GetMeshRendererFromBodyPart(GameObject root, EBodyPartID id)
        {
            var nameid = DidimoMeshPartNames.bodyPartDefaultNames[(int)id];
            var ob = ComponentUtility.GetChildWithName(root, nameid, true);
            return ob != null ? ob.GetComponent<Renderer>() : null;
        }

        public static Mesh DeformMesh(Mesh A, float[] deformationData)
        {
            return null;
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

        static void AddOrIncrement<A>(Dictionary<A,int> uniques, A value)
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