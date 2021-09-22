using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Didimo
{
    public static class TextureLoader
    {
        public static async Task<Texture2D> LoadFromFilePath(string filePath)
        {
            try
            {
                Uri fileUri = new Uri(filePath);
                using UnityWebRequest request = UnityWebRequestTexture.GetTexture(fileUri);

                await request.SendWebRequest();

                if (request.error != null)
                {
                    Debug.LogWarning(request.error);
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (request.downloadHandler is DownloadHandlerTexture textureDownloadHandler)
                    {
                        return textureDownloadHandler.texture;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Downloading texture exception: {e}");
            }

            return null;
        }

        public static async Task<Texture2D> LoadFromURL(string url)
        {
            try
            {
                using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

                await request.SendWebRequest();

                if (request.error != null)
                {
                    Debug.LogWarning(request.error);
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (request.downloadHandler is DownloadHandlerTexture textureDownloadHandler)
                    {
                        return textureDownloadHandler.texture;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Downloading texture exception: {e}");
            }

            return null;
        }
    }
}