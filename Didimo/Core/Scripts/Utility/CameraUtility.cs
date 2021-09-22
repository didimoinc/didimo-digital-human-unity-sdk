using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Didimo
{
    public static class CameraUtility
    {
        private const int RENDER_TEXTURE_DEPTH = 24;

        public static void GetNextCameraFrameImage(Action<Texture2D> callback, int width = 0, int height = 0, Texture2D overlay = null) { ThreadingUtility.Instance.StartCoroutine(RenderCameraCoroutine(callback, width, height, overlay)); }

        private static IEnumerator RenderCameraCoroutine(Action<Texture2D> callback, int width, int height, Texture2D overlay)
        {
            if (width == 0)
            {
                width = Screen.width;
            }

            if (height == 0)
            {
                height = Screen.height;
            }

            yield return new WaitForEndOfFrame();

            GameObject cameraGo = Object.Instantiate(DidimoResources.ScreenshotCamera);
            // To also disable the PPP on the main camera
            GameObject mainCameraGo = Camera.main!.gameObject;
            mainCameraGo.SetActive(false);
            Camera camera = cameraGo.GetComponentInChildren<Camera>();

            RenderTexture rt = new RenderTexture(width, height, RENDER_TEXTURE_DEPTH);

            Texture2D screenShot = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);

            camera.targetTexture = rt;
            camera.aspect = (float) screenShot.width / screenShot.height;
            camera.Render();
            camera.ResetAspect();
            mainCameraGo.SetActive(true);

            if (overlay != null)
            {
                Material mat = new Material(Shader.Find("Unlit/Transparent"));
                Graphics.Blit(overlay, rt, mat);
            }

            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
            screenShot.Apply();
            RenderTexture.active = null;

            Object.Destroy(cameraGo);
            Object.Destroy(rt);

            callback(screenShot);
        }
    }
}