using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Didimo.Networking;
using Didimo.Core.Utility;
using NUnit.Framework;
using UnityEditor.PackageManager.UI;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Reflection;
using System;
using System.Linq;
using Didimo.Core.Config;

namespace Didimo.Core.Editor
{
    public class RenderPipelineDidimoManagerTab : DidimoManagerTab
    {
        private Task<(bool success, AccountStatusResponse response)> accountStatusTask = null;

        const string HDRPpackagename = "com.unity.render-pipelines.high-definition";
        public override void OnActivated() 
        {
            DetermineHDRPPackage();
        }
        SearchRequest sr = null;
        ListRequest lr = null;
        static AddRequest PackageInstallRequest = null;


        private Vector2 scrollPosition;
        private Vector2 lrscrollPosition;
        bool hasHDRPPackage = false;
       
        public enum InstanceScopeChoice
        {
            AllInstancesInScene,
            SelectedInstancesOnly,
            AllInstancesInProject,            
        };
        string[] instanceScopeChoiceString = new string[] { "all instances in scene", "selected instances only", "all instances in project" };
        public enum AvatarModeChoice
        {
            DoNothing,
            ReImportAvatar,
            CreateMaterials,
            ApplyOrCreateMaterials,
        }
        string[] avatarModeChoiceString = new string[] { "do nothing", "re-import", "create new pipeline appropraite materials" , "apply existing or create new pipeline appropraite materials" };

        int lightModeChoice = 1;
        float lightScaleFactor = 10000.0f;
        string[] lightModeChoiceString = new string[] { "do nothing", "multiply intensity by ", "divide intensity by " };

        InstanceScopeChoice instanceScope = InstanceScopeChoice.AllInstancesInScene;
        AvatarModeChoice avatarMode = AvatarModeChoice.ReImportAvatar;


        public void DetermineHDRPPackage()
        {
            sr = Client.Search(HDRPpackagename);
            lr = Client.List();            
        }

