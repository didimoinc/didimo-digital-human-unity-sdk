using Didimo.Core.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Didimo.Builder;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Didimo.Core.Editor
{
    public class CoreDidimoManagerTab : DidimoManagerTab
    {
        private const string CORRECT = "Rendering settings are correct. " + "Your didimos will render as intended.";

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
            EditorGUILayout.HelpBox("If you had issues importing didimos" + " into the project, click the button below.", MessageType.Info);

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

        public static async void OpenMeetADidimo()
        {
            Sample? sample = await PackageUtility.GetSample("com.didimo.sdk.core", "Meet A Didimo");
            bool isImported = sample != null && sample!.Value.isImported;
            if (!isImported && sample != null)
            {
                if (!PackageUtility.ImportSample(sample!.Value))
                {
                    return;
                }
            }

            if (sample != null && PackageUtility.LoadSceneFromSample(sample!.Value, "MeetADidimo.unity"))
            {
EditorUtility.DisplayDialog("Re-open Scene",
                    "There is a Unity bug where the first time we open this scene, it doesn't de-serialize properly. If you see errors in the console, please exit play mode, re-open the (MeetADidimo) scene manually , and hit play to see didimos in action.",
                    "OK");

                EditorApplication.EnterPlaymode();
            }
        }

        public override string GetTabName() => "Getting Started";

        public override int GetIndex() => 0;

        public static void ReimportAllDidimos()
        {
            List<string> allDidimoPaths = Directory.GetFiles("Assets", "*.gltf", SearchOption.AllDirectories).ToList();
            List<string> fbxDidimoPaths = Directory.GetFiles("Assets", "*.fbx", SearchOption.AllDirectories).ToList();
            fbxDidimoPaths.RemoveAll(s => !DidimoImporterJsonConfigUtils.CheckIfJsonExists(s));
            allDidimoPaths.AddRange(fbxDidimoPaths);
            foreach (var packageRoot in new[] {"Packages", "Library/PackageCache"})
            {
                foreach (var package in Directory.EnumerateDirectories(packageRoot))
                {
                    if (package.Replace("\\", "/").StartsWith(packageRoot + "/com.didimo"))
                    {
                        allDidimoPaths.AddRange(Directory.GetFiles(package, "*.gltf", SearchOption.AllDirectories));
                        fbxDidimoPaths = Directory.GetFiles(package, "*.fbx", SearchOption.AllDirectories).ToList();
                        fbxDidimoPaths.RemoveAll(s => !DidimoImporterJsonConfigUtils.CheckIfJsonExists(s));
                        allDidimoPaths.AddRange(fbxDidimoPaths);
                    }
                }
            }

            foreach (string didimoPath in allDidimoPaths)
            {
                if (didimoPath.Replace("\\", "/").Contains("~/")) return;
                // if (didimoPath.Replace('\\', '/').StartsWith("Packages/com.didimo"))
                // {
                AssetDatabase.ImportAsset(didimoPath, ImportAssetOptions.ForceUpdate);
                // }
            }
        }
    }
}