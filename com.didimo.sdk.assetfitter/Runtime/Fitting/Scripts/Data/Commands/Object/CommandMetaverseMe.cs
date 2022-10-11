using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.MeshTools;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using Didimo.Extensions;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Object/MetaverseMe")]
    [DisplayName("MetaverseMe")]
    [Width(240)]
    [HeaderColor("#685cd4")]
    public class CommandMetaverseMe : CommandAvatar
    {
        [Output("Prefab"), Expose] public GameObject prefabOutput;
        [Output("Skin")] public SkinnedMeshRenderer shapeOutput;
        [Output("Accessory Skins")] public SkinnedMeshRenderer accessoryOutput;
        [Output("Accessory Filters")] public MeshFilter accessoryFilterOutput;
        [Output("Manifold")] public Mesh manifoldOutput;

        public override Gender gender => !GetGender() ? Gender.None :
            (GetGender() ? Gender.Male : Gender.Female);

        public override GameObject GetPrefab() => prefabOutput;

        GameObject builtPrefab;
        internal override void Build()
        {
            builtPrefab = CloneAsset(this.prefabOutput);
            //builtPrefab.GetComponentsInChildren<Renderer>().ForEach(s => s.transform.rotation *= Quaternion.Euler(-89.98f, 0, 0));
            GraphData.State.Add(builtPrefab);
        }

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (builtPrefab)
            {
                switch (info.Name)
                {
                    case nameof(shapeOutput): // for the bones and in this case is the head only
                        values = new List<object>() { GetHeadSkin() };
                        return true;

                    case nameof(accessoryOutput):
                        values = GetAccessorySkins().ToList<object>();
                        Debug.Log("Accessory count " + values.Count);
                        return true;

                    case nameof(accessoryFilterOutput):
                        values = GetAccessoryFilters().ToList<object>();

                        Debug.Log("Accessory Filter count " + values.Count);
                        return true;

                    case nameof(manifoldOutput):
                        values = new List<object> { CreateManifold() };
                        return true;

                    default:
                        throw new Exception("Unhandled output");
                }
            }
            return base.GetOutputValues(info, out values);
        }

        public override Mesh CreateManifold()
        {
            var headMesh = GetHeadSkin().BakeMeshPreserveBindings();
            var bodyMesh = GetBodySkin().BakeMeshPreserveBindings();

            Mesh combined = CombineMeshes(new List<Mesh>{bodyMesh,
               GetSubMeshFromHead(headMesh,HeadSurface.Head)
            });

            //Matrix4x4 matrix = Matrix4x4.Rotate(Quaternion.Euler(-90, 0, 0));
            //return CommandMeshTransform.TransformVertices(CommandMeshIndexToUV.IndexToUV(Seams.MergeEdges(new List<Mesh> { combined })), matrix)[0];
            return combined;// CommandMeshIndexToUV.IndexToUV(Seams.MergeEdges(new List<Mesh> { combined }))[0];
        }

        // is only used for the head
        static Mesh GetSubMeshFromHead(Mesh mesh, HeadSurface surface)
        {
            Mesh subMesh = ExtractSubMesh(mesh, (int)surface);
            subMesh.name = mesh.name + "_" + Enum.GetName(typeof(HeadSurface), surface);
            return subMesh;
        }

        SkinnedMeshRenderer GetSkin(string id) => builtPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(s => s.name.Contains(id));
        SkinnedMeshRenderer GetHeadSkin() => GetSkin("Head");
        SkinnedMeshRenderer GetBodySkin() => GetSkin("Body");


        bool GetGender() =>
            builtPrefab.name.Contains("Male");

        SkinnedMeshRenderer[] GetAccessorySkins()
        {
            var skins = builtPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).
                Where(s => !s.name.StartsWith("Wolf3D")).ToArray();
            return skins;
        }

        MeshFilter[] GetAccessoryFilters()
        {
            return builtPrefab.GetComponentsInChildren<MeshFilter>(true);
        }

        enum HeadSurface
        {
            Head = 0,
            Eye = 1,
            Teeth = 2,
        }
    }

}

