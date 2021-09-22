using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class GUILayoutHrefLabel
{
    private static GUIStyle _style;

    private static GUIStyle Style
    {
        get
        {
            if (_style == null)
            {
                _style = new GUIStyle(EditorStyles.wordWrappedLabel);
                Style.richText = true;
            }

            return _style;
        }
    }

    private readonly GUIContent            content;
    private readonly System.Action<string> onClickHref;

    public static void Draw(string text, System.Action<string> customOnClickHref = null)
    {
        GUILayout.Label(text, Style);
        Rect rect = GUILayoutUtility.GetLastRect();

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            int stringIndex = Style.GetCursorStringIndex(rect, new GUIContent(text), Event.current.mousePosition);
            string url = GetUrlOnHrefAtPosition(text, stringIndex);
            if (url != null)
            {
                if (customOnClickHref != null)
                {
                    customOnClickHref(url);
                }
                else
                {
                    Application.OpenURL(url);
                }
            }
        }
    }

    // regular expression to match '<a href="URL">TEXT</a>' expression
    private static readonly Regex regex = new Regex("<a href=\\\".+?<\\/a>");

    // Regular expression to match the url part of the expression above
    private static readonly Regex regexUrl = new Regex("\"(.*?)\"");

    static string GetUrlOnHrefAtPosition(string text, int position)
    {
        // Debug.Log(text.Length);
        if (position <= 0 || position >= text.Length) return null;

        Match match = regex.Match(text);
        while (match.Success)
        {
            if (position >= match.Index && position < match.Index + match.Length)
            {
                Match matchUrl = regexUrl.Match(match.Value);
                if (matchUrl.Success)
                {
                    Group group = matchUrl.Groups[1]; // Our regular expression has a single group
                    Capture capture = group.Captures[0];
                    return capture.Value;
                }
            }

            match = match.NextMatch();
        }

        return null;
    }
}