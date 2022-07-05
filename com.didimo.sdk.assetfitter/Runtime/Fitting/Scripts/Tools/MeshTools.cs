using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Didimo.Core.Deformables;
using Didimo.Extensions;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.GeomTools;
using static Didimo.AssetFitter.Editor.Graph.PathTools;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;

namespace Didimo.AssetFitter.Editor.Graph
{
    public static class MeshTools
    {
        public static class Writer
        {
            public static void WriteFBX(Mesh mesh, string assetPath)
            {
                string filePath = Application.dataPath + "/" + assetPath.Substring("Assets".Length);
                CreatePath(filePath, true);
            }

            public static void WriteOBJ(Mesh mesh, string assetPath)
            {
                string filePath = Application.dataPath + "/" + assetPath.Substring("Assets".Length);
                CreatePath(filePath, true);
                ObjWriter.WriteMesh(mesh, filePath);
            }

            class ObjWriter : ObjDeformationUtility
            {
                public static void WriteMesh(Mesh mesh, string path)
                {
                    ObjWriter writer = new ObjWriter();
                    writer.FromMesh(mesh);
                    File.WriteAllBytes(path, writer.Serialize());
                }

                void FromMesh(Mesh mesh)
                {
                    normals = mesh.normals.Select(n => UnityToPipeline(n, false)).ToList();
                    vertices = mesh.vertices.Select(v => UnityToPipeline(v)).ToList();
                    uvs = mesh.uv.ToList();
                    var tris = mesh.triangles;
                    faces = new List<Vector3Int>();
                    for (int j = 0; j < tris.Length; j += 3)
                        faces.Add(new Vector3Int(tris[j + 2], tris[j + 1], tris[j + 0]));
                }
            }
        }

        public static List<Mesh> ExtractSubMeshes(Mesh mesh) =>
            Enumerable.Range(0, mesh.subMeshCount).Select(i => ExtractSubMesh(mesh, i)).ToList();

        public static Mesh ExtractSubMesh(Mesh mesh, int subMeshIndex)
        {
            Dictionary<int, int> remapIndices = new Dictionary<int, int>();
            List<int> remapVertices = new List<int>();
            int[] indices = mesh.GetTriangles(subMeshIndex);
            Debug.Log("ExtractSubMesh: " + mesh.name + " (" + subMeshIndex + ") " + HashArray(indices));

            for (int x = 0; x < indices.Length; x++)
            {
                if (!remapIndices.ContainsKey(indices[x]))
                {
                    remapIndices.Add(indices[x], remapVertices.Count);
                    remapVertices.Add(indices[x]);
                }
            }

            T[] RemapArray<T>(T[] input) => remapVertices.Select(i => input[i]).ToArray();
            var result = new Mesh()
            {
                name = mesh.name + "-" + subMeshIndex,
                vertices = RemapArray(mesh.vertices),
                uv = RemapArray(mesh.uv),
                tangents = RemapArray(mesh.tangents),
                normals = RemapArray(mesh.normals),
                boneWeights = RemapArray(mesh.boneWeights),
                triangles = indices.Select(i => remapIndices[i]).ToArray(),
            };


            result.RecalculateBounds();
            return result;
        }

        public static Mesh CombineMeshes(List<Mesh> meshes)
        {
            // Assign and create the new combined mesh
            Mesh result = new Mesh()
            {
                name = String.Join("_", meshes.Select(m => m.name)),
                vertices = meshes.SelectMany(m => m.vertices).ToArray(),
                normals = meshes.SelectMany(m => m.normals).ToArray(),
                tangents = meshes.SelectMany(m => m.tangents).ToArray(),
                uv = meshes.SelectMany(m => m.uv).ToArray(),
                boneWeights = meshes.SelectMany(m => m.boneWeights).ToArray(),
            };

            int index = 0;
            List<int> triangles = new List<int>();
            foreach (var mesh in meshes)
            {
                triangles.AddRange(mesh.triangles.Select(i => i + index));
                index += mesh.vertices.Length;
            }
            result.triangles = triangles.ToArray();
            result.RecalculateBounds();
            return result;
        }

