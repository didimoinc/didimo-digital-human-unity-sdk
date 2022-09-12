using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

namespace Didimo.Core.Utility
{
    public static class TextureUtility
    {
        public const int PROC_CREATE_EVEN_IF_EXISTS = 0x1;
        public const int PROC_LINEAR_COLOURSPACE = 0x2;
        public const int PROC_LOAD_ORIGINAL_IMAGE = 0x4;
        public const int PROC_CREATE_IF_NOT_PRESENT = 0x8;
        public const int PROC_STAMP_IMAGES = 0x10;
        public const int MODE_LINEAR_TO_SRGB = 0x1;
        public const int MODE_SRGB_TO_LINEAR = 0x2;
        public const int MODE_RG_NORMAL_TO_XYZ = 0x4;
        public static Texture OpacityToDiffuseAlpha(Texture texture) => EncodeToAlpha(texture, Color.white);
        public static Texture RoughnessToMetallicAlpha(Texture texture) => EncodeToAlpha(texture, Color.black, true);

        public static Texture EncodeToAlpha(Texture input, Color rgb, bool invert = false)
        {
            RenderTexture output = new RenderTexture(
                input.width, input.height, 32,
                RenderTextureFormat.ARGB32, input.mipmapCount);

            Material mat = new Material(Shader.Find("Hidden/Didimo/Utility/EncodeToAlpha"));
            mat.SetColor("_Base", rgb);
            mat.SetInt("_Invert", invert ? 1 : 0);
            Graphics.Blit(input, output, mat);

            return output;
        }

