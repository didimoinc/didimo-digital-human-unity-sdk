using Didimo.Inspector;
using UnityEditor;
using UnityEngine;

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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            MinMaxSliderAttribute att = (MinMaxSliderAttribute) attribute;

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    property.floatValue = EditorGUI.Slider(position, label, property.floatValue, att.MIN, att.MAX);
                    break;

                case SerializedPropertyType.Integer:
                    float value = property.intValue;
                    property.intValue = (int) EditorGUI.Slider(position, label, value, att.MIN, att.MAX);
                    break;

                case SerializedPropertyType.Vector2:
                    Rect controlRect = EditorGUI.PrefixLabel(position, label);

                    float floatFieldWidth = Mathf.Min(Constants.FLOAT_FIELD_WIDTH_RATIO * controlRect.width, Constants.MAX_FLOAT_FIELD_WIDTH);
                    float fieldSpacing = Mathf.Min(Constants.FIELD_SPACING_RATIO * controlRect.width, Constants.MAX_FIELD_SPACING);

                    Rect leftFloatRect = new Rect(controlRect.x, controlRect.y, floatFieldWidth, controlRect.height);
                    Rect minMaxSliderRect = new Rect(controlRect.x + floatFieldWidth + fieldSpacing, controlRect.y, controlRect.width - (floatFieldWidth + fieldSpacing) * 2, controlRect.height);
                    Rect rightFloatRect = new Rect(controlRect.x + controlRect.width - floatFieldWidth, controlRect.y, floatFieldWidth, controlRect.height);

                    EditorGUI.BeginChangeCheck();
                    float vectorMin = property.vector2Value.x;
                    float vectorMax = property.vector2Value.y;
                    vectorMin = EditorGUI.FloatField(leftFloatRect, vectorMin);
                    vectorMax = EditorGUI.FloatField(rightFloatRect, vectorMax);
                    EditorGUI.MinMaxSlider(minMaxSliderRect, ref vectorMin, ref vectorMax, att.MIN, att.MAX);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.vector2Value = new Vector2(vectorMin, vectorMax);
                    }

                    break;

                default:
                    EditorGUI.LabelField(position, "MinMaxSlider attribute is only available for float, integer or Vector2 variables.");
                    break;
            }

            EditorGUI.EndProperty();
        }
    }
}