        public static Mesh CombineMeshesIntoSubMeshes(IEnumerable<Mesh> meshes)
        {
            // Assign and create the new combined mesh
            Mesh result = new Mesh()
            {
                name = String.Join("_", meshes.Select(m => m.name)),
                vertices = meshes.SelectMany(m => m.vertices).ToArray(),
                normals = meshes.SelectMany(m => m.normals).ToArray(),
                tangents = meshes.SelectMany(m => m.tangents).ToArray(),
                uv = meshes.SelectMany(m => m.uv).ToArray(),
                boneWeights = meshes.SelectMany(m => m.boneWeights).ToArray(),
                subMeshCount = meshes.Count(),
            };

            // Set triangles per Submesh
            int index = 0, submesh = 0;
            foreach (var mesh in meshes)
            {
                result.SetTriangles(mesh.triangles.Select(i => i + index).ToArray(), submesh++);
                index += mesh.vertices.Length;
            }

            // finalize the new skinned mesh
            result.RecalculateBounds();
            return result;
        }

        /// <summary>Used for connecting and finding seams</summary>
        public static class Seams
        {
            static int[] offsets = { +1, +1, -2 };

            /// <summary>Get pairs of indices for each edge of the mesh</summary>
            public static int[][] GetEdges(Mesh mesh) => GetEdges(mesh.triangles);
            public static int[][] GetEdges(int[] indices)
            {
                Dictionary<long, int> edges = new Dictionary<long, int>();

                indices.Select((index, i) => Edge.GetID(index, indices[i + offsets[i % 3]])).
                    ForEach(e => edges[e] = edges.ContainsKey(e) ? edges[e] + 1 : 1);

                return edges.Where(p => p.Value == 1).Select(p => Edge.GetIndices(p.Key)).ToArray();
            }

            public static int[][] GetEdgeGroups(Mesh mesh)
            {
                var isolatedEdgeGroups = UVGroups.GetGroups(mesh.triangles).SelectMany(g => IsolateEdges(GetEdges(g).Select(e => e.ToList()).ToList())).ToArray();

                // return groups.Select(g => GetEdges(g).SelectMany(e => e).Distinct().OrderBy(i => i).ToArray()).ToArray();


                return isolatedEdgeGroups.Select(g => g.Distinct().OrderBy(i => i).ToArray()).ToArray();
            }

            static List<List<int>> IsolateEdges(List<List<int>> edges)
            {
                for (bool intersection = true; intersection;)
                {
                    intersection = false;
                    for (int x = 0; x < edges.Count; x++)
                    {
                        for (int y = x + 1; y < edges.Count; y++)
                        {
                            if (edges[x].Intersect(edges[y]).Count() > 0)
                            {
                                edges[x].AddRange(edges[y]);
                                edges.RemoveAt(y--);
                                intersection = true;
                            }
                        }
                    }
                }
                return edges;
            }

