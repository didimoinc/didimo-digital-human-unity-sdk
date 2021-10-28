using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Didimo.Builder.JSON
{
    public class MeshBuilder
    {
        public virtual bool TryBuild(DidimoBuildContext context, DidimoModelDataObject dataObject)
        {
            if (context.MeshHierarchyRoot == null)
            {
                Debug.LogWarning("Failed to build mesh as no MeshHierarchyRoot was found.");
                return false;
            }

            IReadOnlyList<DidimoModelDataObject.Mesh> dataMeshes = dataObject.Meshes;
            foreach (DidimoModelDataObject.Mesh dataMesh in dataMeshes)
            {
                if (!GetOrCreateRenderer(context, dataMesh, out Renderer renderer))
                {
                    Debug.LogWarning($"Failed to locate hierarchy transform ({dataMesh.Name})");
                    continue;
                }

                if (BuildMesh(context, dataMesh, out Mesh mesh))
                {
                    ApplyMeshToRenderer(renderer, mesh);
                }
            }

            return true;
        }

        private bool ApplyMeshToRenderer(Renderer renderer, Mesh mesh)
        {
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                skinnedMeshRenderer.sharedMesh = mesh;
                return true;
            }

            if (renderer is MeshRenderer meshRenderer)
            {
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (meshFilter == null) return false;
                meshFilter.sharedMesh = mesh;
                return true;
            }

            return false;
        }

        private bool GetOrCreateRenderer(DidimoBuildContext context, DidimoModelDataObject.Mesh srcMesh, out Renderer renderer)
        {
            if (!context.MeshHierarchyRoot.TryFindRecursive(srcMesh.Name, out Transform meshGO))
            {
                Debug.LogWarning($"Failed to locate hierarchy transform ({srcMesh.Name})");
                renderer = null;
                return false;
            }

            if (srcMesh.HasSkinning)
            {
                SkinnedMeshRenderer smr = meshGO.gameObject.AddComponent<SkinnedMeshRenderer>();
                renderer = smr;

                // TODO: This should come from the model, instead of being hard-coded
                if (smr.gameObject.name.Contains("eyeLash"))
                {
                    smr.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
            else
            {
                MeshRenderer meshRenderer = meshGO.gameObject.AddComponent<MeshRenderer>();
                MeshFilter mf = meshGO.gameObject.AddComponent<MeshFilter>();
                renderer = meshRenderer;
            }

            return true;
        }

        private bool BuildMesh(DidimoBuildContext context, DidimoModelDataObject.Mesh srcMesh, out Mesh mesh)
        {
            mesh = new Mesh();
            mesh.name = srcMesh.Name;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();

            for (int i = 0; i < srcMesh.Vertices.Length; i += 3)
            {
                vertices.Add(new Vector3(-srcMesh.Vertices[i], srcMesh.Vertices[i + 1], srcMesh.Vertices[i + 2]) / context.UnitsPerMeter);
            }

            for (int i = 0; i < srcMesh.Normals.Length; i += 3)
            {
                normals.Add(new Vector3(-srcMesh.Normals[i], srcMesh.Normals[i + 1], srcMesh.Normals[i + 2]));
            }

            // Only one uv set supported
            for (int i = 0; i < srcMesh.UVs[0].Length; i += 2)
            {
                uvs.Add(new Vector2(srcMesh.UVs[0][i], srcMesh.UVs[0][i + 1]));
            }

            List<int> faces = new List<int>();
            for (int i = 0; i < srcMesh.Faces.Length; i += 3)
            {
                faces.Add(srcMesh.Faces[i + 2]);
                faces.Add(srcMesh.Faces[i + 1]);
                faces.Add(srcMesh.Faces[i]);
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(faces.ToArray(), 0, true);
            mesh.RecalculateTangents();

            return true;
        }
    }
}