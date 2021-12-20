using Didimo.Core.Utility;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Didimo.Core.Editor
{
    public class CoreDidimoManagerTab : DidimoManagerTab
    {
        private const string CORRECT =
            "Rendering settings are correct. " +
            "Your didimos will render as intended.";

        public override void Draw(DidimoManager manager)
        {
            CheckProjectSetup();

            GUILayout.Space(PADDING);
            GUILayout.Label("Thank you for using the Didimo Unity SDK.", EditorStyles.wordWrappedLabel);
            GUILayoutHrefLabel.Draw($"• You can manage your account from the <a href=\"{UsefulLinks.CUSTOMER_PORTAL}\">Customer Portal</a>.");
            GUILayoutHrefLabel.Draw($"• For a guide on how to get started with this SDK, visit our <a href=\"{UsefulLinks.DEVELOPER_PORTAL}\"> Developer Portal</a>.");
            GUILayoutHrefLabel.Draw($"• For the Quick Start Guide: ARKit LiveCapture, visit <a href=\"{UsefulLinks.ARKIT_GUIDE}\"> Mocap - Didimo</a>.");
            GUILayoutHrefLabel.Draw("• To check the capabilities of a didimo avatar, <a href=\"\">open the Meet a didimo</a> scene.",
                url =>
                {
                    OpenMeetADidimo();
                });

            GUILayout.Label(" ");
            EditorGUILayout.HelpBox("If you had issues importing didimos" +
                " into the project, click the button below.", MessageType.Info);

            if (GUILayout.Button("Reimport didimos"))
            {
                ReimportAllDidimos();
            }
        }

        public bool CheckProjectSetup()
        {
            var checks = new List<IProjectSettingIssue>();

            checks.Add(new CheckUrpInstalled());
            checks.Add(new CheckDidimosImported());
            checks.Add(new CheckUrpSelected());
            checks.Add(new CheckColorSpace());

            foreach (IProjectSettingIssue check in checks)
            {
                if (check.CheckOk() == false)
                {
                    FixIssueButton(check);
                    return false;
                }
            }

            EditorGUILayout.HelpBox(CORRECT, MessageType.Info);
            return true;
        }

        public void FixIssueButton(IProjectSettingIssue check)
        {
            if (GUILayout.Button("Fix Above Issue"))
            {
                check.Resolve();
            }
        }

        public void OpenMeetADidimo()
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

        public static void ReimportAllDidimos()
        {
            IEnumerable<string> allDidimoPaths = Directory.GetFiles("Assets",
                "*.gltf", SearchOption.AllDirectories);

            foreach (string didimoPath in allDidimoPaths)
            {
                if (didimoPath.Replace('\\', '/').StartsWith("Assets/Didimo"))
                {
                    AssetDatabase.ImportAsset(didimoPath, ImportAssetOptions.ForceUpdate);
                }
            }
        }
    }
}