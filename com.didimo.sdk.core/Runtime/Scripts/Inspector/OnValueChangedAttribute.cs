using UnityEngine;

namespace Didimo.Core.Inspector
{
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public string methodName;

        public OnValueChangedAttribute(string methodName) { this.methodName = methodName; }
    }
}