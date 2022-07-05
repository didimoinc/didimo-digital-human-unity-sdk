using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Prefab/Prefab")]
    [DisplayName("Prefab")]
    [Width(200)]
    [HeaderColor(TypeColors.GameObject)]
    public class CommandPrefab : GraphNode
    {
        [Input("Prefab")] public GameObject prefabInput;
        [Output("Skin")] public SkinnedMeshRenderer skinsOutput;
        [Output("MeshFilter")] public MeshFilter filtersOutput;
        [Output("Mesh")] public Mesh meshOutput;

        [Expose] public Activity active = Activity.IncludeActiveOnly;

        public enum Activity
        {
            IncludeActiveOnly = 0,
            IncludeAll = 1,
        }

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            switch (info.Name)
            {
                case nameof(skinsOutput):
                    values = GetAllComponents<SkinnedMeshRenderer>().ToList<object>();
                    if (values.Count == 0) break;
                    return true;
                case nameof(filtersOutput):
                    values = GetAllComponents<MeshFilter>().ToList<object>();
                    if (values.Count == 0) break;
                    return true;
                case nameof(meshOutput):
                    values = GetAllComponents<SkinnedMeshRenderer>().Select(m => m.sharedMesh).
                        Concat(GetAllComponents<MeshFilter>().Select(m => m.sharedMesh)).ToList<object>();
                    if (values.Count == 0) break;
                    Debug.Log("Prefab Meshes: " + values);
                    return true;
            }
            return base.GetOutputValues(info, out values);
        }

        IEnumerable<T> GetAllComponents<T>() where T : UnityEngine.Component =>
            GetInputValues(nameof(prefabInput)).
                SelectMany(p => (p as GameObject).GetComponentsInChildren<T>(active == Activity.IncludeAll));

    }
}
