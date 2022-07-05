using System.ComponentModel;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Geom/TPS Weights")]
    [DisplayName("TPS Weights")]
    [Width(200)]
    [HeaderColor(TypeColors.TPSWeights)]
    public class CommandTPSWeightsValue : GraphNode
    {
        [Output("TPS"), Expose] public TPSWeights tpsWeightsOutput;
    }
}