using UnityEngine;
using Didimo.AssetFitter.Editor.Graph;
using UnityEditor;

namespace Didimo.Core.Editor
{
    public class AssetFitterDidimoManagerTab : DidimoManagerTab
    {
        private Vector2 scrollPosition;
        static ControllerEditor assetFitterControllerEditor;

        public override void Draw(DidimoManager manager)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
            Didimo.AssetFitter.Editor.Graph.Controller assetFitterController = ControllerLoader.LoadDefault();

            if (!assetFitterControllerEditor)
            {
                assetFitterControllerEditor = (ControllerEditor)ControllerEditor.CreateEditor(assetFitterController);
            }
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.padding = new RectOffset(0, 100, 0, 0);
            style.margin = new RectOffset(0, 50, 0, 0);

            GUILayout.BeginHorizontal(style, GUILayout.MaxWidth(200));
            GUILayout.BeginVertical();
            GUILayout.Label(" ", GUILayout.MaxWidth(20));
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            assetFitterControllerEditor.DrawEditor();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        public override string GetTabName() => "Asset Fitter";
        public override int GetIndex() => 3;
    }
}
