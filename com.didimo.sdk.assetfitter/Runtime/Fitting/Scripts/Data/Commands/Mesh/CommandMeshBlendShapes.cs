using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.MeshTools;
using static Didimo.AssetFitter.Editor.Graph.GraphTools;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using static Didimo.AssetFitter.Editor.Graph.PathTools;
using System;

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Mesh/Mesh Blendshapes")]
    [DisplayName("Blendshapes")]
    [HeaderColor(TypeColors.Mesh)]
    [Width(160)]
    public class CommandMeshBlendShapes : GraphNode
    {
        [Input("Mesh", true)] public Mesh primaryMesh;
        [Input("Delta")] public Mesh deltaMesh;
        [Output("Mesh")] public Mesh meshOutput;
        [Expose] public Action action;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                Mesh primaryMesh = GetInputValue<Mesh>(nameof(this.primaryMesh));
                List<Mesh> deltaMesh = GetInputValues<Mesh>(nameof(this.deltaMesh));
                if (primaryMesh && deltaMesh.Count() > 0)
                {
                    values = new List<object>() { BuildBlendShapes(primaryMesh, deltaMesh) };
                    return true;
                }
            }
            values = null;
            return false;
        }

        Mesh BuildBlendShapes(Mesh primary, List<Mesh> delta)
        {
            Mesh mesh = CloneAsset(primary);

            if ((action & Action.ClearBones) > 0)
            {
                mesh.boneWeights = null;
                mesh.bindposes = null;
            }

            int[] tris1 = primary.triangles;
            Vector3[] vts1 = primary.vertices;
            Dictionary<string, int> names = new Dictionary<string, int>();
            foreach (var d in delta)
            {
                Vector3[] vts2 = d.vertices;
                CheckLengths("Vertex count matching", primary.vertexCount, d.vertexCount);

                string name = d.name;
                if (names.ContainsKey(name)) name += "_" + (++names[name]);
                else names[name] = 0;

                mesh.AddBlendShapeFrame(name, 100, vts1.Select((v, i) => vts2[i] - v).ToArray(), null, null);
            }
            return mesh;
        }

        [Flags]
        public enum Action
        {
            ClearBones = 1 << 0,
        }
    }
}