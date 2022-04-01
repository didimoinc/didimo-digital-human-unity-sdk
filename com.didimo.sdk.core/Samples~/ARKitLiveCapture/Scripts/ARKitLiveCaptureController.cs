using Didimo.Core.Inspector;
using Didimo.Core.Utility;
#if USING_LIVE_CAPTURE
using Unity.LiveCapture.ARKitFaceCapture;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Didimo.LiveCapture
{
    /// <summary>
    /// Class to control the LiveCapture demo scene, where it adds
    /// required components and updates the component fields.
    /// </summary>
    [ExecuteInEditMode]
    public class ARKitLiveCaptureController : MonoBehaviour
    {
        private static readonly Color ERROR_COLOR = new Color(120f/255f, 0f, 0f);
        private static readonly Color CORRECT_COLOR = new Color(0f, 170f/255f, 0f);

        [SerializeField]
        private Text text;
        
        
        
        public void OnEnable()
        {
            if (text != null) UpdateRequirementsText();
#if USING_LIVE_CAPTURE
            if (Application.isPlaying) SetupDidimoForLiveCapture();
#endif
        }
        
        
        
#if USING_LIVE_CAPTURE
        /// <summary>
        /// Setup didimo to use the LiveCapture by adding the FaceActor component
        /// if necessary and mapping it to the FaceDevice component
        /// </summary>
        [Button("Setup didimo for LiveCapture")]
        public void SetupDidimoForLiveCapture()
        {
            DidimoComponents didimo = FindObjectOfType<DidimoComponents>();
            if (didimo == null)
            {
                Debug.LogWarning("Could not find any active didimo in the scene");
                return;
            }
            
            // Update the actor mapper + clear the cache
            FaceActor faceActor =  ComponentUtility.GetOrAdd<FaceActor>(didimo.gameObject);
            faceActor.SetMapper(ScriptableObject.CreateInstance<DidimoLiveCaptureMapper>());
            // faceActor.ClearCache();
            
            // Update the scene device to point to this actor
            FaceDevice faceDevice = FindObjectOfType<FaceDevice>();
            faceDevice.Actor = faceActor;
        }
#endif

        private void UpdateRequirementsText()
        {
#if USING_LIVE_CAPTURE
            text.color = CORRECT_COLOR;
            text.text = Application.isPlaying ? "" : "Enter PlayMode to start the demo!";
#else
            text.color = ERROR_COLOR;
            text.text = "This example scene requires Unity's LiveCapture package to be installed.";
            Debug.LogError("This example scene requires Unity's LiveCapture package to be installed.");
#endif
        }
    }
}
