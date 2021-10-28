using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Didimo.Networking
{
    public class GetHairstylesQuery : Query<DeformResponse>
    {
        private const string INPUT_TYPE_HEADER = "input_type";
        private const string INPUT_TYPE        = "vertex_deform";
        private const string MIME_TYPE         = "application/octet-stream";
        private const string INPUT_FILE_NAME   = "template_deformation";
        public byte[] DeformationMatrix { get; }
        protected override string URL => $"{base.URL}/didimos";

        public GetHairstylesQuery(byte[] deformationMatrix)
        {
            DeformationMatrix = deformationMatrix;
        }

        protected override UnityWebRequest CreateRequest(Uri uri)
        {
            WWWForm form = new WWWForm();
            form.AddField(INPUT_TYPE_HEADER, INPUT_TYPE);
            form.AddBinaryData(INPUT_FILE_NAME, DeformationMatrix, INPUT_FILE_NAME, MIME_TYPE);
            UnityWebRequest request = UnityWebRequest.Post(uri, form);

            return request;
        }
    }
}