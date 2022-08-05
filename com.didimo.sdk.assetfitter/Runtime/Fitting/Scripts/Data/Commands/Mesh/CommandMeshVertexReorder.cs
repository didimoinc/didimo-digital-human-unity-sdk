using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using static Didimo.AssetFitter.Editor.Graph.GeomTools;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Mesh/Mesh Vertex Reorder")]
    [DisplayName("Mesh Vertex Reorder")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshVertexReorder : GraphNode
    {
        [Input("Mesh1", true)] public Mesh mesh1Input;
        [Input("Mesh2", true)] public Mesh mesh2Input;
        [Output("Mesh")] public Mesh meshOutput;
        [Expose] public float threshold = 0.000001f;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                values = ReorderVertices(GetInputValues<Mesh>(nameof(mesh1Input)), GetInputValues<Mesh>(nameof(mesh2Input))).ToList<object>();
                return true;
            }
            values = null;
            return false;
        }

        List<Mesh> ReorderVertices(List<Mesh> meshes1, List<Mesh> meshes2)
        {
            CheckLengths("Meshes lengths do not match!", meshes1.Count, meshes2.Count);

            List<Mesh> output = new List<Mesh>();
            for (int i = 0; i < meshes1.Count; i++)
                output.Add(ReorderVertices(meshes1[i], meshes2[i], threshold));
            return output;
        }

        public static Mesh ReorderVertices(Mesh mesh1, Mesh mesh2, float threshold)
        {
            CheckLengths("Vertex count matching", mesh1.vertexCount, mesh2.vertexCount);

            int[] indexRemap = CompareVertex.Positions(mesh2.vertices, mesh1.vertices, threshold).ToArray();
            CheckLengths("Match vertices", indexRemap.Length, mesh1.vertexCount);

            mesh2 = ReorderVertices(CloneAsset(mesh2), indexRemap);
            mesh2.triangles = mesh1.triangles;
            return mesh2;
        }

        public static Mesh ReorderVertices(Mesh mesh, int[] order)
        {
            return MeshTools.ReorderVertices(mesh, order);
            // T[] remapArray<T>(T[] input) => order.Select(i => input[i]).ToArray();
            // mesh.vertices = remapArray(mesh.vertices);
            // mesh.normals = remapArray(mesh.normals);
            // mesh.tangents = remapArray(mesh.tangents);

            // if (mesh.blendShapeCount > 0)
            // {
            //     Blendshape[] blendshapes = new Blendshape[mesh.blendShapeCount];
            //     for (int b = 0; b < mesh.blendShapeCount; b++)
            //     {
            //         blendshapes[b] = new Blendshape { name = mesh.GetBlendShapeName(b) };
            //         var name = mesh.GetBlendShapeName(b);
            //         for (int f = 0, fc = mesh.GetBlendShapeFrameCount(b); f < fc; f++)
            //         {
            //             var frame = new Blendshape.Frame(mesh.vertexCount) { weight = mesh.GetBlendShapeFrameWeight(b, f) };
            //             blendshapes[b].frames.Add(frame);
            //             mesh.GetBlendShapeFrameVertices(b, f, frame.dv, frame.dn, frame.dt);
            //             remapArray(frame.dv);
            //             remapArray(frame.dn);
            //             remapArray(frame.dt);
            //         }
            //     }

            //     mesh.ClearBlendShapes();
            //     foreach (var blendshape in blendshapes)
            //         foreach (var frame in blendshape.frames)
            //             mesh.AddBlendShapeFrame(blendshape.name, frame.weight, remapArray(frame.dv), remapArray(frame.dn), remapArray(frame.dt));

            // }
        }

        class Blendshape
        {
            public class Frame
            {
                public Vector3[] dv, dn, dt;
                public float weight;
                public Frame(int count)
                {
                    dv = new Vector3[count];
                    dn = new Vector3[count];
                    dt = new Vector3[count];
                }
            }
            public string name;
            public List<Frame> frames = new List<Frame>();
        }
    }
}
