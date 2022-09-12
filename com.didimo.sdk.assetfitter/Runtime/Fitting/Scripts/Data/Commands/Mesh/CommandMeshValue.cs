using System.ComponentModel;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Mesh/Mesh")]
    [DisplayName("Mesh")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshValue : GraphNode
    {
        [Output("Mesh"), Expose(false)] public Mesh meshOutput;
    }
}
