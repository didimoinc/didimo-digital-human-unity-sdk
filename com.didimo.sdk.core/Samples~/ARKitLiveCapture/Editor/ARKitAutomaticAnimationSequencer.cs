using Didimo.Core.Animation;
using UnityEditor;
using UnityEngine;

namespace Didimo.LiveCapture.Editor
{
    public class ARKitAutomaticAnimationSequencerEditor : EditorWindow
    {

        private const string EDITOR_PREFS_SAVE_LOCATION = "Didimo.LiveCaptureToJson.LastSaveLocation";
        protected const string DEFAULT_MOCAP_NAME = "mocap.json";

        [SerializeField]
        private TextAsset jsonFile;

        [SerializeField]
        private static GameObject didimo;

        [MenuItem("CONTEXT/DidimoComponents/Didimo/Animation Setup In didimo")]
        public static void ShowWindow(MenuCommand command)
        {
            didimo = ((DidimoComponents)command.context).gameObject;
            ARKitAutomaticAnimationSequencerEditor window = GetWindow<ARKitAutomaticAnimationSequencerEditor>();
            window.titleContent = new GUIContent("Animation Setup In didimo");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            jsonFile = EditorGUILayout.ObjectField("Json Animation", jsonFile, typeof(TextAsset), false) as TextAsset;

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Place animation in didimo", GUILayout.ExpandWidth(true))) PlaceAnimation();
        }

        private void PlaceAnimation()
        {
            if(didimo.GetComponent<DidimoComponents>() == null)
            {
                Debug.LogError("Not a didimo");
            } else
            {
                if(didimo.GetComponent<AnimationSequencer>() != null)
                {
                    didimo.GetComponent<AnimationSequencer>().AddMocapAnimation(jsonFile);
                } else
                {
                    AnimationSequencer test =  didimo.AddComponent<AnimationSequencer>();
                    EditorApplication.delayCall += () => test.AddMocapAnimation(jsonFile);

                }
            }
        }
    }
}