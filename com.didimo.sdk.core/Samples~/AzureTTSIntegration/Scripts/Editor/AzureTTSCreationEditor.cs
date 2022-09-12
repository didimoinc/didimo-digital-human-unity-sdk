#if !UNITY_WEBGL
using UnityEngine;
using UnityEditor;
namespace Didimo.Core.Examples.AzureTTSIntegration
{
    [CustomEditor(typeof(AzureTTSCreation))]
    public class AzureTTSCreationEditor : UnityEditor.Editor
    {
        // Layout Config Constants
        private const float SPACING = 5;

        public AzureTTSCreation azureTTSCreation;

        void OnEnable()
        {
            azureTTSCreation = (AzureTTSCreation)target;
        }

        public override async void OnInspectorGUI()
        {
            EditorGUILayout.Space(SPACING);

            // Show default script like default InspectorGUI
            GUI.enabled = false;
            try
            {
                EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(azureTTSCreation), typeof(AzureTTSCreation), false);
            }
            catch (UnityEngine.ExitGUIException)
            {
                // Warning: Unity has an issue with async OnInspectorGUI and MonoScript.FromMonoBehaviour and it can throw an ExitGUIException.
                // They recommend throwing this exception, to be caught by them later, but this is not the case in the async version.
                // From our tests, returning here before drawing anything on this loop causes no issue.
                return;
            }
            finally
            {
                GUI.enabled = true;
            }

            EditorGUILayout.Space(SPACING);

            EditorGUILayout.LabelField("API Connection Details", EditorStyles.boldLabel);
            azureTTSCreation.yourSubscriptionKey = EditorGUILayout.TextField("Your Subscription Key", azureTTSCreation.yourSubscriptionKey);
            azureTTSCreation.yourServiceRegion = EditorGUILayout.TextField("Your Service Region", azureTTSCreation.yourServiceRegion);


            EditorGUILayout.Space(SPACING);

            EditorGUILayout.LabelField("File Information", EditorStyles.boldLabel);
            azureTTSCreation.fileName = EditorGUILayout.TextField("File Name", azureTTSCreation.fileName);
            azureTTSCreation.audioFilePath = EditorGUILayout.TextField("Audio File Path", azureTTSCreation.audioFilePath);
            azureTTSCreation.animationFilePath = EditorGUILayout.TextField("Animation File Path", azureTTSCreation.animationFilePath);


            EditorGUILayout.Space(SPACING);

            EditorGUILayout.LabelField("Creation Mode", EditorStyles.boldLabel);
            azureTTSCreation.ttsCreationMode = (AzureTTSCreation.TTSMode)EditorGUILayout.EnumPopup("TTS Source", azureTTSCreation.ttsCreationMode);
            EditorGUI.indentLevel++;
            switch (azureTTSCreation.ttsCreationMode)
            {
                case AzureTTSCreation.TTSMode.CreateFromString:
                    azureTTSCreation.textSample = EditorGUILayout.TextField("Text Sample", azureTTSCreation.textSample);
                    azureTTSCreation.voiceName = EditorGUILayout.TextField("Voice Name", azureTTSCreation.voiceName);
                    break;

                case AzureTTSCreation.TTSMode.CreateFromSSML:
                    azureTTSCreation.ssmlFile = EditorGUILayout.TextField("SSML File", azureTTSCreation.ssmlFile);
                    break;
            }
            EditorGUI.indentLevel--;


            EditorGUILayout.Space(SPACING * 2);

            if (GUILayout.Button("Create TTS Files"))
            {
                await azureTTSCreation.CreateTTSFiles();
            }
        }
    }
}

#endif