            public static List<Mesh> MergeEdges(List<Mesh> meshes) => meshes.Select(m => Seams.MergeEdges(m as Mesh)).ToList();
            public static Mesh MergeEdges(Mesh mesh, bool self = false) => MergeEdges(mesh, GetEdgeGroups(mesh), self);
            public static Mesh MergeEdges(Mesh mesh, int[][] seams, bool self = false)
            {
                var vertices = mesh.vertices;
                Dictionary<int, List<int>> remap = new Dictionary<int, List<int>>();

                void CompareSeams(int[] seam1, int[] seam2)
                {
                    for (int i = 0; i < seam1.Length; i++)
                    {
                        for (int j = 0; j < seam2.Length; j++)
                        {
                            int _1 = seam1[i], _2 = seam2[j];

                            // always remap to lower index
                            if (_1 > _2) { int s = _1; _1 = _2; _2 = s; }

                            if (CompareVertex.Position(vertices[_1], vertices[_2]))
                            {
                                if (remap.ContainsKey(_1)) remap[_1].Add(_2);
                                else remap[_1] = new List<int> { _2 };
                            }
                        }
                    }
                }

                for (int x = 0; x < seams.Length; x++)
                    for (int y = x + (self ? 0 : 1); y < seams.Length; y++)
                        CompareSeams(seams[x], seams[y]);

                // Remap ------
                // key  value
                // 10 - [40,50]
                // 40 - [60,70]
                // 50 - [60,70]

                // First ------
                // value  key
                // 10  -  10
                // 40  -  10
                // 50  -  10
                // 60  -  40
                // 70  -  40
                // 60  -  50
                // 70  -  50

                // result -----
                // value  key
                // 10  -  10
                // 40  -  10
                // 60  -  10
                // 70  -  10


                // value  key     value  key
                // 40  =  10,     50  =  10
                // 60  =  40,     70  =  40
                var indexRemap = Enumerable.Range(0, vertices.Length).Select(i => i).ToArray();
                foreach (var pair in remap.OrderBy(p => p.Key))
                    foreach (var value in pair.Value)
                        indexRemap[value] = indexRemap[pair.Key];

                // check for errors
                for (int i = 0; i < indexRemap.Length; i++)
                    if (indexRemap[indexRemap[i]] != indexRemap[i])
                        Debug.Log("Wrong " + i + " " + indexRemap[indexRemap[i]] + " " + indexRemap[i]);

                // value  key
                // 10  -  10
                // 40  -  10
                // 50  -  50
                // 60  -  50
                // 70  -  10
                var nvertices = mesh.vertices.ToList();
                var originalRemap = indexRemap.ToArray();
                for (int i = 0, o = 0, n = indexRemap.Length; i < n; o++, i++)
                {
                    if (originalRemap[i] != i)
                    {
                        nvertices.RemoveAt(o--);

                        //shuffle indices down
                        for (int j = 0; j < n; j++)
                            if (indexRemap[j] > o)
                                indexRemap[j]--;
                    }
                }

                for (int i = 0, n = indexRemap.Length; i < n; i++)
                {
                    if (indexRemap[i] >= nvertices.Count)
                    {
                        Debug.Log("OOB " + i + " to " + indexRemap[i] + "/" + nvertices.Count);
                    }
                }

                var result = new Mesh()
                {
                    name = mesh.name,
                    vertices = nvertices.ToArray(),
                    uv = new Vector2[nvertices.Count],
                    triangles = mesh.triangles.Select(i => indexRemap[i]).ToArray(),
                };

                result.RecalculateNormals();
                result.RecalculateTangents();
                result.RecalculateBounds();
                return result;
            }
        }

        public static class Measure
        {
            public static Bounds GetBounds(Mesh mesh, int[] indices)
            {
                var vertices = mesh.vertices;
                var bounds = new Bounds(vertices[indices.First()], Vector3.zero);
                indices.ForEach(i => bounds.Encapsulate(vertices[i]));
                return bounds;
            }
        }

        /// <summary>Identify the UV groups in a mesh</summary>
        public static class UVGroups
        {
            const int MaxGroups = 50;
            public static Mesh ConvertToSubmeshes(Mesh mesh)
            {
                var groups = GetGroups(mesh);
                Mesh result = CloneAsset(mesh);
                result.subMeshCount = groups.Length;

                Debug.Log("ConvertToSubmeshes: " + mesh.triangles.Length + "/" + groups.Select(g => g.Length).Sum());
                for (int i = 0; i < result.subMeshCount; i++)
                {
                    Debug.Log(i + ") " + groups[i].Length + " " + HashArray(groups[i]) + " \n" + String.Join(",", groups[i]));
                    result.SetTriangles(groups[i], i);
                }
                return result;
            }

            /// <summary>Get the indices groups of the UV Group</summary>
            internal static int[][] GetGroups(Mesh mesh) =>
               GetGroups(mesh.triangles);

