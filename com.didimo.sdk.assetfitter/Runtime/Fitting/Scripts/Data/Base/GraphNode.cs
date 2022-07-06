using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Didimo.AssetFitter.Editor.Graph.GraphData;

namespace Didimo.AssetFitter.Editor.Graph
{
    [Serializable]
    [HeaderColor(0, 0, 0, 0.8f)]
    [Width(200)]
    public class GraphNode
    {
        const float DefaultWidth = 160;

        public string guid;
        public string id;
        public Vector2 position;
        public bool expanded = true;

        public string title => GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? GetType().Name;
        public float width => GetType().GetCustomAttribute<WidthAttribute>()?.width ?? DefaultWidth;
        public string tooltip => GetType().GetCustomAttribute<TooltipAttribute>()?.tooltip ?? null;

        public GraphNode() => RenewGuid();
        public GraphNode Clone(bool RenewGuid = true)
        {
            GraphNode node = Activator.CreateInstance(GetType()) as GraphNode;
            string guid = node.guid;
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(this), node);
            if (RenewGuid) node.guid = guid;
            return node;
        }
        internal string RenewGuid() => guid = Guid.NewGuid().ToString();

        public static implicit operator bool(GraphNode empty) => empty != null;

        // COMPILING AND RUNNING //////////////////////

        // Logging
        public static void Log(string message) => Debug.Log(message);
        public static void Warning(string message) => Debug.LogWarning(message);
        public static void Error(string message) => Debug.LogError(message);
        public static bool CheckLengths(string message, params int[] counts)
        {
            if (counts.Min() == counts.Max()) return true;
            Error(message + ": Lengths do not match! (" + String.Join("!=", counts) + ")");
            return false;
        }

        // input / output
        public IEnumerable<FieldInfo> inputs => GetType().GetFields().Where(f => f.GetCustomAttribute<InputAttribute>() != null);
        public IEnumerable<FieldInfo> outputs => GetType().GetFields().Where(f => f.GetCustomAttribute<OutputAttribute>() != null);
        public IEnumerable<FieldInfo> exposedFields => GetType().GetFields().Where(f => f.GetCustomAttribute<ExposeAttribute>() != null);

        internal virtual void EndPoint(bool Build = false) { }

        protected T GetInputValue<T>(string fieldName)
        {
            var values = GetInputValues(fieldName);
            if (values.Count == 0) Warning("No input values found in '" + fieldName + "'");
            return (T)values.FirstOrDefault();
        }
        protected List<T> GetInputValues<T>(string fieldName) => GetInputValues(fieldName).Cast<T>().ToList();
        protected List<object> GetInputValues(string fieldName) => GetInputValues(GetType().GetField(fieldName));
        protected List<T> GetInputValues<T>(FieldInfo info) => GetInputValues(info).Cast<T>().ToList();
        protected List<object> GetInputValues(FieldInfo info)
        {
            string key = State.CKEY(this, info.Name);
            if (!State.Has(key))
            {
                List<object> values = new List<object>();
                foreach (var edge in State.data.FindEdges(new GraphEdge.Connector(this, info)))
                    foreach (var v in State.data.FindNode(edge.output.node).GetOutputValues(edge.output.field))
                        values.Add(v);

                if (values.Count == 0) values = GetExposedValue(info);
                State.AddValues(key, values.Where(v => v != null).ToList());
            }
            return State.GetValues(key);
        }

        List<object> GetOutputValues(string fieldName) => GetOutputValues(GetType().GetField(fieldName));
        List<object> GetOutputValues(FieldInfo info)
        {
            string key = State.CKEY(this, info.Name);
            if (!State.Has(key))
                State.AddValues(key, GetOutputValues(info, out List<object> values) ? values : GetExposedValue(info));
            return State.GetValues(key);
        }

        protected virtual bool GetOutputValues(FieldInfo info, out List<object> values)
        {
            values = null;
            return false;
        }

        public FieldInfo GetExposedInfo(string name) => exposedFields.FirstOrDefault(f => f.Name == name);

        public bool SetExposedValue(string name, object value)
        {
            var info = GetExposedInfo(name);
            if (info == null) return false;
            SetExposedValue(info, value);
            return true;
        }

        protected void SetExposedValue(FieldInfo info, object value) =>
            info.SetValue(this, value);

        protected List<object> GetExposedValue(FieldInfo info) => info.GetCustomAttribute<ExposeAttribute>() == null ?
            new List<object>() : new List<object>() { info.GetValue(this) };
    }
}