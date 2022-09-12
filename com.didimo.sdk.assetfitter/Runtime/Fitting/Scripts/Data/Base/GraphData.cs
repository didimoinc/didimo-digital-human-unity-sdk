using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.PathTools;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Didimo.AssetFitter.Editor.Graph
{
    [CreateAssetMenu(fileName = "New Graph Data", menuName = "Didimo/Graph/Graph Data", order = 10)]
    public class GraphData : ScriptableObject
    {
        [SerializeReference] public List<GraphNode> nodes = new List<GraphNode>();
        public List<GraphEdge> edges = new List<GraphEdge>();

        // Editor View
        public Vector3 viewPosition, viewScale = Vector3.one;
        [SerializeField] Config config;

        [Serializable]
        public class Config
        {
            public string[] exposeFields;
        }

        public static CachedState State;

        public void Run()
        {
#if UNITY_EDITOR
            if (State) return;

            //Phase 2
            EditorApplication.delayCall += () => EditorApplication.delayCall += () =>
            {
                Debug.Log("Run Complete");
                State.Dispose();
                State = null;
            };

            // Phase 0
            State = new CachedState(this);
            Debug.Log("State Created");

            Debug.Log("Starting Graph: '" + this.name + "'");
            Build();

            //Phase 1
            EditorApplication.delayCall += () =>
            {
                Debug.Log("Parsing End Points");
                this.nodes.ForEach(n => n.EndPoint());
            };
#endif
        }

        private void Build()
        {
            nodes.ForEach(n => n.Build());
        }

        // Nodes ///////////////
        public void AddNode(GraphNode node) => nodes.Add(node);
        public void RemoveNode(GraphNode node) => nodes.Remove(node);
        public GraphNode FindNode(string guid) => nodes.FirstOrDefault(n => n.guid == guid);
        public T FindNode<T>(string id) where T : GraphNode => nodes.FirstOrDefault(n => n.GetType() == typeof(T) && n.id == id) as T;

        // Edges ///////////////
        public GraphEdge AddEdge(GraphEdge.Connector input, GraphEdge.Connector output)
        {
            var edge = new GraphEdge(input, output);
            edges.Add(edge);
            return edge;
        }

        public void RemoveEdge(GraphEdge edge) => edges.Remove(edge);
        public GraphEdge FindEdge(GraphEdge.Connector input, GraphEdge.Connector output) =>
            edges.FirstOrDefault(e => (input == e.input && output == e.output) || (output == e.input && input == e.output));
        public GraphEdge FindEdge(GraphEdge.Connector connector) => FindEdges(connector).FirstOrDefault();
        public IEnumerable<GraphEdge> FindEdges(GraphEdge.Connector connector) =>
            edges.Where(e => (connector == e.input || connector == e.output));

        // Compile //////////////

        public class CachedState : IDisposable
        {
            public string TempPath = "Assets/_graphtemp";
            public GraphData data;

            public CachedState(GraphData data)
            {
                this.data = data;
                CreateAssetPath(TempPath);
            }

            List<UnityEngine.Object> hierarchyObjects = new List<UnityEngine.Object>();
            Dictionary<string, object> values = new Dictionary<string, object>();

            // Dispose
            public void Dispose()
            {
                //hierarchyObjects.ForEach(g => { if (g) DestroyImmediate(g); });
                Debug.Log("Destroying" + " " + hierarchyObjects.Count);

                foreach (var g in hierarchyObjects)
                {
                    if (g)
                    {
                        Debug.Log("DestroyImmediate" + " " + g.name);
                        DestroyImmediate(g);
                    }
                }
                RemoveAssetPath(TempPath);
            }

            // Hierarchy
            public void Add(UnityEngine.Object obj) => hierarchyObjects.Add(obj);

            // Data
            // public List<object> this[string key] => values[key];
            public List<object> GetValues(string key) => GetValue(key) as List<object>;
            public object GetValue(string key) => values[key];
            public T GetValue<T>(string key) => (T)values[key];
            public bool Has(string key) => values.ContainsKey(key);
            public object AddValues(string key, object value) => this.values[key] = value;
            public string CKEY(GraphNode node, string name) => node.guid + "::" + name;
            public static implicit operator bool(CachedState empty) => empty != null;

        }

    }
}

#if UNITY_EDITOR
namespace Didimo.AssetFitter.Editor.Graph
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(GraphData))]
    public class GraphData_Editor : UnityEditor.Editor
    {
        GraphData data => target as GraphData;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Number of nodes: " + (data.nodes == null ? 0 : data.nodes.Count));
            GUILayout.Label("Number of edges: " + (data.edges == null ? 0 : data.edges.Count));
            EditorGUILayout.BeginVertical("box");
            if (data.nodes != null) data.nodes.ForEach(DrawNode);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Run"))
            {
                //data.Build();
                data.Run();
            }
        }

        void DrawNode(GraphNode node)
        {
            GUILayout.Label(node.GetType().ToString());
        }
    }

    class ScriptableObjectOnImport : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedPaths, bool didDomainReload)
        {
            foreach (string path in imported.Where(p => p.EndsWith(".asset")))
            {
                GraphData graphData = AssetDatabase.LoadAssetAtPath<GraphData>(path);
                if (graphData)
                {
                    var _nodes = new List<GraphNode>();
                    Dictionary<string, string> guids = new Dictionary<string, string>();
                    foreach (var node in graphData.nodes)
                    {
                        _nodes.Add(node.Clone(false));
                        guids.Add(node.guid, _nodes.Last().guid);
                    }

                    graphData.nodes = _nodes;
                    foreach (var edge in graphData.edges)
                    {
                        edge.input.node = guids[edge.input.node];
                        edge.output.node = guids[edge.output.node];
                    }
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
#endif
