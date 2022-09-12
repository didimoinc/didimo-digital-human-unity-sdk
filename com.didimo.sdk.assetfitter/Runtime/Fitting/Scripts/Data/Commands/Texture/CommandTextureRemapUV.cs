using System;
using System.Collections;
using System.Collections.Generic;
using Didimo.Core.Inspector;
using UnityEngine;
using static UnityEngine.Object;
using System.Linq;
using Didimo.Extensions;
using System.ComponentModel;
using System.Reflection;

namespace Didimo.AssetFitter.Editor.Graph
{
    // [StyleSheet("DefaultNode")]
    [System.Serializable]
    [MenuPath("Texture/Texture Remap UV")]
    [DisplayName("Remap UV")]
    [Width(160)]
    [HeaderColor(TypeColors.Texture)]
    public class CommandTextureRemapUV : GraphNode
    {
        [Input("Texture")] public Texture textureInput;
        [Input("Mesh1", true)] public Mesh mesh1Input;
        [Input("Mesh2", true)] public Mesh mesh2Input;
        [Expose] public Size size = Size._2048;
        [Expose] public RenderType type = RenderType.Opaque;
        [Output("Texture")] public Texture textureOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(textureOutput))
            {
                values = RenderRemapUV(
                    GetInputValue<Mesh>(nameof(mesh1Input)),
                    GetInputValue<Mesh>(nameof(mesh2Input)),
                    GetInputValues<Texture2D>(nameof(textureInput)),
                    type).ToList<object>();
                return true;
            }
            values = null;
            return false;
        }

        List<Texture2D> RenderRemapUV(Mesh mesh1, Mesh mesh2, List<Texture2D> textures, RenderType renderType)
        {
            List<Texture2D> value = new List<Texture2D>();
            if (mesh1 && mesh2)
            {
                string getShaderPath() => renderType == RenderType.Transparent ? "Unlit/Transparent" : "Unlit/Texture";
                Material material = new Material(Shader.Find(getShaderPath()));

                using (var remapper = new UVRemapper())
                {
                    remapper.ComputerUV(mesh1, mesh2);
                    foreach (var texture in textures)
                    {
                        using (var drawer = new RenderTextureSurface(texture.width, texture.height))
                        {
                            string path = Application.dataPath + "/temp/output1.png";
                            material.SetTexture("_MainTex", texture);
                            drawer.RenderTextureRemapped(remapper.indices, remapper.validIndices, remapper.uv1, remapper.uv2, material);
                            value.Add(drawer.GetTexture(texture.name + " (remap)"));
                        }
                    }
                }
            }
            return value;
        }

        class UVRemapper : IDisposable
        {
            MeshCollider collider;
            public Vector2[] uv1, uv2;
            public bool[] validIndices;
            public int[] indices;

            public UVRemapper()
            {
                collider = new GameObject("Collider Object").AddComponent<MeshCollider>();
            }

            public void ComputerUV(Mesh mesh1, Mesh mesh2, float threshold = 0.001f)
            {
                collider.sharedMesh = mesh2;

                var hits = GetRays(mesh1, threshold).Select((r, i) =>
                    new { ret = collider.Raycast(r, out RaycastHit h, threshold * 2), index = i, hit = h });

                var validHits = hits.Where(h => h.ret);

                validIndices = hits.Select(h => h.ret).ToArray();
                indices = mesh1.triangles;
                uv1 = mesh1.uv;
                uv2 = uv1.ToArray();

                validHits.ForEach(h => uv2[h.index] = h.hit.textureCoord);

                Debug.Log("Output:\n" + String.Join("\n", validHits.Select(h => h.ret + " " + h.hit.point + " " + h.hit.textureCoord2 + " " + uv1[h.index] + " " + h.index)));


                // Debug.Log("Hit count: " + validHits.Count());
                // Debug.Log("validIndices: " + validIndices.Length);
                // Debug.Log("indices: " + indices.Length);
                // Debug.Log("uv1: " + uv1.Length);
            }

            public void Dispose()
            {
                DestroyImmediate(collider.gameObject);
            }

            IEnumerable<Ray> GetRays(Mesh mesh, float threshold = 0.001f)
            {
                Vector3[] vertices = mesh.vertices, normals = mesh.normals;
                for (int i = 0; i < vertices.Length; i++)
                    yield return new Ray(vertices[i] + normals[i] * threshold, -normals[i]);
            }
        }

        class RenderTextureSurface : IDisposable
        {
            RenderTexture backActiveRT;
            Vector2Int size;

            public RenderTextureSurface(int width, int height)
            {
                this.size = new Vector2Int(width, height);

                backActiveRT = RenderTexture.active;

                RenderTexture rt = new RenderTexture(size.x, size.y, 0, RenderTextureFormat.ARGB32);
                RenderTexture.active = rt;

                GL.PushMatrix();
                GL.LoadPixelMatrix(0, size.x, 0, size.y);
            }

            public void RenderTextureRemapped(int[] indices, bool[] validIndices, Vector2[] uvs1, Vector2[] uvs2, Material material)
            {
                GL.Clear(true, true, Color.clear);

                material.SetPass(0);

                if (!(validIndices.Length == uvs1.Length && validIndices.Length == uvs2.Length))
                {
                    throw new Exception("UV1 && UV2 && validIndices should be the same length: " +
                        validIndices.Length + "," + validIndices.Length + "," + uvs2.Length);
                }

                int GetIndex(int i)
                {
                    if (i >= indices.Length)
                        throw new Exception("Triangle Index is out of range: " + i + " >= " + indices.Length);
                    int index = indices[i];
                    if (index >= validIndices.Length)
                        throw new Exception("Vertex Index is out of range: " + index + " >= " + validIndices.Length);
                    return validIndices[index] ? index : -1;
                }
                void GLUV(int i)
                {
                    Vector2 uv1 = uvs1[i], uv2 = uvs2[i];
                    GL.TexCoord2(uv1.x, uv1.y);
                    GL.Vertex3(uv2.x * size.x, uv2.y * size.y, 0);
                }

                for (int i = 0; i < indices.Length; i += 3)
                {
                    int i0 = GetIndex(i + 0), i1 = GetIndex(i + 1), i2 = GetIndex(i + 2);
                    if (i0 >= 0 && i1 >= 0 && i2 >= 0)
                    {
                        GL.Begin(GL.TRIANGLES);
                        GLUV(i0);
                        GLUV(i1);
                        GLUV(i2);
                        GL.End();
                    }
                }
            }

            public Texture2D GetTexture(string name)
            {
                Texture2D texture2D = new Texture2D(size.x, size.y) { name = name };
                texture2D.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
                texture2D.Apply();
                return texture2D;
            }

            public void Dispose()
            {
                GL.PopMatrix();
                RenderTexture.active = backActiveRT;
            }
        }

        public enum Size
        {
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
        }

        public enum RenderType
        {
            Opaque = 0,
            Transparent = 1,
        }

    }
}
