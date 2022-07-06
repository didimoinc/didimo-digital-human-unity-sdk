using System;
using System.Reflection;

namespace Didimo.AssetFitter.Editor.Graph
{
    [Serializable]
    public class GraphEdge
    {
        public Connector input, output;

        public GraphEdge(Connector input, Connector output)
        {
            this.input = input;
            this.output = output;
        }

        public static implicit operator bool(GraphEdge empty) => empty != null;

        [Serializable]
        public struct Connector
        {
            public string node;
            public string field;

            public Connector(GraphNode node, FieldInfo field) : this(node, field.Name) { }
            public Connector(GraphNode node, string field)
            {
                this.node = node.guid;
                this.field = field;
            }

            public override int GetHashCode() => base.GetHashCode();
            public override bool Equals(object obj) => base.Equals(obj);
            public static bool operator ==(Connector a, Connector b) => a.field == b.field && a.node == b.node;
            public static bool operator !=(Connector a, Connector b) => a.field != b.field || a.node != b.node;

        }
    }
}