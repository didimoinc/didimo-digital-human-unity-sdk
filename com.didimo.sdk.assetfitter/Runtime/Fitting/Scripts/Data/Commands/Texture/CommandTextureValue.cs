using System.ComponentModel;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Texture/Texture")]
    [DisplayName("Texture")]
    [Width(200)]
    public class CommandTextureValue : GraphNode
    {
        [Output("Texture"), Expose] public Texture textureOutput;
    }
}
