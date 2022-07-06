using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [Width(240)]
    public class CommandAsset : GraphNode
    {
        [Output("Prefab"), Expose] public GameObject prefabOutput;
        public virtual Gender gender => Gender.None;
    }
}
