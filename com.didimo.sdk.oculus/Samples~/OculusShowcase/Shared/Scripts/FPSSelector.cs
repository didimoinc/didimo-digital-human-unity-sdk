using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSSelector : MonoBehaviour
{
#if USING_OCULUS_INTEGRATION_PACKAGE
    [Header("Tile Rendering")]
    [SerializeField] private OVRManager.FixedFoveatedRenderingLevel fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.Off;
    [SerializeField] private bool useDynamicFixedFoveatedRendering = false;

    private enum FPSTarget
    {
        [InspectorName("72 FPS")] FPS_72,
        [InspectorName("80 FPS")] FPS_80,
        [InspectorName("90 FPS")] FPS_90,
        [InspectorName("120 FPS")] FPS_120,
    }

    [Header("FPS")]
    [SerializeField]
    private FPSTarget targetDisplayFrequency = FPSTarget.FPS_90;

#pragma warning disable 0414
    [SerializeField]
    private float displayFrequencyCheckInterval = 5f;

    private float TargetFPS => GetTargetFrequencyAsFloat(targetDisplayFrequency);
#pragma warning restore 0414

    private static float GetTargetFrequencyAsFloat(FPSTarget fpsTarget)
    {
        switch (fpsTarget)
        {
            case FPSTarget.FPS_72: return 72.0f;
            case FPSTarget.FPS_80: return 80.0f;
            case FPSTarget.FPS_90: return 90.0f;
            case FPSTarget.FPS_120: return 120.0f;
            default: return 72.0f;
        }
    }

    private IEnumerator CheckDisplayFrequency()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        while (true)
        {
            if (!Mathf.Approximately(OVRPlugin.systemDisplayFrequency, TargetFPS))
            {
                Debug.LogWarning($"Setting frequency again to to {TargetFPS}");
                OVRPlugin.systemDisplayFrequency = TargetFPS;
            }

            yield return new WaitForSeconds(displayFrequencyCheckInterval);
        }
#else
        yield return null;
#endif
    }

    private void Start()
    {
        OVRManager.fixedFoveatedRenderingLevel = fixedFoveatedRenderingLevel;
        OVRManager.useDynamicFixedFoveatedRendering = useDynamicFixedFoveatedRendering;

        StartCoroutine(CheckDisplayFrequency());
    }

#endif
}
