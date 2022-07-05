using System.ComponentModel;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.GraphTools;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Geom/TPS Weights Save")]
    [DisplayName("TPS Weights Save")]
    [Width(200)]
    [HeaderColor(TypeColors.TPSWeights)]
    public class CommandTPSWeightsSave : GraphNode
    {
        [Input("TPS")] public TPSWeights tpsWeightsInput;
        [Input("Path")] [Expose] public string path;
        internal override void EndPoint(bool Build = false)
        {
#if UNITY_EDITOR

            var tpsWeights = GetInputValues(GetType().GetField(nameof(tpsWeightsInput)));
            // var paths = GetInputValues(GetType().GetField(nameof(this.path)));
            // Debug.Log(meshes.Count + " " + paths.Count);

            if (tpsWeights.Count > 0)
            {
                var path = this.path;
                if (GetAssetFolder(ref path))
                {
                    for (int i = 0; i < tpsWeights.Count; i++)
                    {
                        // var filename = string.Format(path + "/TPSWeights-{0:000}.asset", i);
                        var tps = tpsWeights[i] as TPSWeights;
                        var filename = string.Format(path + "/" + tps.name + ".asset", i);
                        CreateAsset(tps, filename);
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else Debug.LogError("No valid asset path '" + path + "'");
            }
#else
            throw new System.Exception( "'" + GetType() + "'" + " Not implemented for runtime!");
#endif
        }

    }
}
