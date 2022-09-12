using System.ComponentModel;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Texture/Texture")]
    [DisplayName("Texture")]
    [Width(160)]
    [HeaderColor(TypeColors.Texture)]
    public class CommandTextureValue : GraphNode
    {
        [Output("Texture"), Expose(false)] public Texture textureOutput;
    }
}
