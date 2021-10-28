using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Didimo.Inspector;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Didimo.Editor.Inspector
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), true)]
    public class DidimoInspector : UnityEditor.Editor
    {
        private        IEnumerable<MethodInfo> _methods;
        private static GUIStyle                _buttonStyle;

        protected virtual void OnEnable() { _methods = ReflectionUtility.GetAllMethods(target, m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0); }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DrawButtons();
        }

        public static void Button(Object target, MethodInfo methodInfo)
        {
            if (methodInfo.GetParameters().All(p => p.IsOptional))
            {
                ButtonAttribute buttonAttribute = (ButtonAttribute) methodInfo.GetCustomAttributes(typeof(ButtonAttribute), true)[0];
                string buttonText = string.IsNullOrEmpty(buttonAttribute.Text) ? ObjectNames.NicifyVariableName(methodInfo.Name) : buttonAttribute.Text;

                _buttonStyle ??= new GUIStyle(GUI.skin.button) {richText = true};

                if (GUILayout.Button(buttonText, _buttonStyle))
                {
                    object[] defaultParams = methodInfo.GetParameters().Select(p => p.DefaultValue).ToArray();
                    IEnumerator methodResult = methodInfo.Invoke(target, defaultParams) as IEnumerator;

                    if (!Application.isPlaying)
                    {
                        // Set target object and scene dirty to serialize changes to disk
                        EditorUtility.SetDirty(target);

                        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
                        if (stage != null)
                        {
                            // Prefab mode
                            EditorSceneManager.MarkSceneDirty(stage.scene);
                        }
                        else
                        {
                            // Normal scene
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                        }
                    }
                    else if (methodResult != null && target is MonoBehaviour behaviour)
                    {
                        behaviour.StartCoroutine(methodResult);
                    }
                }

                EditorGUI.EndDisabledGroup();
            }
            else
            {
                string warning = typeof(ButtonAttribute).Name + " works only on methods with no parameters";
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }
        }

        protected void DrawButtons()
        {
            if (_methods.Any())
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel);

                foreach (MethodInfo method in _methods)
                {
                    Button(serializedObject.targetObject, method);
                }
            }
        }
    }
}