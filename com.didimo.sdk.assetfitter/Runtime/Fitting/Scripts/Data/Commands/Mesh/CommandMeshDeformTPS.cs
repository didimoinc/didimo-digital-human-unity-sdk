using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;

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
            List<Mesh> meshInputs = GetInputValues<Mesh>(nameof(meshInput));
            Debug.Log(meshInputs.Count);
            if (meshInputs.Count != 0)
            {
                List<TPSWeights> tpsWeightsInputs = GetInputValues<TPSWeights>(nameof(tpsWeightsInput));
                return Transform(meshInputs, tpsWeightsInputs, out values);
            }
            values = null;
            return false;
        }

        public static bool Transform(List<Mesh> meshes, List<TPSWeights> tpsWeights, out List<object> values)
        {
            values = new List<object>();
            for (int i = 0; i < tpsWeights.Count; i++)
            {
                TPSWeights tps = tpsWeights[i] as TPSWeights;
                for (int j = 0; j < meshes.Count; j++)
                {
                    Mesh mesh = meshes[j] as Mesh;
                    Mesh result = CloneAsset(mesh);
                    result.name = mesh.name + " (" + tps.name + ")";
                    result.vertices = tps.Transform(result.vertices);
                    result.RecalculateBounds();
                    values.Add(result);

                    Debug.Log("Deform TPS::Transform " + mesh.name);
                }
            }
            return true;
        }
    }
}
