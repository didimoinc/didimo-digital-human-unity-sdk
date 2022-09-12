using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Texture/Texture resize")]
    [DisplayName("Texture resize")]
    [Width(160)]
    [HeaderColor(TypeColors.Texture)]
    public class CommandTextureResize : GraphNode
    {
        [Input("Texture")] public Texture textureInput;
        [Expose] public Vector2 size = new Vector2(512, 512);
        [Output("Texture")] public Texture textureOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(textureOutput))
            {
                values = Resize(GetInputValues(nameof(textureInput)));
                return true;
            }
            values = null;
            return false;
        }

        List<object> Resize(List<object> textures)
        {
            return textures.ToList();
        }
    }
}