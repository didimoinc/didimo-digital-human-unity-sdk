using Didimo;
using UnityEditor;
using UnityEngine;

namespace Didimo
{
    [CustomPropertyDrawer(typeof(IrisPreset))]
    public class IrisPresetDrawer : PropertyDrawer
    {
        const int   ICON_SIZE = 64;
        const float MARGIN    = 2;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            int rowCount = Mathf.RoundToInt(pos.width / (ICON_SIZE + MARGIN));
            float chooserHeight = ICON_SIZE + 2.0f * rowCount;
            EditorGUILayout.BeginVertical(GUILayout.MinHeight(chooserHeight + 70));
            EditorGUILayout.BeginHorizontal();
            EditorGUI.LabelField(pos, label);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.Height(chooserHeight));
            Vector2 rectSize = new Vector2(ICON_SIZE, ICON_SIZE);
            Rect cr = EditorGUILayout.GetControlRect();
            Rect selRect = new Rect();
            Rect crect = new Rect(cr.position, rectSize);
            EditorGUI.BeginChangeCheck();

            Texture2D[] irises;

            try
            {
                irises = IrisDatabase.Irises;
            }
            catch
            {
                irises = new Texture2D[0];
            }

            for (int i = 0; i < irises.Length; ++i)
            {
                var oldColor = GUI.backgroundColor;
                var oldContentColor = GUI.contentColor;

                GUI.contentColor = Color.white; // PresetValues.presets[i].color;
                GUI.backgroundColor = Color.white;
                SerializedProperty myvalue = prop.FindPropertyRelative("value");

                GUI.backgroundColor *= 2;

                if (myvalue.intValue == i)
                    selRect = crect;
                if (GUI.Button(crect, new GUIContent("", irises[i], irises[i].ToString())))
                {
                    myvalue.intValue = i;
                }

                GUI.backgroundColor = oldColor;
                GUI.contentColor = oldContentColor;
                crect = new Rect(new Vector2(crect.x + rectSize.x + MARGIN, crect.y), rectSize);
                if (crect.xMax > pos.width)
                {
                    crect.x = pos.x;
                    crect.y += (float) ICON_SIZE + MARGIN;
                }
            }

            DrawHelpers.DrawOutlineRect(selRect, Color.white, 2);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                DidimoIrisController IrisControl = (DidimoIrisController) prop.serializedObject.targetObject;
                if (IrisControl)
                {
                    prop.serializedObject.ApplyModifiedProperties();
                    IrisControl.ApplyPreset();
                }
            }
        }
    }
}