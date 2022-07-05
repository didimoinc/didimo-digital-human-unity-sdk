using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    public class NodeMenu : ScriptableObject, ISearchWindowProvider
    {
        EditorWindow window;
        GraphView view;

        private Texture2D _indentationIcon;

        public void Configure(EditorWindow window, GraphView view)
        {
            this.window = window;
            this.view = view;

            //Transparent 1px indentation icon as a hack
            _indentationIcon = new Texture2D(1, 1);
            _indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            _indentationIcon.Apply();
        }

        class PathObject { public Dictionary<string, PathObject> groups = new Dictionary<string, PathObject>(); }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>();
            tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

            IEnumerable<(String path, Type type)> GetTypes() =>
                 Assembly.GetAssembly(NodeView.BaseGraphNodeType).GetTypes().
                    Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(NodeView.BaseGraphNodeType)).
                    Select(t => (t.GetCustomAttribute<MenuPathAttribute>()?.path ?? t.Name, t));

            Dictionary<string, PathObject> groups = new Dictionary<string, PathObject>();
            foreach (var obj in GetTypes().OrderBy(o => o.path))
            {
                var elements = obj.path.Split('/');
                var g = groups;
                int level, i;
                for (i = 0, level = 1; i < elements.Length - 1; i++, level++)
                {
                    var e = elements[i];
                    if (!g.ContainsKey(e))
                    {
                        tree.Add(new SearchTreeGroupEntry(new GUIContent(e), level));
                        g[e] = new PathObject();
                    }
                    g = g[e].groups;
                }
                tree.Add(new SearchTreeEntry(new GUIContent(elements[i], _indentationIcon)) { level = level, userData = obj.type });
            }
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            view.CreateNode(SearchTreeEntry.userData as Type, view.GetGraphPosition(context.screenMousePosition));
            return true;
        }
    }
}