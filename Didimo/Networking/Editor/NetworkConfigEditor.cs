using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Didimo.Networking
{
    [CustomEditor(typeof(NetworkConfig))]
    [CanEditMultipleObjects]
    public class NetworkConfigEditor : UnityEditor.Editor
    {
        private static bool showFeatures = true;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            NetworkConfig networkConfig = target as NetworkConfig;
            if (!networkConfig) return;

            showFeatures = EditorGUILayout.BeginFoldoutHeaderGroup(showFeatures, "Features");
            if (showFeatures)
            {
                foreach (AccountStatusResponse.NewDidimoFeatures.BooleanFeature feature in networkConfig.Features.BooleanFeatures)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    feature.Enabled = EditorGUILayout.Toggle(feature.FeatureName, feature.Enabled);
                    EditorGUILayout.EndHorizontal();
                }

                foreach (AccountStatusResponse.NewDidimoFeatures.ToggleFeature feature in networkConfig.Features.ToggleFeatures)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    feature.ToggledOn = EditorGUILayout.Toggle(feature.FeatureName, feature.ToggledOn);
                    EditorGUILayout.EndHorizontal();
                }

                foreach (AccountStatusResponse.NewDidimoFeatures.MultiOptionFeature feature in networkConfig.Features.MultiOptionFeatures)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    feature.SelectedOption = EditorGUILayout.Popup(feature.FeatureName, feature.SelectedOption, feature.Options.ToArray());
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}