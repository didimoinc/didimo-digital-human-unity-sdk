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
        [Output("Accessory Skins")] public SkinnedMeshRenderer accessoryOutput;
        [Output("Accessory Filters")] public MeshFilter accessoryFilterOutput;
        [Output("Manifold")] public Mesh manifoldOutput;
        [Expose] public EyelashFilter eyelashFilter;

        public override Gender gender => !GetCharacterShapeSkin() ? Gender.None :
            (GetCharacterShapeSkin().sharedMesh.name.Contains("8Female") ? Gender.Female : Gender.Male);

        // public override GameObject GetPrefab() => prefabOutput;
        public override GameObject avatarPrefab { get => prefabOutput; set => prefabOutput = value; }

        GameObject builtPrefab;

        internal override void Build()
        {
            builtPrefab = CloneAsset(this.prefabOutput);
            GraphData.State.Add(builtPrefab);
        }

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            GameObject prefabOutput = builtPrefab;

            if (prefabOutput)
            {
                switch (info.Name)
                {
                    case nameof(shapeOutput):
                        values = new List<object>() { GetCharacterShapeSkin() };
                        return true;

                    case nameof(accessoryOutput):
                        values = GetAccessorySkins().ToList<object>();

                        Debug.Log("Accessory Skin count " + values.Count);
                        return true;

                    case nameof(accessoryFilterOutput):
                        values = GetAccessoryFilters().ToList<object>();

                        Debug.Log("Accessory Filter count " + values.Count);
                        return true;

                    case nameof(manifoldOutput):
                        values = new List<object> { CreateManifold() };
                        return true;

                    default:
                        Surface surface = (Surface)Enum.Parse(typeof(Surface), info.GetCustomAttribute<OutputAttribute>().name);
                        values = new List<object>() { GetSubMesh(GetCharacterShapeSkin().sharedMesh, surface) };
                        return true;
                }
            }
            return base.GetOutputValues(info, out values);
        }

        public override Mesh CreateManifold()
        {
            Mesh mesh = GetCharacterShapeSkin().sharedMesh;
            Mesh combined = CombineMeshesIntoSubMeshes(new[] {
                GetSubMesh(mesh,Surface.Face),
                GetSubMesh(mesh,Surface.Lips),
                GetSubMesh(mesh,Surface.Ears),
                GetSubMesh(mesh,Surface.Torso),
                GetSubMesh(mesh,Surface.Legs),
                GetSubMesh(mesh,Surface.Arms),
            });
            return CommandMeshIndexToUV.IndexToUV(Seams.MergeEdges(new List<Mesh> { combined }))[0];
        }

        Mesh GetSubMesh(Mesh mesh, Surface surface)
        {
            Surface[] order = gender == Gender.Female ? FemaleSurface : MaleSurface;
            int subMeshIndex = order.ToList().IndexOf(surface);
            Mesh submesh = ExtractSubMesh(mesh, subMeshIndex);
            submesh.name = prefabOutput.name + "_" + Enum.GetName(typeof(Surface), surface);
            return submesh;
        }

        SkinnedMeshRenderer GetCharacterShapeSkin() =>
             prefabOutput.GetComponentsInChildren<SkinnedMeshRenderer>(true).
                FirstOrDefault(s => s.name == CharacterNameId + ".Shape");

        SkinnedMeshRenderer[] GetAccessorySkins()
        {
            bool Filter(SkinnedMeshRenderer skin)
            {
                if (skin.name.StartsWith(CharacterNameId))
                {
                    return skin.name.Contains("Eyelashes") && eyelashFilter == EyelashFilter.IncludeEyelashes;
                }
                return true;
            }

            return prefabOutput.GetComponentsInChildren<SkinnedMeshRenderer>(true).Where(Filter).ToArray();
        }

        MeshFilter[] GetAccessoryFilters()
        {
            return prefabOutput.GetComponentsInChildren<MeshFilter>(true);
        }

        string CharacterNameId => prefabOutput.transform.GetChild(0).name;

        public enum EyelashFilter
        {
            ExcludeEyelashes = 0,
            IncludeEyelashes = 1,
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
