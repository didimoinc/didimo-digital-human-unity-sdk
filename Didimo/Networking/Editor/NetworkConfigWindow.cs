using System.Text;
using System.Text.RegularExpressions;
using DigitalSalmon;
using DigitalSalmon.Extensions;
using DigitalSalmon.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace Didimo.Networking
{
    public class NetworkConfigWindow : EditorWindow
    {
        protected const int PADDING       = 10;
        protected const int PADDING_SMALL = 5;
        protected static Color LABEL_BODY => Colours.GREY70;
        protected static Color LABEL_H2 => Colours.GREY90;
        
        private const string DEFAULT_PATH  = "Assets/NetworkConfig.asset";

        private static UnityEditor.Editor configEditor;
        private bool HasExistingConfig => DidimoNetworkingResources.NetworkConfig != null;

        protected void OnGUI()
        {
            Rect area = position.WithPosition(Vector2.zero);
            //DrawHeader(ref area, "Didimo Network Config", "Configure your auth and network settings here.");

            DrawConfigObjectField(ref area);

            if (!HasExistingConfig)
            {
                DrawCreateConfigWizard(ref area);
            }
            else
            {
                DrawExistingConfigEditor(ref area);
            }

           // DrawFooter(ref area);
            Repaint();
        }

        // [MenuItem("Didimo/Network Configuration")]
        // public static void ShowWindow()
        // {
        //     NetworkConfigWindow window = GetWindow<NetworkConfigWindow>();
        //     window.minSize = new Vector2(360, 400);
        //     window.maxSize = window.minSize;
        // }

        private bool TryGetConfigEditor(out UnityEditor.Editor editor)
        {
            editor = null;
            if (DidimoNetworkingResources.NetworkConfig == null) return false;

            if (configEditor != null)
            {
                if (configEditor.target == DidimoNetworkingResources.NetworkConfig)
                {
                    editor = configEditor;
                    return true;
                }
            }

            configEditor = UnityEditor.Editor.CreateEditor(DidimoNetworkingResources.NetworkConfig);
            editor = configEditor;
            return true;
        }

        private void DrawConfigObjectField(ref Rect area)
        {
            const int OBJECT_FIELD_HEIGHT = 22;
            Rect objectFieldArea = area.WithPadding(PADDING_SMALL).WithHeight(OBJECT_FIELD_HEIGHT);
            DidimoNetworkingResources.NetworkConfig = EditorGUI.ObjectField(objectFieldArea, DidimoNetworkingResources.NetworkConfig, typeof(NetworkConfig), false) as NetworkConfig;
            area = area.WithTrimY(OBJECT_FIELD_HEIGHT + PADDING_SMALL + PADDING_SMALL, RectAnchor.Bottom);
        }

        private void DrawExistingConfigEditor(ref Rect area)
        {
            GUILayout.BeginArea(area.WithPadding(PADDING));
            if (TryGetConfigEditor(out UnityEditor.Editor editor))
            {
                editor.OnInspectorGUI();
            }

            GUILayout.EndArea();
        }

        private void CreateNewConfig()
        {
            NetworkConfig config = CreateInstance<NetworkConfig>();
            string path = AssetDatabase.GenerateUniqueAssetPath(DEFAULT_PATH);
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();

            DidimoNetworkingResources.NetworkConfig = AssetDatabase.LoadAssetAtPath<NetworkConfig>(path);
            DidimoNetworkingResources.NetworkConfig.BaseURL = NetworkConfig.DEFAULT_BASEURL;
            DidimoNetworkingResources.NetworkConfig.DownloadRoot = NetworkConfig.DEFAULT_DOWNLOADROOT;
        }

        private void ImportConfig()
        {
            string path = EditorUtility.OpenFilePanel("Import Network Config", Application.dataPath, "txt");
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            byte[] configData = File.ReadAllBytes(path);
            string configStr = Encoding.UTF8.GetString(configData);
            if (string.IsNullOrEmpty(configStr)) return;

            if (TryReadConfigStr(configStr, out string apiKey, out string secret))
            {
                CreateNewConfig();

                if (HasExistingConfig)
                {
                    DidimoNetworkingResources.NetworkConfig.ApiKey = apiKey;
                }
            }
        }

        private bool TryReadConfigStr(string configStr, out string apiKey, out string secret)
        {
            apiKey = string.Empty;
            secret = string.Empty;

            Match apiKeyMatch = Regex.Match(configStr, "Key: \"(.*)\"");
            if (apiKeyMatch.Success)
            {
                apiKey = apiKeyMatch.Groups[1].Value;
            }

            Match secretMatch = Regex.Match(configStr, "Secret: \"(.*)\"");
            if (secretMatch.Success)
            {
                secret = secretMatch.Groups[1].Value;
            }

            return !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(secret);
        }

        private void DrawCreateConfigWizard(ref Rect area)
        {
            Rect textArea = area.WithPadding(PADDING);

            GUILayout.BeginArea(textArea);
            Style.GUI.LayoutLabel("No existing network configuration asset found.", LABEL_BODY);
            GUILayout.EndArea();

            area = area.WithTrimY(32, RectAnchor.Bottom);

            if (GUILayout.Button( "Create New Network Auth Config"))
            {
                CreateNewConfig();
            }

            if (GUILayout.Button("Import Auth Config"))
            {
                ImportConfig();
            }

            area = area.WithTrimY(PADDING, RectAnchor.Bottom);

            GUILayout.Label( "OR");

            textArea = area.WithPadding(PADDING);

            GUILayout.BeginArea(textArea);
            Style.GUI.LayoutLabel("In your Project window, Right click", LABEL_BODY);
            Style.GUI.LayoutLabel("Create | Didimo | Network Config", LABEL_H2);
            GUILayout.Space(PADDING_SMALL);
            Style.GUI.LayoutLabel("Assign your created config object to", LABEL_BODY);
            Style.GUI.LayoutLabel("Didimo | Content | Resources | DidimoResources", LABEL_H2);
            GUILayout.EndArea();
        }
    }
}