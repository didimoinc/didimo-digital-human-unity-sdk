using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if USING_UNITY_URP
using UnityEngine.Rendering.Universal;

#endif

namespace Didimo
{
    public class CoreDidimoManagerTab : DidimoManagerTab
    {
        public override void Draw(DidimoManager manager)
        {
            bool urpPackageInstalled = true;
            bool usingURPRenderPipelineAsset = false;
#if !USING_UNITY_URP
            urpPackageInstalled = false;
            EditorGUILayout.HelpBox("didimos can only be rendered with URP. Please install the URP package.", MessageType.Error);
#else
            usingURPRenderPipelineAsset = GraphicsSettings.renderPipelineAsset != null && typeof(UniversalRenderPipelineAsset) == GraphicsSettings.renderPipelineAsset.GetType();
#endif

            if (!usingURPRenderPipelineAsset)
            {
                EditorGUILayout.HelpBox("didimos can only be rendered with URP. Please select a UniversalRenderPipelineAsset in the graphics settings.", MessageType.Error);
            }

            if (urpPackageInstalled && usingURPRenderPipelineAsset)
            {
                EditorGUILayout.HelpBox("Rendering settings are correct. Your didimos will be rendered properly.", MessageType.Info);
            }

            GUILayout.Space(PADDING);

            GUILayout.Label("Thank you for using the Didimo Unity SDK.", EditorStyles.wordWrappedLabel);

            GUILayoutHrefLabel.Draw($"• You can manage your account from the <a href=\"{UsefulLinks.CUSTOMER_PORTAL}\">Customer Portal</a>.");

            GUILayoutHrefLabel.Draw($"• For a guide on how to get started with this SDK, visit our <a href=\"{UsefulLinks.DEVELOPER_PORTAL}\"> Developer Portal</a>.");

            GUILayoutHrefLabel.Draw("• To check the capabilities of a didimo avatar, <a href=\"\">open the Meet a didimo</a> scene.",
                url =>
                {
                    MeetADidimo();
                });
        }

        private void MeetADidimo()
        {
            string path = "Assets/Didimo/Core/Examples/MeetADidimo/MeetADidimo.unity";
            SceneAsset obj = AssetDatabase.LoadAssetAtPath(path, typeof(SceneAsset)) as SceneAsset;
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
            EditorApplication.isPlaying = true;
        }

        public override string GetTabName() => "Getting Started";

        public override int GetIndex() => 0;
    }
}