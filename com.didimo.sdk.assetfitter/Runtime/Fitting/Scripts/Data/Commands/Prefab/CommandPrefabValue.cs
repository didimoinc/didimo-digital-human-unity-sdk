using System.ComponentModel;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Prefab/Prefab Value")]
    [DisplayName("Prefab Value")]
    [Width(200)]
    [HeaderColor(64 / 255f, 64 / 255f, 192 / 255f, 0.8f)]
    public class CommandPrefabValue : GraphNode
    {
        [Output("Prefab"), Expose] public GameObject prefabOutput;
    }
}
