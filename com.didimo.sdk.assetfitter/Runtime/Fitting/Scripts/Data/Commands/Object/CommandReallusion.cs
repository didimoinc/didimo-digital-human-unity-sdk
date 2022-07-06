using System;
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
    [MenuPath("Object/Reallusion")]
    [DisplayName("Reallusion")]
    [Width(240)]
    [HeaderColor("#2883bcCC")]
    public class CommandReallusion : CommandAvatar
    {
        [Output("Prefab"), Expose] public GameObject prefabOutput;
        [Output("Skin")] public SkinnedMeshRenderer shapeOutput;
        [Output(nameof(BodySurface.Head))] public Mesh headOutput;
        [Output(nameof(BodySurface.Body))] public Mesh bodyOutput;
        [Output(nameof(BodySurface.Arm))] public Mesh armOutput;
        [Output(nameof(BodySurface.Leg))] public Mesh legOutput;
        [Output(nameof(BodySurface.Nails))] public Mesh nailsOutput;
        [Output(nameof(BodySurface.Eyelash))] public Mesh eyelashOutput;

        // [Output(nameof(EyeSurface.REye))] public Mesh rightEyeOutput;
        // [Output(nameof(EyeSurface.RCornea))] public Mesh rightCorneaOutput;
        // [Output(nameof(EyeSurface.LEye))] public Mesh leftEyeOutput;
        // [Output(nameof(EyeSurface.LCornea))] public Mesh leftCorneaOutput;

        // [Output(nameof(EyeOcclusionSurface.ROcclusion))] public Mesh rightOcclusionOutput;
        // [Output(nameof(EyeOcclusionSurface.LOcclusion))] public Mesh leftOcclusionOutput;

        // [Output(nameof(TearlineSurface.RTearline))] public Mesh rightTearlineOutput;
        // [Output(nameof(TearlineSurface.LTearline))] public Mesh leftTearlineOutput;

        // [Output(nameof(TeethSurface.UpperTeeth))] public Mesh upperTeethOutput;
        // [Output(nameof(TeethSurface.LowerTeeth))] public Mesh lowerTeethOutput;

        // [Output("Tongue")] public SkinnedMeshRenderer tongueOutput;

        [Output("Accessory")] public SkinnedMeshRenderer accessoryOutput;
        [Output("Manifold")] public Mesh manifoldOutput;

        public override Gender gender => !GetGender() ? Gender.None :
            (GetGender() ? Gender.Female : Gender.Male);

        public override GameObject GetPrefab() => prefabOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (prefabOutput)
            {
                switch (info.Name)
                {
                    case nameof(shapeOutput):
                        values = new List<object>() { GetShapeSkin() };
                        return true;

                    case nameof(accessoryOutput):
                        values = GetAccessorySkins().ToList<object>();
                        Debug.Log("Accessory count " + values.Count);
                        return true;

                    case nameof(manifoldOutput):
                        values = CreateManifold();
                        return true;

                    default:
                        var surface = (BodySurface)Enum.Parse(typeof(BodySurface), info.GetCustomAttribute<OutputAttribute>().name);
                        values = new List<object>() { GetSubMesh(GetShapeSkin().sharedMesh, surface) };
                        return true;
                }
            }
            return base.GetOutputValues(info, out values);
        }

        public override List<object> CreateManifold()
        {
            Mesh mesh = GetShapeSkin().sharedMesh;
            Mesh combined = CombineMeshesIntoSubMeshes(new[] {
                GetSubMesh(mesh, BodySurface.Head),
                GetSubMesh(mesh, BodySurface.Body),
                GetSubMesh(mesh, BodySurface.Arm),
                GetSubMesh(mesh, BodySurface.Leg),
            });
            return CommandMeshIndexToUV.IndexToUV(
                    Seams.MergeEdges(new List<Mesh> { combined })).ToList<object>();

        }

        Mesh GetSubMesh(Mesh mesh, BodySurface surface)
        {
            BodySurface[] order = ReallusionBodySurface;
            int subMeshIndex = order.ToList().IndexOf(surface);
            Mesh subMesh = ExtractSubMesh(mesh, subMeshIndex);

            /*  Quaternion rotation = Quaternion.Euler(90, 0, 0);
              subMesh.vertices = subMesh.vertices.Select(v => rotation * v).ToArray();
              subMesh.normals = subMesh.normals.Select(v => rotation * v).ToArray();
              subMesh.RecalculateBounds();
              subMesh.RecalculateTangents(); */

            subMesh.name = prefabOutput.name + "_" + Enum.GetName(typeof(BodySurface), surface);
            return subMesh;
        }

        SkinnedMeshRenderer GetShapeSkin() =>
            prefabOutput.GetComponentsInChildren<SkinnedMeshRenderer>(true).
                FirstOrDefault(s => s.name.Contains("CC_Base_Body"));

        SkinnedMeshRenderer GetGender() =>
            prefabOutput.GetComponentsInChildren<SkinnedMeshRenderer>(true).
                FirstOrDefault(s => s.name.Contains("Female"));

        SkinnedMeshRenderer[] GetAccessorySkins() =>
            prefabOutput.GetComponentsInChildren<SkinnedMeshRenderer>(true).
                Where(s => !s.name.StartsWith("CC_Base_") &&
                           !s.name.Contains("Camera") &&
                           !s.name.Contains("Shadow")).ToArray();


        enum BodySurface
        {
            Head = 0,
            Body = 1,
            Arm = 2,
            Leg = 3,
            Nails = 4,
            Eyelash = 5,
        }

        /*        enum EyeSurface
                {
                    REye = 0,
                    RCornea = 1,
                    LEye = 2,
                    LCornea = 3,
                }

                enum EyeOcclusionSurface
                {
                    ROcclusion = 0,
                    LOcclusion = 1,
                }

                enum TearlineSurface
                {
                    RTearline = 0,
                    LTearline = 1,
                }

                enum TeethSurface
                {
                    UpperTeeth = 0,
                    LowerTeeth = 1,
                } */

        BodySurface[] ReallusionBodySurface =
        {
            BodySurface.Head,
            BodySurface.Body,
            BodySurface.Arm,
            BodySurface.Leg,
            BodySurface.Nails,
            BodySurface.Eyelash,
        };


    }

}

