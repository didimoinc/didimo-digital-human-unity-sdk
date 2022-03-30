#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Didimo.Core.Inspector;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

[CreateAssetMenu(fileName = "Didimo Builder", menuName = "Didimo/Internal/Builder")]
public class DidimoBuilder : ScriptableObject
{
    const string BuildPath = "DidimoBuilder.buildPath";

    public SceneAsset[] scenes;

    static Dictionary<string, List<AssetMarker>> markers;

    void OnValidate()
    {
        Show();
    }

    [Button("Show")]
    void Show()
    {
        markers = new Dictionary<string, List<AssetMarker>>();
        scenes = GetBuildScenes().Select((s, i) => new SceneAsset(s, i)).ToArray();

        foreach (var scene in scenes)
        {
            foreach (var asset in scene.dependencies)
            {
                if (markers.ContainsKey(asset.guid)) markers[asset.guid].Add(asset);
                else markers[asset.guid] = new List<AssetMarker>(new[] { asset });
            }
        }

        EditorApplication.projectWindowItemOnGUI -= DrawMarkers;
        EditorApplication.projectWindowItemOnGUI += DrawMarkers;
    }

    static IEnumerable<string> EmptyFolders =>
        Directory.GetDirectories(Application.dataPath, "*.*", SearchOption.AllDirectories).
            Where(p => Directory.GetDirectories(p).Length == 0 && Directory.GetFiles(p).Length == 0);

    [Button("List Empty Folders (Recursively)")]
    void listEmptyFolders()
    {
        Debug.Log(">>>>>>>>>>>>>>>> EMPTY FOLDERS (START) >>>>>>>>>>>>>>>>>>>>>");
        EmptyFolders.ToList().ForEach(d => Debug.Log(d));
        Debug.Log(">>>>>>>>>>>>>>>> EMPTY FOLDERS (END) >>>>>>>>>>>>>>>>>>>>>");
    }

    [Button("Remove Empty Folders (Recursively)")]
    void removeEmptyFolders()
    {
        var empty = EmptyFolders.ToList();

        // If no empty folders found exit
        if (empty.Count == 0) return;

        // Remove all folders
        empty.ForEach(d => Directory.Delete(d));

        removeEmptyFolders();
    }

    // [Button ("Build")]
    // void build()
    // {
    //     string buildPath = EditorUtility.SaveFilePanel ("Build Android ", EditorPrefs.GetString (BuildPath), "Build", "apk");
    //     if (String.IsNullOrEmpty (buildPath)) return;

    //     scenes = new string[0];
    //     assets = new BuiltAsset[0];

    //     EditorPrefs.SetString (BuildPath, buildPath);

    //     BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions ();
    //     buildPlayerOptions.scenes = GetBuildScenes ().ToArray ();
    //     scenes = buildPlayerOptions.scenes.ToArray ();

    //     buildPlayerOptions.locationPathName = buildPath;
    //     buildPlayerOptions.target = BuildTarget.Android;
    //     buildPlayerOptions.options = BuildOptions.None;

    //     BuildReport report = BuildPipeline.BuildPlayer (buildPlayerOptions);

    //     Debug.Log ("Complete!");
    //     BuildSummary summary = report.summary;

    //     if (summary.result == BuildResult.Succeeded)
    //     {
    //         Debug.Log ("Build succeeded: " + summary.totalSize + " bytes");
    //         assets = report.files.Select (f => new BuiltAsset (f)).Where (f => !String.IsNullOrEmpty (f.path)).ToArray ();
    //     }

    //     if (summary.result == BuildResult.Failed)
    //     {
    //         Debug.Log ("Build failed");
    //     }
    // }

    static IEnumerable<string> GetBuildScenes()
    {
        // Debug.Log ("Scene Count: " + SceneManager.sceneCountInBuildSettings);
        for (int i = 0, n = SceneManager.sceneCountInBuildSettings; i < n; i++)
            yield return SceneUtility.GetScenePathByBuildIndex(i); //Path.ChangeExtension (SceneUtility.GetScenePathByBuildIndex (i), null);
    }

    [Serializable]
    public class SceneAsset
    {
        public string name;
        public string path;
        public string guid;
        public int index;
        public AssetMarker[] dependencies;
        public SceneAsset(string path, int index)
        {
            this.name = Path.GetFileNameWithoutExtension(path);
            this.path = path;
            this.guid = AssetDatabase.AssetPathToGUID(path);
            this.index = index;
            this.dependencies = AssetDatabase.GetDependencies(path).
                Select(d => new AssetMarker { sceneIndex = index, path = d, guid = AssetDatabase.AssetPathToGUID(d) }).ToArray();
        }
    }

    [Serializable]
    public class AssetMarker
    {
        public int sceneIndex;
        public string path;
        public string guid;
        public Color color = Color.black;
    }

    // [Serializable]
    // public class BuiltAsset
    // {
    //     public BuiltAsset(BuildFile file)
    //     {
    //         guid = Path.GetFileName (file.path);
    //         path = AssetDatabase.GUIDToAssetPath (guid);
    //         role = file.role;
    //         size = file.size;
    //         name = path + " " + size;
    //     }

    //     public string guid;
    //     public string name;
    //     public string path;
    //     public string role;
    //     public ulong size;
    // }

    static void DrawMarkers(string guid, Rect rect)
    {
        if (markers == null || !markers.ContainsKey(guid)) return;
        if (rect.height > 16) return;

        Rect r = new Rect(rect) { x = 2, width = rect.height * 2, height = rect.height - 1 };
        EditorGUI.DrawRect(r, markers[guid].First().color);
        GUI.Label(r, String.Join("", markers[guid].Select(a => a.sceneIndex)), EditorStyles.miniLabel);
    }
}

#endif
