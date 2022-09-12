using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Mesh/Mesh Transform")]
    [DisplayName("Transform")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshTransform : GraphNode
    {
        [Input("Mesh", true)] public Mesh meshInput;
        [Output("Mesh")] public Mesh meshOutput;
        [Expose] public Vector3 offset;
        [Expose] public Vector3 rotation;
        [Expose] public Vector3 scale = Vector3.one;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                Matrix4x4 matrix = Matrix4x4.TRS(offset, Quaternion.Euler(rotation), scale);

                values = TransformVertices(GetInputValues<Mesh>(nameof(meshInput)), matrix).ToList<object>();
                return true;
            }
            values = null;
            return false;
        }

        static List<Mesh> TransformVertices(List<Mesh> meshes, Matrix4x4 matrix)
        {
            List<Mesh> output = new List<Mesh>();
            for (int i = 0; i < meshes.Count; i++)
            {
                Mesh mesh = CloneAsset(meshes[i]);
                mesh.vertices = mesh.vertices.Select(v => matrix.MultiplyPoint(v)).ToArray();
                mesh.normals = mesh.normals.Select(v => matrix.MultiplyVector(v)).ToArray();
                //mesh.tangents = mesh.tangents.Select(v => new Vector3(v.x, v.y, v.z)).Select(v => matrix.MultiplyVector(v)).Select(v => new Vector4(v.x, v.y, v.z, 0)).ToArray();
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();
                output.Add(mesh);
            }
            return output;
        }


    }
}
