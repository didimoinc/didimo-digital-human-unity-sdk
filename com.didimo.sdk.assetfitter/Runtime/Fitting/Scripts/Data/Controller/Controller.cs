using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using static Didimo.AssetFitter.Editor.Graph.Controller;

namespace Didimo.AssetFitter.Editor.Graph
{
    [CreateAssetMenu(fileName = "New Controller", menuName = "Didimo/Graph/Asset Fitter/Controller")]
    public class Controller : ScriptableObject
    {
        public string title;
        public string description;
        public string documentation;
        public Texture2D titleImage;
        public AssetGraph[] assetGraphs;
        public GameObject input;
        public GameObject output;
        public string path;
        public CommandPrefabSave.SaveType saveType;

        public int usingGraphIndex = -1;

        internal int GetGraphIndex(AssetGraph graph) => Array.IndexOf(assetGraphs, graph);

        public AssetGraph GetValidGraph(GameObject input, GameObject output)
        {
            var validGraph = assetGraphs.FirstOrDefault(g => g.IsValid(input, output, saveType));
            if (usingGraphIndex > -1)
            {
                int index = GetGraphIndex(validGraph);
                if (index > -1) usingGraphIndex = index;
                return assetGraphs[usingGraphIndex];
            }
            return validGraph;
        }

        [Serializable]
        public class AssetGraph
        {
            public GraphData graph;
            public Gender gender;
            public string name => graph?.name ?? "Null graph";

            [HideInInspector] GraphData tempGraph;

            public void Run(GameObject input, GameObject output, CommandPrefabSave.SaveType saveType)
            {
                if (!IsValid(input, output, saveType))
                    throw new Exception("Graph is not valid!");
                tempGraph.Run();
            }

            public bool IsValid(GameObject input, GameObject output, CommandPrefabSave.SaveType saveType)
            {
                if (!graph || !input || !output) return false;

                tempGraph = AssetTools.CloneAsset(graph);

                CommandDidimo didimoNode = tempGraph.FindNode<CommandDidimo>("output");
                // if (!didimoNode) throw new Exception("No didimo to Output");

                CommandAvatar avatarNode = tempGraph.FindNode<CommandAvatar>("input");
                // if (!avatarNode) throw new Exception("No 3rd Party Avatar Input");

                CommandPrefabSave prefabNode = tempGraph.FindNode<CommandPrefabSave>("SavePrefab");
                // if (!prefabNode) throw new Exception("Save Node not correct");

                if (avatarNode)
                {
                    avatarNode.avatarPrefab = input;
                    if (avatarNode.gender == gender)
                    {
                        if (didimoNode)
                        {
                            didimoNode.avatarPrefab = output;
                            if (prefabNode)
                            {
                                prefabNode.saveType = saveType;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            public static implicit operator bool(AssetGraph empty) => empty != null;
        }
    }
}

#if UNITY_EDITOR
namespace Didimo.AssetFitter.Editor.Graph
{
    using System.Linq;
    using UnityEditor;

    public class ControllerLoader
    {
        static Controller defaultController;

        const string assetFitterID = "Asset Fitter Beta";

        public static Controller LoadDefault()
        {
            if (!defaultController)
            {
                string guid = UnityEditor.AssetDatabase.FindAssets("t:Didimo.AssetFitter.Editor.Graph.Controller").FirstOrDefault();
                if (guid != null)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                    if (Path.GetFileNameWithoutExtension(path) == assetFitterID)
                    {
                        defaultController = UnityEditor.AssetDatabase.LoadAssetAtPath<Controller>(path);
                    }
                }
            }

            if (!defaultController)
            {
                Debug.Log("Can't find it");
            }

            return defaultController;
        }
    }

    [CustomEditor(typeof(Controller))]
    public class ControllerEditor : Editor
    {
        const float LineSpace = 10;
        new Controller target => base.target as Controller;

        protected override void OnHeaderGUI()
        {
            DrawTitleImage(target.titleImage);
            GUILayout.Space(LineSpace);
        }

        public override void OnInspectorGUI()
        {
            DrawLabel(target.title, Styles.Title);

            DrawLinkButton(target.documentation, Styles.Link);
            GUILayout.Space(LineSpace);

            DrawLabel(target.description, Styles.Body);
            GUILayout.Space(LineSpace);

            bool AvatarsHorizontal = true;
            if (AvatarsHorizontal)
            {
                GUILayout.BeginHorizontal();
                target.input = GameObjectDropBox(target.input, "3rd Party Avatar", Validate3rdParty);
                GUILayout.Space(4);
                target.output = GameObjectDropBox(target.output, "Target Didimo", ValidateDidimo);
                GUILayout.EndHorizontal();
                GUILayout.Space(LineSpace);
            }
            else
            {
                target.input = GameObjectDropBox(target.input, "3rd Party Avatar", Validate3rdParty);
                GUILayout.Space(LineSpace);
                target.output = GameObjectDropBox(target.output, "Target Didimo", ValidateDidimo);
                GUILayout.Space(LineSpace);
            }

            //EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Controller.saveType)));


            Controller.AssetGraph graph = target.GetValidGraph(target.input, target.output);

            UsingGraph(graph);
            GUILayout.Space(LineSpace);
            RunGraph(graph);
        }

        void RunGraph(AssetGraph graph)
        {
            GUI.enabled = graph;
            if (GUILayout.Button("Transfer Assets", Styles.Button, GUILayout.Height(50)))
                graph.Run(target.input, target.output, target.saveType);

        }

        void UsingGraph(AssetGraph graph)
        {
            var graphs = new[] { "Unknown graph" }.Concat(target.assetGraphs.Select(g => g.name)).ToArray();

            var index = target.usingGraphIndex > -1 || !graph ? target.usingGraphIndex : Array.IndexOf(graphs, graph.name);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Using Graph:");
            target.usingGraphIndex = EditorGUILayout.Popup(index + 1, graphs) - 1;
            GUILayout.EndHorizontal();
        }

        public void DrawEditor()
        {
            OnHeaderGUI();
            OnInspectorGUI();
        }

        GameObject Validate3rdParty(GameObject[] gameObjects)
        {
            return gameObjects.FirstOrDefault();
        }

        GameObject ValidateDidimo(GameObject[] gameObjects) =>
            gameObjects.FirstOrDefault(g => g.GetComponent<DidimoComponents>());

        #region "GameObject drop box"
        GameObject GameObjectDropBox(GameObject gameObject, string title, Func<GameObject[], GameObject> validate)
        {
            GUILayout.BeginVertical();
            DrawLabel(title, Styles.Header);
            Rect area = GUILayoutUtility.GetRect(64, Mathf.Min(200, Screen.width * 1f));// GUILayout.ExpandWidth(true));

            if (gameObject)
            {
                gameObject = EditorGUILayout.ObjectField(gameObject, gameObject.GetType(), true) as GameObject;
                DrawGameObject(gameObject, area);
            }
            else
            {
                GUI.Box(area, "");
                GUI.Label(area, "Drag & Drop", Styles.DragAndDrop);
            }

            if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform) &&
                area.Contains(Event.current.mousePosition))
            {
                GameObject droppedGameObject = validate(DragAndDrop.objectReferences.Where(o => o is GameObject).Select(o => o as GameObject).ToArray());
                if (droppedGameObject)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        gameObject = droppedGameObject;
                    }
                }
            }
            GUILayout.EndVertical();
            return gameObject;
        }
        #endregion

