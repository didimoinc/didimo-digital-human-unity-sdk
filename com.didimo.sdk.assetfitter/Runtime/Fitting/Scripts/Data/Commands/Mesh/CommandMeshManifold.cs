using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Mesh/Mesh Manifold")]
    [DisplayName("Mesh Manifold")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshManifold : GraphNode
    {
        [Input("Mesh")] public Mesh meshInput;
        [Output("Mesh")] public Mesh meshOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            throw new System.Exception("This command is not implement!");
            // if (info.Name == nameof(meshOutput))
            // {
            //     values = MergeSeams(GetInputValues<Mesh>(nameof(meshInput))).ToList<object>();
            //     return true;
            // }
            //values = null;
            //return false;
        }

        // public static List<Mesh> MergeSeams(List<Mesh> meshes) =>
        //     meshes.Select(m => Seams.MergeEdges(m as Mesh)).ToList();
    }
}