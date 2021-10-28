using System;
using System.Linq;
using System.Reflection;
using Didimo.Inspector;
using UnityEditor;
using UnityEngine;

namespace Didimo.Editor.Inspector
{
    [CustomPropertyDrawer(typeof(OnValueChangedAttribute))]
    public class OnValueChangedAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);

            if (EditorGUI.EndChangeCheck())
            {
                CallOnValueChangedCallbacks(property);
            }

            EditorGUI.EndProperty();
        }

        public static MethodInfo GetMethod(object target, string methodName) { return ReflectionUtility.GetAllMethods(target, m => m.Name.Equals(methodName, StringComparison.InvariantCulture)).FirstOrDefault(); }

        public static void CallOnValueChangedCallbacks(SerializedProperty property)
        {
            OnValueChangedAttribute[] onValueChangedAttributes = ReflectionUtility.GetAttributes<OnValueChangedAttribute>(property);
            if (onValueChangedAttributes.Length == 0)
            {
                return;
            }

            object target = ReflectionUtility.GetTargetObjectWithProperty(property);
            property.serializedObject.ApplyModifiedProperties(); // We must apply modifications so that the new value is updated in the serialized object

            foreach (OnValueChangedAttribute onValueChangedAttribute in onValueChangedAttributes)
            {
                MethodInfo callbackMethod = GetMethod(target, onValueChangedAttribute.methodName);
                if (callbackMethod != null && callbackMethod.ReturnType == typeof(void) && callbackMethod.GetParameters().Length == 0)
                {
                    callbackMethod.Invoke(target, new object[] { });
                }
                else
                {
                    string warning = string.Format("{0} can invoke only methods with 'void' return type and 0 parameters", onValueChangedAttribute.GetType().Name);

                    Debug.LogWarning(warning, property.serializedObject.targetObject);
                }
            }
        }
    }
}