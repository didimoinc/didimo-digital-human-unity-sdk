using System;
using TMPro;
using UnityEngine;

namespace Didimo.Oculus.Internal
{
    public class OculusTestApplicationManager : MonoBehaviour
    {
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

        private void UpdateGuideAndSwitchSceneTexts()
        {
            if (currentSceneGuideText != null) currentSceneGuideText.text = CurrentGuideText;
            if (switchSceneGuideText != null) switchSceneGuideText.text = SwitchSceneText;
        }

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
    }
}