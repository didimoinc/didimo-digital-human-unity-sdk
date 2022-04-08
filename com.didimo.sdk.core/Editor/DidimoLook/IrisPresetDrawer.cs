using System;
using Didimo.Core.Config;
using UnityEditor;
using UnityEngine;

namespace Didimo.Core.Editor
{
    [CustomPropertyDrawer(typeof(IrisPreset))]
    public class IrisPresetDrawer : PropertyDrawer
    {
        const   int       ICON_SIZE = 64;
        private Texture[] irises;

        private int itemsPerRow;
        private int itemWidth;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (irises == null)
            {
                var irisDatabase = UnityEngine.Resources.Load<IrisDatabase>("IrisDatabase");
                irises = irisDatabase == null ? Array.Empty<Texture>() : irisDatabase.Irises;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.LabelField(pos, label);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();

            if (Event.current.type == EventType.Repaint)
            {
                itemsPerRow = Mathf.Max(1, (int) pos.width / ICON_SIZE);
                itemWidth = ICON_SIZE;
            }

            GUIStyle skin = new GUIStyle(GUI.skin.button);
            skin.fixedWidth = itemWidth;
            skin.fixedHeight = itemWidth;
            SerializedProperty myvalue = prop.FindPropertyRelative("value");
            myvalue.intValue = GUILayout.SelectionGrid(myvalue.intValue, irises, Mathf.Max(1, itemsPerRow), skin);
            if (EditorGUI.EndChangeCheck())
            {
                DidimoIrisController irisControl = (DidimoIrisController) prop.serializedObject.targetObject;
                if (irisControl)
                {
                    prop.serializedObject.ApplyModifiedProperties();
                    irisControl.ApplyPreset();
                }
            }

            GUI.EndScrollView();
        }
    }
}
