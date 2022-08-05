using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Mesh/Mesh List")]
    [DisplayName("Mesh List")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshList : GraphNode
    {
        [Output("Mesh")] public Mesh meshOutput;
        [Expose] public MeshList meshList;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                values = meshList.meshes.ToList<object>();
                return true;
            }
            values = null;
            return false;
        }
    }
}
