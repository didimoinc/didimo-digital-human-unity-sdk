using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using static Didimo.AssetFitter.Editor.Graph.GeomTools;

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Mesh/Vertex Order")]
    [DisplayName("Vertex Order")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshVertexOrder : GraphNode
    {
        [Input("Mesh")] public Mesh meshInput;
        [Input("Mesh1", true)] public Mesh mesh1Input;
        [Input("Mesh2", true)] public Mesh mesh2Input;
        [Output("Mesh")] public Mesh meshOutput;
        [Expose] public float threshold = 0.000001f;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                values = ReorderVertices(
                    GetInputValues<Mesh>(nameof(meshInput)),
                    GetInputValues<Mesh>(nameof(mesh1Input)),
                    GetInputValues<Mesh>(nameof(mesh2Input))).ToList<object>();
                return true;
            }
            values = null;
            return false;
        }

        List<Mesh> ReorderVertices(List<Mesh> meshes, List<Mesh> meshes1, List<Mesh> meshes2)
        {
            CheckLengths("Meshes lengths do not match!", meshes1.Count, meshes2.Count, meshes.Count);

            List<Mesh> output = new List<Mesh>();
            for (int i = 0; i < meshes1.Count; i++)
                output.Add(ReorderVertices(meshes[i], meshes1[i], meshes2[i], threshold));
            return output;
        }

        public static Mesh ReorderVertices(Mesh mesh, Mesh mesh1, Mesh mesh2, float threshold)
        {
            CheckLengths("Vertex count matching", mesh.vertexCount, mesh1.vertexCount, mesh2.vertexCount);

            int[] indexRemap = CompareVertex.Positions(mesh1.vertices, mesh2.vertices, threshold).ToArray();
            CheckLengths("Match vertices", indexRemap.Length, mesh1.vertices.Length, mesh2.vertexCount);

            mesh = CommandMeshVertexReorder.ReorderVertices(CloneAsset(mesh), indexRemap);
            mesh.triangles = mesh2.triangles;

            // [SEAN]
            // mesh = CloneAsset(mesh);
            // CommandMeshVertexReorder.ReorderVertices(mesh, indexRemap);
            // mesh.triangles = mesh2.triangles;

            return mesh;
        }

    }
}