            internal static int[][] GetGroups(int[] indices)
            {
                Dictionary<long, List<Triangle>> edgeMap = GetTriangles(indices, out List<Triangle> triangles);
                List<Triangle> group = new List<Triangle>();
                List<int[]> groups = new List<int[]>();

                void Fill(Triangle triangle)
                {
                    triangle.filled = true;
                    group.Add(triangle);
                    foreach (var edge in triangle.edges) foreach (var t in edgeMap[edge])
                            if (!t.filled) Fill(t);
                }

                foreach (var t in triangles)
                {
                    if (t.filled) continue;
                    Fill(t);
                    if (group.Count > 0)
                    {
                        if (groups.Count > MaxGroups) throw new Exception("Too many UV Groups!");
                        groups.Add(group.SelectMany(t => t.indices).ToArray());
                        group.Clear();
                    }
                }
                return groups.ToArray();
            }

            static Dictionary<long, List<Triangle>> GetTriangles(int[] indices, out List<Triangle> triangles)
            {
                triangles = new List<Triangle>();
                Dictionary<long, List<Triangle>> edgeMap = new Dictionary<long, List<Triangle>>();

                for (int i = 0; i < indices.Length; i += 3)
                {
                    var t = new Triangle(indices[i + 0], indices[i + 1], indices[i + 2]);
                    triangles.Add(t);
                    void Add(long e) { if (edgeMap.ContainsKey(e)) edgeMap[e].Add(t); else edgeMap[e] = new List<Triangle>() { t }; }
                    t.edges.ForEach(e => Add(e));
                }

                var edges = triangles.SelectMany(t => t.edges).ToArray();
                var distinctEdges = edges.Distinct().ToArray();
                var distinctEdgesHEX = edges.Distinct().Select(e => e.ToString("X16")).ToArray();
                var distinctIndices = indices.Distinct().ToArray();

                return edgeMap;
            }

            class Triangle
            {
                public readonly int[] indices;
                public readonly long[] edges;
                public bool filled;

                public Triangle(params int[] indices)
                {
                    this.indices = indices;
                    this.edges = new[] { Edge.GetID(indices[0], indices[1]), Edge.GetID(indices[1], indices[2]), Edge.GetID(indices[2], indices[0]) };
                }
                public static implicit operator bool(Triangle empty) => empty != null;
            }
        }

        public static class Edge
        {
            const int Bits = 32;
            const long Mask = 0x00000000ffffffff;
            static long ID(int _0, int _1) => ((long)_0 << Bits) | ((long)_1 & Mask);
            public static long GetID(int _0, int _1) => _0 > _1 ? ID(_0, _1) : ID(_1, _0);
            public static int[] GetIndices(long edgeID) => new[] { (int)(edgeID & Mask), (int)(edgeID >> Bits) };
        }

        public static string Report(this Mesh mesh)
        {
            string output = "";

            return output;
        }


        public static Blendshape[] GetBlendshapes(Mesh mesh)
        {
            Blendshape[] blendshapes = new Blendshape[mesh.blendShapeCount];
            for (int b = 0; b < mesh.blendShapeCount; b++)
            {
                blendshapes[b] = new Blendshape { name = mesh.GetBlendShapeName(b) };
                var name = mesh.GetBlendShapeName(b);
                for (int f = 0, fc = mesh.GetBlendShapeFrameCount(b); f < fc; f++)
                {
                    var frame = new Blendshape.Frame(mesh.vertexCount) { weight = mesh.GetBlendShapeFrameWeight(b, f) };
                    blendshapes[b].frames.Add(frame);
                    mesh.GetBlendShapeFrameVertices(b, f, frame.dv, frame.dn, frame.dt);
                }
            }
            return blendshapes;
        }

        [Serializable]
        public class Blendshape
        {
            public class Frame
            {
                public Vector3[] dv, dn, dt;
                public float weight;
                public Frame(int count)
                {
                    dv = new Vector3[count];
                    dn = new Vector3[count];
                    dt = new Vector3[count];
                }
            }
            public string name;
            public List<Frame> frames = new List<Frame>();
        }

    }
}
