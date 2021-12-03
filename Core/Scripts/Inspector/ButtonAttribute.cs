using System;

namespace Didimo.Core.Inspector
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ButtonAttribute : Attribute
    {
        public readonly string Text;

        public ButtonAttribute() { }
        public ButtonAttribute(string text) { Text = text; }
    }
}