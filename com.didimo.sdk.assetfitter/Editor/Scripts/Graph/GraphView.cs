using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Didimo.AssetFitter.Editor.Graph
{
    public class GraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        public GraphData graphData => window.graphData;
        new public NodeView[] nodes => base.nodes.Cast<NodeView>().ToArray();
        public NodeView[] selectedNodes => nodes.Where(n => n.selected).ToArray();
        NodeMenu nodeMenu;
        GraphWindow window;

        public GraphView(GraphWindow window)
        {
            this.window = window;
            styleSheets.Add(UnityEngine.Resources.Load<StyleSheet>("Graph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            AddManipulators();
            AddGrid();
            AddSearchWindow();
            AddMenuOperations();
            graphViewChanged += OnGraphViewChanged;
            viewTransformChanged += OnViewTransformChanged;
            Rebuild();
        }

        void Rebuild()
        {
            if (!graphData) return;
            graphData.nodes.ToList().ForEach(n => CreateNodeView(n));
            graphData.edges.ToList().ForEach(e => CreateEdge(e));
            UpdateViewTransform(graphData.viewPosition, graphData.viewScale);
        }

        void OnViewTransformChanged(UnityEditor.Experimental.GraphView.GraphView graphView)
        {
            graphData.viewPosition = this.viewTransform.position;
            graphData.viewScale = this.viewTransform.scale;
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
            {
                change.edgesToCreate.ForEach(CreateEdge);
            }

            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is Edge) RemoveEdge(element as Edge);
                    else if (element is NodeView) graphData.RemoveNode((element as NodeView).node);
                    else Debug.Log("Unknown type " + element.GetType());
                }
            }

            EditorUtility.SetDirty(graphData);
            return change;
        }

        protected override bool canDuplicateSelection => true;

        void AddManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new SelectionDropper());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());
        }

        void AddGrid()
        {
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        }

        private void AddSearchWindow()
        {
            (nodeMenu = ScriptableObject.CreateInstance<NodeMenu>()).Configure(window, this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), nodeMenu);
        }

        public Vector2 GetGraphPosition(Vector2 mouse)
        {
            var root = window.rootVisualElement;
            var position = root.ChangeCoordinatesTo(root.parent, mouse - window.position.position);
            return contentViewContainer.WorldToLocal(position);
        }

        #region "Nodes"
        public NodeView FindNode(string node) => nodes.ToList().FirstOrDefault(n => n.node.guid == node);
        public NodeView FindNode(GraphNode node) => nodes.ToList().FirstOrDefault(n => n.node.guid == node.guid);

        public NodeView CreateNode(Type type, Vector2 position)
        {
            var node = (GraphNode)Activator.CreateInstance(type);
            node.position = position;
            graphData.AddNode(node);
            return CreateNodeView(node);
        }

        public NodeView CreateNodeView(GraphNode data)
        {
            var node = new NodeView(data);
            node.SetPosition(new Rect(data.position, new Vector2(100, 100)));
            AddElement(node);
            return node;
        }
        #endregion

        #region "Edges"
        public Edge CreateEdge(GraphEdge edge)
        {
            NodeView inputNode = FindNode(edge.input.node), outputNode = FindNode(edge.output.node);
            if (inputNode && outputNode)
            {
                Port inputPort = inputNode.FindPort(edge.input.field);
                Port outputPort = outputNode.FindPort(edge.output.field);

                if (inputPort != null && outputPort != null)
                {
                    var _edge = new Edge() { input = inputPort, output = outputPort };
                    _edge?.input.Connect(_edge);
                    _edge?.output.Connect(_edge);
                    Add(_edge);
                    return _edge;
                }
            }
            graphData.RemoveEdge(edge);
            return null;
        }
        GraphEdge.Connector GetEdgeConnector(Port port) => new GraphEdge.Connector((port.node as NodeView).node, (port.node as NodeView).ports[port].fieldInfo.Name);
        void CreateEdge(Edge edge) => graphData.AddEdge(GetEdgeConnector(edge.input), GetEdgeConnector(edge.output));
        void RemoveEdge(Edge edge) => graphData.RemoveEdge(graphData.FindEdge(GetEdgeConnector(edge.input), GetEdgeConnector(edge.output)));
        public override List<Port> GetCompatiblePorts(Port port1, NodeAdapter nodeAdapter)
        {
            var ports = new List<Port>();
            var connector1 = GetEdgeConnector(port1);

            foreach (var port2 in this.ports.ToList())
            {
                if (port1.node == port2.node) continue;
                if (port1.portType != port2.portType) continue;
                if (port1.direction == port2.direction) continue;
                if (graphData.FindEdge(connector1, GetEdgeConnector(port2))) continue;
                ports.Add(port2);
            }
            return ports;
        }
        #endregion

        #region "Serialize"
        Vector2 mousePosition;
        void AddMenuOperations()
        {
            serializeGraphElements += OnSerializeElements;
            unserializeAndPaste += OnDeserializeElements;
            RegisterCallback<MouseMoveEvent>((x) =>
            {
                mousePosition = x.localMousePosition;
                // Debug.Log(x.localMousePosition + " " + x.mousePosition + " " + GetGraphPosition(x.localMousePosition));
            });
        }

        [Serializable]
        class SerializeNodes
        {
            [SerializeField] string[] nodes;
            [SerializeField] string[] types;
            public SerializeNodes(IEnumerable<GraphNode> nodes)
            {
                this.nodes = nodes.Select(n => JsonUtility.ToJson(n)).ToArray();
                this.types = nodes.Select(n => n.GetType().AssemblyQualifiedName).ToArray();
            }
            public List<GraphNode> GetNodes()
            {
                var graphNodes = new GraphNode[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    graphNodes[i] = JsonUtility.FromJson(nodes[i], Type.GetType(types[i])) as GraphNode;
                    graphNodes[i].guid = Guid.NewGuid().ToString();
                }
                return graphNodes.ToList();
            }
        }

        string OnSerializeElements(IEnumerable<GraphElement> elements)
        {
            var nodes = elements.Where(e => e is NodeView).Select(e => (e as NodeView).node);
            var serialized = new SerializeNodes(nodes);
            return JsonUtility.ToJson(serialized);
        }

        void OnDeserializeElements(string operation, string data)
        {
            SerializeNodes serializedNodes = JsonUtility.FromJson<SerializeNodes>(data);

            ClearSelection();
            var nodes = JsonUtility.FromJson<SerializeNodes>(data).GetNodes();

            switch (operation)
            {
                case "Duplicate":
                    nodes.ForEach(n => n.position += Vector2.one * 16);
                    break;

                case "Paste":
                    // var center = nodes.Aggregate(Vector2.zero, (p, n) => p += n.position) / nodes.Count;
                    // Debug.Log("Mouse Position = " + mousePosition);
                    nodes.ForEach(n => n.position += Vector2.one * 16);
                    break;
            }

            nodes.ForEach(n => graphData.AddNode(n));
            nodes.Select(n => CreateNodeView(n) as ISelectable).ForAll(n => AddToSelection(n));
        }
        #endregion

        public static GraphView FindView(VisualElement element)
        {
            for (; element != null; element = element.parent)
                if (element is GraphView) return element as GraphView;
            return null;
        }
    }
}