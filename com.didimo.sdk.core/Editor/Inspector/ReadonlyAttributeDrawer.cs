using Didimo.Core.Inspector;
using UnityEditor;
using UnityEngine;

namespace Didimo.Editor.Inspector
{
    /// <summary>
    /// Read-Only attribute to show on the inspector.
    /// Allows you to show any property on the editor that
    /// is grayed out and cannot be changed. 
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadonlyAttribute))]
    public class ReadonlyAttributeDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool previousGUIEnabledState = GUI.enabled;
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = previousGUIEnabledState;
            EditorGUI.EndProperty();
        }
    }
}