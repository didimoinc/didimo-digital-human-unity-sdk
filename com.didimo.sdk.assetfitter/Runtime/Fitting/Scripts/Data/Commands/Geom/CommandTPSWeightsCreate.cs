using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Geom/TPS Weights Create")]
    [DisplayName("TPS Weights")]
    [Width(200)]
    [HeaderColor(TypeColors.TPSWeights)]
    public class CommandTPSWeightsCreate : GraphNode
    {
        [Input("Mesh1", true)] public Mesh mesh1Input;
        [Input("Mesh2")] public Mesh mesh2Input;
        [Output("TPS")] public TPSWeights tpsWeightsOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            values = null;
            if (info.Name == nameof(tpsWeightsOutput))
            {
                var mesh1Inputs = GetInputValues(nameof(mesh1Input));
                var mesh2Inputs = GetInputValues(nameof(mesh2Input));
                return Prerequisite(mesh1Inputs, mesh2Inputs) && CreateTPSWeights(mesh1Inputs, mesh2Inputs, out values);
            }
            return false;
        }

        bool CreateTPSWeights(List<object> meshes1, List<object> meshes2, out List<object> tpsWeights)
        {
            tpsWeights = new List<object>();
            for (int i = 0; i < meshes1.Count; i++)
            {
                var mesh1 = meshes1[i] as Mesh;
                for (int j = 0; j < meshes2.Count; j++)
                {
                    var mesh2 = meshes2[j] as Mesh;
                    var tps = TPSWeights.CreateInstance(mesh1.vertices, mesh2.vertices);
                    tps.name = mesh1.name + " to " + mesh2.name;
                    tpsWeights.Add(tps);
                }
            }
            return true;
        }

        bool Prerequisite(List<object> meshes1, List<object> meshes2)
        {
            bool ret = true;
            if (CheckLengths("Mesh Inputs", meshes1.Count, meshes2.Count))
            {
                for (int i = 0; i < meshes1.Count; i++)
                {
                    if (!CheckLengths("Vertices", (meshes1[i] as Mesh).vertices.Length, (meshes2[i] as Mesh).vertices.Length))
                        ret = false;
                }
            }
            return ret;
        }


    }
}
