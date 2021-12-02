using System;
using TMPro;
using UnityEngine;

namespace Didimo.Oculus.Example
{
    /// <summary>
    /// Class that manages which mode is being used by the didimo on the OculusTestApplication scene.
    /// It enables switching between the Distance Viewer, Face Animation and Lip Sync modes.
    /// </summary>
    public class OculusTestApplicationManager : MonoBehaviour
    {
#if USING_OCULUS_INTEGRATION_PACKAGE
        private enum ApplicationScene
        {
            DistanceViewer = 0,
            LipSync        = 1,
            FaceAnimation  = 2
        }
        
        [SerializeField]
        private ApplicationScene activeScene;

        [SerializeField]
        private TextMeshPro currentSceneGuideText;
        
        [SerializeField]
        private TextMeshPro switchSceneGuideText;
                
        [SerializeField]
        private OculusSceneDistanceViewer distanceViewer;
                
        [SerializeField]
        private OculusSceneLipSync        lipSync;
                
        [SerializeField]
        private OculusSceneFaceAnimation  faceAnimation;

        private static readonly string[] guideTexts =
        {
            "View the didimo at different distances.\nX to view closer and Y to view further or any trigger to reset.",
            "Use Oculus' LipSync to make your didimo move.\nMake sure the mic is unmuted and speak.",
            "View the didimo playing different facial expressions and movements."
        };

        private static int NumberOfScenes => Enum.GetValues(typeof(ApplicationScene)).Length;
        private string CurrentGuideText => guideTexts[(int) activeScene];
        private string SwitchSceneText => $"Current Scene: {activeScene.ToString()}\nPress A for {GetNextScene().ToString()} or B for {GetPreviousScene().ToString()}";
        
        private ApplicationScene GetNextScene() => (ApplicationScene) (((int) activeScene + 1) % NumberOfScenes);

        private ApplicationScene GetPreviousScene() => (ApplicationScene) (((int) activeScene - 1 + NumberOfScenes) % NumberOfScenes);

        private void Start()
        {
            UpdateGuideAndSwitchSceneTexts();
        }

        private void Update()
        {
            // Don't call OVRInput.Update() as we have OVRManager on scene that does it. On the docs it's an OR nat an AND, so if you call it again it breaks
            bool aPress;
            if ((aPress = OVRInput.GetDown(OVRInput.Button.One)) || OVRInput.GetDown(OVRInput.Button.Two))
            {
                // Disable current scene scripts, enable new scene scripts
                GetSceneController(activeScene).enabled = false;
                activeScene = aPress ? GetNextScene() : GetPreviousScene();
                GetSceneController(activeScene).enabled = true;
                UpdateGuideAndSwitchSceneTexts();
            }
        }

        /// <summary>
        /// Updates the texts on scene to the proper information.
        /// </summary>
        private void UpdateGuideAndSwitchSceneTexts()
        {
            if (currentSceneGuideText != null) currentSceneGuideText.text = CurrentGuideText;
            if (switchSceneGuideText != null) switchSceneGuideText.text = SwitchSceneText;
        }

        /// <summary>
        /// Get the controller for each mode of this scene.
        /// </summary>
        /// <param name="scene">Enabled mode.</param>
        /// <returns>Controller for the enabled mode.</returns>
        /// <exception cref="Exception">Current mode <c>scene</c> not handled.</exception>
        private MonoBehaviour GetSceneController(ApplicationScene scene)
        {
            switch (scene)
            {
                case ApplicationScene.DistanceViewer: return distanceViewer;
                case ApplicationScene.FaceAnimation:  return faceAnimation;
                case ApplicationScene.LipSync:        return lipSync;
                default:                              throw new Exception("Unmapped application scene to controller");
            }
        }
#endif
    }
}