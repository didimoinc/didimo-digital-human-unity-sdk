using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Texture/Texture List")]
    [DisplayName("Texture List")]
    [Width(160)]
    [HeaderColor(TypeColors.Texture)]
    public class CommandTextureList : GraphNode
    {
        [Output("Texture List"), Expose(false)] public TextureList textureListOutput;
        [Output("Textures")] public Texture texturesOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (textureListOutput)
            {
                switch (info.Name)
                {
                    case nameof(textureListOutput):
                        values = new List<object> { textureListOutput };
                        return true;

                    case nameof(texturesOutput):
                        values = textureListOutput.textures.ToList<object>();
                        return true;
                }
            }
            return base.GetOutputValues(info, out values);
        }
    }
}
