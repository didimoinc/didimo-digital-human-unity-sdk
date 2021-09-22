
using Didimo;
using UnityEditor;
using UnityEngine;


public class DrawHelpers
    {
        static public void DrawOutlineRect(Rect bounds, Color color, float border)
        {
            float h = bounds.height;
            float w = bounds.width;
            EditorGUI.DrawRect(new Rect(bounds.x, bounds.y, border, h), Color.white);
            EditorGUI.DrawRect(new Rect(bounds.xMax - border, bounds.y, border, h), Color.white);
            w -= border * 2;
            float x1 = bounds.x + border;
            EditorGUI.DrawRect(new Rect(x1, bounds.y, w, border), Color.white);
            EditorGUI.DrawRect(new Rect(x1, bounds.yMax - border, w, border), Color.white);
        }
    }  

    [CustomPropertyDrawer(typeof(HairPreset))]
    public class HairPresetColorDrawer : PropertyDrawer
    {
        const int ICON_SIZE = 32;
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUI.LabelField(pos, label);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.Height(34));
            Vector2 rectSize = new Vector2(ICON_SIZE, ICON_SIZE);
            Rect cr = EditorGUILayout.GetControlRect();
            const float margin = 2;
            Rect selRect = new Rect();
            Rect crect = new Rect(cr.position, rectSize);
            //EditorGUILayout.BeginArea
            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < PresetValues.presets.Length; ++i)
            {          
                var oldColor = GUI.backgroundColor;
                var oldContentColor = GUI.contentColor;
                 
                GUI.contentColor = Color.white;// PresetValues.presets[i].color;
                GUI.backgroundColor =  PresetValues.presets[i].color;
                SerializedProperty myenum = prop.FindPropertyRelative("value");
             
                GUI.backgroundColor *= 2;
                
                if (myenum.enumValueIndex == i)
                    selRect = crect;
                if (GUI.Button(crect, new GUIContent("", ((EHairPresetColor)i).ToString())))
                {
                    myenum.enumValueIndex = i;
                                  
                }
                GUI.backgroundColor = oldColor;
                GUI.contentColor = oldContentColor;                                
                crect = new Rect(new Vector2(crect.x + rectSize.x + margin, crect.y), rectSize);
            }
            DrawHelpers.DrawOutlineRect(selRect, Color.white, 2);            
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Hair hair = (Hair)prop.serializedObject.targetObject;
                if (hair)
                {
                    //myenum.serializedObject.Update();
                    prop.serializedObject.ApplyModifiedProperties();
                    hair.ApplyPreset();
                }
            }
        }
    }
