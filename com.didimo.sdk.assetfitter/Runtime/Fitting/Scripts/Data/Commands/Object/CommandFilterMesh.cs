using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Object/Filter Mesh")]
    [DisplayName("Filter Mesh")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandFilterMesh : GraphNode
    {
        [Input("Filter")] public MeshFilter filterInput;
        [Output("Mesh")] public Mesh meshOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                List<object> input = GetInputValues(nameof(filterInput));
                values = Convert(input);
                return true;
            }
            return base.GetOutputValues(info, out values);
        }

        List<object> Convert(List<object> filters) =>
            new List<object>(filters.Select(s => (s as MeshFilter).sharedMesh).Where(m => m).Select(m => UnityEngine.Object.Instantiate(m)));
    }
}