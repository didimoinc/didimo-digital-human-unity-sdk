using Didimo.Editor.Inspector;
using UnityEditor;
using UnityEngine;

namespace Didimo.Networking
{
    [CustomEditor(typeof(NetworkDemo))]
    [CanEditMultipleObjects]
    public class NetworkDemoEditor : DidimoInspector
    {
        private NetworkDemo networkDemo;

        public bool ShouldUpdateProgressBar => networkDemo && !string.IsNullOrEmpty(networkDemo.ProgressMessage);

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (ShouldUpdateProgressBar)
            {
                Rect r = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(r, networkDemo.Progress / 100f, networkDemo.ProgressMessage);
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return ShouldUpdateProgressBar;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            networkDemo = target as NetworkDemo;
        }
    }
}