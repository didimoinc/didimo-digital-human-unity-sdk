using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Didimo.AssetFitter.Editor.Graph
{
    public class GraphWindow : EditorWindow
    {
        public GraphData graphData;
        // public GraphView view;
        internal Toolbar toolbar;

        static Dictionary<int, GraphView> State = new Dictionary<int, GraphView>();

        [OnOpenAssetAttribute]
        public static bool OpenGraph(int instanceID, int line)
        {
            var graphData = EditorUtility.InstanceIDToObject(instanceID) as GraphData;
            if (!graphData) return false;

            GraphWindow window = UnityEngine.Resources.FindObjectsOfTypeAll<GraphWindow>().FirstOrDefault(w => w.graphData == graphData);
            if (window) window.Focus();
            else window = CreateInstance<GraphWindow>();
            window.graphData = graphData;
            window.titleContent = new GUIContent(graphData.name);
            window.CreateGraphView();
            window.Show();
            return true;
        }

        void OnEnable()
        {
            BuildView();
        }

        internal void BuildView()
        {
            CreateGraphView();
            CreateToolbar();
        }

        void CreateGraphView()
        {
            GraphView view = rootVisualElement.Q<GraphView>("graphview");
            if (view != null) rootVisualElement.Remove(view);
            view = new GraphView(this) { name = "graphview" };
            rootVisualElement.Add(view);
            view.StretchToParentSize();
        }

        void CreateToolbar()
        {
            if (rootVisualElement.Q("toolbar") != null) rootVisualElement.Remove(rootVisualElement.Q("toolbar"));
            toolbar = new Toolbar() { name = "toolbar" };
            toolbar.Add(new VisualElement() { style = { width = 20 } });

            toolbar.Add(new Button(() => graphData.Run()) { text = "Run Graph" });

            void Save()
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(graphData, out string guid, out long localid))
                {
                    EditorUtility.SetDirty(graphData);
                    AssetDatabase.SaveAssetIfDirty(new GUID(guid));
                }
            }
            toolbar.Add(new Button(() => Save()) { text = "Save Graph" });
            Debug.Log("Adding toolbar");
            rootVisualElement.Add(toolbar);
        }

        void SaveGraphData()
        {
            EditorUtility.SetDirty(graphData);
            Debug.Log("IsDirty: " + EditorUtility.IsDirty(graphData));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(graphData));
            Debug.Log("IsDirty (post) : " + EditorUtility.IsDirty(graphData));
        }
    }

    class ScriptableObjectOnImport : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedPaths, bool didDomainReload)
        {
            foreach (string path in imported.Where(p => p.EndsWith(".asset")))
            {
                var graphData = AssetDatabase.LoadAssetAtPath<GraphData>(path);
                if (graphData)
                {
                    GraphWindow window = UnityEngine.Resources.FindObjectsOfTypeAll<GraphWindow>().FirstOrDefault(w => w.graphData == graphData);
                    if (window) EditorApplication.delayCall += () => window.BuildView();
                }
            }
        }
    }
}

