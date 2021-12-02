using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.Stats
{
    /// <summary>
    /// Example component that provides stats for a given didimo. Contains useful information
    /// such as number of meshes, vertex count and triangle count.
    /// Visit the DidimoInspector scene to see it in action or look at the <c>DidimoStatsExample</c> script.
    /// </summary>
    public static class DidimoGameObjectStats
    {
        /// <summary>
        /// Struct that holds information about all the meshes of a didimo.
        /// Should be created using the public <c>GetMeshData</c> method.
        /// </summary>
        public readonly struct MeshData
        {
            /// <summary>
            /// Struct that holds information about a particular mesh
            /// (mesh name, vertex and triangle count)
            /// </summary>
            public readonly struct Mesh
            {
                public readonly  string              Name;
                public readonly  int                 VertexCount;
                public readonly  int                 TriangleCount;
                private readonly SkinnedMeshRenderer meshRenderer;

                public bool IsActive => meshRenderer.enabled && meshRenderer.gameObject.activeInHierarchy;

                public Mesh(SkinnedMeshRenderer meshRenderer)
                {
                    UnityEngine.Mesh mesh = meshRenderer.sharedMesh;
                    Name = mesh.name;
                    VertexCount = mesh.vertexCount;
                    TriangleCount = mesh.triangles.Length / 3;
                    this.meshRenderer = meshRenderer;
                }

                public override bool Equals(object obj) => obj is Mesh other
                    && meshRenderer.Equals(other.meshRenderer);

                public override int GetHashCode() => meshRenderer.GetHashCode();

                public static bool operator ==(Mesh left, Mesh right) => left.Equals(right);
                public static bool operator !=(Mesh left, Mesh right) => !left.Equals(right);

            }

            private readonly List<Mesh> meshes;
            public int MeshCount => meshes.Count;
            public int ActiveMeshCount => meshes.Count(mesh => mesh.IsActive);
            public IReadOnlyList<Mesh> Meshes => meshes;
            public IReadOnlyList<Mesh> ActiveMeshes => meshes.FindAll(mesh => mesh.IsActive);
            public int TotalVertexCount => meshes.Sum(mesh => mesh.VertexCount);
            public int TotalActiveVertexCount => meshes.Sum(mesh => mesh.IsActive ? mesh.VertexCount : 0);

            public int TotalTriangleCount => meshes.Sum(mesh => mesh.TriangleCount);
            public int TotalActiveTriangleCount => meshes.Sum(mesh => mesh.IsActive ? mesh.TriangleCount : 0);

            public MeshData(IEnumerable<SkinnedMeshRenderer> meshRenderers)
            {
                meshes = new List<Mesh>();
                foreach (SkinnedMeshRenderer meshRenderer in meshRenderers)
                {
                    meshes.Add(new Mesh(meshRenderer));
                }
            }
        }

        /// <summary>
        /// Build a <c>MeshData</c> object that contains the information of the meshes of the didimo.
        /// </summary>
        /// <param name="target">Didimo root gameObject</param>
        /// <returns><c>MeshData</c> struct with information of the didimo's meshes</returns>
        public static MeshData GetMeshData(GameObject target)
        {
            SkinnedMeshRenderer[] meshRenderers = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            return new MeshData(meshRenderers);
        }
    }
}