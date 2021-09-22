using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Didimo
{
    public static class JsonLoader
    {
        public static async Task<(bool success, string json)> TryLoadFromPathAsync(string filePath)
        {
            // Use web request so it also works on more restrictive devices like android
            UnityWebRequest request = UnityWebRequest.Get(new Uri(filePath));
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone && request.error == null)
            {
                await Task.Delay(100);
            }

            if (request.result != UnityWebRequest.Result.Success || request.error != null)
            {
                Debug.LogError($"Request url: {request.url}, request error: {request.error}");
                return (false, null);
            }

            string json = request.downloadHandler.text;
            return (!string.IsNullOrEmpty(json), json);
        }
    }
}