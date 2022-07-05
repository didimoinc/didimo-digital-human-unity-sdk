using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using static UnityEditor.Experimental.GraphView.Port;

namespace Didimo.AssetFitter.Editor.Graph
{
    public class NodeView : Node
    {
        public static Type BaseGraphNodeType = typeof(GraphNode);
        public static Color DefaultBackgroundColor = new Color(0.18f, 0.18f, 0.18f, 0.8f);
        public static Color DefaultBorderColor = new Color(0.13f, 0.13f, 0.13f, 1);

        public GraphView view => GraphView.FindView(this);
        public Dictionary<Port, PortReference> ports = new Dictionary<Port, PortReference>();
        public GraphNode node;

        Color bgcolor => node.GetType().GetCustomAttribute<HeaderColorAttribute>()?.color ?? DefaultBackgroundColor;

        public NodeView(GraphNode node)
        {
            this.node = node;

            title = node.title;

            // VisualElement titleContainer = this.Q("title");
            titleContainer.style.backgroundColor = bgcolor;
            titleContainer.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleContainer.style.height = 32;

            VisualElement titleLabel = titleContainer.Q("title-label");
            titleLabel.tooltip = node.tooltip;

            Button collapseButton = titleButtonContainer.Q<Button>("collapse-button");

            Button button;
            titleContainer.Insert(0, button = new Button(ToggleIDField) { text = "+", style = { width = 10, marginTop = 8, marginBottom = 8 } });
            button.visible = node is IExposable;

            style.width = node.width;

            CreateMonoScriptField();
            CreatePorts();
            if (!node.expanded) ToggleCollapse();
            RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);

            // EditorApplication.delayCall += () => initialize();
        }

        // void initialize()
        // {
        //     RefreshExpandedState();
        // }

        void OnGeometryChangedEvent(GeometryChangedEvent e)
        {
            RefreshExpandedState();
            node.position = e.newRect.position;
            node.expanded = expanded;
        }

        public Port FindPort(string fieldName) => ports.Values.FirstOrDefault(p => p.fieldInfo.Name == fieldName)?.port ?? null;

        void CreatePorts()
        {
            foreach (var info in node.GetType().GetFields())
            {
                DirectionAttribute direction = info.GetCustomAttribute<DirectionAttribute>();
                if (direction != null)
                {
                    var expose = info.GetCustomAttribute<ExposeAttribute>();
                    Port port = CreatePort(expose == null ? direction.name : "", direction, info.FieldType);

                    if (expose != null)
                    {
                        port.contentContainer.Children().ToList().ForEach(c => c.style.flexShrink = 0);
                        port.contentContainer.Add(GetExposedControl(info));
                    }

                    (direction is InputAttribute ? inputContainer : outputContainer).Add(port);
                    ports[port] = new PortReference() { nodeView = this, fieldInfo = info, port = port };
                }
                else if (info.GetCustomAttribute<ExposeAttribute>() != null)
                {
                    TryGetContainer("properties").Add(GetExposedControl(info, info.Name));
                }
            }

            inputContainer.style.flexShrink = 1;
            outputContainer.style.flexShrink = 1;

            RefreshExpandedState();
            RefreshPorts();
        }

        Port CreatePort(string name, DirectionAttribute direction, Type type)
        {
            var port = InstantiatePort(Orientation.Horizontal, direction is InputAttribute ? Direction.Input : Direction.Output, direction.single ? Capacity.Single : Capacity.Multi, type);
            port.portName = name;
            if (TypeColors.GetColorByType(type, out Color color, 2f))
                port.portColor = color;
            return port;
        }

        public class PortReference
        {
            public NodeView nodeView;
            public FieldInfo fieldInfo;
            public Port port;
        }

        void OnExposedValueChanged(FieldInfo fieldInfo) { }

        VisualElement GetExposedControl(FieldInfo fieldInfo, string label = "") =>
            FieldView.GetInstance(node, fieldInfo).element;

        VisualElement TryGetContainer(string name) => this.Q(name) != null ? this.Q(name) : AddContainer(name);

        VisualElement AddContainer(string name)
        {
            VisualElement contents = this.Q("contents"), container;
            contents.Add(new VisualElement() { style = { height = 1, backgroundColor = DefaultBorderColor } });
            contents.Add(container = new VisualElement() { name = name, style = { paddingLeft = 4, paddingRight = 4, paddingBottom = 4, paddingTop = 4, backgroundColor = DefaultBackgroundColor } });
            return container;
        }

        void CreateMonoScriptField()
        {
            var guids = AssetDatabase.FindAssets("t:MonoScript " + node.GetType().Name);
            for (int i = 0; i < guids.Length; i++)
            {
                var scriptAsset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i]), typeof(MonoScript)) as MonoScript;
                if (scriptAsset.name == node.GetType().Name)
                {
                    // var contents = this.Q("contents");
                    var container = new VisualElement() { style = { backgroundColor = bgcolor } };
                    container.Add(new ObjectField() { objectType = typeof(MonoScript), value = scriptAsset });
                    this.Q("contents").Insert(0, container);
                    return;
                }
            }
            Debug.LogWarning("Script not found! " + node.GetType().Name + " " + guids.Length);
        }

        VisualElement idFieldContainer;
        void RemoveIDField()
        {
            if (idFieldContainer == null) return;
            idFieldContainer.parent.Remove(idFieldContainer);
            idFieldContainer = null;
        }

        void ToggleIDField()
        {
            if (idFieldContainer == null) CreateIDField();
            else RemoveIDField();
        }

        void CreateIDField()
        {
            if (idFieldContainer != null) return;
            TextField idField;
            idFieldContainer = new VisualElement() { style = { backgroundColor = bgcolor } };
            idFieldContainer.Add(idField = new TextField());
            this.Q("contents").Insert(0, idFieldContainer);
            idField.RegisterValueChangedCallback(evt => node.id = idField.text);
            idField.SetValueWithoutNotify(node.id);
        }

        public static implicit operator bool(NodeView empty) => empty != null;
    }
}