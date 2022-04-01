using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public abstract class PostQuery<TResult> : Query<TResult>
    {
        private byte[] bodyData;

        protected override UnityWebRequest CreateRequest(Uri uri)
        {
            UploadHandler uploadHandler = new UploadHandlerRaw(bodyData);
            uploadHandler.contentType = "application/json";

            return new UnityWebRequest(uri) {uploadHandler = uploadHandler, downloadHandler = new DownloadHandlerBuffer(), method = "POST"};
        }

        public PostQuery(Dictionary<string, string> bodyContent)
        {
            string metaDataJson = JsonConvert.SerializeObject(bodyContent);
            bodyData = Encoding.UTF8.GetBytes(metaDataJson);
        }
    }
}