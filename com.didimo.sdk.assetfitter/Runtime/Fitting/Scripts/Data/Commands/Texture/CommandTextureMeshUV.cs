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
    [MenuPath("Texture/Texture Draw Mesh UVs")]
    [DisplayName("Draw Mesh UV")]
    [Width(160)]
    [HeaderColor(TypeColors.Texture)]
    public class CommandTextureMeshUV : GraphNode
    {
        [Input("Mesh")] public Mesh meshInput;
        [Expose] public Size size = Size._2048;
        [Expose] public ColorScheme color = ColorScheme.WhiteOnBlack;
        [Expose] public RenderType type = RenderType.MeshesToTextures;
        [Output("Texture")] public Texture textureOutput;

        protected override bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            if (info.Name == nameof(textureOutput))
            {
                values = RenderMeshUV(GetInputValues<Mesh>(nameof(meshInput))).ToList<object>();
                return true;
            }
            values = null;
            return false;
        }

        List<Texture2D> RenderMeshUV(List<Mesh> meshes)
        {
            List<Texture2D> textures = new List<Texture2D>();
            if (meshes.Count > 0)
            {
                switch (type)
                {
                    case RenderType.MeshesToTexture:
                        using (RenderTextureSurface rts = new RenderTextureSurface((int)size, color))
                        {
                            foreach (var mesh in meshes)
                                rts.RenderMesh(mesh);
                            textures.Add(rts.GetTexture(meshes.First().name + "-combined"));
                        }
                        break;

                    case RenderType.MeshesToTextures:
                        foreach (var mesh in meshes)
                        {
                            using (RenderTextureSurface rts = new RenderTextureSurface((int)size, color))
                            {
                                rts.RenderMesh(mesh);
                                textures.Add(rts.GetTexture(mesh.name));
                            }
                        }
                        break;
                }
            }
            return textures;
        }

        class RenderTextureSurface : IDisposable
        {
            Material material;
            RenderTexture backActiveRT;
            int size;

            public RenderTextureSurface(int size, ColorScheme color)
            {
                backActiveRT = RenderTexture.active;
                RenderTexture rt = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32);
                RenderTexture.active = rt;
                this.size = size;

                Color fg, bg;
                switch (color)
                {
                    default:
                    case ColorScheme.WhiteOnBlack: fg = Color.white; bg = Color.black; break;
                    case ColorScheme.BlackOnWhite: fg = Color.black; bg = Color.white; break;
                }

                GL.Clear(true, true, bg);

                material = new Material(Shader.Find("GUI/Text Shader"))
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    shader = { hideFlags = HideFlags.HideAndDontSave },
                };

                material.SetPass(0);
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, size, 0, size);
                GL.Color(fg);
            }

            public void RenderMesh(Mesh mesh)
            {
                int[] indices = mesh.triangles;
                Vector2[] uv = mesh.uv;

                void GLUV(int i)
                {
                    Vector2 p = uv[indices[i]];
                    GL.Vertex3(p.x * size, p.y * size, 0);
                }

                for (int i = 0; i < indices.Length; i += 3)
                {
                    GL.Begin(GL.LINE_STRIP);
                    GLUV(i + 0);
                    GLUV(i + 1);
                    GLUV(i + 2);
                    GLUV(i + 0);
                    GL.End();
                }
            }

            public Texture2D GetTexture(string name)
            {
                Texture2D texture2D = new Texture2D(size, size) { name = name };
                texture2D.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                texture2D.Apply();
                return texture2D;
            }

            public void Dispose()
            {
                GL.PopMatrix();
                RenderTexture.active = backActiveRT;
            }
        }





        // Texture2D RenderUVLines(Mesh mesh)
        // {

        //     // int s = (int)size;
        //     // int[] indices = mesh.triangles;
        //     // Vector2[] uv = mesh.uv;

        //     // RenderTexture rt = new RenderTexture(s, s, 0, RenderTextureFormat.ARGB32);
        //     // RenderTexture.active = rt;

        //     // Color fg, bg;
        //     // switch (color)
        //     // {
        //     //     default:
        //     //     case ColorScheme.WhiteOnBlack:
        //     //         fg = Color.white;
        //     //         bg = Color.black;
        //     //         break;
        //     //     case ColorScheme.BlackOnWhite:
        //     //         fg = Color.black;
        //     //         bg = Color.white;
        //     //         break;
        //     // }

        //     // GL.Clear(true, true, bg);

        //     // material.SetPass(0);
        //     // GL.PushMatrix();
        //     // GL.LoadPixelMatrix(0, s, 0, s);
        //     GL.Color(fg);

        //     void GLUV(int i)
        //     {
        //         var p = uv[indices[i]];
        //         GL.Vertex3(p.x * s, p.y * s, 0);
        //     }

        //     for (int i = 0; i < indices.Length; i += 3)
        //     {
        //         GL.Begin(GL.LINE_STRIP);
        //         GLUV(i + 0);
        //         GLUV(i + 1);
        //         GLUV(i + 2);
        //         GLUV(i + 0);
        //         GL.End();
        //     }

        //     GL.PopMatrix();

        //     // Texture2D texture2D = new Texture2D(s, s) { name = mesh.name };
        //     // texture2D.ReadPixels(new Rect(0, 0, s, s), 0, 0);
        //     // texture2D.Apply();

        //     // RenderTexture.active = active;

        //     return texture2D;
        // }

        public enum Size
        {
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
        }

        public enum ColorScheme
        {
            WhiteOnBlack = 0,
            BlackOnWhite = 1,
        }

        public enum RenderType
        {
            MeshesToTextures = 0,
            MeshesToTexture = 1,
        }

    }
}
