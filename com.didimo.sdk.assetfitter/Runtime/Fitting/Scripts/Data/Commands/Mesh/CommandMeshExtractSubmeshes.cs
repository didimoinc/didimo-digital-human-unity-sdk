using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Mesh/Mesh Extract Submeshes")]
    [DisplayName("Extract Submeshes")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshExtractSubmeshes : GraphNode
    {
        [Input("Mesh", true)] public Mesh meshInput;
        [Output("Mesh")] public Mesh meshOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                values = ExtractSubMeshes(GetInputValues<Mesh>(nameof(meshInput))).ToList<object>();
                return true;
            }
            values = null;
            return false;
        }

        public static List<Mesh> ExtractSubMeshes(List<Mesh> meshes) =>
            meshes.SelectMany(mesh => MeshTools.ExtractSubMeshes(mesh)).ToList();
    }
}