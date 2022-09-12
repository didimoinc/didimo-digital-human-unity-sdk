using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEditor.Presets;

namespace Didimo.Core.Utility
{
    public static class EditorTextureUtility
    {       
        public static Texture2D SerialiseTexture(Texture2D texture, string fileName, int flags = 0)
        {
            Debug.Log("Saving texture file to '" + fileName + "'");
            fileName = IOUtility.NormalizePath(fileName);
            
            byte[] itemBGBytes = texture.EncodeToPNG();
            TextureImporter importer = null;
                     
            FileStream file = File.Create(fileName);

            BinaryWriter binary = new BinaryWriter(file);
            binary.Write(itemBGBytes);
            file.Close();

            if (flags != 0)
            {
                importer = (TextureImporter)TextureImporter.GetAtPath(fileName);
                if (importer)
                {
                    importer.sRGBTexture = ((flags & (int)TextureUtility.PROC_LINEAR_COLOURSPACE) == 0);
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
                else
                    Debug.Log("Importer object for '" + fileName + "' could not be generated");
                AssetDatabase.ImportAsset(fileName, ImportAssetOptions.ForceUpdate);
            }

            Texture2D retTex = retTex = (Texture2D)AssetDatabase.LoadAssetAtPath(fileName, typeof(Texture2D));
            if (importer != null)
                AssetDatabase.ImportAsset(fileName, ImportAssetOptions.ForceUpdate);
            if (!retTex)                           
                retTex = (Texture2D)AssetDatabase.LoadAssetAtPath(fileName, typeof(Texture2D));
            return retTex;
        }

        public static Texture2D CreateSerialisedMergedTextureFromMaterial(Material sourceMaterial, string textureFileName, string[] propertyNames, int processFlags)
        {
            var existingTexture = HandleExistingTexture(textureFileName, processFlags);
            if (existingTexture != null)
                return existingTexture;
            Debug.Log("Creating merged texture: " + textureFileName);
            Texture2D mergedTexture = TextureUtility.CreateMergedTextureFromMaterial(sourceMaterial, propertyNames);
            return SerialiseTexture(mergedTexture, textureFileName, processFlags);
        }

        static Texture2D HandleExistingTexture(string textureFileName, int processFlags)
        {
            if (File.Exists(textureFileName))
            {
                TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(textureFileName);
                bool recreate = (processFlags & TextureUtility.PROC_CREATE_EVEN_IF_EXISTS) != 0;
                if (importer.sRGBTexture && ((processFlags & TextureUtility.PROC_LINEAR_COLOURSPACE) != 0))
                    recreate = true;                

                if (recreate)
                {
                    File.Delete(textureFileName);
                }
                else
                {
                    return (Texture2D)AssetDatabase.LoadAssetAtPath(textureFileName, typeof(Texture2D));
                }
            }
            else
            {
                var dir = Directory.GetParent(textureFileName).FullName;
                Directory.CreateDirectory(dir);                
            }
            return null;
        }

        public static Texture2D CreateSerialisedTextureAtlas(Material[] sourceMaterials, string propertyName, Vector2Int atlasSize, Vector2Int cellCount, string textureFileName, int processFlags, int modeFlags)
        {
            List<Texture2D> textureList = new List<Texture2D>();
            for (int i = 0; i < sourceMaterials.Length; ++i)
            {
                if (sourceMaterials[i])
                {
                    try
                    {
                        if (sourceMaterials[i].HasProperty(propertyName))
                        {
                            Texture2D tex = sourceMaterials[i].GetTexture(propertyName) as Texture2D;
                            if ((processFlags & TextureUtility.PROC_LOAD_ORIGINAL_IMAGE) != 0)
                            {

                                var texpath = AssetDatabase.GetAssetPath(tex);
                                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texpath);

                            }
                            textureList.Add(tex);
                        }
                        else
                            Debug.Log("Texture property not found! '" + propertyName + "' on '" + sourceMaterials[i].name);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }

                }
            }
            return CreateSerialisedTextureAtlas(textureList.ToArray(), atlasSize, cellCount, textureFileName, processFlags, modeFlags);
        }

        public static Texture2D CreateSerialisedTextureAtlas(Texture2D[] sourceTextures, Vector2Int atlasSize, Vector2Int cellCount, string textureFileName, int processFlags, int modeFlags)
        {
            Texture2D existingTexture = HandleExistingTexture(textureFileName, processFlags);
            if (existingTexture)
                return existingTexture;
            var mergedTexture = TextureUtility.CreateTextureAtlas(sourceTextures, atlasSize, cellCount, processFlags, modeFlags);
            return SerialiseTexture(mergedTexture, textureFileName, 0);
        }

        public static Texture FindTextureFile(string[] fileList, string[] prefixes, string failLog)
        {
            foreach (var textureFile in fileList)
            {
                var ctextureFile = textureFile;
                if (textureFile.ToLower().Contains("assets"))
                {
                    var textureDir = Path.GetDirectoryName(textureFile);
                    prefixes = new List<string>(prefixes).Prepend(textureDir).ToArray();
                    ctextureFile = Path.GetFileName(textureFile);
                }
                foreach (var prefix in prefixes)
                {
                    if (ctextureFile.StartsWith("*")) //maybe do a more sophisticated regex or something here
                    {
                        if (prefix.Length > 0)
                        {
                            try
                            {
                                var files = Directory.GetFiles(prefix);
                                var pattern = ctextureFile.Substring(1).ToLower();
                                foreach (var file in files)
                                {
                                    if (file.ToLower().Contains(pattern))
                                    {
                                        //var texturePathName = Path.Join(prefix, file);
                                        var texturePathName = IOUtility.NormalizePath(file);

                                        Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(texturePathName);
                                        if (tex == null)
                                            failLog += ((failLog.Length != 0) ? ", \n" : "") + "    '" + texturePathName + "'";
                                        else
                                        {
                                            return tex;
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.Log("Error searching for texture file in directory: " + e.Message);
                            }
                        }
                    }
                    else
                    {
                        var texturePathName = System.IO.Path.Combine(prefix, ctextureFile);
                        texturePathName = IOUtility.NormalizePath(texturePathName);
                        Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(texturePathName);

                        if (tex == null)
                            failLog += ((failLog.Length != 0) ? ", \n" : "") + "    '" + texturePathName + "'";
                        else
                        {
                            return tex;
                        }

                        failLog += "Problem found with: '" + texturePathName + "'";
                    }
                }
            }
            return null;
        }

    }
}