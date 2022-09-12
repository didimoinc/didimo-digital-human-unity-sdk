using System;
using System.ComponentModel;
using UnityEngine;
using System.IO;
using static Didimo.AssetFitter.Editor.Graph.MeshTools;
using static Didimo.AssetFitter.Editor.Graph.GraphTools;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using static Didimo.AssetFitter.Editor.Graph.PathTools;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Mesh/Mesh Transform To Mesh")]
    [DisplayName("Transform 2 Mesh")]
    [Tooltip("Saves a transform positions as a single mesh!")]
    [Width(200)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandTransformToMesh : GraphNode
    {
        [Input("Transform")] public Transform[] transformsInput;
        [Output("Mesh")] public Mesh meshOutput;
        [Expose] public float size = 0.01f;

        // protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        // {
        //     if (info.Name == nameof(meshOutput))
        //     {
        //         values = GetInputValues<Transform>(nameof(transformsInput)).ToList<object>();
        //         return true;
        //     }
        //     values = null;
        //     return false;
        // }

        // Mesh[] buildMesh(List<Transform[]> transformGroups)
        // {
        //     Mesh createMesh(Transform[] transforms)
        //     {
        //         var mesh = new Mesh(); mesh.name = "Transforms";
        //         return mesh;
        //     }
        //     return mesh;
        // }
    }
}
