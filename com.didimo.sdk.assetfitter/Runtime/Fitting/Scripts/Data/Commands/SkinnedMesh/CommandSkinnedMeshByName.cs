using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Skinned Mesh/Skinned Mesh By Name")]
    [DisplayName("Skin Filter")]
    [Width(240)]
    [HeaderColor(TypeColors.SkinnedMeshRenderer)]
    public class CommandSkinnedMeshByName : GraphNode
    {
        [Output("GameObject"), Expose] public GameObject gameObjectOutput;
        [Output("Skin")] public SkinnedMeshRenderer skinOutput;
        [Expose] public string name;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(skinOutput))
            {
                values = gameObjectOutput.GetComponentsInChildren<SkinnedMeshRenderer>().
                    Where(s => s.name.ToLower().Contains(name.ToLower())).
                    ToList<object>();
                return true;
            }
            values = null;
            return false;
        }
    }
}
