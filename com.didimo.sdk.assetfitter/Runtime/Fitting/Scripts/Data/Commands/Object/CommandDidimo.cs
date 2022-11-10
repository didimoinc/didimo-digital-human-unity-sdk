using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.MeshTools;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using Didimo.Core.Utility;
using static Didimo.Core.Utility.DidimoParts;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Object/Didimo")]
    [DisplayName("Didimo")]
    [Width(240)]
    [HeaderColor("#50009FCC")]
    public class CommandDidimo : CommandAvatar
    {
        [Output("Prefab"), Expose] public GameObject prefabOutput;
        [Output("Template"), Expose] public GameObject templateOutput;
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
        public override GameObject avatarPrefab { get => prefabOutput; set => prefabOutput = value; }

        DidimoParts didimoParts => prefabOutput.GetComponent<DidimoParts>();

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (prefabOutput)
            {
                // SkinnedMeshRenderer skin = null;
                // bool result = false;

                switch (info.Name)
                {
                    case nameof(headOutput): if ((values = GetBodyPart(BodyPart.HeadMesh)) != null) return true; break;
                    case nameof(leyeOutput): if ((values = GetBodyPart(BodyPart.LeftEyeMesh)) != null) return true; break;
                    case nameof(reyeOutput): if ((values = GetBodyPart(BodyPart.RightEyeMesh)) != null) return true; break;
                    case nameof(eyeLashOutput): if ((values = GetBodyPart(BodyPart.EyeLashesMesh)) != null) return true; break;
                    case nameof(mouthOutput): if ((values = GetBodyPart(BodyPart.MouthMesh)) != null) return true; break;
                    case nameof(bodyOutput): if ((values = GetBodyPart(BodyPart.BodyMesh)) != null) return true; break;
                    case nameof(garmentOutput): if ((values = GetBodyPart(BodyPart.ClothingMesh)) != null) return true; break;
                    // case nameof(hairOutput): if ((values = GetBodyPart(Hair)) != null) return true; break;

                    case nameof(manifoldOutput): values = new List<object> { CreateManifold() }; return true;
                }
            }
            return base.GetOutputValues(info, out values);
        }

        // Mesh GetComponentByName(string name)
        // {
        //     Mesh prefabMesh = CloneAsset(prefabOutput.GetComponentsInChildren<SkinnedMeshRenderer>(true)?.FirstOrDefault(s => s.name == name).sharedMesh);
        //     Mesh templateMesh = templateOutput.GetComponentsInChildren<SkinnedMeshRenderer>(true)?.FirstOrDefault(s => s.name == name).sharedMesh;

        //     if (prefabMesh.subMeshCount > 1) throw new Exception("Submeshes not support");

        //     prefabMesh.triangles = templateMesh.triangles;
        //     return prefabMesh;
        // }

        Mesh GetComponentByPart(BodyPart part)
        {
            Mesh prefabMesh = CloneAsset(prefabOutput.GetComponent<DidimoParts>().BodyPartToRenderer(part).GetMesh());
            Mesh templateMesh = templateOutput.GetComponent<DidimoParts>().BodyPartToRenderer(part).GetMesh();

            if (prefabMesh.subMeshCount > 1) throw new Exception("Submeshes not support");

            prefabMesh.triangles = templateMesh.triangles;
            return prefabMesh;
        }


        List<object> GetBodyPart(BodyPart part) => new List<object>() { GetComponentByPart(part) };

        public override Mesh CreateManifold()
        {
            //GetComponentByName(HeadMesh), bodyMesh = GetComponentByName(Body);
            Mesh headMesh = didimoParts.HeadMeshRenderer.sharedMesh;
            Mesh bodyMesh = didimoParts.BodyMeshRenderer.sharedMesh;

            // remove mouth bag
            headMesh = ExtractSubMeshes(UVGroups.ConvertToSubmeshes(headMesh)).OrderBy(m => m.bounds.Volume()).Last();

            // stitch back of head seam only
            int[] headBackSeam = Seams.GetEdgeGroups(headMesh).OrderBy(e => Measure.GetBounds(headMesh, e).Volume()).Last();
            headMesh = Seams.MergeEdges(headMesh, new int[][] { headBackSeam }, true);

            // stitch all internal seams
            bodyMesh = Seams.MergeEdges(bodyMesh, true);

            // merge the meshs
            Mesh combined = CombineMeshes(new List<Mesh> { headMesh, bodyMesh });

            Mesh merged = Seams.MergeEdges(combined);
            merged.name = prefabOutput.name + "_manifold";
            return CommandMeshIndexToUV.IndexToUV(new List<Mesh> { merged })[0];
        }
    }
}
