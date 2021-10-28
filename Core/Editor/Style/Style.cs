using System.Reflection;
using DigitalSalmon.Extensions;
using UnityEngine;

namespace DigitalSalmon.UI
{
    public static partial class Style
    {
        //-----------------------------------------------------------------------------------------
        // Backing Fields:
        //-----------------------------------------------------------------------------------------

        private static StyleGroup _hrStyleGroup;

        public static GUIStyle Hr => HrStyleGroup.Instance(nameof(Hr)).SetBackgroundColor(Colours.GREY50).Style;

        public static GUIStyle HrBright => HrStyleGroup.Instance(nameof(HrBright)).SetBackgroundColor(Color.white.WithAlpha(0.2f)).Style;

        public static GUIStyle HrDark => HrStyleGroup.Instance(nameof(HrDark)).SetBackgroundColor(Color.black.WithAlpha(0.2f)).Style;

        //-----------------------------------------------------------------------------------------
        // Decorators:
        //-----------------------------------------------------------------------------------------

        private static StyleGroup HrStyleGroup
        {
            get
            {
                if (_hrStyleGroup != null) return _hrStyleGroup;

                GUIStyle baseStyle = new GUIStyle(GUIStyle.none) {fixedHeight = 1, stretchWidth = true};

                _hrStyleGroup = new StyleGroup(baseStyle);

                return _hrStyleGroup;
            }
        }

        //-----------------------------------------------------------------------------------------
        // Private Properties:
        //-----------------------------------------------------------------------------------------

        private static bool IsLinear => QualitySettings.activeColorSpace == ColorSpace.Linear;

        //-----------------------------------------------------------------------------------------
        // Protected Methods:
        //-----------------------------------------------------------------------------------------

        public static void ClearCache()
        {
            FieldInfo[] fields = typeof(Style).GetFields(BindingFlags.NonPublic | BindingFlags.Static);

            foreach (FieldInfo field in fields)
            {
                if (field.IsLiteral || field.IsInitOnly) continue;
                field.SetValue(null, null);
            }

            StyleGroup.ClearCache();

            Buttons.ClearCache();
            Text.ClearCache();
            Box.ClearCache();
        }
    }
}