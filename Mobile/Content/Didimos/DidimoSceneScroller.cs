using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Inspector;
using Didimo.Core.Utility;
using UnityEngine;

public class DidimoSceneScroller : ASingletonBehaviour<DidimoSceneScroller>
{
    [SerializeField]
    private GameObject[] didimos;

    public float scrollTime = 0.5f;

    private List<GameObject> didimoInstances;

    private int  currentDidimo = -1;
    private bool running;

    public bool InitializeIfNeeded(int initialDidimoIndex = 0)
    {
        if (didimoInstances == null)
        {
            didimoInstances = new List<GameObject>();
            foreach (var didimo in didimos)
            {
                GameObject didimoInstance = Instantiate(didimo);
                didimoInstance.SetActive(false);
                didimoInstances.Add(didimoInstance);
            }

            currentDidimo = initialDidimoIndex;
            didimoInstances[initialDidimoIndex].SetActive(true);
            return true;
        }

        return false;
    }

    // For use with button UI for ScrollToDidimo only
    [SerializeField]
    private int scrollToDidimo = 0;

    [Button]
    public void Stop()
    {
        if (running || !Application.isPlaying) return;

        foreach (GameObject didimoInstance in didimoInstances)
        {
            Destroy(didimoInstance);
        }

        currentDidimo = -1;
        didimoInstances = null;
    }

    [Button]
    public void ScrollToDidimo(int didimoIndex = -1, Action<float> progress = null)
    {
        if (running || !Application.isPlaying) return;
        
        if (didimoIndex == -1) didimoIndex = scrollToDidimo;
        if (didimoIndex < 0 || didimoIndex >= didimos.Length)
        {
            throw new Exception($"Index out of bounds. Must be equal to or greater than 0, and less than total didimos ({didimos.Length}).");
        }

        if (InitializeIfNeeded(didimoIndex)) return;

        GameObject didimoOut = didimoInstances[currentDidimo];
        GameObject didimoIn = didimoInstances[didimoIndex];
        StartCoroutine(ScrollCoroutine(didimoOut, didimoIn, didimoIndex > currentDidimo, progress));
        currentDidimo = didimoIndex;
    }

    [Button]
    public void NextDidimo(Action<float> progress = null)
    {
        if (running || !Application.isPlaying) return;

        if (InitializeIfNeeded()) return;

        GameObject didimoOut = didimoInstances[currentDidimo];
        currentDidimo++;
        currentDidimo %= didimos.Length;
        GameObject didimoIn = didimoInstances[currentDidimo];
        StartCoroutine(ScrollCoroutine(didimoOut, didimoIn, true, progress));
    }

    [Button]
    public void PreviousDidimo(Action<float> progress = null)
    {
        if (running || !Application.isPlaying) return;

        if (InitializeIfNeeded()) return;

        GameObject didimoOut = didimoInstances[currentDidimo];
        currentDidimo--;
        if (currentDidimo < 0) currentDidimo = didimos.Length + currentDidimo;
        GameObject didimoIn = didimoInstances[currentDidimo];
        StartCoroutine(ScrollCoroutine(didimoOut, didimoIn, false, progress));
    }

    Bounds GetBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return bounds;
    }

    /// <summary>
    /// Get the position we need to place the object at, for it to be off screen (outside camera frustum)
    /// </summary>
    /// <param name="obj">The object we want to place off screen</param>
    /// <param name="right">If true, places the object to the right of the screen, else places it to the left.</param>
    /// <returns></returns>
    Vector3 GetOffScreenPosition(GameObject obj, bool right)
    {
        Bounds bounds = GetBounds(obj);
        Camera mainCamera = Camera.main;
        Vector3 position = obj.transform.position;

        float distanceFromObjToCamera = Vector3.Distance(mainCamera!.transform.position, bounds.center);
        Vector3 screenCenter = mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth / 2f, mainCamera.pixelHeight / 2f, distanceFromObjToCamera));
        Vector3 centerLeftObjOffscreenPos = mainCamera.ScreenToWorldPoint(new Vector3(0, mainCamera.pixelHeight / 2f, distanceFromObjToCamera));

        Vector3 centerRightObjOffscreenPos = mainCamera.ScreenToWorldPoint(new Vector3(mainCamera.pixelWidth, mainCamera.pixelHeight / 2f, distanceFromObjToCamera));

        Vector3 offset;
        if (right)
        {
            offset = centerRightObjOffscreenPos - screenCenter;
            Vector3 boundsOffset = bounds.ClosestPoint(centerLeftObjOffscreenPos) - centerRightObjOffscreenPos;
            offset += mainCamera.transform.rotation * boundsOffset;
        }
        else
        {
            offset = centerLeftObjOffscreenPos - screenCenter;
            Vector3 boundsOffset = bounds.ClosestPoint(centerRightObjOffscreenPos) - centerLeftObjOffscreenPos;
            offset += mainCamera.transform.rotation * boundsOffset;
        }

        return position + offset;
    }

    IEnumerator ScrollCoroutine(GameObject didimoOut, GameObject didimoIn, bool scrollRight, Action<float> progress)
    {
        running = true;
        didimoIn.SetActive(true);

        Vector3 initialPositionOldDidimo = didimoOut.transform.position;
        Vector3 finalPositionOldDidimo = GetOffScreenPosition(didimoOut, !scrollRight);
        Vector3 initialPositionNewDidimo = GetOffScreenPosition(didimoIn, scrollRight);

        float time = 0f;
        while (time < scrollTime)
        {
            float smoothTime = Mathf.SmoothStep(0, 1, time / scrollTime);
            didimoOut.transform.position = Vector3.Lerp(initialPositionOldDidimo, finalPositionOldDidimo, smoothTime);
            didimoIn.transform.position = Vector3.Lerp(initialPositionNewDidimo, initialPositionOldDidimo, smoothTime);
            yield return null;
            if (progress != null)
            {
                progress(smoothTime);
            }

            time += Time.deltaTime;
        }

        didimoIn.transform.position = initialPositionOldDidimo;

        didimoOut.SetActive(false);

        running = false;
    }
}
