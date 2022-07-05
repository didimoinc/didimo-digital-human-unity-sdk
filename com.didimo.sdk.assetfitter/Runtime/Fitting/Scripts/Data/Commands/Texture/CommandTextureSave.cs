using System.ComponentModel;
using UnityEngine;
using System.IO;
using static Didimo.AssetFitter.Editor.Graph.GraphTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Texture/Texture Save")]
    [DisplayName("Texture Save")]
    [Width(200)]
    public class CommandTextureSave : GraphNode
    {
        [Input("Texture")] public Texture textureInput;
        [Input("Path")] [Expose] public string path;
        [Expose] public FileType fileType = FileType.UnityAsset;
        internal override void EndPoint(bool Build = false)
        {
#if UNITY_EDITOR

            var textures = GetInputValues(GetType().GetField(nameof(textureInput)));
            if (textures.Count > 0)
            {
                var path = this.path;
                if (GetAssetFolder(ref path))
                {
                    for (int i = 0; i < textures.Count; i++)
                    {
                        var filename = path + "/" + "texture-" + i.ToString().PadLeft(3, '0');
                        string filePath = Application.dataPath + "/" + filename.Substring("Assets".Length);
                        var texture = textures[i] as Texture;

                        Debug.Log("Writing Texture: " + filename);

                        switch (fileType)
                        {
                            case FileType.UnityAsset:
                                AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(textureInput), filename + ".asset");
                                break;

                            case FileType.JPG:
                                File.WriteAllBytes(filePath, (texture as Texture2D).EncodeToJPG());
                                break;

                            case FileType.PNG:
                                File.WriteAllBytes(filePath, (texture as Texture2D).EncodeToPNG());
                                break;
                        }
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else Debug.LogError("No valid asset path '" + path + "'");
            }
#else
            throw new System.Exception( "'" + GetType() + "'" + " Not implemented for runtime!");

#endif

        }

        public enum FileType
        {
            Simulate = 0,
            UnityAsset = 1,
            JPG = 2,
            PNG = 3
        }
    }
}
