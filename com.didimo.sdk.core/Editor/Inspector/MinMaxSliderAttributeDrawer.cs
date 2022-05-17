using Didimo.Core.Inspector;
using UnityEditor;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Didimo.Editor.Inspector
{
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderAttributeDrawer : PropertyDrawer
    {
        public static class Constants
        {
            public const float FLOAT_FIELD_WIDTH_RATIO = 0.2f;
            public const float MAX_FLOAT_FIELD_WIDTH   = 75f;

            public const float FIELD_SPACING_RATIO = 0.05f;
            public const float MAX_FIELD_SPACING   = 10f;
        }

        MinMaxSliderAttribute att => (MinMaxSliderAttribute) attribute;

        float getHeight(int lines) => (lines - 1) * EditorGUIUtility.standardVerticalSpacing + lines * EditorGUIUtility.singleLineHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Vector2:
                    return base.GetPropertyHeight(property, label);
                default:
                    SerializedProperty minProperty = property.FindPropertyRelative("min");
                    SerializedProperty maxProperty = property.FindPropertyRelative("max");
                    if (minProperty != null && maxProperty != null)
                    {
                        if (minProperty.propertyType == SerializedPropertyType.Vector2 && maxProperty.propertyType == SerializedPropertyType.Vector2)
                            return property.isExpanded ? getHeight(3) : getHeight(1);
                        else if (minProperty.propertyType == SerializedPropertyType.Vector3 && maxProperty.propertyType == SerializedPropertyType.Vector3)
                            return property.isExpanded ? getHeight(4) : getHeight(1);
                        else if (minProperty.propertyType == SerializedPropertyType.Float && maxProperty.propertyType == SerializedPropertyType.Float)
                            return getHeight(1);
                    }

                    break;
            }

            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    property.floatValue = EditorGUI.Slider(position, label, property.floatValue, att.MIN, att.MAX);
                    break;

                case SerializedPropertyType.Integer:
                    property.intValue = (int) EditorGUI.Slider(position, label, (float) property.intValue, att.MIN, att.MAX);
                    break;

                case SerializedPropertyType.Vector2:
                    DrawMinMaxSlider(position, label, property.FindPropertyRelative("x"), property.FindPropertyRelative("y"));
                    break;

                default:
                    SerializedProperty minProperty = property.FindPropertyRelative("min");
                    SerializedProperty maxProperty = property.FindPropertyRelative("max");

                    if (minProperty != null && maxProperty != null)
                    {
                        if (minProperty.propertyType == SerializedPropertyType.Vector2 && maxProperty.propertyType == SerializedPropertyType.Vector2)
                        {
                            DrawMinMaxSlider(position, label, property, minProperty, maxProperty, new string[] {"x", "y"});
                        }
                        else if (minProperty.propertyType == SerializedPropertyType.Vector3 && maxProperty.propertyType == SerializedPropertyType.Vector3)
                        {
                            DrawMinMaxSlider(position, label, property, minProperty, maxProperty, new string[] {"x", "y", "z"});
                        }
                        else if (minProperty.propertyType == SerializedPropertyType.Float && maxProperty.propertyType == SerializedPropertyType.Float)
                        {
                            DrawMinMaxSlider(position, label, minProperty, maxProperty);
                        }
                    }
                    else
                    {
                        EditorGUI.LabelField(position, "MinMaxSlider attribute is only available for float, integer, Vector2, or Vector3 types.");
                    }

                    break;
            }

            EditorGUI.EndProperty();
        }

        private void DrawMinMaxSlider(Rect position, GUIContent label, SerializedProperty property, SerializedProperty minProperty, SerializedProperty maxProperty, string[] fields)
        {
            position.height = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, property.displayName))
            {
                using (var indent = new EditorGUI.IndentLevelScope(1))
                {
                    foreach (string field in fields)
                    {
                        position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                        DrawMinMaxSlider(position, new GUIContent(field), minProperty.FindPropertyRelative(field), maxProperty.FindPropertyRelative(field));
                    }
                }
            }
        }

        private void DrawMinMaxSlider(Rect position, GUIContent label, SerializedProperty minProperty, SerializedProperty maxProperty)
        {
            Rect controlRect = EditorGUI.PrefixLabel(position, label);

            float floatFieldWidth = Mathf.Min(Constants.FLOAT_FIELD_WIDTH_RATIO * controlRect.width, Constants.MAX_FLOAT_FIELD_WIDTH);
            float fieldSpacing = Mathf.Min(Constants.FIELD_SPACING_RATIO * controlRect.width, Constants.MAX_FIELD_SPACING);

            Rect leftFloatRect = new Rect(controlRect.x, controlRect.y, floatFieldWidth, controlRect.height);
            Rect minMaxSliderRect = new Rect(controlRect.x + floatFieldWidth + fieldSpacing,
                controlRect.y,
                controlRect.width - (floatFieldWidth + fieldSpacing) * 2,
                controlRect.height);
            Rect rightFloatRect = new Rect(controlRect.x + controlRect.width - floatFieldWidth, controlRect.y, floatFieldWidth, controlRect.height);

            // EditorGUI.BeginChangeCheck();

            using (var indent = new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel))
            {
                float minValue = minProperty.floatValue, maxValue = maxProperty.floatValue;
                EditorGUI.MinMaxSlider(minMaxSliderRect, ref minValue, ref maxValue, att.MIN, att.MAX);
                minProperty.floatValue = EditorGUI.FloatField(leftFloatRect, minValue);
                maxProperty.floatValue = EditorGUI.FloatField(rightFloatRect, maxValue);
            }

            // if (EditorGUI.EndChangeCheck())
            // {
            //     property.vector2Value = new Vector2(vectorMin, vectorMax);
            // }
        }
    }
}