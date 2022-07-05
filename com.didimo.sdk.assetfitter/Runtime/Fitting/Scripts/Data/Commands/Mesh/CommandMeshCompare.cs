using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Mesh/Mesh Compare")]
    [DisplayName("Compare")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshCompare : GraphNode
    {
        [Input("Mesh")] public Mesh meshInput;

        internal override void EndPoint(bool Build = false)
        {
            var meshes = GetInputValues<Mesh>(GetType().GetField(nameof(meshInput)));

            Debug.Log("Compare " + meshes.Count + " meshes:");
            Debug.Log(String.Join(",", meshes.Select(m => m.name + " v:" + m.vertexCount + " t:" + m.triangles.Length / 3)));

            for (int x = 0; x < meshes.Count; x++)
            {
                var tris1 = meshes[x].triangles;
                for (int y = x; y < meshes.Count; y++)
                {
                    var tris2 = meshes[y].triangles;
                    var b = CompareSequence(tris1, tris2, out string r);
                    Debug.Log("Compare " + x + " == " + y + " " + tris1.Length + " == " + tris2.Length + " && " + b + " == true [" + r + "] (" + tris1.Intersect(tris2).Count() + ")");
                }
            }
        }

        bool CompareSequence(int[] array1, int[] array2, out string result)
        {
            const int Distance = 3;
            result = "";
            if (array1.Length == array2.Length)
            {
                if (Enumerable.SequenceEqual(array1, array2)) return true;
                else
                {
                    for (int i = 0, n = array1.Length; i < n; i++)
                    {
                        if (array1[i] != array2[i])
                        {
                            int i1 = Mathf.Max(0, i - Distance), i2 = Mathf.Min(n - 1, i + Distance);
                            result = String.Join(",", array1.Skip(i1).Take(i2 - i1)) + " != " + String.Join(",", array2.Skip(i1).Take(i2 - i1)) + " @ " + i;
                            break;
                        }
                    }
                }
            }
            return false;
        }
    }


}