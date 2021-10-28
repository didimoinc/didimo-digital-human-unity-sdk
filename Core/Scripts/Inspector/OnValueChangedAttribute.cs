using UnityEngine;

namespace Didimo.Inspector
{
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public string methodName;

        public OnValueChangedAttribute(string methodName) { this.methodName = methodName; }
    }
}