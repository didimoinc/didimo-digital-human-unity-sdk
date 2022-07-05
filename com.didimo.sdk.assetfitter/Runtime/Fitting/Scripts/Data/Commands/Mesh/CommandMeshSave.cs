using System;
using System.ComponentModel;
using UnityEngine;
using System.IO;
using static Didimo.AssetFitter.Editor.Graph.MeshTools;
using static Didimo.AssetFitter.Editor.Graph.GraphTools;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using static Didimo.AssetFitter.Editor.Graph.PathTools;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Mesh/Mesh Save")]
    [DisplayName("Mesh Save")]
    [Tooltip("Saves a Mesh asset in various formats!")]
    [Width(200)]
    [HeaderColor(TypeColors.Mesh)]
    public class CommandMeshSave : GraphNode
    {
        [Input("Mesh")] public Mesh meshInput;
        [Input("Path")] [Expose] public string path;
        [Expose] public ActionType actionType = ActionType.PathIsPath;
        [Expose] public FileType fileType = FileType.UnityAsset;

        internal override void EndPoint(bool Build = false)
        {
#if UNITY_EDITOR
            var meshes = GetInputValues(GetType().GetField(nameof(meshInput)));

            if (meshes.Count > 0)
            {
                var path = this.path;
                if (GetAssetFolder(ref path))
                {
                    for (int i = 0; i < meshes.Count; i++)
                    {
                        // var filename = path + "/" + "mesh-" + i.ToString().PadLeft(3, '0');
                        var mesh = meshes[i] as Mesh;
                        string filename;
                        switch (actionType)
                        {
                            default:
                            case ActionType.PathIsPath: filename = path + "/" + mesh.name; break;
                            case ActionType.PathIsFilename: filename = path + (meshes.Count > 1 ? "-" + i : ""); break;
                        }

                        Debug.Log(
                            "Writing Mesh: " + // + mesh.name +
                            " path:" + filename +
                            " vertices:" + mesh.vertexCount +
                            " indicies:" + mesh.triangles.Length +
                            " blendshapes:" + mesh.blendShapeCount +
                            " submeshes:" + mesh.subMeshCount + "\n" +
                            " submeshes.indices:" + String.Join(",", Enumerable.Range(0, mesh.subMeshCount).Select(i => mesh.GetIndices(i).Length)));

                        switch (fileType)
                        {
                            case FileType.UnityAsset:
                                CreateAsset(CloneAsset(mesh), filename + ".asset");
                                break;

                            case FileType.OBJ:
                                Writer.WriteOBJ(mesh, filename + ".obj");
                                break;

                            case FileType.FBX:
                                Writer.WriteFBX(mesh, filename + ".fbx");
                                break;

                            case FileType.InSceneAsMesh:
                                {
                                    var meshFilter = new GameObject(mesh.name).AddComponent<MeshFilter>();
                                    meshFilter.sharedMesh = mesh;
                                    var renderer = meshFilter.gameObject.AddComponent<MeshRenderer>();
                                    renderer.sharedMaterials = GetColorMaterials(mesh.subMeshCount);
                                }
                                break;

                            case FileType.InSceneAsSkin:
                                {
                                    var renderer = new GameObject(mesh.name).AddComponent<SkinnedMeshRenderer>();
                                    renderer.sharedMesh = mesh;
                                    renderer.sharedMaterials = GetColorMaterials(mesh.subMeshCount);
                                }
                                break;

                            case FileType.Raw:
                                {
                                    CreatePath(Path.GetDirectoryName(path));
                                    File.WriteAllText(filename + ".vertices.txt", String.Join("\n", mesh.vertices.Select(v => v.x + "," + v.y + "," + v.z)));
                                    var triangles = mesh.triangles;
                                    File.WriteAllText(filename + ".indices.txt", String.Join("\n",
                                        Enumerable.Range(0, triangles.Length / 3).Select(i =>
                                            triangles[i * 3 + 0] + "," + triangles[i * 3 + 1] + "," + triangles[i * 3 + 2])));
                                }
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
            OBJ = 2,
            FBX = 3,
            InSceneAsMesh = 4,
            InSceneAsSkin = 5,
            Raw = 6,
        }

        public enum ActionType
        {
            PathIsPath = 0,
            PathIsFilename = 1,
        }
#if UNITY_EDITOR
        void WriteMeshes(Mesh mesh, string path)
        {
            AssetDatabase.CreateAsset(mesh, path + ".asset");
        }
#endif

    }
}
