using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public abstract class Query
    {
        protected virtual string URL => DidimoNetworkingResources.NetworkConfig.BaseURL;

        protected static void BuildAuthHeaders(Uri uri, UnityWebRequest request)
        {
            APIKeyRequestHeader header = APIKeyRequestHeader.FromConfig();
            if (header.TryGetHeaders(uri, out Dictionary<string, string> headers))
            {
                foreach (KeyValuePair<string, string> kvp in headers)
                {
                    request.SetRequestHeader(kvp.Key, kvp.Value);
                }
            }
        }
    }

    public abstract class Query<TResult> : Query
    {
        public async Task<TResult> ExecuteQuery()
        {
            Task<(bool successs, string message, long responseCode)> httpTask = PostAsync();
            await httpTask;

            if (!httpTask.Result.successs)
            {
                Debug.LogWarning($"Call to '{URL}' resulted in status error '{httpTask.Result.responseCode}'");
                return default;
            }

            if (typeof(TResult) == typeof(DidimoEmptyResponse))
            {
                return default;
            }

            try
            {
                TResult result = JsonConvert.DeserializeObject<TResult>(httpTask.Result.message);
                return result;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to parse response from call to '{URL}'. Exception: '{e}'");
                return default;
            }
        }

        protected abstract UnityWebRequest CreateRequest(Uri uri);
        protected virtual void BuildAdditionalHeaders(Uri uri, UnityWebRequest request) { }

        protected async Task<(bool successs, string message, long responseCode)> PostAsync()
        {
            Uri uri = new Uri(URL);

            UnityWebRequest request = CreateRequest(uri);

            BuildAuthHeaders(uri, request);
            BuildAdditionalHeaders(uri, request);

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[{request.responseCode}] {request.error} ({uri}) ({request.downloadHandler?.text})");
                return (false, string.Empty, request.responseCode);
            }

            return (true, request.downloadHandler?.text, request.responseCode);
        }
    }
}