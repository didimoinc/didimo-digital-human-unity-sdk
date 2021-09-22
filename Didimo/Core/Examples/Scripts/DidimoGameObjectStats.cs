using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Didimo.Stats
{
    public static class DidimoGameObjectStats
    {
        public readonly struct MeshData
        {
            public readonly struct Mesh
            {
                public readonly  string              Name;
                public readonly  int                 VertexCount;
                public readonly  int                 TriangleCount;
                private readonly SkinnedMeshRenderer _meshRenderer;

                public bool IsActive => _meshRenderer.enabled && _meshRenderer.gameObject.activeInHierarchy;

                public Mesh(SkinnedMeshRenderer meshRenderer)
                {
                    UnityEngine.Mesh mesh = meshRenderer.sharedMesh;
                    Name = mesh.name;
                    VertexCount = mesh.vertexCount;
                    TriangleCount = mesh.triangles.Length / 3;
                    _meshRenderer = meshRenderer;
                }

                public override bool Equals(object obj) => obj is Mesh other && _meshRenderer.Equals(other._meshRenderer);

                public override int GetHashCode() => _meshRenderer.GetHashCode();

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

        public static MeshData GetMeshData(GameObject target)
        {
            SkinnedMeshRenderer[] meshRenderers = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            return new MeshData(meshRenderers);
        }
    }
}