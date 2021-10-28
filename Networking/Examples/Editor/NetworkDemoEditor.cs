using Didimo.Editor.Inspector;
using UnityEditor;
using UnityEngine;

namespace Didimo.Networking
{
    [CustomEditor(typeof(NetworkDemo))]
    [CanEditMultipleObjects]
    public class NetworkDemoEditor : DidimoInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            NetworkDemo networkDemo = serializedObject.targetObject as NetworkDemo;
            if (networkDemo)
            {
                if (!string.IsNullOrEmpty( networkDemo.ProgressMessage))
                {
                    Rect r = EditorGUILayout.BeginVertical();
                    EditorGUI.ProgressBar(r, networkDemo.Progress / 100f, networkDemo.ProgressMessage);
                    GUILayout.Space(18);
                    EditorGUILayout.EndVertical();
                }
            }
        }
    }
}