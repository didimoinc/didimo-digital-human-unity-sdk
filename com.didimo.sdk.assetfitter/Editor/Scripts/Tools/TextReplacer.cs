using UnityEngine;
using UnityEditor;

public class TextReplacer : EditorWindow
{
    [TextArea] public string text;
    [TextArea] public string replace;
    [TextArea] public string result;

    public Vector2 textScroll;
    public Vector2 replaceScroll;
    public Vector2 resultScroll;

    [MenuItem("Window/Text Replacer")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        TextReplacer window = (TextReplacer)EditorWindow.GetWindow(typeof(TextReplacer));
        window.Show();
    }

    void OnGUI()
    {

        GUILayout.BeginVertical(new GUIStyle { padding = new RectOffset(10, 0, 0, 10) });
        if (GUILayout.Button("Replace"))
        {
            result = text;
            foreach (var line in replace.Split("\n"))
            {
                string[] e = line.Split(",");
                result = result.Replace(e[0], e[1]);
            }
        }

        void TextArea(string label, ref string t, ref Vector2 s)
        {
            GUILayout.Label(label);
            s = EditorGUILayout.BeginScrollView(s, GUILayout.MaxHeight(200));
            t = EditorGUILayout.TextArea(t, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        TextArea("Result", ref result, ref resultScroll);
        TextArea("Text", ref text, ref textScroll);
        TextArea("Replace", ref replace, ref replaceScroll);

        GUILayout.BeginVertical();

    }
}