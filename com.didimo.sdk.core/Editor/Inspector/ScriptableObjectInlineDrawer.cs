using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace Didimo.Editor.Inspector
{
    // InlineTest = your ScriptableObject
    // [UnityEditor.CustomPropertyDrawer(typeof(InlineTest), true)]
    // public class InlineTest_Drawer : ScriptableObjectInlineDrawer<InlineTest> { }

    public class ScriptableObjectInlineDrawer<T> : PropertyDrawer where T : ScriptableObject
    {
        protected virtual DrawStyle drawStyle => DrawStyle.Folder;
        protected virtual bool newInherited => false;

        static float LineHeight => EditorGUIUtility.singleLineHeight;
        static float Spacing => EditorGUIUtility.standardVerticalSpacing;
        const float ButtonWidth = 44;

        string getPath(SerializedProperty property, ScriptableObject data)
        {
            if (data) return AssetDatabase.GetAssetPath(data);
            var target = property.serializedObject.targetObject as UnityEngine.Component;
            return target && target.gameObject.scene != null ? target.gameObject.scene.path : "Assets";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (GUI.enabled)
            {
                int c = property.objectReferenceValue == null ? 1 : 2;
                Rect r1 = new Rect(position) { width = position.width - ButtonWidth * c + 1, height = LineHeight };
                EditorGUI.ObjectField(r1, property, label);

                Rect rb = new Rect(position) { x = r1.xMax, width = c == 1 ? ButtonWidth : ButtonWidth * 2 - 1, height = LineHeight };
                switch (GUI.Toolbar(rb, -1, new string[] { "New", "Clone" }.Take(c).ToArray()))
                {
                    case 0: newData(property, typeof(T)); break;
                    case 1: setData(property, CloneInstance(property)); break;
                }
                showInEditor(position, property);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? iterateVisibleProperties(new Rect(), property) : base.GetPropertyHeight(property, label);
        }

        public void newData(SerializedProperty property, Type type) =>
            setData(property, CreateInstance(type, getPath(property, GetObject(property))));

        void setData(SerializedProperty property, ScriptableObject data)
        {
            if (data == null) return;
            property.serializedObject.Update();
            assign(property, data);
            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.ApplyModifiedProperties();
            RefreshSelection();
        }

        static void RefreshSelection()
        {
            var objects = Selection.objects.ToArray();
            Selection.activeObject = null;
            EditorApplication.delayCall += () => Selection.objects = objects;
        }

        void showInEditor(Rect position, SerializedProperty property)
        {
            if (!property.objectReferenceValue) return;

            void draw(int indent = 0)
            {
                var backupIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel += indent;
                iterateVisibleProperties(position, property, (rect, prop) => EditorGUI.PropertyField(rect, prop, true));
                EditorGUI.indentLevel = backupIndent;
            }

            switch (drawStyle)
            {
                case DrawStyle.None:
                    break;
                case DrawStyle.Folder:
                    if (property.isExpanded = EditorGUI.Foldout(new Rect(position) { height = LineHeight }, property.isExpanded, GUIContent.none))
                        draw(1);
                    break;
                case DrawStyle.Open:
                    draw();
                    break;
            }
        }

        static float iterateVisibleProperties(Rect position, SerializedProperty property, Action<Rect, SerializedProperty> action = null)
        {
            Rect pos = new Rect(position) { y = position.y + LineHeight + Spacing };
            if (property.objectReferenceValue && property.isExpanded)
            {
                var data = property.objectReferenceValue as ScriptableObject;
                if (data == null) return LineHeight;

                SerializedObject serializedObject = new SerializedObject(data);
                SerializedProperty prop = serializedObject.GetIterator();

                for (bool firstTime = true; prop.NextVisible(firstTime); firstTime = false)
                {
                    if (prop.name == "m_Script")
                        continue;
                    pos.height = EditorGUI.GetPropertyHeight(serializedObject.FindProperty(prop.name), null, true) + Spacing;
                    action?.Invoke(pos, prop);
                    pos.y += pos.height + Spacing;
                }

                if (GUI.changed)
                    serializedObject.ApplyModifiedProperties();

            }
            return pos.y - position.y;
        }

        public virtual void assign(SerializedProperty property, ScriptableObject objectReferenceValue) =>
            property.objectReferenceValue = objectReferenceValue;

        protected enum DrawStyle { None, Folder, Open, }

        static string getAssetPath(string path) => "Assets" + path.Substring(Application.dataPath.Length);

        public static ScriptableObject CreateInstance(Type type, string path = "Assets")
        {
            string name = type.Name;
            path = EditorUtility.SaveFilePanel("Save " + name, path, "New " + name, "asset");
            if (path.Length > 0)
            {
                var instance = ScriptableObject.CreateInstance(type);
                var assetPath = getAssetPath(path);
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return instance;
            }
            return null;
        }

        public static ScriptableObject CloneInstance(SerializedProperty property, string path = "")
        {
            ScriptableObject target = GetObject(property);
            string name = target.GetType().Name;
            path = EditorUtility.SaveFilePanel("Save " + name, path == "" ? AssetDatabase.GetAssetPath(target) : path, target.name, "asset");
            if (path.Length > 0)
            {
                var assetPath = AssetDatabase.GetAssetPath(target);
                var newAssetPath = getAssetPath(path);
                if (AssetDatabase.CopyAsset(assetPath, newAssetPath))
                {
                    var instance = AssetDatabase.LoadAssetAtPath(newAssetPath, target.GetType());
                    return (ScriptableObject)instance;
                }
            }
            return null;
        }

        const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        ///<summary> Get target object from SerializedProperty</summary>
        static ScriptableObject GetObject(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                    obj = GetValue(obj, element.Substring(0, element.IndexOf("[")),
                        System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", "")));
                else obj = GetValue(obj, element);
            }
            return obj as ScriptableObject;
        }

        static object GetValue(object source, string name)
        {
            if (source == null) return null;
            for (var type = source.GetType(); type != null; type = type.BaseType)
            {
                var f = type.GetField(name, Flags);
                if (f != null) return f.GetValue(source);
            }
            return null;
        }

        static object GetValue(object source, string name, int index)
        {
            var enumerator = (GetValue(source, name) as IEnumerable)?.GetEnumerator();
            for (int i = 0; i <= index; i++)
                if (!enumerator.MoveNext()) return null;
            return enumerator.Current;
        }
    }
}