        //Similar to Graphics.Blit but with more control over position, size and material
        public static void Blit2(Texture source, RenderTexture dest, Vector2 pos, Vector2 dim, Material mat, int pass = 0, bool executeImmediately = true)
        {
            var original = RenderTexture.active;

            RenderTexture.active = dest;
            string[] texpropnames = mat.GetTexturePropertyNames();
            if (mat.HasProperty("_MainTex"))
                mat.SetTexture("_MainTex", source);
            else
                Debug.Log("Material does not have property _MainTex");
            if (mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", source);
            else
                Debug.Log("Material does not have property _BaseMap");
            if (mat.HasProperty("_SRGBWrite"))
                mat.SetInt("_SRGBWrite", 1);
            else
                Debug.Log("Material does not have property _SRGBWrite");
            GL.PushMatrix();
            GL.LoadOrtho();

            mat.SetPass(pass);
            // draw a quad over whole screen
            float idw = 1.0f / dest.width;
            float idh = 1.0f / dest.height;
            float x1 = pos.x * idw;
            float y1 = pos.y * idh;
            float x2 = (pos.x + dim.x) * idw;
            float y2 = (pos.y + dim.y) * idh;
            GL.Begin(GL.QUADS);
            GL.TexCoord2(0f, 0f); GL.Vertex3(x1, y1, 0f);    /* note the order! */
            GL.TexCoord2(0f, 1f); GL.Vertex3(x1, y2, 0f);    /* also, we need TexCoord2 */
            GL.TexCoord2(1f, 1f); GL.Vertex3(x2, y2, 0f);
            GL.TexCoord2(1f, 0f); GL.Vertex3(x2, y1, 0f);
            GL.End();
            GL.PopMatrix();
            if (executeImmediately)
                GL.Flush();
            RenderTexture.active = original;    /* restore */
        }

        public static bool DrawGrid(Texture2D[] sourceTextures, RenderTexture rt, Vector2 cellSizef, Vector2Int CellCount, Material mat)
        {
            Vector2 cpos = new Vector2(0, 0);
            int ci = 0;
            for (int y = 0; y < CellCount.y; ++y)
            {
                cpos.x = 0;
                for (int x = 0; x < CellCount.x; ++x)
                {
                    if (ci >= sourceTextures.Length)
                        return false;
                    var sourceTex = sourceTextures[ci];
                    Blit2(sourceTex, rt, cpos, cellSizef, mat);
                    cpos.x += cellSizef.x;
                    ++ci;
                }
                cpos.y += cellSizef.y;
            }
            return true;
        }

        public static Texture2D CreateTextureAtlas(Texture2D[] sourceTextures, Vector2Int atlasSize, Vector2Int cellCount, int processFlags, int modeFlags)
        {
            Vector2Int cellSize = new Vector2Int(atlasSize.x / cellCount.x, atlasSize.y / cellCount.y);
            RenderTexture mergedTextureRT = RenderTexture.GetTemporary(
                     atlasSize.x,
                     atlasSize.y,
                     0,
                     RenderTextureFormat.Default,
                     RenderTextureReadWrite.Linear);

            Vector2 cellSizef = new Vector2(cellSize.x, cellSize.y);

            var shader = Shader.Find("Blitter/BlitAlpha");
            if (!shader)
                Debug.Log("Failed to find shader 'Blitter', cannot perform texture atlasing");
            Material mat = new Material(shader);
            mat.SetColor("_Color", new Color(1, 1, 1, 1));
            mat.SetInt("_ModeFlags", modeFlags);
            DrawGrid(sourceTextures, mergedTextureRT, cellSizef, cellCount, mat);
            bool linear = ((processFlags & PROC_LINEAR_COLOURSPACE) != 0);
            Texture2D mergedTexture = new Texture2D(atlasSize.x, atlasSize.y, TextureFormat.RGBA32, false, linear);
            Graphics.CopyTexture(mergedTextureRT, 0, mergedTexture, 0);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = mergedTextureRT;

            mergedTexture.ReadPixels(new Rect(0, 0, mergedTexture.width, mergedTexture.height), 0, 0); //this is needed to copy the GPU side data that we've written with 'Blit2' over to the CPU, where it's needed for any 'EncodeToPNG' calls
            RenderTexture.active = prev;

            if ((processFlags & PROC_STAMP_IMAGES) != 0)
            {
                int cidx = 0;
                Vector2Int offset = new Vector2Int(480, 750);
                for (int y = 0; y < cellCount.y; ++y)
                    for (int x = 0; x < cellCount.x; ++x)
                    {
                        RenderToTexture.DrawBitFontString(mergedTexture, RenderToTexture.bitfont, x * cellSize.x + offset.x, y * cellSize.y + offset.y, "Tex #" + cidx.ToString(), Color.black);
                        cidx++;
                    }
            }
            return mergedTexture;
        }

        public static Texture2D CreateMergedTextureFromMaterial(Material sourceMaterial, string[] propertyNames)
        {
            Texture2D[] sourceTextures = new Texture2D[propertyNames.Length];
            Vector2Int minSize = new Vector2Int(int.MaxValue, int.MaxValue), maxSize = new Vector2Int(0, 0);
            for (int i = 0; i < propertyNames.Length; ++i)
            {
                var propName = propertyNames[i];
                if (sourceMaterial.HasProperty(propName))
                {
                    try
                    {
                        if (sourceMaterial.HasProperty(propName))
                        {
                            var sourceTexture = (Texture2D)sourceMaterial.GetTexture(propName);

                            if (sourceTexture)
                            {
                                minSize.x = Math.Min(sourceTexture.width, minSize.x);
                                minSize.y = Math.Min(sourceTexture.height, minSize.y);
                                maxSize.x = Math.Max(sourceTexture.width, maxSize.x);
                                maxSize.y = Math.Max(sourceTexture.height, maxSize.y);
                            }
                            sourceTextures[i] = sourceTexture;
                        }
                        else
                            Debug.Log("Failed to find texture property on material: " + propName);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Error creating merged texture: " + e.Message);
                    }
                }
            }

            //Now make readable copies, downscale if necessary
            for (int i = 0; i < propertyNames.Length; ++i)
            {
                if (sourceTextures[i])
                {
                    var sourceTextureSrc = sourceTextures[i];

                    RenderTexture sourceTexture = RenderTexture.GetTemporary(
                           minSize.x,
                           minSize.y,
                           0,
                           RenderTextureFormat.Default,
                           RenderTextureReadWrite.Linear);
                    Graphics.Blit(sourceTextureSrc, sourceTexture); //blit it over
                    RenderTexture prev = RenderTexture.active;

                    RenderTexture.active = sourceTexture;

                    Texture2D readableTexture = new Texture2D(minSize.x, minSize.y);
                    readableTexture.ReadPixels(new Rect(0, 0, minSize.x, minSize.y), 0, 0);
                    readableTexture.Apply();
                    sourceTextures[i] = readableTexture; //reassign readable
                    RenderTexture.active = prev;
                    RenderTexture.ReleaseTemporary(sourceTexture);
                }
            }

            Texture2D mergedTexture = new Texture2D(minSize.x, minSize.y, TextureFormat.ARGB32, false, true);  //merged texture ought to be linear     
            Color outPix = new Color();
            NativeArray<int>[] sourceTextureData = new NativeArray<int>[propertyNames.Length];
            int pixCount = minSize.x * minSize.y;
            for (int i = 0; i < propertyNames.Length; ++i)
            {
                if (sourceTextures[i])
                    sourceTextureData[i] = sourceTextures[i].GetPixelData<int>(0);
                else
                {
                    sourceTextureData[i] = new NativeArray<int>(pixCount, Allocator.Temp, NativeArrayOptions.ClearMemory);
                    if (i == 3) //if we're in an alpha channel, set to 255
                    {
                        for (int px = 0; px < pixCount; ++px)
                            sourceTextureData[i][px] = 255;
                    }
                }
            }

            for (int i = 0; i < pixCount; ++i)
            {
                outPix.r = (sourceTextureData[0][i] & 0xFF) / 255.0f;
                outPix.g = (sourceTextureData[1][i] & 0xFF) / 255.0f;
                outPix.b = (sourceTextureData[2][i] & 0xFF) / 255.0f;
                outPix.a = (sourceTextureData[3][i] & 0xFF) / 255.0f;
                mergedTexture.SetPixel(i % minSize.x, i / minSize.x, outPix);
            }

            for (int i = 0; i < propertyNames.Length; ++i)
            {
                sourceTextureData[i].Dispose();
            }

            return mergedTexture;
        }
    }
}