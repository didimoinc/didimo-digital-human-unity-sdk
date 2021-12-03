using Didimo.Core.Inspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Didimo.Core.Editor.Inspector
{
    [CustomPropertyDrawer(typeof(ListToPopupAttribute))]
    public class ListToPopupDrawer : PropertyDrawer
    {
        private int selectedIndex = 0;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ListToPopupAttribute.IListToPopup listToPopup = property.serializedObject.targetObject as ListToPopupAttribute.IListToPopup;
            if (listToPopup == null)
            {
                EditorGUI.LabelField(position, "ListToPopupDrawer attribute requires the class to implement ListToPopupAttribute.IListToPopup.");
                return;
            }

            if (!string.Equals(property.type, "int", StringComparison.InvariantCultureIgnoreCase))
            {
                EditorGUI.LabelField(position, "ListToPopupDrawer attribute only works for integer variables");
                return;
            }

            List<string> values = listToPopup.ListToPopupGetValues();
            if (values != null && values.Any())
            {
                EditorGUI.BeginProperty(position, label, property);
                selectedIndex = EditorGUI.Popup(position, property.displayName, selectedIndex, values.ToArray());
                EditorGUI.EndProperty();
                property.intValue = selectedIndex;
            }
        }
    }
}