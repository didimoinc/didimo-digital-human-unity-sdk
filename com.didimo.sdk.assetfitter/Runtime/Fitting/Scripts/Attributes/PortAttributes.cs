using System;

namespace Didimo.AssetFitter.Editor.Graph
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DirectionAttribute : Attribute
    {
        public string name;
        public bool single;
        public DirectionAttribute(string name) => this.name = name;
        public DirectionAttribute(string name, bool single)
        {
            this.name = name;
            this.single = single;
        }
    }

    public class InputAttribute : DirectionAttribute
    {
        public InputAttribute(string name) : base(name) { }
        public InputAttribute(string name, bool single) : base(name, single) { }
    }

    public class OutputAttribute : DirectionAttribute
    {
        public OutputAttribute(string name) : base(name) { }
        public OutputAttribute(string name, bool single) : base(name, single) { }
    }

    public class ExposeAttribute : Attribute
    {
        public bool showLabel;
        public ExposeAttribute(bool showLabel = true) { this.showLabel = showLabel; }
    }

    public class OutputValuesAttribute : Attribute
    {
        public string method;
        public OutputValuesAttribute(string method)
        {
            this.method = method;
        }
    }
}
