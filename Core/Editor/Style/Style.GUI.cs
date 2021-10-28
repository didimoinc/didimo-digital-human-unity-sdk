using UnityEditor;
using UnityEngine;
using ImGUI = UnityEngine.GUI;

namespace DigitalSalmon.UI
{
    public static partial class Style
    {
        public class GUI
        {
            private static Texture2D _whiteTexture;

            private static Texture2D WhiteTexture
            {
                get
                {
                    if (_whiteTexture == null)
                    {
                        _whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    }

                    return _whiteTexture;
                }
            }
            //-----------------------------------------------------------------------------------------
            // Public Methods:
            //-----------------------------------------------------------------------------------------

            public static void Box(Rect area, Color color) { Texture(area, Texture2D.whiteTexture, color); }

            public static bool Button(Rect area) => Button(area, string.Empty);

            public static bool Button(Rect area, string label, FontWeights weight = FontWeights.Normal, Text.FontSize fontSize = Text.FontSize.Normal)
            {
                bool hovered = area.Contains(Event.current.mousePosition);
                bool clicked = hovered && Event.current.button == 0 && Event.current.type == EventType.MouseDown;

                Color backgroundColor = hovered ? Colours.GREY26 : Colours.GREY30;

                Box(area, backgroundColor);

                Color textColor = hovered ? Colours.WHITE : Colours.GREY80;
#if UNITY_EDITOR
                EditorGUIUtility.AddCursorRect(area, MouseCursor.Link);
#endif

                Label(area, label, textColor, TextAnchor.MiddleCenter, weight, fontSize);

                return clicked;
            }

            public static void Label(Rect area, string text, TextAnchor anchor = TextAnchor.UpperLeft, FontWeights weight = FontWeights.Normal, Text.FontSize fontSize = Text.FontSize.Normal) { Label(area, text, Colours.GUI.LABEL_BODY, anchor, weight, fontSize); }

            public static void Label(Rect area, string text, Color color, TextAnchor anchor = TextAnchor.UpperLeft, FontWeights weight = FontWeights.Normal, Text.FontSize fontSize = Text.FontSize.Normal) { Label(area, text, color, Text.GetStyle(anchor, weight, fontSize)); }

            public static void LayoutLabel(string text, TextAnchor anchor = TextAnchor.UpperLeft, FontWeights weight = FontWeights.Normal, Text.FontSize fontSize = Text.FontSize.Normal) { LayoutLabel(text, Colours.GUI.LABEL_BODY, anchor, weight, fontSize); }

            public static void LayoutLabel(string text, Color color, TextAnchor anchor = TextAnchor.UpperLeft, FontWeights weight = FontWeights.Normal, Text.FontSize fontSize = Text.FontSize.Normal) { LayoutLabel(text, color, Text.GetStyle(anchor, weight, fontSize)); }

            public static void LayoutLabel(string text, Color color, GUIStyle style)
            {
                Color prevColor = ImGUI.color;
                ImGUI.color = color * prevColor;
                GUILayout.Label(text, style);
                ImGUI.color = prevColor;
            }

            public static void Label(Rect area, string text, Color color, GUIStyle style)
            {
                Color prevColor = ImGUI.color;
                ImGUI.color = color * prevColor;
                ImGUI.Label(area, text, style);
                ImGUI.color = prevColor;
            }

            public static void Texture(Rect area, Texture texture)
            {
                if (texture == null) return;
                ImGUI.DrawTexture(area, texture);
            }

            public static void Texture(Rect area, Texture texture, Color tint)
            {
                Color guiColor = ImGUI.color;
                ImGUI.color = tint;
                Texture(area, texture);
                ImGUI.color = guiColor;
            }
        }
    }
}