using System.Reflection;
using UnityEngine;

namespace DigitalSalmon.UI
{
    public static partial class Style
    {
        public class Buttons
        {
            //-----------------------------------------------------------------------------------------
            // Backing Fields:
            //-----------------------------------------------------------------------------------------

            private static StyleGroup _serviceMenuButtonGroup;

            //-----------------------------------------------------------------------------------------
            // Public StyleGroups:
            //-----------------------------------------------------------------------------------------

            public static StyleGroup ServiceMenuButtonGroup
            {
                get
                {
                    if (_serviceMenuButtonGroup != null) return _serviceMenuButtonGroup;

                    Texture2D normalBackgroundTexture = Textures.FlatTexture(Color.clear);

                    GUIStyle baseStyle = new GUIStyle(GUIStyle.none)
                    {
                        border = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(22, 22, 2, 0),
                        padding = new RectOffset(8, 8, 4, 4),
                        contentOffset = new Vector2(0, 3),
                        fontSize = 11,
                        normal = {background = normalBackgroundTexture},
                        hover = {background = normalBackgroundTexture, textColor = Colours.LIGHT_BLUE}
                    };

                    _serviceMenuButtonGroup = new StyleGroup(baseStyle);

                    _serviceMenuButtonGroup.SetColor(Colours.GREY50, Colours.BLUE);
                    _serviceMenuButtonGroup.SetFontWeight(FontWeights.Bold);

                    return _serviceMenuButtonGroup;
                }
            }

            //-----------------------------------------------------------------------------------------
            // Public GUIStyles:
            //-----------------------------------------------------------------------------------------

            public static GUIStyle MenuButton => ServiceMenuButtonGroup.Style;

            public static GUIStyle MenuButtonHighlighted => ServiceMenuButtonGroup.Instance(nameof(MenuButtonHighlighted)).SetColor(Colours.LIGHT_BLUE).Style;

            public static GUIStyle MenuButtonCentered => ServiceMenuButtonGroup.Instance(nameof(MenuButtonCentered)).SetAnchor(TextAnchor.MiddleCenter).Style;

            public static GUIStyle MenuButtonHighlightedCentered => ServiceMenuButtonGroup.Instance(nameof(MenuButtonHighlightedCentered)).SetAnchor(TextAnchor.MiddleCenter).SetColor(Colours.LIGHT_BLUE).Style;

            //-----------------------------------------------------------------------------------------
            // Public Methods:
            //-----------------------------------------------------------------------------------------

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static void ClearCache()
            {
                FieldInfo[] fields = typeof(Buttons).GetFields(BindingFlags.NonPublic | BindingFlags.Static);

                foreach (FieldInfo field in fields)
                {
                    if (field.IsLiteral || field.IsInitOnly) continue;
                    field.SetValue(null, null);
                }
            }
        }
    }
}