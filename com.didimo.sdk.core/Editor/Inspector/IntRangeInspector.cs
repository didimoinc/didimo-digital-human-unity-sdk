using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Didimo;
using UnityEditor;
using UnityEngine;

namespace Didimo.Editor.Inspector
{
    [CustomPropertyDrawer(typeof(IntRange))]
    public class IntRangeInspector : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var r = new Rect(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);
            var maxProp = property.FindPropertyRelative("_maxValue");
            var valueProp = property.FindPropertyRelative("_value");
            valueProp.intValue = Mathf.Clamp(EditorGUI.IntField(r, "Value", valueProp.intValue), 0, maxProp.intValue - 1);
        }
    }
}