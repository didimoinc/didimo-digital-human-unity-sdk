using UnityEditor;
using UnityEngine;

namespace Didimo.Core.Editor
{
    public class CheckColorSpace : IProjectSettingIssue
    {
        private const string REASON =
            "didimos should be rendered with linear color space.\n" +
            "Please go to Player Settings → Other Settings → Rendering " +
            "and select 'Linear' for the color space.";

        public bool CheckOk()
        {
            if (PlayerSettings.colorSpace == ColorSpace.Linear)
            {
                return true;
            }

            EditorGUILayout.HelpBox(REASON, MessageType.Error);

            return false;
        }

        public void Resolve()
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
        }

        public string ResolveText()
        {
            return REASON;
        }
    }
}