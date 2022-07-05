using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.MeshTools;

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Mesh/Mesh Combine")]
    [DisplayName("Mesh Combine")]
    [Width(160)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshCombine : GraphNode
    {
        [Input("Mesh")] public Mesh meshInput;
        [Output("Mesh")] public Mesh meshOutput;
        [Expose] public CombineType combine = CombineType.Default;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(meshOutput))
            {
                values = Combine(GetInputValues<Mesh>(nameof(meshInput)), combine).ToList<object>();
                return true;
            }
            values = null;
            return false;
        }

        public static List<Mesh> Combine(List<Mesh> meshes, CombineType type)
        {
            switch (type)
            {
                default:
                case CombineType.Default:
                    return new List<Mesh>() { CombineMeshes(meshes) };
                case CombineType.CreateSubMeshes:
                    return new List<Mesh>() { CombineMeshesIntoSubMeshes(meshes.ToArray()) };
                case CombineType.CreateUVMeshes:
                    // return new List<Mesh>() { GetUVGroups(meshes) };
                    return new List<Mesh>() { UVGroups.ConvertToSubmeshes(CombineMeshes(meshes)) };
            }
        }

        // static Mesh GetUVGroups(List<Mesh> meshes)
        // {
        //     var mesh = MeshTools.CombineMeshes(meshes.ToArray());
        //     var groups = GetGroups(mesh.triangles);

        //     Debug.Log("Groups: " + groups.Length);

        //     Mesh result = CloneAsset(mesh);
        //     result.subMeshCount = groups.Length;

        //     // Set triangles per Submesh
        //     for (int i = 0; i < groups.Length; i++)
        //         result.SetTriangles(groups[i], i);

        //     // finalize the new skinned mesh
        //     result.RecalculateBounds();
        //     return result;
        // }

        // static int[][] offsets = { new[] { +1, +2 }, new[] { -1, 1 }, new[] { -2, -1 } };
        // static int[][] GetGroups(int[] indices)
        // {
        //     List<Triangle> triangles = GetTriangles(indices);
        //     HashSet<Triangle> fillMarkers = new HashSet<Triangle>();
        //     List<Triangle> group = new List<Triangle>();
        //     List<Triangle[]> groups = new List<Triangle[]>();

        //     void fill(Triangle triangle)
        //     {
        //         if (fillMarkers.Contains(triangle)) return;
        //         fillMarkers.Add(triangle);
        //         group.Add(triangle);
        //         foreach (var n in triangle.neighbours)
        //             if (n != null)
        //                 fill(n);
        //     }
        //     foreach (var triangle in triangles)
        //     {
        //         fill(triangle);
        //         if (group.Count > 0)
        //         {
        //             groups.Add(group.ToArray());
        //             group.Clear();
        //         }
        //     }
        //     return groups.Select(g => g.SelectMany(t => t.indices).ToArray()).ToArray();
        // }

        // static List<Triangle> GetTriangles(int[] indices)
        // {
        //     var triangles = new List<Triangle>();
        //     for (int i = 0; i < indices.Length; i += 3)
        //     {
        //         var triangle1 = new Triangle(indices.Skip(i).Take(3).ToArray());
        //         foreach (var triangle2 in triangles)
        //             triangle2.connect(triangle1);
        //         triangles.Add(triangle1);
        //     }
        //     return triangles;
        // }

        // class Triangle
        // {
        //     public int[] indices;
        //     public Triangle[] neighbours;
        //     public Triangle(int[] indices)
        //     {
        //         this.indices = indices;
        //         this.neighbours = new Triangle[3];
        //     }

        //     public void connect(Triangle triangle)
        //     {
        //         for (int i = 0; i < 3; i++)
        //         {
        //             for (int j = 0; j < 3; j++)
        //             {
        //                 if (indices[i] == triangle.indices[(j + 1) % 3] && indices[(i + 1) % 3] == triangle.indices[j])
        //                 {
        //                     this.neighbours[i] = triangle;
        //                     triangle.neighbours[j] = this;
        //                 }
        //             }
        //         }
        //     }
        // }


        public enum CombineType
        {
            Default = 0,
            CreateSubMeshes = 1,
            CreateUVMeshes = 2,
        }
    }
}