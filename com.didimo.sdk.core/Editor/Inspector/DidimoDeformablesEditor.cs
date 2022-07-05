using System.Collections;
using System.Collections.Generic;
using Didimo.Core.Deformables;
using UnityEditor;
using UnityEngine;

namespace Didimo.Core.Editor
{
    [CustomEditor(typeof(DidimoDeformables))]
    public class DidimoDeformablesEditor : UnityEditor.Editor
    {
        // Target script
        private DidimoDeformables didimoDeformables;

        private static bool     editorDeformablesFoldout = true;
        private static GUIStyle deformableFoldoutStyle;

        // Layout Config Constants
        private const float SPACING        = 5;
        private const int   TAB_WIDTH      = 15;
        private const int   BUTTON_PADDING = 5;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Control existing deformables
            EditorGUILayout.Space(SPACING);
            deformableFoldoutStyle ??= new GUIStyle(EditorStyles.foldout) {fontStyle = FontStyle.Bold};
            editorDeformablesFoldout = EditorGUILayout.Foldout(editorDeformablesFoldout, "Current Deformables", true, deformableFoldoutStyle);

            if (editorDeformablesFoldout)
            {
                EditorGUI.indentLevel++;
                if (didimoDeformables.deformables.Count == 0) EditorGUILayout.HelpBox("No deformables have been added/found", MessageType.Info);
                else
                {
                    EditorGUILayout.BeginVertical();
                    foreach ((string deformableName, Deformable deformable) in didimoDeformables.deformables)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(TAB_WIDTH * EditorGUI.indentLevel);
                        EditorGUILayout.BeginHorizontal(EditorStyles.textField);
                        EditorGUILayout.LabelField(deformableName);
                        if (GUILayout.Button("Select")) Selection.objects = new[] {(Object) deformable.gameObject};
                        if (GUILayout.Button("Remove"))
                        {
                            didimoDeformables.DestroyDeformable(deformable);
                            Repaint();
                            return;
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(TAB_WIDTH * EditorGUI.indentLevel);
                if (GUILayout.Button("Refresh/Update current deformables")) didimoDeformables.UpdateDeformablesList();
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(SPACING);
            }

            // Add deformable section (Use DidimoHairPieceSelector for now)
        }

        public void OnEnable()
        {
            didimoDeformables = (DidimoDeformables) target;
        }

    }
}