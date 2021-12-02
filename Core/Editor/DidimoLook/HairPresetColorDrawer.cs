using UnityEditor;
using UnityEngine;
using Didimo.Core.Deformables;
using Didimo.Core.Config;

namespace Didimo.Core.Editor
{
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
        SerializedProperty activeProperty = null;
        HairLayerDatabaseGroupEntry activeHairDB = null;
        void SaveAsPreset()
        {
            if (activeHairDB != null)
            {
                Hair hair = (Hair)activeProperty.serializedObject.targetObject;
                if (hair)
                {
                    var hairPresetDatabase = UnityEngine.Resources
                        .Load<HairPresetDatabase>("HairPresetDatabase");

                    activeHairDB.SetEntry(hair.innerHairLayer, "", HairLayer.Inner);
                    activeHairDB.SetEntry(hair.outerHairLayer, "", HairLayer.Outer);
                    activeProperty.serializedObject.Update();
                    activeProperty.serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(hairPresetDatabase);
                    activeProperty.serializedObject.UpdateIfRequiredOrScript();
                }
            }
        }

        void SaveAsMeshPreset()
        {
            if (activeHairDB != null)
            {
                Hair hair = (Hair)activeProperty.serializedObject.targetObject;
                if (hair)
                {
                    var hairPresetDatabase = UnityEngine.Resources
                        .Load<HairPresetDatabase>("HairPresetDatabase");
                    var hairpieceName = HairLayerSettings.GetHairIDFromObject(hair);
                    activeHairDB.SetEntry(hair.innerHairLayer, hairpieceName, HairLayer.Inner);
                    activeHairDB.SetEntry(hair.outerHairLayer, hairpieceName, HairLayer.Outer);
                    activeProperty.serializedObject.Update();
                    activeProperty.serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(hairPresetDatabase);
                    activeProperty.serializedObject.UpdateIfRequiredOrScript();
                }
            }
        }

        const int ICON_SIZE = 32;
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            bool changeOccured = false;
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
            EditorGUI.BeginChangeCheck();

            var hairPresetDatabase = UnityEngine.Resources
                .Load<HairPresetDatabase>("HairPresetDatabase");

            if (hairPresetDatabase.Hairs != null)
            {
                Event e = Event.current;
                for (int i = 0; i < hairPresetDatabase.Hairs.Length; ++i)
                {
                    var oldColor = GUI.backgroundColor;
                    var oldContentColor = GUI.contentColor;
                    GUI.contentColor = Color.white;
                    var hdb = hairPresetDatabase.Hairs[i];
                    var hairPresetEntry = hdb.list.Count > 0 ? hdb.list[0] : null;
                    Color ic = hairPresetEntry != null ? hairPresetEntry.color : new Color(0.0f, 0.0f, 0.0f, 0.0f);
                    Color c = new Color(ic.r, ic.g, ic.b, 1.0f);
                    GUI.backgroundColor = c;
                    SerializedProperty myenum = prop.FindPropertyRelative("value");
                    GUI.backgroundColor *= 2;
                    if (myenum.intValue == i)
                        selRect = crect;
                    if (e.button == 1 && crect.Contains(e.mousePosition))
                    {
                        activeProperty = prop;
                        activeHairDB = hdb;
                        GenericMenu context = new GenericMenu();
                        context.AddItem(new GUIContent("Save setting"), false, SaveAsPreset);
                        context.AddItem(new GUIContent("Save setting as hairpiece specific"), false, SaveAsMeshPreset);
                        context.ShowAsContext();
                    }
                    if (GUI.Button(crect, new GUIContent("", hdb.name)))
                    {
                        if (e.button == 0)
                        {
                            changeOccured = true;
                            myenum.intValue = i;
                        }
                    }
                    GUI.backgroundColor = oldColor;
                    GUI.contentColor = oldContentColor;
                    crect = new Rect(new Vector2(crect.x + rectSize.x + margin, crect.y), rectSize);
                }
            }
            DrawHelpers.DrawOutlineRect(selRect, Color.white, 2);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                if (changeOccured)
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
    }

}