using System.Collections.Generic;
using UnityEngine;

namespace DigitalSalmon.UI
{
    public static partial class Style
    {
        public class Text
        {
            //-----------------------------------------------------------------------------------------
            // Constants:
            //-----------------------------------------------------------------------------------------

            public enum FontSize
            {
                Microscopic,
                Tiny,
                Small,
                Normal,
                Large,
                Subheader,
                Header
            }

            //-----------------------------------------------------------------------------------------
            // Public StyleGroups:
            //-----------------------------------------------------------------------------------------

            private static readonly Dictionary<int, GUIStyle> styleLookup = new Dictionary<int, GUIStyle>();

            //-----------------------------------------------------------------------------------------
            // Public GUIStyles:
            //-----------------------------------------------------------------------------------------

            public static GUIStyle GetStyle(TextAnchor anchor = TextAnchor.UpperLeft, FontWeights weight = FontWeights.Normal, FontSize size = FontSize.Normal)
            {
                int hashCode = GetHashCodeForStyle(anchor, weight, size);
                if (styleLookup.ContainsKey(hashCode)) return styleLookup[hashCode];
                GUIStyle style = new GUIStyle(GUIStyle.none)
                {
                    font = GetFontFromFontGroupAndWeight(FontGroup.Default, weight),
                    alignment = anchor,
                    fontSize = GetFontSize(size),
                    padding = new RectOffset(0, 0, 0, 0),
                    margin = new RectOffset(0, 0, 0, 0),
                    wordWrap = true,
                    normal = {textColor = Color.white},
                    hover = {textColor = Color.white},
                    active = {textColor = Color.white},
                    onNormal = {textColor = Color.white},
                    onHover = {textColor = Color.white},
                    onActive = {textColor = Color.white}
                };
                styleLookup.Add(hashCode, style);
                return style;
            }

            public static int GetFontSize(FontSize size)
            {
                const int FONTSIZE_HEADER = 26;
                const int FONTSIZE_SUBHEADER = 16;
                const int FONTSIZE_LARGE = 13;
                const int FONTSIZE_NORMAL = 12;
                const int FONTSIZE_SMALL = 11;
                const int FONTSIZE_TINY = 9;
                const int FONTSIZE_MICROSCOPIC = 7;
                switch (size)
                {
                    case FontSize.Microscopic:
                        return FONTSIZE_MICROSCOPIC;
                    case FontSize.Tiny:
                        return FONTSIZE_TINY;
                    case FontSize.Small:
                        return FONTSIZE_SMALL;
                    case FontSize.Normal:
                        return FONTSIZE_NORMAL;
                    case FontSize.Large:
                        return FONTSIZE_LARGE;
                    case FontSize.Subheader:
                        return FONTSIZE_SUBHEADER;
                    case FontSize.Header:
                        return FONTSIZE_HEADER;
                }

                return FONTSIZE_NORMAL;
            }

            public static Font GetFontFromFontGroupAndWeight(FontGroup group, FontWeights weight)
            {
                switch (weight)
                {
                    case FontWeights.Light:
                        return group.Light;
                    case FontWeights.Normal:
                        return group.Standard;
                    case FontWeights.Bold:
                        return group.Bold;
                }

                return null;
            }

            //-----------------------------------------------------------------------------------------
            // Public Methods:
            //-----------------------------------------------------------------------------------------

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static void ClearCache() { styleLookup.Clear(); }

            private static int GetHashCodeForStyle(TextAnchor anchor, FontWeights weight, FontSize size)
            {
                int hashCode = 967803377;
                hashCode = hashCode * -1521134295 + anchor.GetHashCode();
                hashCode = hashCode * -1521134295 + weight.GetHashCode();
                hashCode = hashCode * -1521134295 + size.GetHashCode();
                return hashCode;
            }
        }
    }
}