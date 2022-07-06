using UnityEngine;
using Didimo.AssetFitter.Editor.Graph;

namespace Didimo.Core.Editor
{
    public class AssetFitterDidimoManagerTab : DidimoManagerTab
    {
        private Vector2 scrollPosition;
        static ControllerEditor assetFitterControllerEditor;

        public override void Draw(DidimoManager manager)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            Didimo.AssetFitter.Editor.Graph.Controller assetFitterController = ControllerLoader.LoadDefault();

            if (!assetFitterControllerEditor)
            {
                assetFitterControllerEditor = (ControllerEditor)ControllerEditor.CreateEditor(assetFitterController);
            }

            GUILayout.BeginVertical(GUILayout.Width(600));
            assetFitterControllerEditor.DrawEditor();
            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        public override string GetTabName() => "Asset Fitter";
        public override int GetIndex() => 3;
    }
}
