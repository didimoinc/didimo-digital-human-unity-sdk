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

            int[] indices = CompareVertex.Positions(mesh2.vertices, mesh1.vertices, threshold).ToArray();
            CheckLengths("Match vertices", indices.Length, mesh1.vertices.Length);

            var mesh = CloneAsset(mesh2);
            ReorderVertices(mesh, indices);
            mesh.SetTriangles(mesh1.triangles, 0);
            return mesh;
        }

        public static void ReorderVertices(Mesh mesh, int[] order)
        {
            T[] RemapArray<T>(T[] input) => order.Select(i => input[i]).ToArray();
            mesh.vertices = RemapArray(mesh.vertices);
            mesh.normals = RemapArray(mesh.normals);
            mesh.tangents = RemapArray(mesh.tangents);

            // remap blendshapes
            if (mesh.blendShapeCount > 0)
            {
                Blendshape[] blendshapes = new Blendshape[mesh.blendShapeCount];
                for (int b = 0; b < mesh.blendShapeCount; b++)
                {
                    blendshapes[b] = new Blendshape { name = mesh.GetBlendShapeName(b) };
                    var name = mesh.GetBlendShapeName(b);
                    for (int f = 0, fc = mesh.GetBlendShapeFrameCount(b); f < fc; f++)
                    {
                        var frame = new Blendshape.Frame(mesh.vertexCount) { weight = mesh.GetBlendShapeFrameWeight(b, f) };
                        blendshapes[b].frames.Add(frame);
                        mesh.GetBlendShapeFrameVertices(b, f, frame.dv, frame.dn, frame.dt);
                        RemapArray(frame.dv);
                        RemapArray(frame.dn);
                        RemapArray(frame.dt);
                    }
                }

                mesh.ClearBlendShapes();

                foreach (var blendshape in blendshapes)
                    foreach (var frame in blendshape.frames)
                        mesh.AddBlendShapeFrame(blendshape.name, frame.weight, RemapArray(frame.dv), RemapArray(frame.dn), RemapArray(frame.dt));

            }
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
