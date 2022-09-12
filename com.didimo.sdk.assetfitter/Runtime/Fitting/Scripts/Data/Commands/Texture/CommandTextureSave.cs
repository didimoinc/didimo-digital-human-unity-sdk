using System.ComponentModel;
using UnityEngine;
using System.IO;
using static Didimo.AssetFitter.Editor.Graph.GraphTools;
using static Didimo.AssetFitter.Editor.Graph.PathTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Texture/Texture Save")]
    [DisplayName("Texture Save")]
    [Width(200)]
    [HeaderColor(TypeColors.Texture)]
    public class CommandTextureSave : GraphNode
    {
        [Input("Texture")] public Texture textureInput;
        [Input("Path")] [Expose] public string path;
        [Expose] public ActionType actionType = ActionType.PathIsPath;
        [Expose] public FileType fileType = FileType.UnityAsset;
        internal override void EndPoint(bool Build = false)
        {
#if UNITY_EDITOR

            System.Collections.Generic.List<object> textures = GetInputValues(GetType().GetField(nameof(textureInput)));
            if (textures.Count > 0)
            {
                string path = this.path;
                if (GetAssetFolder(ref path))
                {
                    for (int i = 0; i < textures.Count; i++)
                    {
                        Texture2D texture = textures[i] as Texture2D;
                        string filename;
                        switch (actionType)
                        {
                            default:
                            case ActionType.PathIsPath: filename = path + "/" + texture.name; break;
                            case ActionType.PathIsFilename: filename = path + (textures.Count > 1 ? "-" + i : ""); break;
                        }

                        Debug.Log("Writing Texture: " + filename);

                        CreatePath(Path.GetDirectoryName(filename));

                        switch (fileType)
                        {
                            case FileType.UnityAsset:
                                AssetDatabase.CreateAsset(UnityEngine.Object.Instantiate(texture), filename + ".asset");
                                break;

                            case FileType.JPG:
                                File.WriteAllBytes(filename, texture.EncodeToJPG(90));
                                break;

                            case FileType.PNG:
                                File.WriteAllBytes(filename + ".png", texture.EncodeToPNG());
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

        public enum ActionType
        {
            PathIsPath = 0,
            PathIsFilename = 1,
        }
    }
}
