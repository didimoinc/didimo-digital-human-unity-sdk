using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Didimo.OculusApplication
{
    public class OculusApplicationManager : MonoBehaviour
    {
#if USING_OCULUS_INTEGRATION_PACKAGE
        private const int FPS_FRAMES_AVERAGE = 10;
        private const float TARGET_DISPLAY_FREQUENCY = 90.0f;
        private const float FPS_UPDATE_TIME_FREQUENCY = 0.5f; // we'll update the text every X seconds

        [SerializeField]
        private TextMeshPro text;
        private List<float> _fpsTracker;
        private float fpsUpdateCumulativeTime;

        private void Start()
        {
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
            string freqs = string.Join(" ", new List<float>(OVRPlugin.systemDisplayFrequenciesAvailable).ConvertAll(i => i.ToString(CultureInfo.InvariantCulture)).ToArray());
            Debug.Log($"Available Frequencies: {freqs}");
            if (!Array.Exists(OVRPlugin.systemDisplayFrequenciesAvailable, value => Mathf.Approximately(value, TARGET_DISPLAY_FREQUENCY)))
            {
                Debug.LogWarning($"Frequency {TARGET_DISPLAY_FREQUENCY} not found in Display Frequencies available. Still will try to set it :)");
            }

            OVRPlugin.systemDisplayFrequency = TARGET_DISPLAY_FREQUENCY;
            _fpsTracker = new List<float>(FPS_FRAMES_AVERAGE);
        }

        public void Update()
        {
            // Check target FPS is set to 90. if not, don't average it (it'd be dirty data)
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!Mathf.Approximately(OVRPlugin.systemDisplayFrequency, TARGET_DISPLAY_FREQUENCY))
            {
                Debug.LogWarning($"Setting frequency again to to {TARGET_DISPLAY_FREQUENCY}");
                OVRPlugin.systemDisplayFrequency = TARGET_DISPLAY_FREQUENCY;
                return;
            }
#endif

            // Add FPS data
            if (_fpsTracker.Count >= FPS_FRAMES_AVERAGE)
            {
                _fpsTracker.RemoveAt(0);
            }

            float fps = 1 / Time.deltaTime;
            _fpsTracker.Add(fps);

            // Update the text every so often
            fpsUpdateCumulativeTime += Time.deltaTime;
            if (fpsUpdateCumulativeTime < FPS_UPDATE_TIME_FREQUENCY) return;

            fpsUpdateCumulativeTime = 0f;
            float maxFPS = _fpsTracker.Max();
            float minFPS = _fpsTracker.Min();

            List<float> fpsValues = new List<float>(_fpsTracker);
            fpsValues.Remove(maxFPS);
            fpsValues.Remove(minFPS);

            float averageFPS = fpsValues.Average();
            text.text = $"FPS Performance\nAve:\t{averageFPS:F1}\nMax:\t{maxFPS:F0}\nMin:\t{minFPS:F0}";
        }
#endif
    }
}