using System;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MenuPathAttribute : Attribute
    {
        public string path;
        public MenuPathAttribute(string path) =>
            this.path = path;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class WidthAttribute : Attribute
    {
        public float width;
        public WidthAttribute(float width) =>
            this.width = width;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HeaderColorAttribute : Attribute
    {
        public Color color;
        public HeaderColorAttribute(string htmlColor) => this.color = ColorUtility.TryParseHtmlString(htmlColor, out Color color) ? color : Color.magenta;
        public HeaderColorAttribute(Color color) => this.color = color;
        public HeaderColorAttribute(float r, float g, float b, float a) => this.color = new Color(r, g, b, a);

    }
}
