using System;
using UnityEngine;

namespace Didimo.Oculus.Internal
{
    public class OculusSceneLipSync : DidimoBehaviour
    {
        [SerializeField]
        private OVRLipSyncContextBase lipSyncContext;

        [SerializeField]
        private OVRLipSyncMicInput lipSyncMicInput;

        [SerializeField]
        private AudioSource lipSyncAudioSource;

        private static readonly string[] visemeNames = Enum.GetNames(typeof(OVRLipSync.Viseme));

        private void OnEnable()
        {
            // Enable all the required components for lipsync
            if (lipSyncContext != null) lipSyncContext.enabled = true;
            if (lipSyncMicInput != null) lipSyncMicInput.enabled = true;
            if (lipSyncAudioSource != null) lipSyncAudioSource.enabled = true;
        }

        private void Update()
        {
            
            if (lipSyncContext == null) return;

            OVRLipSync.Frame frame = lipSyncContext.GetCurrentPhonemeFrame();
            if (frame == null) return;
            
            float[] visemeWeights = frame.Visemes;
            for (int i = 0; i < visemeNames.Length; i++)
            {
                string visemeName = $"oculus_{visemeNames[i]}";
                float visemeWeight = visemeWeights[i];
                DidimoComponents.PoseController.SetWeightForPose(visemeName, visemeWeight);
            }
        }

        private void OnDisable()
        {
            DidimoComponents.PoseController.ResetAll();
            // Disable all the required components for lipsync
            if (lipSyncContext != null) lipSyncContext.enabled = false;
            if (lipSyncMicInput != null) lipSyncMicInput.enabled = false;
            if (lipSyncAudioSource != null) lipSyncAudioSource.enabled = false;
        }
    }
}