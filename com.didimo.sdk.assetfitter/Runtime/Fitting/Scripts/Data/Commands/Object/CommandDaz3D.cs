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
    [MenuPath("Object/Daz3D")]
    [DisplayName("Daz3D")]
    [Width(240)]
    [HeaderColor("#977317CC")]
    public class CommandDaz3D : CommandAvatar
    {
        [Output("Prefab"), Expose] public GameObject prefabOutput;
        [Output("Skin")] public SkinnedMeshRenderer shapeOutput;
        [Output(nameof(Surface.Face))] public Mesh faceOutput;
        [Output(nameof(Surface.Lips))] public Mesh lipsOutput;
        [Output(nameof(Surface.Teeth))] public Mesh teethOutput;
        [Output(nameof(Surface.Torso))] public Mesh torsoOutput;
        [Output(nameof(Surface.Ears))] public Mesh earsOutput;
        [Output(nameof(Surface.Legs))] public Mesh legsOutput;
        [Output(nameof(Surface.EyeSocket))] public Mesh eyeSocketOutput;
        [Output(nameof(Surface.Mouth))] public Mesh mouthOutput;
        [Output(nameof(Surface.Arms))] public Mesh armsOutput;
        [Output(nameof(Surface.Pupils))] public Mesh pupilsOutput;
        [Output(nameof(Surface.EyeMoisture))] public Mesh eyeMoistureOutput;
        [Output(nameof(Surface.Fingernails))] public Mesh fingenailsOutput;
        [Output(nameof(Surface.Cornea))] public Mesh corneaOutput;
        [Output(nameof(Surface.Irises))] public Mesh irisesOutput;
        [Output(nameof(Surface.Sclera))] public Mesh scleraOutput;
        [Output(nameof(Surface.Toenails))] public Mesh toenailsOutput;
        [Output("Accessory")] public SkinnedMeshRenderer accessoryOutput;
        [Output("Manifold")] public Mesh manifoldOutput;

        public override Gender gender => !GetShapeSkin() ? Gender.None :
            (GetShapeSkin().sharedMesh.name.Contains("8Female") ? Gender.Female : Gender.Male);

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
                        Debug.Log(string.Join(",", values.Select(v => (v as SkinnedMeshRenderer).name)));

                        Debug.Log("Accessory count " + values.Count);
                        return true;

                    case nameof(manifoldOutput):
                        values = CreateManifold();
                        return true;

                    default:
                        var surface = (Surface)Enum.Parse(typeof(Surface), info.GetCustomAttribute<OutputAttribute>().name);
                        values = new List<object>() { GetSubMesh(GetShapeSkin().sharedMesh, surface) };
                        return true;
                }
            }
            return base.GetOutputValues(info, out values);
        }

        public override List<object> CreateManifold()
        {
            var mesh = GetShapeSkin().sharedMesh;
            var combined = CombineMeshesIntoSubMeshes(new[] {
                GetSubMesh(mesh,Surface.Face),
                GetSubMesh(mesh,Surface.Lips),
                GetSubMesh(mesh,Surface.Ears),
                GetSubMesh(mesh,Surface.Torso),
                GetSubMesh(mesh,Surface.Legs),
                GetSubMesh(mesh,Surface.Arms),
            });
            return CommandMeshIndexToUV.IndexToUV(
                Seams.MergeEdges(new List<Mesh> { combined })).ToList<object>();
        }

        Mesh GetSubMesh(Mesh mesh, Surface surface)
        {
            var order = gender == Gender.Female ? FemaleSurface : MaleSurface;
            int subMeshIndex = order.ToList().IndexOf(surface);
            var submesh = ExtractSubMesh(mesh, subMeshIndex);
            submesh.name = prefabOutput.name + "_" + Enum.GetName(typeof(Surface), surface);
            return submesh;
        }

        SkinnedMeshRenderer GetShapeSkin() =>
             prefabOutput.GetComponentsInChildren<SkinnedMeshRenderer>(true).
                FirstOrDefault(s => s.transform.parent.name == s.name.Split(".")[0]);

        SkinnedMeshRenderer[] GetAccessorySkins()
        {
            var skins = prefabOutput.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            //Debug.Log(string.Join(",", skins.Select(v => (v as SkinnedMeshRenderer).name)));
            var name = GetShapeSkin().transform.parent.name;
            return skins.Where(s => !s.name.StartsWith(name)).ToArray();
        }


        enum Surface
        {
            Face = 0,
            Lips = 1,
            Teeth = 2,
            Torso = 3,
            Ears = 4,
            Legs = 5,
            EyeSocket = 6,
            Mouth = 7,
            Arms = 8,
            Pupils = 9,
            EyeMoisture = 10,
            Fingernails = 11,
            Cornea = 12,
            Irises = 13,
            Sclera = 14,
            Toenails = 15,
        }

        Surface[] MaleSurface =
        {
            Surface.Face,
            Surface.Lips,
            Surface.Teeth,
            Surface.Torso,
            Surface.Ears,
            Surface.Legs,
            Surface.EyeSocket,
            Surface.Mouth,
            Surface.Arms,
            Surface.Pupils,
            Surface.EyeMoisture,
            Surface.Fingernails,
            Surface.Cornea,
            Surface.Irises,
            Surface.Sclera,
            Surface.Toenails,
        };

        Surface[] FemaleSurface =
        {
            Surface.Torso,
            Surface.Face,
            Surface.Lips,
            Surface.Teeth,
            Surface.Ears,
            Surface.Legs,
            Surface.EyeSocket,
            Surface.Mouth,
            Surface.Arms,
            Surface.Pupils,
            Surface.EyeMoisture,
            Surface.Fingernails,
            Surface.Cornea,
            Surface.Irises,
            Surface.Sclera,
            Surface.Toenails,
        };
    }
}
