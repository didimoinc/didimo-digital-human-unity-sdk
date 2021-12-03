using System;
using System.Collections.Generic;
using System.Globalization;
using Didimo.Stats;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Didimo.Oculus.Example
{
    /// <summary>
    /// Class that manages and controls some FPS settings for Oculus apps.
    /// </summary>
    public class OculusApplicationManager : MonoBehaviour
    {

#if USING_OCULUS_INTEGRATION_PACKAGE
        public enum FPSTarget
        {
            [InspectorName("72 FPS")]  FPS_72,
            [InspectorName("80 FPS")]  FPS_80,
            [InspectorName("90 FPS")]  FPS_90,
            [InspectorName("120 FPS")] FPS_120,
        }
        
        public        FPSTarget targetDisplayFrequency    = FPSTarget.FPS_90;
        
        private const int       FPS_FRAMES_AVERAGE        = 10;
        private const float     FPS_UPDATE_TIME_FREQUENCY = 0.5f; // we'll update the text every X seconds

        private SceneStats.FPSData fpsData;
        private float              fpsUpdateCumulativeTime;
        private float TargetFPS => GetTargetFrequencyAsFloat(targetDisplayFrequency);

        
        [SerializeField]
        private TextMeshPro fpsText;

        /// <summary>
        /// Transform the chosen <c>targetDisplayFrequency</c> value into a float.
        /// </summary>
        /// <param name="fpsTarget"><c>FPSTarget</c> chosen in the inspector for the current scene.</param>
        /// <returns>FPS target float as a float.</returns>
        private static float GetTargetFrequencyAsFloat(FPSTarget fpsTarget)
        {
            switch (fpsTarget)
            {
                case FPSTarget.FPS_72:  return 72.0f;
                case FPSTarget.FPS_80:  return 80.0f;
                case FPSTarget.FPS_90:  return 90.0f;
                case FPSTarget.FPS_120: return 120.0f;
                default:                return 72.0f;
            }
        }

        private void Start()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            Application.targetFrameRate = (int) TargetFPS;
            QualitySettings.vSyncCount = 0;
            string freqs = string.Join(" ", new List<float>(OVRPlugin.systemDisplayFrequenciesAvailable).ConvertAll(i => i.ToString(CultureInfo.InvariantCulture)).ToArray());
            Debug.Log($"Available Frequencies: {freqs}");
            if (!Array.Exists(OVRPlugin.systemDisplayFrequenciesAvailable, value => Mathf.Approximately(value, TargetFPS)))
            {
                Debug.LogWarning($"Frequency {targetDisplayFrequency} not found in Display Frequencies available. Still will try to set it :)");
            }

            OVRPlugin.systemDisplayFrequency = TargetFPS;
#endif
            fpsData = SceneStats.GetFPSData(FPS_FRAMES_AVERAGE);
        }

        public void Update()
        {
            // Check target FPS is set to 90. if not, don't average it (it'd be dirty data)
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!Mathf.Approximately(OVRPlugin.systemDisplayFrequency, TargetFPS))
            {
                Debug.LogWarning($"Setting frequency again to to {targetDisplayFrequency}");
                OVRPlugin.systemDisplayFrequency = TargetFPS;
                return;
            }
#endif
            if (fpsText == null) return;
            fpsData.AddFrame(Time.deltaTime);

            // Update the max/min/average and text every so often
            fpsUpdateCumulativeTime += Time.deltaTime;
            if (fpsUpdateCumulativeTime >= FPS_UPDATE_TIME_FREQUENCY)
            {
                fpsUpdateCumulativeTime = 0f;
                fpsText.text = $"FPS: {fpsData.Average:F1}/{targetDisplayFrequency}\nMax: {fpsData.Max:F0}\nMin: {fpsData.Min:F0}";
            }
        }
#endif
    }
}
