using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Mesh/Mesh Index To UV")]
    [DisplayName("Index To UV")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshIndexToUV : GraphNode
    {
        [Input("Mesh")] public Mesh meshInput;
        [Output("Mesh")] public Mesh meshOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                values = IndexToUV(GetInputValues<Mesh>(nameof(meshInput))).ToList<object>();
                return true;
            }
            values = null;
            return false;
        }

        public static List<Mesh> IndexToUV(List<Mesh> meshes)
        {
            Mesh _UVIndex(Mesh mesh)
            {
                mesh = CloneAsset(mesh);
                IEnumerable<Vector2> GetUV()
                {
                    for (int i = 0, n = mesh.vertexCount; i < n; i++)
                        yield return new Vector2((float)i / n, 0);
                }
                mesh.uv = GetUV().ToArray();
                return mesh;
            }
            return meshes.Select(_UVIndex).ToList();
        }

    }
}