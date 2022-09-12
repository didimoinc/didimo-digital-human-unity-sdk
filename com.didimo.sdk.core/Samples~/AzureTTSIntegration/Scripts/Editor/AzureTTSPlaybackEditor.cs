#if !UNITY_WEBGL
using UnityEngine;
using UnityEditor;

namespace Didimo.Core.Examples.AzureTTSIntegration
{
    [CustomEditor(typeof(AzureTTSPlayback))]
    public class AzureTTSPlaybackEditor : UnityEditor.Editor
    {
        // Layout Config Constants
        private const float SPACING = 5;

        public AzureTTSPlayback azureTTSPlayback;

        void OnEnable()
        {
            azureTTSPlayback = (AzureTTSPlayback)target;
        }

        public override async void OnInspectorGUI()
        {
            EditorGUILayout.Space(SPACING);

            // Show default script like default InspectorGUI
            GUI.enabled = false;
            try
            {
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(azureTTSPlayback), typeof(AzureTTSPlayback), false);
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

            EditorGUI.BeginChangeCheck();

            //defaults
            EditorGUILayout.Space(SPACING);
            azureTTSPlayback.didimoComponents = (DidimoComponents)EditorGUILayout.ObjectField("Didimo Componenents:", azureTTSPlayback.didimoComponents, typeof(DidimoComponents), true);
            azureTTSPlayback.audioSource = (AudioSource)EditorGUILayout.ObjectField("Audio Source:", azureTTSPlayback.audioSource, typeof(AudioSource), true);

            //Mode
            EditorGUILayout.Space(SPACING);
            EditorGUILayout.LabelField("Playback Mode", EditorStyles.boldLabel);
            azureTTSPlayback.playbackMode = (AzureTTSPlayback.PlaybackMode)EditorGUILayout.EnumPopup("Playback Source", azureTTSPlayback.playbackMode);
            EditorGUI.indentLevel++;
            switch (azureTTSPlayback.playbackMode)
            {
                case AzureTTSPlayback.PlaybackMode.FileMode:
                    azureTTSPlayback.textAnimation = (TextAsset)EditorGUILayout.ObjectField("Text Animation", azureTTSPlayback.textAnimation, typeof(TextAsset), true);
                    azureTTSPlayback.animationAudioClip = (AudioClip)EditorGUILayout.ObjectField("Animation Audio Clip", azureTTSPlayback.animationAudioClip, typeof(AudioClip), true);
                    break;

                case AzureTTSPlayback.PlaybackMode.StreamStringMode:
                    EditorGUILayout.Space(SPACING);
                    EditorGUILayout.LabelField("API Connection Details", EditorStyles.boldLabel);
                    azureTTSPlayback.yourSubscriptionKey = EditorGUILayout.TextField("Your Subscription Key", azureTTSPlayback.yourSubscriptionKey);
                    azureTTSPlayback.yourServiceRegion = EditorGUILayout.TextField("Your Service Region", azureTTSPlayback.yourServiceRegion);
                    EditorGUILayout.Space(SPACING);

                    azureTTSPlayback.textSample = EditorGUILayout.TextField("Text Sample", azureTTSPlayback.textSample);
                    azureTTSPlayback.voiceName = EditorGUILayout.TextField("Voice Name", azureTTSPlayback.voiceName);
                    break;

                case AzureTTSPlayback.PlaybackMode.StreamSSMLMode:
                    EditorGUILayout.Space(SPACING);
                    EditorGUILayout.LabelField("API Connection Details", EditorStyles.boldLabel);
                    azureTTSPlayback.yourSubscriptionKey = EditorGUILayout.TextField("Your Subscription Key", azureTTSPlayback.yourSubscriptionKey);
                    azureTTSPlayback.yourServiceRegion = EditorGUILayout.TextField("Your Service Region", azureTTSPlayback.yourServiceRegion);
                    EditorGUILayout.Space(SPACING);

                    azureTTSPlayback.ssmlFile = EditorGUILayout.TextField("SSML File", azureTTSPlayback.ssmlFile);
                    break;

            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(SPACING * 2);

            if (GUILayout.Button("Playback TTS")) await azureTTSPlayback.Playback();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(azureTTSPlayback);
            }
        }
    }
}
#endif