        public void MultiplyLightIntensity(InstanceScopeChoice instanceScope, float scaleFactor)
        {
            var gameObjects = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.GetComponent<Light>() != null).ToArray();
            foreach (var go in gameObjects)
            {
                Light l = go.GetComponent<Light>();
                if (l != null)
                    l.intensity *= scaleFactor;
            }
        }
        public void PerformInstanceActions(EPipelineType pipeline, InstanceScopeChoice instanceScope, AvatarModeChoice avatarMode)
        {
            if (avatarMode != AvatarModeChoice.DoNothing)
            {
                GameObject[] gameObjects = null;
                switch (instanceScope)
                {
                    case InstanceScopeChoice.AllInstancesInScene:
                        gameObjects = GameObject.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.GetComponent<DidimoComponents>() != null).ToArray();
                        break;
                    case InstanceScopeChoice.SelectedInstancesOnly:
                        gameObjects = Selection.gameObjects.Select(g => g as GameObject).Where(g => g.GetComponent<DidimoComponents>() != null).ToArray();
                        break;
                }


#if UNITY_EDITOR
                switch (avatarMode)
                {
                    case AvatarModeChoice.ReImportAvatar:
                    
                    var mpathlist = MeshUtils.GetUniqueMeshAssetPaths(gameObjects);
                    foreach(var mpath in mpathlist)
                    {                       
                       AssetDatabase.ImportAsset(mpath);
                    }
                    foreach (var g in gameObjects)
                    {
                        var obj = PrefabUtility.GetCorrespondingObjectFromSource(g);
                        if (obj) 
                        {
                            PrefabUtility.RevertPrefabInstance(g, InteractionMode.AutomatedAction); //ensure material overrides are remembered - these could be from the wrong render pipeline. TODO: this will have to become more sophisticated if override preservation is desired.
                        }
                    }

                    break;
                    case AvatarModeChoice.CreateMaterials:
                        EditorMaterialUtility.FixUpMaterialsToCurrentPipeline(gameObjects, true);break;
                    case AvatarModeChoice.ApplyOrCreateMaterials:
                        EditorMaterialUtility.FixUpMaterialsToCurrentPipeline(gameObjects, false);break;
                }
                #endif
            }
        }


        public override void Draw(DidimoManager manager)
        {
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.Label("Here you can switch the current project between URP & HDRP and manage didimo assets appropriately", EditorStyles.wordWrappedLabel);
            GUILayout.Space(PADDING_SMALL);
#if SHOW_ALL_PACKAGES
            if (lr != null && lr.Result != null && lr.Status == StatusCode.Success)
            {
                lrscrollPosition = GUILayout.BeginScrollView(lrscrollPosition, GUILayout.Height(Screen.height / 5));
                //lr.
                PackageCollection pc = lr.Result;
                foreach (var p in pc)
                {
                    var ptype = p.type;
                    GUILayout.Label(p.name);
                }
               
                GUILayout.EndScrollView();
            }
#endif
            bool HDRP_installed = false;
            if (sr != null)
            {

                if (sr.Status == UnityEditor.PackageManager.StatusCode.Failure)
                {
                    string message = "In order to switch to the HDRP render pipeline, the HDRP unity package needs to be installed. Press the button bellow to remedy this. ";
                    if (sr.Result != null)                    
                        message += "\n Resukt message was: " + sr.Result.ToString();
                    
                    if (sr.Error != null)
                        message += "\n Error message was: " + sr.Error.message;
                    EditorGUILayout.HelpBox(message,
                    MessageType.Error);
                    if (GUILayout.Button("Install HDRP"))
                    {
                        PackageInstallRequest = Client.Add(HDRPpackagename);                        
                        EditorApplication.update += PackageInstallProgress;
                    }
                }
                else
                {
                    if (sr.Result != null)
                    {
                        EditorGUILayout.HelpBox("HDRP packages installed", MessageType.None);
                        HDRP_installed = true;
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("No search result yet", MessageType.Info);
                    }
                }
            }
            if (HDRP_installed)
            {

                GUILayout.Space(PADDING_SMALL);
                var PipelineID = ResourcesLoader.GetAppropriateID();
                var OtherID = (EPipelineType)((int)PipelineID ^ 1);

                GUILayout.Space(PADDING_SMALL);
                GUILayout.BeginHorizontal();
                GUILayout.Label("For each didimo avatar ", GUILayout.ExpandWidth(false));

                if (GUILayout.Button(avatarModeChoiceString[(int)avatarMode], GUILayout.ExpandWidth(false)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddSeparator("");
                    var values = (AvatarModeChoice[])Enum.GetValues(typeof(AvatarModeChoice));
                    foreach (var value in values)
                    {
                        menu.AddItem(new GUIContent(avatarModeChoiceString[(int)value]), avatarMode == value, (v) => avatarMode = value, null); ;
                    }
                    menu.ShowAsContext();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(PADDING_SMALL);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Apply to ", GUILayout.ExpandWidth(false));
                if (GUILayout.Button(instanceScopeChoiceString[(int)instanceScope], GUILayout.ExpandWidth(false)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddSeparator("");
                    var values = (InstanceScopeChoice[])Enum.GetValues(typeof(InstanceScopeChoice));
                    foreach (var value in values)
                    {
                        menu.AddItem(new GUIContent(instanceScopeChoiceString[(int)value]), instanceScope == value, (v) => instanceScope = value, null); ;
                    }
                    menu.ShowAsContext();
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(PADDING_SMALL);
                GUILayout.BeginHorizontal();
                GUILayout.Label("For each light ", GUILayout.ExpandWidth(false));
                int lightPipelineMode = (PipelineID == EPipelineType.EPT_URP ? 1 : 2);
                if (GUILayout.Button(lightModeChoiceString[(lightModeChoice == 0) ? 0 : lightPipelineMode], GUILayout.ExpandWidth(false)))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent(lightModeChoiceString[0]), lightModeChoice == 0, (v) => lightModeChoice = 0, null);
                    menu.AddItem(new GUIContent(lightModeChoiceString[lightPipelineMode]), lightModeChoice == 1, (v) => lightModeChoice = 1, null);

                    menu.ShowAsContext();
                }
                if (lightModeChoice != 0)
                    lightScaleFactor = float.Parse(GUILayout.TextField(lightScaleFactor.ToString(), GUILayout.ExpandWidth(false)));
                GUILayout.EndHorizontal();
                GUILayout.Space(PADDING_SMALL);
                if (GUILayout.Button(" to " + ResourcesLoader.PipelineName[(int)OtherID]))
                {
                    ResourcesLoader.SetPipeline(OtherID);
                    PerformInstanceActions(OtherID, instanceScope, avatarMode);
                    if (lightModeChoice != 0)
                        MultiplyLightIntensity(instanceScope, (PipelineID == EPipelineType.EPT_URP ? lightScaleFactor : 1.0f / lightScaleFactor));
                }
            }

            GUILayout.EndScrollView();
        }

        static void PackageInstallProgress()
        {
            if (PackageInstallRequest.IsCompleted)
            {
                if (PackageInstallRequest.Status == StatusCode.Success)
                    Debug.Log("Installed: " + PackageInstallRequest.Result.packageId);
                else if (PackageInstallRequest.Status >= StatusCode.Failure)
                    Debug.Log(PackageInstallRequest.Error.message);

                EditorApplication.update -= PackageInstallProgress;
            }
        }


        public override string GetTabName() => "Render Pipeline";

        public override int GetIndex() => 1;
    }
}