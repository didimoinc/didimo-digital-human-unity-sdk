using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Didimo.Networking
{
    public class NetworkingDidimoManagerTab : DidimoManagerTab
    {
        private Task<(bool success, AccountStatusResponse response)> accountStatusTask = null;

        private Vector2 scrollPosition;

        public override void Draw(DidimoManager manager)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.Label("The NetworkConfig asset shown below will be used as configuration on API calls, for the Networking module.", EditorStyles.wordWrappedLabel);

            if (!DidimoNetworkingResources.NetworkConfig)
            {
                DidimoNetworkingResources.NetworkConfig = NetworkConfig.CreateDefault();
            }

            NetworkConfig networkConfig = DidimoNetworkingResources.NetworkConfig;
            UnityEditor.Editor networkConfigEditor = UnityEditor.Editor.CreateEditor(networkConfig);
            networkConfigEditor.OnInspectorGUI();

            bool hasApiUrl = !string.IsNullOrEmpty(DidimoNetworkingResources.NetworkConfig.BaseURL);
            bool hasApiKey = !string.IsNullOrEmpty(DidimoNetworkingResources.NetworkConfig.ApiKey);

            if (!hasApiUrl)
            {
                EditorGUILayout.HelpBox("Please set the API base URL.", MessageType.Error);
            }

            if (!hasApiKey)
            {
                EditorGUILayout.HelpBox("Please set the API Key. One can be created through the Customer Portal.",
                    MessageType.Error);
            }

            if (hasApiKey && hasApiUrl)
            {
                if (networkConfig.GetFeaturesForApi().Count == 0)
                {
                    EditorGUILayout.HelpBox(
                        "To be able to create didimos, you must first select a collection of features. Click the button below to get the list of available features for your account.",
                        MessageType.Error);
                }

                GUILayout.Space(PADDING_SMALL);
                if (GUILayout.Button("Get Available Account Features"))
                {
                    GetAccountNewDidiimoParameters();
                }
            }
            GUILayoutHrefLabel.Draw($"To manage your API Keys, go to the <a href=\"{UsefulLinks.CUSTOMER_PORTAL}\">Customer Portal</a>.");


            GUILayout.Space(PADDING_SMALL);
            GUILayoutHrefLabel.Draw("To explore an example on how to interact with the Didimo API, <a href=\"\">open the Network Demo Scene</a>. " +
                                    "Please note that this API module is meant for demo purposes only, and shouldn't be distributed with public apps as it will have embedded API Keys.",
                url =>
                {
                    OpenNetworkDemoScene();
                });

            GUILayout.Space(PADDING_SMALL);
            GUILayoutHrefLabel.Draw(
                $"For documentation on how to use the \"Network Demo Scene\", go to the documentation page on the <a href=\"{UsefulLinks.CREATING_A_DIDIMO_DEVELOPER_PORTAL}\">Developer Portal</a>.");

            GUILayout.EndScrollView();
        }

        private void OpenNetworkDemoScene()
        {
            string path = "Assets/Didimo/Networking/Examples/Scenes/NetworkDemo.unity";
            SceneAsset obj = AssetDatabase.LoadAssetAtPath(path, typeof(SceneAsset)) as SceneAsset;
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
        }

        private async void GetAccountNewDidiimoParameters()
        {
            if (accountStatusTask != null)
            {
                return;
            }

            try
            {
                accountStatusTask = Api.Instance.AccountStatus();
                await accountStatusTask;
            }
            catch (System.Exception)
            {
                accountStatusTask = null;
            }

            if (accountStatusTask != null && accountStatusTask.Result.success)
            {
                EditorUtility.DisplayDialog("API Configuration", "Features updated with success.", "OK");
                DidimoNetworkingResources.NetworkConfig.SetFeatures(accountStatusTask.Result.response.newDidimoFeatures);
            }
            else
            {
                EditorUtility.DisplayDialog("API Configuration",
                    "Your API configuration seems to be incorrect. Please verify that the base URL and API Key are correct. Check the Console for logs on the issue.",
                    "OK");
            }

            accountStatusTask = null;
        }

        public override string GetTabName() => "Networking";

        public override int GetIndex() => 1;
    }
}