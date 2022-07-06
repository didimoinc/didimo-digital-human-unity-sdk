using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Mesh/Mesh Deform TPS")]
    [DisplayName("Deform TPS")]
    [Width(200)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshDeformTPS : GraphNode
    {
        [Input("Mesh")] public Mesh meshInput;
        [Input("TPS")] public TPSWeights tpsWeightsInput;
        [Output("Mesh")] public Mesh meshOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                var meshInputs = GetInputValues(nameof(meshInput));
                var tpsWeightsInputs = GetInputValues(nameof(tpsWeightsInput));
                return Transform(meshInputs, tpsWeightsInputs, out values);
            }
            values = null;
            return false;
        }

        bool Transform(List<object> meshes, List<object> tpsWeights, out List<object> values)
        {
            values = new List<object>();
            for (int i = 0; i < tpsWeights.Count; i++)
            {
                var tps = tpsWeights[i] as TPSWeights;
                for (int j = 0; j < meshes.Count; j++)
                {
                    var mesh = meshes[j] as Mesh;
                    var result = Object.Instantiate(mesh);
                    result.name = mesh.name + " (" + tps.name + ")";
                    result.vertices = tps.Transform(result.vertices);
                    values.Add(result);
                }
            }
            return true;
        }
    }
}
