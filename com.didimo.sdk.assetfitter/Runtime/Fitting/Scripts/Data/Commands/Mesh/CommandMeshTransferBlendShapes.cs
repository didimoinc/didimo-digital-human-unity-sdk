using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Mesh/Mesh Transfer Blend Shapes")]
    [DisplayName("Transfer Blend Shapes")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshTransferBlendShapes : GraphNode
    {
        [Input("Mesh1")] public Mesh mesh1Input;
        [Input("Mesh2")] public Mesh mesh2Input;
        [Output("Mesh")] public Mesh meshOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                values = TransferBlendShapes(GetInputValues<Mesh>(nameof(mesh1Input)), GetInputValues<Mesh>(nameof(mesh2Input))).ToList<object>();
                return true;
            }
            values = null;
            return false;
        }

        public List<Mesh> TransferBlendShapes(List<Mesh> meshes1, List<Mesh> meshes2)
        {
            CheckLengths("Meshes lengths do not match!", meshes1.Count, meshes2.Count);

            List<Mesh> meshes = new List<Mesh>();
            for (int i = 0; i < meshes1.Count; i++)
                meshes.Add(TransferBlendShapes(meshes1[i], meshes2[i]));
            return meshes;
        }

        public Mesh TransferBlendShapes(Mesh mesh1, Mesh mesh2)
        {
            CheckLengths("Mesh Vertex count do not match!", mesh1.vertexCount, mesh2.vertexCount);

            int count = mesh1.vertexCount;
            Vector3[] deltas = new Vector3[count], normals = new Vector3[count], tangents = new Vector3[count];

            var mesh = AssetTools.CloneAsset(mesh2);

            for (int i = 0; i < mesh1.blendShapeCount; i++)
            {
                var name = mesh1.GetBlendShapeName(i);
                name = name.Split('.').Last();
                for (int f = 0, bsc = mesh1.GetBlendShapeFrameCount(i); f < bsc; f++)
                {
                    Debug.Log("Adding " + name + " " + mesh1.GetBlendShapeFrameWeight(i, f));
                    var weight = mesh1.GetBlendShapeFrameWeight(i, f);
                    mesh1.GetBlendShapeFrameVertices(i, f, deltas, normals, tangents);

                    mesh.AddBlendShapeFrame(name, weight, deltas, normals, tangents);
                }
            }
            return mesh;
        }
    }
}