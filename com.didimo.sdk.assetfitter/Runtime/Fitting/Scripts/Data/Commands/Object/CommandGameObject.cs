using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.MeshTools;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Object/GameObject")]
    [DisplayName("GameObject")]
    [Width(240)]
    [HeaderColor(TypeColors.GameObject)]
    public class CommandGameObject : CommandAvatar
    {
        [Output("Prefab"), Expose] public GameObject prefabOutput;
        [Output("Skin"), Expose] public GameObject templateOutput;
        [Output("Head")] public Mesh headOutput;
        [Output("Left Eye")] public Mesh leyeOutput;
        [Output("Right Eye")] public Mesh reyeOutput;
        [Output("EyeLash")] public Mesh eyeLashOutput;
        [Output("Mouth")] public Mesh mouthOutput;
        [Output("Body")] public Mesh bodyOutput;
        [Output("Garment")] public Mesh garmentOutput;
        // [Output("Hair")] public Mesh hairOutput;
        [Output("Manifold")] public Mesh manifoldOutput;


        public override Gender gender => Gender.Male;
        public override GameObject GetPrefab() => prefabOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (prefabOutput)
            {
                // SkinnedMeshRenderer skin = null;
                // bool result = false;

                switch (info.Name)
                {
                    case nameof(headOutput): if ((values = getBodyPart(HeadMesh)) != null) return true; break;
                    case nameof(leyeOutput): if ((values = getBodyPart(LeftEyeMesh)) != null) return true; break;
                    case nameof(reyeOutput): if ((values = getBodyPart(RightEyeMesh)) != null) return true; break;
                    case nameof(eyeLashOutput): if ((values = getBodyPart(EyeLashes)) != null) return true; break;
                    case nameof(mouthOutput): if ((values = getBodyPart(Mouth)) != null) return true; break;
                    case nameof(bodyOutput): if ((values = getBodyPart(Body)) != null) return true; break;
                    case nameof(garmentOutput): if ((values = getBodyPart(Clothing)) != null) return true; break;
                    // case nameof(hairOutput): if ((values = getBodyPart(Hair)) != null) return true; break;

                    case nameof(manifoldOutput): values = CreateManifold(); return true;
                }
            }
            return base.GetOutputValues(info, out values);
        }

        Mesh getComponentByName(string name)
        {
            var prefabMesh = CloneAsset(prefabOutput.GetComponentsInChildren<SkinnedMeshRenderer>(true)?.FirstOrDefault(s => s.name == name).sharedMesh);
            var templateMesh = templateOutput.GetComponentsInChildren<SkinnedMeshRenderer>(true)?.FirstOrDefault(s => s.name == name).sharedMesh;

            if (prefabMesh.subMeshCount > 1) throw new Exception("Submeshes not support");

            prefabMesh.triangles = templateMesh.triangles;
            return prefabMesh;
        }


        List<object> getBodyPart(string name) => new List<object>() { getComponentByName(name) };

        public override List<object> CreateManifold()
        {
            Mesh headMesh = getComponentByName(HeadMesh), bodyMesh = getComponentByName(Body);

            // remove mouth bag
            headMesh = ExtractSubMeshes(UVGroups.ConvertToSubmeshes(headMesh)).OrderBy(m => m.bounds.Volume()).Last();

            // stitch back of head seam only
            var headBackSeam = Seams.GetEdgeGroups(headMesh).OrderBy(e => Measure.GetBounds(headMesh, e).Volume()).Last();
            headMesh = Seams.MergeEdges(headMesh, new int[][] { headBackSeam }, true);

            // stitch all internal seams
            bodyMesh = Seams.MergeEdges(bodyMesh, true);

            // merge the meshs
            var combined = CombineMeshes(new List<Mesh> { headMesh, bodyMesh });

            var merged = Seams.MergeEdges(combined);
            merged.name = prefabOutput.name + "_manifold";
            return CommandMeshIndexToUV.IndexToUV(new List<Mesh> { merged }).ToList<object>();
        }

        public static string HeadJoint = "Head";
        public static string HeadMesh = "mesh_m_low_baseFace_001";
        public static string EyeLashes = "mesh_m_low_eyeLashes_001";
        public static string Mouth = "mesh_m_low_mouth_001";
        public static string Body = "mesh_m_low_baseBody_001";
        public static string Clothing = "mesh_m_low_baseClothing_001";
        public static string LeftEyeMesh = "mesh_l_low_eye_001";
        public static string RightEyeMesh = "mesh_r_low_eye_001";
        public static string Hat = "BaseballHat_Hair_01";
        public static string Hair = "hair_001";

    }
}
