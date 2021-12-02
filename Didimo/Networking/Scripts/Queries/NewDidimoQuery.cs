using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public class NewDidimoQuery : Query<NewDidimoResponse>
    {
        private const string INPUT_TYPE_HEADER = "input_type";
        private const string INPUT_TYPE        = "photo";
        public string FilePath { get; }
        protected string mimeType;
        public string FileName => Path.GetFileName(FilePath);

        protected override string URL => $"{base.URL}/didimos";
        protected readonly List<ApiFeature> features;

        [Serializable]
        public class ApiFeature
        {
            public string FeatureName;
            public string FeatureValue;
        }

        public NewDidimoQuery(string filePath, List<ApiFeature> features)
        {
            this.features = features;
            FilePath = filePath;
            if (filePath.EndsWith(".jpg") || filePath.EndsWith(".jpeg"))
            {
                mimeType = "image/jpeg";
            }
            else if (filePath.EndsWith(".png"))
            {
                mimeType = "image/png";
            }
            else if (filePath.EndsWith(".heif") || filePath.EndsWith(".heic"))
            {
                mimeType = "image/heif";
            }
            else
            {
                throw new Exception($"Photo input has unsupported extension for NewDidimoQuery: {filePath}");
            }
        }

        protected byte[] GetSerializedData() => File.ReadAllBytes(FilePath);

        protected override UnityWebRequest CreateRequest(Uri uri)
        {
            WWWForm form = new WWWForm();
            form.AddField(INPUT_TYPE_HEADER, INPUT_TYPE);
            foreach (ApiFeature apiFeatures in features)
            {
                form.AddField(apiFeatures.FeatureName, apiFeatures.FeatureValue);
            }

            form.AddBinaryData(INPUT_TYPE, GetSerializedData(), FileName, mimeType);

            UnityWebRequest request = UnityWebRequest.Post(uri, form);

            return request;
        }
    }
}