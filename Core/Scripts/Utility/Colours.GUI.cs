using UnityEngine;

namespace DigitalSalmon
{
    public static partial class Colours
    {
        public class GUI
        {
            public const  string PREF_KEY    = "Editor_ProSkin";
            public static bool   IsDarkTheme = PlayerPrefs.GetInt(PREF_KEY) == 1;
            public static Color  LABEL_BOLD  = IsDarkTheme ? GREY95 : GREY22;

            // Node Graph

            public static Color CANVAS_BACKGROUND => IsDarkTheme ? GREY08 : GREY35;
            public static Color CANVAS_BACKGROUND_ACCENT => IsDarkTheme ? GREY10 : GREY30;

            public static Color ELEMENT_BACKGROUND => IsDarkTheme ? GREY12 : GREY70;

            public static Color ELEMENT_FEATURE_DEFAULT => IsDarkTheme ? GREY18 : GREY55;
            public static Color ELEMENT_FEATURE_ACCENT => BLUE;
            public static Color ELEMENT_FEATURE_HOVERED => IsDarkTheme ? GREY28 : GREY40;
            public static Color ELEMENT_FEATURE_BRIGHT => IsDarkTheme ? GREY60 : WHITE;

            public static Color ELEMENT_SECTION_BACKGROUND => IsDarkTheme ? GREY14 : GREY75;
            public static Color ELEMENT_HEADER_BACKGROUND => IsDarkTheme ? GREY08 : GREY85;

            public static Color LABEL_HEADER => IsDarkTheme ? WHITE : GREY02;
            public static Color LABEL_SUBHEADER => IsDarkTheme ? GREY80 : GREY05;
            public static Color LABEL_BODY => IsDarkTheme ? GREY70 : GREY10;

            public static Color ICON_STRONG => IsDarkTheme ? GREY80 : GREY15;
            public static Color ICON_WEAK => IsDarkTheme ? GREY40 : GREY65;

            public static Color INSPECTOR_BACKGROUND => IsDarkTheme ? GREY20 : GREY80;
            public static Color INSPECTOR_BACKGROUND_HIGHLIGHT => IsDarkTheme ? GREY16 : GREY85;

            public static Color INSPECTOR_LINE_VERYWEAK => IsDarkTheme ? WHITE_05 : BLACK_05;
            public static Color INSPECTOR_LINE_WEAK => IsDarkTheme ? WHITE_10 : BLACK_10;

            public static Color LABEL => IsDarkTheme ? GREY70 : GREY16;
            public static Color LABEL_INACTIVE => IsDarkTheme ? GREY65 : GREY50;

            public static Color BUTTON => IsDarkTheme ? GREY26 : GREY35;
            public static Color BUTTON_HOVERED => IsDarkTheme ? GREY28 : GREY30;

            public static Color PRIMARY => BLUE;
        }
    }
}