using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Mesh/Mesh Extract Blend Shapes")]
    [DisplayName("Extract Blend Shapes")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshExtractBlendShapes : GraphNode
    {
        [Input("Mesh", true)] public Mesh meshInput;
        [Output("Mesh")] public Mesh meshOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                values = ExtractBlendShapes(GetInputValues<Mesh>(nameof(meshInput))).ToList<object>();
                return true;
            }
            values = null;
            return false;
        }

        public static List<Mesh> ExtractBlendShapes(List<Mesh> meshes)
        {
            IEnumerable<Mesh> Extract()
            {
                foreach (var mesh in meshes)
                {
                    for (int i = 0; i < mesh.blendShapeCount; i++)
                        for (int j = 0, n = mesh.GetBlendShapeFrameCount(i); j < n; j++)
                            yield return ExtractBlendShape(mesh, i, j);
                }
            }
            return Extract().ToList<Mesh>();
        }

        public static Mesh ExtractBlendShape(Mesh mesh, int index, int frameIndex)
        {
            List<Mesh> meshes = new List<Mesh>();
            Vector3[] vertexDeltas = new Vector3[mesh.vertexCount], normalDeltas = new Vector3[mesh.vertexCount],
                tangentDeltas = new Vector3[mesh.vertexCount];

            mesh.GetBlendShapeFrameVertices(index, frameIndex, vertexDeltas, normalDeltas, tangentDeltas);
            float w = mesh.GetBlendShapeFrameWeight(index, frameIndex);
            Mesh m = new Mesh()
            {
                name = mesh.name + "_" + mesh.GetBlendShapeName(index) + "_" + mesh.GetBlendShapeFrameWeight(index, frameIndex),
                vertices = mesh.vertices.Select((v, k) => v + vertexDeltas[k]).ToArray(),
                normals = mesh.normals.Select((v, k) => v + normalDeltas[k]).ToArray(),
                tangents = mesh.tangents.Select((v, k) => v + (Vector4)tangentDeltas[k]).ToArray(),
                triangles = mesh.triangles,
            };
            m.RecalculateBounds();
            return m;
        }
    }
}