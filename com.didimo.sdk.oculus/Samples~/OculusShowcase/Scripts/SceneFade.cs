using System;
using System.Collections;
using UnityEngine;
/// <summary>
/// Class to fade screen using a prefab on scene changes.
/// </summary>
public class SceneFade : MonoBehaviour
{
    const float uvScaleStart = 1f, uvScaleEnd = 2.5f;


    Material material => GetComponent<MeshRenderer>()?.sharedMaterial ?? null;

    // [Range (0, 1)] public float fadeValue;
    // void OnValidate()
    // {
    //     setMaterial (fadeValue);
    // }

    static SceneFade GetInstance(SceneFade prefab)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return null;
        SceneFade instance = Instantiate(prefab, Camera.main.transform);
        instance.transform.localPosition = Vector3.forward * (Camera.main.nearClipPlane + 0.01f);
        return instance;
    }

    public static void FadeIn(SceneFade prefab, float transitionTime, Action complete = null) =>
        GetInstance(prefab)?.fade(1, 0, transitionTime, complete);

    public static void FadeOut(SceneFade prefab, float transitionTime, Action complete = null) =>
        GetInstance(prefab)?.fade(0, 1, transitionTime, complete);

    private void fade(float start, float end, float transitionTime, Action complete = null) =>
        StartCoroutine(_fade(start, end, transitionTime, complete));

    IEnumerator _fade(float start, float end, float transitionTime, Action complete = null)
    {
        if (material && transitionTime > 0)
        {
            for (float startTime = Time.time, f = 0; f < 1;)
            {
                SetMaterial(Mathf.Lerp(start, end, f = Mathf.Clamp01((Time.time - startTime) / transitionTime)));
                yield return null;
            }
        }
        complete?.Invoke();
        yield return null;
        Destroy(gameObject);
    }

    // 0 full transparent / 1 opaque
    public void SetMaterial(float f)
    {
        if (material)
        {
            material.mainTextureScale = Vector2.one * Mathf.Lerp(uvScaleStart, uvScaleEnd, f);
        }
    }
}
