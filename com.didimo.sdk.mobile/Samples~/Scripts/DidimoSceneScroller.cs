using System;
using System.Collections;
using System.Collections.Generic;
using Didimo.Core.Inspector;
using Didimo.Core.Utility;
using UnityEngine;

namespace Didimo.Mobile
{
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
                for (int i = 0; i < didimos.Length; i++)
                {
                    GameObject didimoInstance = Instantiate(didimos[i]);
                    didimoInstance.SetActive(false);
                    didimoInstances.Add(didimoInstance);
                    DidimoComponents didimoComponents = didimoInstance.GetComponent<DidimoComponents>();
                    didimoComponents.DidimoKey = i.ToString();
                    DidimoCache.Add(didimoComponents);
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
            if (didimoInstances == null || !Application.isPlaying) return;

            StopAllCoroutines();
            foreach (GameObject didimoInstance in didimoInstances)
            {
                DidimoCache.TryDestroy(didimoInstance.GetComponent<DidimoComponents>().DidimoKey);
            }

            currentDidimo = -1;
            didimoInstances = null;
        }

        [Button]
        public void ScrollToDidimo(int didimoIndex = -1, Action<float> progress = null)
        {
            if (running || !Application.isPlaying || didimoIndex == currentDidimo) return;

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

        private float? IntersectCameraFrustumWithRay(Camera cam, Ray ray)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

            foreach (Plane plane in planes)
            {
                if (plane.Raycast(ray, out float distance))
                {
                    return distance;
                }
            }

            return null;
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
            var cameraRightDir = mainCamera!.transform.right;

            Vector3 offsecreenDirection = right ?cameraRightDir: -cameraRightDir;

            Vector3 centerRightOffscreenPos = bounds.center + (cameraRightDir * IntersectCameraFrustumWithRay(mainCamera, new Ray(bounds.center, cameraRightDir))!.Value);
            Vector3 centerLeftOffscreenPos = bounds.center + (-cameraRightDir * IntersectCameraFrustumWithRay(mainCamera, new Ray(bounds.center, -cameraRightDir))!.Value);

            Vector3 result = obj.transform.position;

            // Why is the distance sometimes negative?
            float maxDimBounds = Mathf.Max(Mathf.Max(bounds.extents.x, bounds.extents.y), bounds.extents.z);

            Vector3 offscreen = (right ? centerRightOffscreenPos : centerLeftOffscreenPos);
            float distanceToOffscreen = Vector3.Distance(bounds.center, offscreen) + maxDimBounds;

            result += offsecreenDirection * distanceToOffscreen;

            return result;
        }

        IEnumerator ScrollCoroutine(GameObject didimoOut, GameObject didimoIn, bool scrollRight, Action<float> progress)
        {
            running = true;
            didimoIn.SetActive(true);

            // Place it in the center, so we can calculate the off screen position correctly
            didimoIn.transform.position = didimoOut.transform.position;
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
}