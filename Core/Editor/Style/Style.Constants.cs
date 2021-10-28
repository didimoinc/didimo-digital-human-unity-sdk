using UnityEngine;
using UGUI = UnityEngine.GUI;
using UGUILayout = UnityEngine.GUILayout;

namespace DigitalSalmon.UI
{
    public static partial class Style
    {
        //-----------------------------------------------------------------------------------------
        // Constants:
        //-----------------------------------------------------------------------------------------

        public const int DIVIDER_HEIGHT = 5;

        public const int PADDING_SMALL  = 2;
        public const int PADDING        = 5;
        public const int PADDING_DOUBLE = PADDING * 2;
        public const int PADDING_LARGE  = 15;

        public const int BUTTON_HEIGHT = 34;

        //-----------------------------------------------------------------------------------------
        // Delegates:
        //-----------------------------------------------------------------------------------------

        public delegate void DrawEventHandler(ref Rect area);
    }
}