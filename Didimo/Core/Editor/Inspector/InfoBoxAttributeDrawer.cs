using Didimo.Inspector;
using UnityEditor;
using UnityEngine;

namespace Didimo.Editor.Inspector
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxAttributeDrawer : DecoratorDrawer
    {
        private InfoBoxAttribute InfoBoxAttribute => (InfoBoxAttribute) attribute;

        public override void OnGUI(Rect position)
        {
            Rect titleRect = position;
            EditorGUI.HelpBox(titleRect, InfoBoxAttribute.Title, (MessageType) InfoBoxAttribute.MessageType);
        }

        public override float GetHeight()
        {
            GUIStyle style = GUI.skin.box;
            float infoBoxHeight = style.CalcHeight(new GUIContent(InfoBoxAttribute.Title), Screen.width);
            return infoBoxHeight;
        }
    }
}