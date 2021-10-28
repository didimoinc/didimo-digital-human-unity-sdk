using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Didimo;
using UnityEngine;

public class ObjDeformationUtility : DeformationUtility
{
    // stored in pipeline coordinates
    private List<Vector3>    vertices;
    private List<Vector3>    normals;
    private List<Vector2>    uvs;
    private List<Vector3Int> faces;

    public override List<Vector3> GetVertices() => PipelineToUnity(vertices);


    public override void SetVertices(List<Vector3> newVertices)
    {
        if (newVertices.Count != vertices.Count) throw new ArgumentException($"Provided list of vertices doesn't match already existing list size.");

        vertices = UnityToPipeline(newVertices);
    }

    public override byte[] Serialize()
    {
        CultureInfo cultureInfo = CultureInfo.InvariantCulture;
        StringBuilder stringBuilder = new StringBuilder();

        foreach (Vector3 vertex in vertices)
        {
            stringBuilder.Append($"v {vertex[0].ToString(cultureInfo)} {vertex[1].ToString(cultureInfo)} {vertex[2].ToString(cultureInfo)}\n");
        }

        foreach (Vector3 normal in normals)
        {
            stringBuilder.Append($"vn {normal[0].ToString(cultureInfo)} {normal[1].ToString(cultureInfo)} {normal[2].ToString(cultureInfo)}\n");
        }

        foreach (Vector2 uv in uvs)
        {
            stringBuilder.Append($"vt {uv[0].ToString(cultureInfo)} {uv[1].ToString(cultureInfo)}\n");
        }

        foreach (Vector3Int face in faces)
        {
            int vertexIdX = face[0] + 1;
            int vertexIdY = face[1] + 1;
            int vertexIdZ = face[2] + 1;
            stringBuilder.Append($"f {vertexIdX}/{vertexIdX}/{vertexIdX} {vertexIdY}/{vertexIdY}/{vertexIdY} {vertexIdZ}/{vertexIdZ}/{vertexIdZ}\n");
        }

        return Encoding.ASCII.GetBytes(stringBuilder.ToString());
    }

    public void DeserializeFromText(string objText)
    {
        vertices = new List<Vector3>();
        normals = new List<Vector3>();
        uvs = new List<Vector2>();
        faces = new List<Vector3Int>();

        StringReader file = new StringReader(objText);
        string line;
        while ((line = file.ReadLine()) != null)
        {
            string[] lineSplit = line.Split();
            string token = lineSplit[0];
            switch (token)
            {
                case "v":
                    vertices.Add(ParseVector3(lineSplit));
                    break;

                case "vn":
                    normals.Add(ParseVector3(lineSplit));
                    break;

                case "vt":
                    uvs.Add(ParseVector2(lineSplit));
                    break;

                case "f":
                    faces.Add(ParseFaceIndex(lineSplit));
                    break;
            }
        }
    }

    public override void Deserialize(byte[] data) { DeserializeFromText(Encoding.ASCII.GetString(data)); }

    private static Vector3Int ParseFaceIndex(string[] lineSplit)
    {
        return new Vector3Int(int.Parse(lineSplit[3].Split('/')[0], CultureInfo.InvariantCulture) - 1,
            int.Parse(lineSplit[2].Split('/')[0], CultureInfo.InvariantCulture) - 1,
            int.Parse(lineSplit[1].Split('/')[0], CultureInfo.InvariantCulture) - 1);
    }

    private static Vector3 ParseVector3(string[] lineSplit)
    {
        return new Vector3(float.Parse(lineSplit[1], CultureInfo.InvariantCulture),
            float.Parse(lineSplit[2], CultureInfo.InvariantCulture),
            float.Parse(lineSplit[3], CultureInfo.InvariantCulture));
    }

    private static Vector2 ParseVector2(string[] lineSplit)
    {
        return new Vector2(float.Parse(lineSplit[1], CultureInfo.InvariantCulture), float.Parse(lineSplit[2], CultureInfo.InvariantCulture));
    }

    public override void ApplyToMeshFilters(MeshFilter[] meshFilters)
    {
        int vertIndexOffset = 0;
        int faceIndexOffset = 0;
        foreach (MeshFilter meshFilter in meshFilters)
        {
            // Vertices
            Vector3[] meshFilterVertices = new Vector3[meshFilter.mesh.vertexCount];
            for (int i = 0; i < meshFilter.mesh.vertexCount; i++)
            {
                meshFilterVertices[i] = PipelineToUnity(vertices[i + vertIndexOffset]);
            }

            meshFilter.mesh.vertices = meshFilterVertices;

            // normals
            Vector3[] meshFilterNormals = new Vector3[meshFilterVertices.Length];
            for (int i = 0; i < meshFilter.mesh.vertexCount; i++)
            {
                meshFilterNormals[i] = PipelineToUnity(normals[i + vertIndexOffset], false);
            }

            meshFilter.mesh.normals = meshFilterNormals;

            // uvs
            Vector2[] meshFilterUvs = new Vector2[meshFilterVertices.Length];
            for (int i = 0; i < meshFilter.mesh.vertexCount; i++)
            {
                meshFilterUvs[i] = uvs[i + vertIndexOffset];
            }

            meshFilter.mesh.uv = meshFilterUvs;

            vertIndexOffset += meshFilterVertices.Length;

            // faces
            for (int i = 0; i < meshFilter.mesh.subMeshCount; i++)
            {
                int indexCount = meshFilter.mesh.GetSubMesh(i).indexCount;
                int[] indices = new int[indexCount];
                for (int j = 0; j < indexCount / 3; j++)
                {
                    for (int faceIndex = 0; faceIndex < 3; faceIndex++)
                    {
                        try
                        {
                            indices[(j * 3) + faceIndex] = faces[faceIndexOffset + j][faceIndex];
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                }

                faceIndexOffset += indexCount / 3;
            }
        }
    }

    public override void FromMeshFilters(MeshFilter[] meshFilters, bool shared = true)
    {
        vertices = new List<Vector3>();
        normals = new List<Vector3>();
        uvs = new List<Vector2>();
        faces = new List<Vector3Int>();
        int indexOffset = 0;

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter == null || meshFilter.sharedMesh == null) continue;

            Mesh mesh = shared ? meshFilter.sharedMesh : meshFilter.mesh;
            {
                Vector3[] vertexList = mesh.vertices;
                for (int i = 0; i < vertexList.Length; i++)
                {
                    vertices.Add(UnityToPipeline(vertexList[i]));
                }
            }

            {
                Vector3[] normalsList = mesh.normals;
                for (int i = 0; i < normalsList.Length; i++)
                {
                    normals.Add(UnityToPipeline(normalsList[i], false));
                }
            }

            uvs.AddRange(new List<Vector2>(mesh.uv));

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] indices = mesh.GetIndices(i);

                if (indices.Length % 3 != 0)
                {
                    Debug.LogError("Number of vertices of mesh must be multiple of 3.");
                    return;
                }

                for (int j = 0; j < indices.Length / 3; j++)
                {
                    faces.Add(new Vector3Int(indices[(j * 3) + 2] + indexOffset, indices[(j * 3) + 1] + indexOffset, indices[j * 3] + indexOffset));
                }
            }

            indexOffset += mesh.vertexCount;
        }
    }
}