        #region "GameObject preview Editors"
        Dictionary<GameObject, Editor> gameObjectEditors = new Dictionary<GameObject, Editor>();

        Editor GetGameObjectEditor(GameObject gameObject)
        {
            return gameObjectEditors.ContainsKey(gameObject) ?
            gameObjectEditors[gameObject] : gameObjectEditors[gameObject] = Editor.CreateEditor(gameObject);
        }

        void DrawGameObject(GameObject gameObject, Rect position) =>
            GetGameObjectEditor(gameObject).OnInteractivePreviewGUI(position, Styles.PreviewGameObject);
        #endregion

        #region "Using Custom GUIStyles"

        void DrawLabel(string text, GUIStyle style)
        { if (!string.IsNullOrEmpty(text)) GUILayout.Label(text, style); }

        void DrawLinkButton(string link, GUIStyle style)
        { if (!string.IsNullOrEmpty(link) && GUILayout.Button(link, Styles.Link)) Application.OpenURL(link); }

        void DrawTitleImage(Texture2D image) =>
            GUILayout.Label(image, GUILayout.Width(Screen.width), GUILayout.Height(Screen.width * image.height / image.width));

        // bool drawButton(string text) => GUILayout.Button(text, Styles.Button);

        public static class Styles
        {
            static Styles()
            {
                Body = new GUIStyle(EditorStyles.label)
                { wordWrap = true, fontSize = 14, richText = true, };

                DragAndDrop = new GUIStyle(EditorStyles.label)
                { wordWrap = true, fontStyle = FontStyle.Bold, fontSize = 18, richText = true, alignment = TextAnchor.MiddleCenter, };

                Title = new GUIStyle(EditorStyles.label)
                { normal = { textColor = Color.white }, wordWrap = true, fontSize = 26, richText = true, };

                Header = new GUIStyle(EditorStyles.label)
                { wordWrap = false, fontStyle = FontStyle.Bold, fontSize = 18, richText = true, };

                Link = new GUIStyle(EditorStyles.label)
                { wordWrap = false, normal = { textColor = new Color32(255, 154, 0, 255) }, fontSize = 14, richText = true, }; //stretchWidth = false

                PreviewGameObject = new GUIStyle();

                Button = new GUIStyle(GUI.skin.button);
                // {
                //     normal = { background = getTextureColor(new Color32(255 / 2, 154 / 2, 0, 255 / 2)) },
                //     hover = { background = getTextureColor(new Color32(255, 154, 0, 255)) },
                //     dis = { background = getTextureColor(new Color32(255, 154, 0, 255)) },
                // };
            }

            public static GUIStyle Title, Header, Body, Link, DragAndDrop, PreviewGameObject, Button;

            static Texture2D GetTextureColor(Color color)
            { Texture2D t = new Texture2D(1, 1); t.SetPixel(0, 0, color); t.Apply(); return t; }
        }


        #endregion
    }
}
#endif